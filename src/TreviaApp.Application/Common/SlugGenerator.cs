namespace TreviaApp.Application.Common;

using System.Globalization;
using System.Text;

public static class SlugGenerator
{
    public static string GenerateSlug(string input, int maxLen = 250)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var clean = sb.ToString().Normalize(NormalizationForm.FormC);

        sb.Clear();
        var prevDash = false;
        foreach (var c in clean)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
                prevDash = false;
            }
            else if (c == ' ' || c == '-' || c == '_' || c == '/')
            {
                if (!prevDash)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }
        }

        var result = sb.ToString().Trim('-');

        if (result.Length > maxLen)
            result = result.Substring(0, maxLen).Trim('-');

        return result;
    }

    public static async Task<string> GenerateUniqueSlug(string name, Func<string, Task<bool>> slugExists, int maxLen = 250)
    {
        var baseSlug = GenerateSlug(name, maxLen);
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "exercise";

        var current = baseSlug;
        var counter = 2;

        while (await slugExists(current))
        {
            var suffix = $"-{counter}";
            var allowedLen = maxLen - suffix.Length;
            var truncatedBase = baseSlug.Length > allowedLen ? baseSlug.Substring(0, allowedLen) : baseSlug;
            current = $"{truncatedBase}{suffix}";
            counter++;
        }

        return current;
    }
}
