using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Common.Models
{
    public class RegExProperty
    {
        private readonly static string PossibleSymbols = "!@#$%^&*()-+=~`[\\]{}\\\\|;:'\"\",<.>?";
        public int? PartnerId { get; set; }
        public bool Numeric { get; set; }
        public bool Lowercase { get; set; }
        public bool Uppercase { get; set; }
        public bool Symbol { get; set; }
        public bool IsDigitRequired { get; set; }
        public bool IsLowercaseRequired { get; set; }
        public bool IsUppercaseRequired { get; set; }
        public bool IsSymbolRequired { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public RegExProperty() { }
        public RegExProperty(string regEx)
        {
            if (regEx.Contains("(?=.*[0-9])"))
                IsDigitRequired = true;
            if (regEx.Contains("(?=.*[a-z])"))
                IsLowercaseRequired = true;
            if (regEx.Contains("(?=.*[A-Z])"))
                IsUppercaseRequired = true;
            if (regEx.Contains("(?=.*[" + PossibleSymbols + "])"))
                IsSymbolRequired = true;
            regEx= regEx.Replace("(?=.*[0-9])", string.Empty).Replace("(?=.*[a-z])", string.Empty)
                        .Replace("(?=.*[A-Z])", string.Empty).Replace("(?=.*["+ PossibleSymbols +"])", string.Empty);
            if (regEx.Contains("a-z") || IsLowercaseRequired)
                Lowercase = true;
            if (regEx.Contains("A-Z") || IsUppercaseRequired)
                Uppercase = true;
            if (regEx.Contains("0-9") || IsDigitRequired)
                Numeric = true;
            if (!regEx.Contains("(?!.*[" + PossibleSymbols + "])"))
                Symbol = true;
            var lengthRegEx = new Regex(@"\(\?=\^\.{(.*),(.*)}\$\)");
            MinLength = Convert.ToInt32(lengthRegEx.Matches(regEx)[0].Groups[1].Value);
            MaxLength = Convert.ToInt32(lengthRegEx.Matches(regEx)[0].Groups[2].Value);
        }
        public string GetExpression()
        {
            var expression = new StringBuilder("(?=^.{" + MinLength + "," + MaxLength + "}$)");
            if (IsLowercaseRequired)
                expression.Append("(?=.*[a-z])");
            if (IsUppercaseRequired)
                expression.Append("(?=.*[A-Z])");
            if (IsDigitRequired)
                expression.Append("(?=.*[0-9])");
            if (IsSymbolRequired)
                expression.Append("(?=.*[" + PossibleSymbols + "])");
            else if (!Symbol)
                expression.Append("(?!.*[" + PossibleSymbols + "])");
            if ((Lowercase && !IsLowercaseRequired) || (Uppercase && !IsUppercaseRequired) || (Numeric && !IsDigitRequired))
            {
                expression.Append('[');
                if (Lowercase)
                    expression.Append("a-z");
                if (Uppercase)
                    expression.Append("A-Z");
                if (Numeric)
                    expression.Append("0-9");
                if (Symbol && !IsSymbolRequired)
                    expression.Append(PossibleSymbols);
                expression.Append(']');
            }           
            return expression.ToString();
        }

        public static string StringBasedOnRegEx(string pattern)
        {
            var lowercases = "abcdefghijkmnopqrstuvwxyz";
            var uppercases = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            var digits = "0123456789";
            var possibleCharacters = string.Empty;
            if (pattern.Contains("a-z"))
                possibleCharacters = lowercases;
            if (pattern.Contains("A-Z"))
                possibleCharacters += uppercases;
            if (pattern.Contains("0-9"))
                possibleCharacters += digits;
            if (!pattern.Contains("(?!.*[" + PossibleSymbols + "])"))
                possibleCharacters += PossibleSymbols;
            var lengthRegex = new Regex(@"\{(\d+),(\d+)\}");
            Match match = lengthRegex.Match(pattern);
            var minLength = 0;
            var maxLength = 0;
            if (match.Success && match.Groups.Count == 3)
            {
                minLength = int.Parse(match.Groups[1].Value);
                maxLength = int.Parse(match.Groups[2].Value);
            }
            var r = new Random();
            var resultLen = r.Next(minLength, maxLength);
            var random = new Random(Guid.NewGuid().GetHashCode());
            var result = new string(
                Enumerable.Repeat(possibleCharacters, resultLen)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            while (!Regex.IsMatch(result, pattern))
            {
                result = new string(
                Enumerable.Repeat(possibleCharacters, resultLen)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            }
            return result;
        }
    }
}