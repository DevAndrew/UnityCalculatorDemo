using System;
using System.Collections.Generic;

namespace DevAndrew.Calculator.Core.Models
{
    [Serializable]
    public class CalculatorState
    {
        private readonly List<HistoryEntry> _history = new List<HistoryEntry>();

        public string InputExpression { get; private set; } = string.Empty;
        public IReadOnlyList<HistoryEntry> History => _history;

        public static CalculatorState CreateDefault()
        {
            return new CalculatorState();
        }

        public bool TrySetInputExpression(string inputExpression)
        {
            var normalized = inputExpression ?? string.Empty;
            if (string.Equals(InputExpression, normalized, StringComparison.Ordinal))
            {
                return false;
            }

            InputExpression = normalized;
            return true;
        }

        public void AddHistoryEntry(HistoryEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            _history.Add(entry);
        }

        public void AddSuccessHistory(string expression, long result)
        {
            _history.Add(HistoryEntry.Success(expression, result));
        }

        public void AddErrorHistory(string expression)
        {
            _history.Add(HistoryEntry.Error(expression));
        }
    }
}
