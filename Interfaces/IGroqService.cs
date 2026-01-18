using GroqAudioBenchmark.Models;

namespace GroqAudioBenchmark.Interfaces
{
    public interface IGroqService
    {
        Task<string> TranscribeAudioAsync(TranscribeAudioRequestModel request, CancellationToken cancellationToken = default);
    }
}
