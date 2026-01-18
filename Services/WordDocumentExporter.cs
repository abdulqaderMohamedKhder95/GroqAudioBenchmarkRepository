using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using GroqAudioBenchmark.Interfaces;
using GroqAudioBenchmark.Models;

namespace GroqAudioBenchmark.Services
{
    public class WordDocumentExporter : IDocumentExporter
    {
        public void ExportToWord(List<BenchmarkResult> results, string outputPath)
        {
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create Word document
            using var wordDocument = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);

            // Add main document part
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Add title
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleRun = titleParagraph.AppendChild(new Run());
            titleRun.AppendChild(new Text("Table showing transcription Benchmark results"));

            // Format title (bold and centered)
            var titleParagraphProperties = titleParagraph.InsertAt(new ParagraphProperties(), 0);
            titleParagraphProperties.AppendChild(new Justification { Val = JustificationValues.Center });
            var titleRunProperties = titleRun.InsertAt(new RunProperties(), 0);
            titleRunProperties.AppendChild(new Bold());

            // Add spacing after title
            body.AppendChild(new Paragraph());

            // Create table
            var table = new Table();

            // Table properties (borders)
            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 }
                )
            );
            table.AppendChild(tableProperties);

            // Create header row
            var headerRow = new TableRow();
            headerRow.Append(
                CreateHeaderCell("File Name"),
                CreateHeaderCell("Audio Duration\n(min)"),
                CreateHeaderCell("File Size\n(Mbs)"),
                CreateHeaderCell("Processing Time\n(min)"),
                CreateHeaderCell("RTF"),
                CreateHeaderCell("Output Tokens")  // NEW COLUMN
            );
            table.Append(headerRow);

            // Add data rows
            foreach (var result in results)
            {
                var dataRow = new TableRow();
                dataRow.Append(
                    CreateDataCell(result.FileName),
                    CreateDataCell(result.AudioDurationMinutes.ToString("F2")),
                    CreateDataCell(result.FileSizeMB.ToString("F2")),
                    CreateDataCell(result.ProcessingTimeMinutes.ToString("F2")),
                    CreateDataCell(result.RTF.ToString("F6")),
                    CreateDataCell(result.OutputTokens.ToString())  // NEW COLUMN
                );
                table.Append(dataRow);
            }

            body.Append(table);
            mainPart.Document.Save();
        }

        private static TableCell CreateHeaderCell(string text)
        {
            var cell = new TableCell();

            // Cell properties with shading (gray background)
            var cellProperties = new TableCellProperties(
                new Shading
                {
                    Val = ShadingPatternValues.Clear,
                    Color = "auto",
                    Fill = "D9D9D9" // Light gray
                }
            );
            cell.Append(cellProperties);

            // Paragraph with text
            var paragraph = new Paragraph();
            var run = new Run();

            // Bold text
            var runProperties = new RunProperties();
            runProperties.Append(new Bold());
            run.Append(runProperties);
            run.Append(new Text(text));

            paragraph.Append(run);

            // Center align
            var paragraphProperties = new ParagraphProperties();
            paragraphProperties.Append(new Justification { Val = JustificationValues.Center });
            paragraph.InsertAt(paragraphProperties, 0);

            cell.Append(paragraph);
            return cell;
        }

        private static TableCell CreateDataCell(string text)
        {
            var cell = new TableCell();
            var paragraph = new Paragraph();
            var run = new Run(new Text(text));
            paragraph.Append(run);

            // Center align
            var paragraphProperties = new ParagraphProperties();
            paragraphProperties.Append(new Justification { Val = JustificationValues.Center });
            paragraph.InsertAt(paragraphProperties, 0);

            cell.Append(paragraph);
            return cell;
        }
    }
}
