using System.Diagnostics;

namespace GoDoDevTools;

internal sealed record CheckResult(string Name, bool Passed, IReadOnlyList<string> Messages);

internal static class CheckRunner
{
    public static async Task<IReadOnlyList<CheckResult>> RunAsync(string projectRoot, bool runBuild, string? godotExecutable)
    {
        if (!Directory.Exists(projectRoot) || !File.Exists(Path.Combine(projectRoot, "project.godot")))
            return [new CheckResult("Project", false, ["Select a directory containing project.godot."])];

        var results = new List<CheckResult> { LocalizationChecker.Check(projectRoot) };
        if (runBuild)
        {
            var build = await RunProcessAsync("Build", "dotnet", projectRoot, ["build", "GoDoGame.sln"]);
            results.Add(build);
            if (!build.Passed) return results;
        }
        if (godotExecutable is not null)
        {
            if (!File.Exists(godotExecutable))
                results.Add(new CheckResult("Godot UI regression", false, ["Godot executable does not exist."]));
            else
                results.Add(await RunProcessAsync("Godot UI regression", godotExecutable, projectRoot,
                    ["--headless", "--path", projectRoot, "Verification/Automated/StarterUiFlowRegression.tscn"]));
        }
        return results;
    }

    public static string Format(CheckResult result) =>
        $"{(result.Passed ? "PASS" : "FAIL")} {result.Name}\n{string.Join("\n", result.Messages)}";

    private static async Task<CheckResult> RunProcessAsync(string name, string executable, string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(executable)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);

            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                return new CheckResult(name, false, ["Timed out after three minutes."]);
            }

            var output = (await stdout + "\n" + await stderr).Trim();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var summary = process.ExitCode == 0 ? lines.TakeLast(5) : lines.Take(50);
            return new CheckResult(name, process.ExitCode == 0,
                [.. summary.DefaultIfEmpty($"Process exited with code {process.ExitCode}.")]);
        }
        catch (Exception exception)
        {
            return new CheckResult(name, false, [exception.Message]);
        }
    }
}
