using GroqAudioBenchmark.Models;

namespace GroqAudioBenchmark.Interfaces
{
    public interface IBenchmarkTracker
    {
        void AddResult(BenchmarkResult result);
        void ExportToWord(string outputPath); 
    }
}
