namespace Analog.Tests;

[UsesVerify]
public class ScannerTests
{
    [Fact]
    public async Task Scan_ScansLogStream()
    {
        using MemoryStream stream = new(Log.Content);
        IEnumerable<Scanner.Log> logs = await Scanner.Scan(stream, CancellationToken.None);
        await Verify(logs);
    }
}