using GroqAudioBenchmark.Interfaces;
using GroqAudioBenchmark.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroqAudioBenchmark.Services
{
    public class GroqService : IGroqService
    {
        private readonly string _baseUrl = "https://api.groq.com/openai/v1/";
        private readonly string _model;
        private readonly string _apiKey;
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<GroqService> _logger;

        public GroqService(
            IHttpClientService httpClientService,
            IOptions<GroqSettings> settings,
            ILogger<GroqService> logger)
        {
            _model = settings.Value.Model;
            _apiKey = settings.Value.ApiKey;
            _httpClientService = httpClientService;
            _logger = logger;
        }

        public async Task<string> TranscribeAudioAsync(TranscribeAudioRequestModel requestModel, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = _baseUrl + "audio/transcriptions";

                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(requestModel.AudioStream), "file", requestModel.FileName);
                form.Add(new StringContent(_model), "model");

                var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {_apiKey}" } };

                var response = await _httpClientService.PostAsync<GroqTranscriptionResponse>(url, form, headers);

                return !string.IsNullOrEmpty(response.Text) ? response.Text : throw new Exception("No transcription received from Groq");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Groq transcription service");
                throw;
            }
        }
    }
}
