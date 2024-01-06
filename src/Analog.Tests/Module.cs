using System.Runtime.CompilerServices;

namespace Analog.Tests;

public static class Module
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.UseStrictJson();
        VerifierSettings.DontScrubDateTimes();
    }
}