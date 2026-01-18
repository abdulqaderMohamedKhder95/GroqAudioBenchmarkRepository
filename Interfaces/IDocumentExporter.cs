using GroqAudioBenchmark.Models;

namespace GroqAudioBenchmark.Interfaces
{
    public interface IDocumentExporter
    {
        void ExportToWord(List<BenchmarkResult> results, string outputPath);
    }
}
