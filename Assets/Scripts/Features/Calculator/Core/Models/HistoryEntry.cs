using System;

namespace DevAndrew.Calculator.Core.Models
{
    [Serializable]
    public class HistoryEntry
    {
        public string Expression { get; private set; }
        public bool IsError { get; private set; }
        public long Result { get; private set; }

        private HistoryEntry()
        {
            Expression = string.Empty;
            IsError = true;
            Result = 0L;
        }

        private HistoryEntry(string expression, bool isError, long result)
        {
            Expression = expression ?? string.Empty;
            IsError = isError;
            Result = result;
        }

        public static HistoryEntry Success(string expression, long result)
        {
            return new HistoryEntry(expression, false, result);
        }

        public static HistoryEntry Error(string expression)
        {
            return new HistoryEntry(expression, true, 0L);
        }

        public static HistoryEntry Restore(string expression, bool isError, long result)
        {
            if (isError)
            {
                return Error(expression);
            }

            return Success(expression, result);
        }
    }
}
