using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public static class SecretUtil
{
    private static readonly Regex SecretPatternSecrets =
        new("secrets\\.(?<secret>([a-zA-Z0-9]+[\\/._-])*[a-zA-Z0-9]+)", RegexOptions.Compiled);

    private static readonly Regex SecretPatternParentheses =
        new("\\{\\{\\s*secrets\\.(?<secret>\\S+?\\s*)}}", RegexOptions.Compiled);
    
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
             match =>  ResolveSecretValueAsync(secretReplacer, match));
    }   

    private static async Task<string> ReplaceSecretsWithoutParenthesesAsync(string input,
        Func<string, Task<string>> secretReplacer)
    {
        return await ReplaceTokensAsync(input, SecretPatternSecrets,
             match =>  ResolveSecretValueAsync(secretReplacer, match));
    }

    private static async Task<string> ResolveSecretValueAsync(Func<string, Task<string>> secretReplacer, Match match)
    {
        var secretName = match.Groups["secret"].Value.Trim();
        if (!string.IsNullOrWhiteSpace(secretName))
        {
            var result = await secretReplacer(secretName);
            return result ?? match.Value;
        }
        return null;
    }
    
    private static async Task<string> ReplaceTokensAsync(string input, Regex pattern,
    Func<Match, Task<string>> converter)
    {
        var matches = pattern.Matches(input);
        if (matches.Count == 0)
        {
            return input;
        }

        var sb = new System.Text.StringBuilder();
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            sb.Append(input, lastIndex, match.Index - lastIndex);
            var replacedValue = await converter(match);
            sb.Append(replacedValue);
            lastIndex = match.Index + match.Length;
        }

        sb.Append(input, lastIndex, input.Length - lastIndex);
        return sb.ToString();
    }
}