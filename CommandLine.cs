namespace GoDoDevTools;

internal static class CommandLine
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args is ["self-test"])
            return SelfTest.Run();

        if (args.Length < 2 || args[0] != "check")
        {
            Console.Error.WriteLine("Usage: GoDoDevTools check <project-directory> [--no-build] [--godot <executable>]");
            return 2;
        }

        var runBuild = true;
        string? godotExecutable = null;
        for (var index = 2; index < args.Length; index++)
        {
            if (args[index] == "--no-build") runBuild = false;
            else if (args[index] == "--godot" && index + 1 < args.Length) godotExecutable = args[++index];
            else
            {
                Console.Error.WriteLine($"Unknown or incomplete option: {args[index]}");
                return 2;
            }
        }

        var results = await CheckRunner.RunAsync(args[1], runBuild, godotExecutable);
        foreach (var result in results)
            Console.WriteLine(CheckRunner.Format(result));
        return results.All(result => result.Passed) ? 0 : 1;
    }
}
