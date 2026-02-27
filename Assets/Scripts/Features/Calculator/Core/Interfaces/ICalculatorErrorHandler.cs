using System;

namespace DevAndrew.Calculator.Core.Interfaces
{
    public interface ICalculatorErrorHandler
    {
        void ShowError(string title, string message, Action onClosed);
    }
}
