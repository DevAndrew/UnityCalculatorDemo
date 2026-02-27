using System;
using System.Text.RegularExpressions;

namespace DevAndrew.Calculator.Core.Logic
{
    public static class ExpressionEvaluator
    {
        private static readonly Regex ValidExpressionPattern = new Regex(
            "^[0-9]+(\\+[0-9]+)*$",
            RegexOptions.Compiled);

        public static bool TryEvaluate(string expression, out long sum)
        {
            sum = 0L;

            if (!IsValidExpression(expression))
            {
                return false;
            }

            return TryComputeSum(expression, out sum);
        }

        private static bool IsValidExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            return ValidExpressionPattern.IsMatch(expression);
        }

        private static bool TryComputeSum(string expression, out long sum)
        {
            sum = 0L;
            long currentNumber = 0L;
            var hasDigitInCurrentToken = false;

            for (var i = 0; i < expression.Length; i++)
            {
                var c = expression[i];

                if (c >= '0' && c <= '9')
                {
                    hasDigitInCurrentToken = true;
                    var digit = c - '0';

                    if (!TryAppendDigit(currentNumber, digit, out currentNumber))
                    {
                        return false;
                    }

                    continue;
                }

                if (c == '+')
                {
                    if (!hasDigitInCurrentToken)
                    {
                        return false;
                    }

                    if (!TryAdd(sum, currentNumber, out sum))
                    {
                        return false;
                    }

                    currentNumber = 0L;
                    hasDigitInCurrentToken = false;
                    continue;
                }

                return false;
            }

            if (!hasDigitInCurrentToken)
            {
                return false;
            }

            return TryAdd(sum, currentNumber, out sum);
        }

        private static bool TryAppendDigit(long currentNumber, int digit, out long updatedNumber)
        {
            try
            {
                checked
                {
                    updatedNumber = (currentNumber * 10L) + digit;
                }

                return true;
            }
            catch (OverflowException)
            {
                updatedNumber = 0L;
                return false;
            }
        }

        private static bool TryAdd(long left, long right, out long result)
        {
            try
            {
                checked
                {
                    result = left + right;
                }

                return true;
            }
            catch (OverflowException)
            {
                result = 0L;
                return false;
            }
        }
    }
}
