using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public sealed class OutputIndexService : IOutputIndexService
{
    private const string IndexFileName = "report_index.json";
    private const string SchemaName = "police-image-toolkit-report-index";
    private const int CurrentSchemaVersion = 1;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> DirectoryLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public Task AppendEntriesAsync(string outputDirectory, IEnumerable<OutputIndexEntry> entries, CancellationToken cancellationToken = default)
    {
        var safeEntries = entries
            .Where(IsValidEntry)
            .Select(entry => entry with
            {
                OutputFile = Path.GetFileName(entry.OutputFile),
                SourceFile = Path.GetFileName(entry.SourceFile)
            })
            .ToList();

        if (safeEntries.Count == 0) return Task.CompletedTask;

        return UpdateAsync(outputDirectory, document =>
        {
            foreach (var entry in safeEntries)
            {
                document.Entries.RemoveAll(existing => string.Equals(existing.OutputFile, entry.OutputFile, StringComparison.OrdinalIgnoreCase));
                document.Entries.Add(entry);
            }
        }, cancellationToken);
    }

    public Task RemoveEntriesAsync(string outputDirectory, IEnumerable<string> outputFiles, CancellationToken cancellationToken = default)
    {
        var names = outputFiles
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (names.Count == 0 || !File.Exists(Path.Combine(outputDirectory, IndexFileName))) return Task.CompletedTask;

        return UpdateAsync(outputDirectory, document =>
        {
            document.Entries.RemoveAll(entry => names.Contains(entry.OutputFile));
        }, cancellationToken);
    }

    private static bool IsValidEntry(OutputIndexEntry entry) =>
        !string.IsNullOrWhiteSpace(entry.OutputFile) &&
        !string.IsNullOrWhiteSpace(entry.SourceFile) &&
        !string.IsNullOrWhiteSpace(entry.SourceType);

    private static async Task UpdateAsync(string outputDirectory, Action<OutputIndexDocument> update, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("輸出資料夾不可為空白。", nameof(outputDirectory));
        }

        string fullDirectory = Path.GetFullPath(outputDirectory);
        var gate = DirectoryLocks.GetOrAdd(fullDirectory, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(fullDirectory);
            string indexPath = Path.Combine(fullDirectory, IndexFileName);
            var document = await ReadDocumentAsync(indexPath, cancellationToken);
            update(document);
            document.Schema = SchemaName;
            document.SchemaVersion = CurrentSchemaVersion;
            document.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await WriteAtomicallyAsync(indexPath, document, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private static async Task<OutputIndexDocument> ReadDocumentAsync(string indexPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(indexPath)) return new OutputIndexDocument();

        try
        {
            string json = await File.ReadAllTextAsync(indexPath, cancellationToken);
            var document = JsonSerializer.Deserialize<OutputIndexDocument>(json, JsonOptions);
            if (document is null || !string.Equals(document.Schema, SchemaName, StringComparison.Ordinal))
            {
                throw new InvalidDataException("既有 report_index.json 不是本工具建立的索引格式。");
            }

            document.Entries ??= [];
            return document;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("既有 report_index.json 無法讀取，未變更該索引。", ex);
        }
    }

    private static async Task WriteAtomicallyAsync(string indexPath, OutputIndexDocument document, CancellationToken cancellationToken)
    {
        string temporaryPath = Path.Combine(
            Path.GetDirectoryName(indexPath)!,
            $".{Path.GetFileName(indexPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            string json = JsonSerializer.Serialize(document, JsonOptions);
            await File.WriteAllTextAsync(temporaryPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
            File.Move(temporaryPath, indexPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private sealed class OutputIndexDocument
    {
        public string Schema { get; set; } = OutputIndexService.SchemaName;
        public int SchemaVersion { get; set; } = OutputIndexService.CurrentSchemaVersion;
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public List<OutputIndexEntry> Entries { get; set; } = [];
    }
}
