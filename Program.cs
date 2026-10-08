using Avalonia;

namespace GoDoDevTools;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0)
            return await CommandLine.RunAsync(args);

        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .StartWithClassicDesktopLifetime(args);
        return 0;
    }
}
