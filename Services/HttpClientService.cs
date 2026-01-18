using GroqAudioBenchmark.Interfaces;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GroqAudioBenchmark.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpClientService> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

        public HttpClientService(
            IHttpClientFactory httpClientFactory,
            ILogger<HttpClientService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(response => (int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (delegateResult, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Request failed. Waiting {TimeSpan} before retry. Retry attempt {RetryCount}",
                            timeSpan,
                            retryCount);
                    });

            _circuitBreaker = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => (int)response.StatusCode >= 500)
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5,
                    samplingDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 8,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (delegateResult, duration) =>
                    {
                        _logger.LogWarning("Circuit breaker opened for {DurationSeconds} seconds", duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    });
        }

        public async Task<T> PostAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null)
        {
            var client = CreateClient(headers);
            var response = await ExecuteWithPolicies(() => client.PostAsync(url, content));
            return await HandleResponse<T>(response);
        }

        private HttpClient CreateClient(Dictionary<string, string>? headers)
        {
            var client = _httpClientFactory.CreateClient();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<HttpResponseMessage> ExecuteWithPolicies(Func<Task<HttpResponseMessage>> action)
        {
            return await _retryPolicy
                .WrapAsync(_circuitBreaker)
                .ExecuteAsync(action);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {content}");
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }
    }
}
