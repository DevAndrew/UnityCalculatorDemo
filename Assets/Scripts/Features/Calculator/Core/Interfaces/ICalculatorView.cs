using System;
using System.Collections.Generic;

namespace DevAndrew.Calculator.Core.Interfaces
{
    public interface ICalculatorView
    {
        event Action ResultClicked;
        event Action<string> InputChanged;

        string InputText { get; }

        void SetInputText(string value);

        void SetHistory(IReadOnlyList<string> lines);

        void SetResultInteractable(bool isInteractable);

        void SetInputInteractable(bool isInteractable);
    }
}
