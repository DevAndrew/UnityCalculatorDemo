using System;

namespace DevAndrew.Calculator.Core.Models
{
    [Serializable]
    public class HistoryEntry
    {
        public string Expression;
        public bool IsError;
        public long Result;

        public static HistoryEntry Success(string expression, long result)
        {
            return new HistoryEntry
            {
                Expression = expression,
                IsError = false,
                Result = result
            };
        }

        public static HistoryEntry Error(string expression)
        {
            return new HistoryEntry
            {
                Expression = expression,
                IsError = true,
                Result = 0
            };
        }
    }
}
