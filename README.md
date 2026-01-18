# Groq Audio Transcription Benchmark

A console application for benchmarking Groq's Whisper API audio transcription performance. This tool processes multiple audio files, tracks performance metrics, and exports detailed results to Word and CSV formats.

## Features

✅ **Batch Audio Processing** - Process multiple audio files from a folder  
✅ **Performance Metrics** - Track processing time, RTF (Real-Time Factor), and token counts  
✅ **Audio Duration Detection** - Automatically detects audio file duration  
✅ **Word Document Export** - Professional formatted table output (.docx)  
✅ **CSV Export** - Backup CSV file for data analysis  
✅ **Organized Output** - Each benchmark run gets its own timestamped folder  
✅ **Error Handling** - Retry logic with circuit breaker pattern  
✅ **Progress Tracking** - Real-time console progress display  

## Prerequisites

- .NET 8.0 SDK or higher
- Groq API Key ([Get one here](https://console.groq.com/keys))
- Audio files in supported formats: `.mp3`, `.wav`, `.m4a`, `.ogg`, `.flac`, `.webm`

## Installation

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd GroqAudioBenchmark
Restore dependencies


dotnet restore
Build the project


dotnet build
Configuration
Edit appsettings.json to configure the application:


{
  "Groq": {
    "ApiKey": "your-groq-api-key-here",
    "Model": "whisper-large-v3-turbo"
  },
  "Benchmark": {
    "InputFolder": "C:\\AudioFiles",
    "OutputFolder": "Output",
    "SupportedExtensions": [ ".mp3", ".wav", ".m4a", ".ogg", ".flac", ".webm" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
Configuration Options:
Groq:ApiKey - Your Groq API key (required)
Groq:Model - Whisper model to use (default: whisper-large-v3-turbo)
Benchmark:InputFolder - Folder containing audio files to process
Benchmark:OutputFolder - Output folder name (created on Desktop)
Benchmark:SupportedExtensions - Audio file formats to process
Usage
Add your Groq API key to appsettings.json

Create input folder and add audio files


mkdir C:\AudioFiles
# Copy your audio files to this folder
Run the application


dotnet run
View results

Results are saved to: Desktop/Output/[YYYYMMDD_HHMMSS]/
Files created:
benchmark_results.docx - Word document with formatted table
benchmark_results.csv - CSV file for backup/analysis
Output Format
The benchmark generates a Word document with the following columns:

Column	Description
File Name	Name of the audio file
Audio Duration (min)	Duration of the audio in minutes
File Size (Mbs)	File size in megabytes
Processing Time (min)	Time taken to transcribe in minutes
RTF	Real-Time Factor (ProcessingTime / AudioDuration)
Output Tokens	Number of tokens in the transcription
Summary Metrics
After processing, the application displays:

Total files processed
Success/Error counts
Success rate percentage
Average Output Tokens
Project Structure

GroqAudioBenchmark/
│
├── Program.cs                          # Main application entry point
├── appsettings.json                    # Configuration file
│
├── Services/
│   ├── GroqService.cs                  # Groq API integration
│   ├── HttpClientService.cs            # HTTP client with retry/circuit breaker
│   ├── BenchmarkTracker.cs             # Tracks and exports results
│   └── WordDocumentExporter.cs         # Word document generation
│
├── Models/
│   ├── BenchmarkResult.cs              # Benchmark result data model
│   ├── GroqSettings.cs                 # Configuration model
│   └── TranscribeAudioRequestModel.cs  # Request model
│
├── Interfaces/
│   ├── IGroqService.cs                 # Groq service interface
│   ├── IHttpClientService.cs           # HTTP client interface
│   ├── IBenchmarkTracker.cs            # Tracker interface
│   └── IDocumentExporter.cs            # Document exporter interface
│
└── Enums/
    └── ProcessingStatus.cs             # Processing status enum
Dependencies
Package	Version	Purpose
Microsoft.Extensions.DependencyInjection	9.0.0	Dependency injection
Microsoft.Extensions.Configuration	9.0.0	Configuration management
Microsoft.Extensions.Logging	9.0.0	Logging framework
Microsoft.Extensions.Http	9.0.0	HTTP client factory
Polly	7.2.4	Retry and circuit breaker policies
DocumentFormat.OpenXml	3.0.0	Word document generation
NAudio	2.2.1	Audio file duration detection
TiktokenSharp	1.0.7	Token counting
Example Output

==============================================
    GROQ AUDIO TRANSCRIPTION BENCHMARK
==============================================
Input Folder: C:\AudioFiles
Output Folder: Desktop/Output
Model: whisper-large-v3-turbo
==============================================

Found 6 audio file(s) to process

[1/6] Processing: test-audio-01.mp3 (24.30 MB, 18.02 min)
✓ SUCCESS - Time: 0.06 min, RTF: 0.003553, Tokens: 2847
  Transcription preview: Hello, this is a test recording...

[2/6] Processing: test-audio-02.mp3 (1.70 MB, 1.00 min)
✓ SUCCESS - Time: 0.01 min, RTF: 0.013551, Tokens: 187

...

==============================================
              BENCHMARK SUMMARY
==============================================
Total Files Processed: 6
Successful: 6
Errors: 0
Success Rate: 100.0%

Average Output Tokens: 2450

Results folder: C:\Users\...\Desktop\Output\20260118_143052
Files created:
  - benchmark_results.docx
  - benchmark_results.csv
==============================================
Features Explained
Real-Time Factor (RTF)
RTF measures transcription speed relative to audio duration:

RTF < 1.0 = Faster than real-time (e.g., 0.003 means ~333x faster)
RTF = 1.0 = Real-time speed
RTF > 1.0 = Slower than real-time
Token Counting
Output tokens are counted using the GPT-4 tokenizer, providing insight into:

Transcription length and complexity
API usage estimation
Performance correlation analysis
Error Handling
Retry Policy: 3 retries with exponential backoff for transient errors
Circuit Breaker: Opens after 50% failure rate to prevent cascade failures
Graceful Degradation: Failed files are logged; processing continues
Troubleshooting
"Input folder does not exist"
Verify the path in appsettings.json
Create the folder: mkdir C:\AudioFiles
"No audio files found"
Check that files have supported extensions (.mp3, .wav, etc.)
Verify files are in the input folder
"Could not read audio duration"
The file may be corrupted or in an unsupported codec
Processing will continue, but duration will be 0
API Errors
Verify your Groq API key is correct
Check your API quota/limits
Ensure internet connectivity
Contributing
Contributions are welcome! Please feel free to submit a Pull Request.


Acknowledgments
Groq for the Whisper API
Built with .NET 8.0
