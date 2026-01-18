namespace GroqAudioBenchmark.Models
{
    public class TranscribeAudioRequestModel
    {
        public required Stream AudioStream { get; init; }
        public required string FileName { get; init; }
    }
}
