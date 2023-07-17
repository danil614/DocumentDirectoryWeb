namespace DocumentDirectoryWeb.Helpers;

public static class Scripts
{
    public static string KeepLettersAndSpacesOnly(string input)
    {
        var output = new string(input.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
        return output;
    }
}