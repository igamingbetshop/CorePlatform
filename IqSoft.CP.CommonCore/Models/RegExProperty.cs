using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Common.Models
{
    public class RegExProperty
    {
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
            if (regEx.Contains("(?=.*[!@#$%^&*./'\":`;()])"))
                IsSymbolRequired = true;
            regEx= regEx.Replace("(?=.*[0-9])", string.Empty).Replace("(?=.*[a-z])", string.Empty)
                        .Replace("(?=.*[A-Z])", string.Empty).Replace("(?=.*[!@#$%^&*./'\":`;()])", string.Empty);
            if (regEx.Contains("a-z"))
                Lowercase = true;
            if (regEx.Contains("A-Z"))
                Uppercase = true;
            if (regEx.Contains("0-9"))
                Numeric = true;
            if (!regEx.Contains("(?!.*[!@#$%^&*./'\":`;()])"))
                Symbol = true;
            var lenghtRegEx = new Regex(@"\(\?=\^\.{(.*),(.*)}\$\)");
            MinLength = Convert.ToInt32(lenghtRegEx.Matches(regEx)[0].Groups[1].Value);
            MaxLength =Convert.ToInt32(lenghtRegEx.Matches(regEx)[0].Groups[2].Value);
        }
        public string GetExpression()
        {
            var expression = new StringBuilder("(?=^.{" + MinLength + "," + MaxLength + "}$)");
            if ((Lowercase && !IsLowercaseRequired) || (Uppercase && !IsUppercaseRequired) || (Numeric && !IsDigitRequired))
            {
                expression.Append('[');
                if (Lowercase && !IsLowercaseRequired)
                    expression.Append("a-z");
                if (Uppercase && !IsUppercaseRequired)
                    expression.Append("A-Z");
                if (Numeric && !IsDigitRequired)
                    expression.Append("0-9");
                expression.Append(']');
            }
            if (IsLowercaseRequired)
                expression.Append("(?=.*[a-z])");
            if (IsUppercaseRequired)
                expression.Append("(?=.*[A-Z])");
            if (IsDigitRequired)
                expression.Append("(?=.*[0-9])");
            if (!Symbol)
                expression.Append("(?!.*[!@#$%^&*./'\":`;()])");
            else if (IsSymbolRequired)
                expression.Append("(?=.*[!@#$%^&*./'\":`;()])");
            return expression.ToString();
        }

        public static string StringBasedOnRegEx(string pattern)
        {
            var lowercases = "abcdefghijkmnopqrstuvwxyz";
            var uppercases = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            var digits = "0123456789";
            var symbols = "!@#$%^&*()";
            var possibleCharacters = string.Empty;
            if (pattern.Contains("a-z"))
                possibleCharacters = lowercases;
            if (pattern.Contains("A-Z"))
                possibleCharacters += uppercases;
            if (pattern.Contains("0-9"))
                possibleCharacters += digits;
            if (!pattern.Contains("(?!.*[!@#$%^&*./'\":`;()])"))
                possibleCharacters += symbols;
            var commaInd = pattern.IndexOf(",");
            var minLength = Convert.ToInt32(pattern.Substring(6, commaInd - 6));
            var maxLenght = Convert.ToInt32(pattern.Substring(commaInd + 1, pattern.IndexOf("}$)") - commaInd - 1));
            var r = new Random();
            var resultLen = r.Next(minLength, maxLenght);
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