namespace GroqAudioBenchmark.Interfaces
{
    public interface IHttpClientService
    {
        Task<T> PostAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null);
    }
}
