namespace GoDoDevTools;

internal static class SelfTest
{
    public static int Run()
    {
        try
        {
            var english = LocalizationChecker.ReadCatalog("msgid \"\"\nmsgstr \"\"\n\"Language: en\\n\"\n\nmsgid \"A.B\"\nmsgstr \"Value\"");
            var chinese = LocalizationChecker.ReadCatalog("msgid \"A.B\"\nmsgstr \"值\"");
            Require(english.Count == 1 && LocalizationChecker.Compare(english, chinese,
                new Dictionary<string, string> { ["A.B"] = "test.cs:1" }).Count == 0);
            Require(LocalizationChecker.Compare(english, chinese,
                new Dictionary<string, string> { ["A.MISSING"] = "test.cs:2" }).Count == 2);
            Require(LocalizationChecker.Compare(english,
                LocalizationChecker.ReadCatalog("msgid \"A.C\"\nmsgstr \"值\""),
                new Dictionary<string, string>()).Count == 2);
            try
            {
                LocalizationChecker.ReadCatalog("msgid \"A.B\"\nmsgstr \"\"");
                throw new Exception("Empty translation was accepted.");
            }
            catch (InvalidDataException) { }
            Console.WriteLine("Self-test passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new Exception("Self-test assertion failed.");
    }
}
