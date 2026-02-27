using System;
using System.Collections.Generic;

namespace DevAndrew.Calculator.Core.Models
{
    [Serializable]
    public class CalculatorState
    {
        public string InputExpression;
        public List<HistoryEntry> History;

        public static CalculatorState CreateDefault()
        {
            return new CalculatorState
            {
                InputExpression = string.Empty,
                History = new List<HistoryEntry>()
            };
        }
    }
}
