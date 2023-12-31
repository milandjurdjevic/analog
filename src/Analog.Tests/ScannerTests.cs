namespace Analog.Tests;

[UsesVerify]
public class ScannerTests
{
    [Fact]
    public async Task Scan_ScansLogStream()
    {
        using MemoryStream stream = new(Input.Bytes);
        await Verify(await Scanner.Scan(stream));
    }
}