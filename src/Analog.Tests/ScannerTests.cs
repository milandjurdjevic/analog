namespace Analog.Tests;

[UsesVerify]
public class ScannerTests
{
    [Fact]
    public async Task Scan_ScansLogStream()
    {
        using MemoryStream stream = new(Input.Bytes);
        IEnumerable<IReadOnlyDictionary<string, string>> logs = await Scanner.Scan(stream);
        await Verify(logs);
    }
}