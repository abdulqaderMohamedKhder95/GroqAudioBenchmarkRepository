using GroqAudioBenchmark.Interfaces;
using GroqAudioBenchmark.Models;
using System.Text;

namespace GroqAudioBenchmark.Services
{
    public class BenchmarkTracker : IBenchmarkTracker
    {
        private readonly List<BenchmarkResult> _results = new();
        private readonly IDocumentExporter _documentExporter;

        public BenchmarkTracker(IDocumentExporter documentExporter)
        {
            _documentExporter = documentExporter;
        }

        public void AddResult(BenchmarkResult result)
        {
            _results.Add(result);
        }


        public void ExportToWord(string outputPath)
        {
            _documentExporter.ExportToWord(_results, outputPath);
        }
    }
}
