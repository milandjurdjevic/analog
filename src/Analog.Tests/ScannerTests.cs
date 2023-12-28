namespace Analog.Tests;

[UsesVerify]
public class ScannerTests
{
    [Fact]
    public async Task Scan_ScansLogStream()
    {
        using MemoryStream stream = new(Log.Content);
        IEnumerable<IReadOnlyDictionary<string, string>> logs = await Scanner.Scan(stream, CancellationToken.None);
        await Verify(logs);
    }
}