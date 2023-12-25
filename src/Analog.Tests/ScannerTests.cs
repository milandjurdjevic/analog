namespace Analog.Tests;

[UsesVerify]
public class ScannerTests
{
    [Fact]
    public async Task Scan_ScansLogStream()
    {
        const string filename = $"{nameof(ScannerTests)}.{nameof(Scan_ScansLogStream)}.input.log";
        string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
        byte[] bytes = await File.ReadAllBytesAsync(path, CancellationToken.None);
        using MemoryStream stream = new(bytes);
        IEnumerable<Scanner.Log> logs = await Scanner.Scan(stream, CancellationToken.None);
        await Verify(logs);
    }
}