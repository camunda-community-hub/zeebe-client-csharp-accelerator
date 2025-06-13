using System;
using System.Collections.Generic;
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

        public static string ReplaceSecrets(string input, Func<string, string> secretReplacer)
        {
            if (input == null)
            {
                throw new InvalidOperationException("Input cannot be null.");
            }

            input = ReplaceSecretsWithParentheses(input, secretReplacer);
            input = ReplaceSecretsWithoutParentheses(input, secretReplacer);
            return input;
        }

        private static async Task<string> ReplaceSecretsWithParenthesesAsync(string input, Func<string, Task<string>> secretReplacer)
        {
            return await ReplaceTokensAsync(input, SecretPatternParentheses, 
                async match => await ResolveSecretValueAsync(secretReplacer, match));
        }

        private static async Task<string> ReplaceSecretsWithoutParenthesesAsync(string input, Func<string, Task<string>> secretReplacer)
        {
            return await ReplaceTokensAsync(input, SecretPatternSecrets, 
                async match => await ResolveSecretValueAsync(secretReplacer, match));
        }

        private static string ReplaceSecretsWithParentheses(string input, Func<string, string> secretReplacer)
        {
            return ReplaceTokens(input, SecretPatternParentheses, match => ResolveSecretValue(secretReplacer, match));
        }

        private static string ReplaceSecretsWithoutParentheses(string input, Func<string, string> secretReplacer)
        {
            return ReplaceTokens(input, SecretPatternSecrets, match => ResolveSecretValue(secretReplacer, match));
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

        private static string ResolveSecretValue(Func<string, string> secretReplacer, Match match)
        {
            var secretName = match.Groups["secret"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(secretName))
            {
                var result = secretReplacer(secretName);
                return result ?? match.Value;
            }
            return match.Value;
        }

        public static async Task<string> ReplaceTokensAsync(string original, Regex pattern, Func<Match, Task<string>> converter)
        {
            var matches = pattern.Matches(original).Cast<Match>().ToList();
            var result = original;
            
            // Process matches in reverse order to maintain string positions
            for (int i = matches.Count - 1; i >= 0; i--)
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

        public static string ReplaceTokens(string original, Regex pattern, Func<Match, string> converter)
        {
            return pattern.Replace(original, match =>
            {
                var replacement = converter(match);
                return replacement ?? match.Value;
            });
        }

        public static List<string> RetrieveSecretKeysInInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            var patterns = new[] { SecretPatternParentheses, SecretPatternSecrets };
            var secretKeys = new HashSet<string>();

            foreach (var pattern in patterns)
            {
                var matches = pattern.Matches(input);
                foreach (Match match in matches)
                {
                    var secretKey = match.Groups["secret"].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(secretKey))
                    {
                        secretKeys.Add(secretKey);
                    }
                }
            }

            return secretKeys.ToList();
        }
    }