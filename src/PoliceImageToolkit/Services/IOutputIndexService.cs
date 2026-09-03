using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public interface IOutputIndexService
{
    Task AppendEntriesAsync(string outputDirectory, IEnumerable<OutputIndexEntry> entries, CancellationToken cancellationToken = default);
    Task RemoveEntriesAsync(string outputDirectory, IEnumerable<string> outputFiles, CancellationToken cancellationToken = default);
}
