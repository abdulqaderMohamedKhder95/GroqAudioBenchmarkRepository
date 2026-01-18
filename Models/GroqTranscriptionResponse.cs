using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroqAudioBenchmark.Models
{
    public record GroqTranscriptionResponse
    {
        public string Text { get; init; } = null!;
    }
}
