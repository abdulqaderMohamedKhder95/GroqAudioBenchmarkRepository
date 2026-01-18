using GroqAudioBenchmark.Models.Enums;

namespace GroqAudioBenchmark.Models
{
    public class BenchmarkResult
    {
        public string FileName { get; set; } = string.Empty;
        public double AudioDurationMinutes { get; set; }
        public double FileSizeMB { get; set; }
        public double ProcessingTimeMinutes { get; set; }
        public double RTF { get; set; }
        public int OutputTokens { get; set; }  
        public ProcessingStatus Status { get; set; }
        public string TranscriptionText { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
