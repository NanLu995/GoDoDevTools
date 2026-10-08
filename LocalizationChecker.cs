using System.Text.RegularExpressions;

namespace GoDoDevTools;

internal static class LocalizationChecker
{
    private static readonly Regex KeyLiteral = new("\"([A-Z][A-Z0-9_]*(?:\\.[A-Z0-9_]+)+)\"", RegexOptions.Compiled);
    private static readonly Regex IdLine = new("^msgid \"(.*)\"$", RegexOptions.Compiled);
    private static readonly Regex ValueLine = new("^msgstr \"(.*)\"$", RegexOptions.Compiled);
    private static readonly Regex Continuation = new("^\"(.*)\"$", RegexOptions.Compiled);
    private static readonly string[] SourceDirectories = ["GameModules", "GameContents", "GameResources", "Shared", "Ui"];
    private static readonly HashSet<string> SourceExtensions = [".cs", ".gd", ".tscn", ".tres"];

    public static CheckResult Check(string projectRoot)
    {
        try
        {
            var english = ReadCatalog(File.ReadAllText(Path.Combine(projectRoot, "Localization", "en.po")));
            var chinese = ReadCatalog(File.ReadAllText(Path.Combine(projectRoot, "Localization", "zh_CN.po")));
            var references = FindReferences(projectRoot);
            var problems = Compare(english, chinese, references);
            return new CheckResult("Localization", problems.Count == 0,
                problems.Count == 0
                    ? [$"{english.Count} catalog keys, {references.Count} static references."]
                    : problems);
        }
        catch (Exception exception)
        {
            return new CheckResult("Localization", false, [exception.Message]);
        }
    }

    internal static Dictionary<string, string> ReadCatalog(string content)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        string? key = null;
        string? translation = null;
        string field = "";

        void Commit()
        {
            if (string.IsNullOrEmpty(key)) return; // gettext header
            if (string.IsNullOrEmpty(translation)) throw new InvalidDataException($"Missing translation: {key}");
            if (!entries.TryAdd(key, translation)) throw new InvalidDataException($"Duplicate key: {key}");
        }

        foreach (var line in content.Split('\n'))
        {
            var value = line.TrimEnd('\r');
            var id = IdLine.Match(value);
            if (id.Success)
            {
                Commit();
                key = id.Groups[1].Value;
                translation = null;
                field = "id";
            }
            else if ((id = ValueLine.Match(value)).Success)
            {
                if (key is null) throw new InvalidDataException("msgstr without msgid.");
                translation = id.Groups[1].Value;
                field = "str";
            }
            else if ((id = Continuation.Match(value)).Success)
            {
                if (field == "id") key += id.Groups[1].Value;
                else if (field == "str") translation += id.Groups[1].Value;
            }
            else if (value.StartsWith("msgid_plural ", StringComparison.Ordinal) ||
                     value.StartsWith("msgctxt ", StringComparison.Ordinal) ||
                     value.StartsWith("msgstr[", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Unsupported PO field: {value}");
            }
        }
        Commit();
        return entries;
    }

    internal static List<string> Compare(IReadOnlyDictionary<string, string> english,
        IReadOnlyDictionary<string, string> chinese, IReadOnlyDictionary<string, string> references)
    {
        var problems = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var key in english.Keys)
            if (!chinese.ContainsKey(key)) problems.Add($"zh_CN.po missing: {key}");
        foreach (var key in chinese.Keys)
            if (!english.ContainsKey(key)) problems.Add($"en.po missing: {key}");
        foreach (var (key, location) in references)
        {
            if (!english.ContainsKey(key)) problems.Add($"en.po missing: {key} ({location})");
            if (!chinese.ContainsKey(key)) problems.Add($"zh_CN.po missing: {key} ({location})");
        }
        return [.. problems];
    }

    private static Dictionary<string, string> FindReferences(string projectRoot)
    {
        var references = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var directory in SourceDirectories)
        {
            var path = Path.Combine(projectRoot, directory);
            if (!Directory.Exists(path)) continue;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                if (!SourceExtensions.Contains(Path.GetExtension(file))) continue;
                var lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    foreach (Match match in KeyLiteral.Matches(line))
                        references.TryAdd(match.Groups[1].Value,
                            $"{Path.GetRelativePath(projectRoot, file)}:{lineNumber}");
                }
            }
        }
        return references;
    }
}
