using GroqAudioBenchmark.Interfaces;
using GroqAudioBenchmark.Models.Enums;
using GroqAudioBenchmark.Models;
using GroqAudioBenchmark.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using NAudio.Wave;
using TiktokenSharp;

class Program
{
    private static IConfiguration _configuration = null!;
    private static ILogger<Program> _logger = null!;
    private static TikToken _tikToken = null!;

    static async Task Main(string[] args)
    {
        // Initialize tokenizer
        _tikToken = TikToken.EncodingForModel("gpt-4");

        // Initialize
        _configuration = BuildConfiguration();
        var serviceProvider = ConfigureServices();
        _logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Get services
        var groqService = serviceProvider.GetRequiredService<IGroqService>();
        var benchmarkTracker = serviceProvider.GetRequiredService<IBenchmarkTracker>();

        // Display header
        DisplayHeader();

        // Get audio files
        var audioFiles = GetAudioFiles();
        if (audioFiles.Count == 0)
        {
            return;
        }

        Console.WriteLine($"Found {audioFiles.Count} audio file(s) to process\n");

        // Process files
        var (successCount, errorCount, results) = await ProcessAudioFiles(audioFiles, groqService, benchmarkTracker);

        // Export and display results
        ExportAndDisplayResults(benchmarkTracker, audioFiles.Count, successCount, errorCount, results);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        services.Configure<GroqSettings>(_configuration.GetSection("Groq"));

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddConfiguration(_configuration.GetSection("Logging"));
        });

        // HttpClient
        services.AddHttpClient();

        // Services
        services.AddSingleton<IHttpClientService, HttpClientService>();
        services.AddSingleton<IGroqService, GroqService>();
        services.AddSingleton<IDocumentExporter, WordDocumentExporter>();
        services.AddSingleton<IBenchmarkTracker, BenchmarkTracker>();

        return services.BuildServiceProvider();
    }

    private static void DisplayHeader()
    {
        var inputFolder = _configuration["Benchmark:InputFolder"] ?? "C:\\AudioFiles";
        var outputFolder = _configuration["Benchmark:OutputFolder"] ?? "Output";
        var groqModel = _configuration["Groq:Model"] ?? "whisper-large-v3-turbo";

        Console.WriteLine("==============================================");
        Console.WriteLine("    GROQ AUDIO TRANSCRIPTION BENCHMARK");
        Console.WriteLine("==============================================");
        Console.WriteLine($"Input Folder: {inputFolder}");
        Console.WriteLine($"Output Folder: Desktop/{outputFolder}");
        Console.WriteLine($"Model: {groqModel}");
        Console.WriteLine("==============================================\n");
    }

    private static List<string> GetAudioFiles()
    {
        var inputFolder = _configuration["Benchmark:InputFolder"] ?? "C:\\AudioFiles";
        var supportedExtensions = _configuration.GetSection("Benchmark:SupportedExtensions").Get<string[]>()
            ?? new[] { ".mp3", ".wav", ".m4a", ".ogg", ".flac", ".webm" };

        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine($"ERROR: Input folder does not exist: {inputFolder}");
            Console.WriteLine("Please create the folder and add audio files, or update the path in appsettings.json");
            return new List<string>();
        }

        var audioFiles = Directory.GetFiles(inputFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
            .ToList();

        if (audioFiles.Count == 0)
        {
            Console.WriteLine($"ERROR: No audio files found in {inputFolder}");
            Console.WriteLine($"Supported formats: {string.Join(", ", supportedExtensions)}");
        }

        return audioFiles;
    }

    private static async Task<(int successCount, int errorCount, List<BenchmarkResult> results)> ProcessAudioFiles(
        List<string> audioFiles,
        IGroqService groqService,
        IBenchmarkTracker benchmarkTracker)
    {
        int successCount = 0;
        int errorCount = 0;
        var results = new List<BenchmarkResult>();
        var groqModel = _configuration["Groq:Model"] ?? "whisper-large-v3-turbo";

        for (int i = 0; i < audioFiles.Count; i++)
        {
            var filePath = audioFiles[i];
            var result = await ProcessSingleFile(filePath, i + 1, audioFiles.Count, groqService, groqModel);

            benchmarkTracker.AddResult(result);
            results.Add(result);

            if (result.Status == ProcessingStatus.Success)
                successCount++;
            else
                errorCount++;
        }

        return (successCount, errorCount, results);
    }

    private static async Task<BenchmarkResult> ProcessSingleFile(
        string filePath,
        int currentIndex,
        int totalFiles,
        IGroqService groqService,
        string modelUsed)
    {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new FileInfo(filePath);
        var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

        double audioDurationMinutes = 0;
        try
        {
            using var audioFile = new AudioFileReader(filePath);
            audioDurationMinutes = audioFile.TotalTime.TotalMinutes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read audio duration for {FileName}", fileName);
            audioDurationMinutes = 0;
        }

        var stopwatch = Stopwatch.StartNew();
        var result = new BenchmarkResult
        {
            FileName = fileName,
            FileSizeMB = fileSizeMB,
            AudioDurationMinutes = audioDurationMinutes,
            ModelUsed = modelUsed,
            Timestamp = DateTime.Now
        };

        try
        {
            using var fileStream = File.OpenRead(filePath);

            var request = new TranscribeAudioRequestModel
            {
                AudioStream = fileStream,
                FileName = fileName
            };

            var transcription = await groqService.TranscribeAudioAsync(request);

            stopwatch.Stop();

            var outputTokens = _tikToken.Encode(transcription).Count;

            result.ProcessingTimeMinutes = stopwatch.Elapsed.TotalMinutes;
            result.Status = ProcessingStatus.Success;
            result.TranscriptionText = transcription;
            result.OutputTokens = outputTokens;

            if (audioDurationMinutes > 0)
                result.RTF = result.ProcessingTimeMinutes / audioDurationMinutes;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            result.ProcessingTimeMinutes = stopwatch.Elapsed.TotalMinutes;
            result.Status = ProcessingStatus.Error;
            result.ErrorMessage = ex.Message;
            result.OutputTokens = 0;

            _logger.LogError(ex, "Error processing file: {FileName}", fileName);
        }

        return result;
    }

    private static void ExportAndDisplayResults(
        IBenchmarkTracker benchmarkTracker,
        int totalFiles,
        int successCount,
        int errorCount,
        List<BenchmarkResult> results)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var outputFolderBase = Path.Combine(desktopPath, "Output");

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputFolder = Path.Combine(outputFolderBase, timestamp);

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        var wordOutputPath = Path.Combine(outputFolder, "benchmark_results.docx");

        benchmarkTracker.ExportToWord(wordOutputPath);

        var successfulResults = results.Where(r => r.Status == ProcessingStatus.Success).ToList();
        var avgOutputTokens = successfulResults.Any() ? successfulResults.Average(r => r.OutputTokens) : 0;
    }

    private static string GetPreview(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
    }
}