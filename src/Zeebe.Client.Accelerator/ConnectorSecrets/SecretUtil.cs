using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public static class SecretUtil
{
    private static readonly Regex SecretPatternSecrets =
        new(@"secrets\.(?<secret>([a-zA-Z0-9]+[\/._-])*[a-zA-Z0-9]+)", RegexOptions.Compiled);

    private static readonly Regex SecretPatternParentheses =
        new(@"\{\{\s*secrets\.(?<secret>\S+?)\s*\}\}", RegexOptions.Compiled);

    public static async Task<string> ReplaceSecretsAsync(string input, Func<string, Task<string>> secretReplacer)
    {
        if (input == null)
        {
            throw new InvalidOperationException("Input cannot be null.");
        }

        input = await ReplaceSecretsWithParenthesesAsync(input, secretReplacer);
        input = await ReplaceSecretsWithoutParenthesesAsync(input, secretReplacer);
        return input;
    }

    private static async Task<string> ReplaceSecretsWithParenthesesAsync(string input,
        Func<string, Task<string>> secretReplacer)
    {
        return await ReplaceTokensAsync(input, SecretPatternParentheses,
            async match => await ResolveSecretValueAsync(secretReplacer, match));
    }

    private static async Task<string> ReplaceSecretsWithoutParenthesesAsync(string input,
        Func<string, Task<string>> secretReplacer)
    {
        return await ReplaceTokensAsync(input, SecretPatternSecrets,
            async match => await ResolveSecretValueAsync(secretReplacer, match));
    }

    private static async Task<string> ResolveSecretValueAsync(Func<string, Task<string>> secretReplacer, Match match)
    {
        var secretName = match.Groups["secret"].Value.Trim();
        if (!string.IsNullOrWhiteSpace(secretName))
        {
            var result = await secretReplacer(secretName);
            return result ?? match.Value;
        }

        return match.Value;
    }

    private static async Task<string> ReplaceTokensAsync(string original, Regex pattern,
        Func<Match, Task<string>> converter)
    {
        var matches = pattern.Matches(original).Cast<Match>().ToList();
        var result = original;

        // Process matches in reverse order to maintain string positions
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var replacement = await converter(match);
            if (replacement != null)
            {
                result = result.Substring(0, match.Index) + replacement + result.Substring(match.Index + match.Length);
            }
        }

        return result;
    }
}