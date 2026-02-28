using System;
using System.Collections.Generic;
using DevAndrew.Calculator.Core.Interfaces;

namespace DevAndrew.Calculator.Presentation
{
    public sealed class CalculatorScreenView : ICalculatorView
    {
        private readonly CalculatorInputView _inputView;
        private readonly CalculatorHistoryView _historyView;

        public CalculatorScreenView(CalculatorInputView inputView, CalculatorHistoryView historyView)
        {
            _inputView = inputView ?? throw new ArgumentNullException(nameof(inputView));
            _historyView = historyView ?? throw new ArgumentNullException(nameof(historyView));
        }

        public event Action ResultClicked
        {
            add => _inputView.ResultClicked += value;
            remove => _inputView.ResultClicked -= value;
        }

        public event Action<string> InputChanged
        {
            add => _inputView.InputChanged += value;
            remove => _inputView.InputChanged -= value;
        }

        public string InputText => _inputView.InputText;

        public void SetInputText(string value)
        {
            _inputView.SetInputText(value);
        }

        public void SetHistory(IReadOnlyList<string> lines)
        {
            _historyView.SetHistory(lines);
        }

        public void AppendHistoryLine(string line)
        {
            _historyView.AppendHistoryLine(line);
        }

        public void SetResultInteractable(bool isInteractable)
        {
            _inputView.SetResultInteractable(isInteractable);
        }

        public void SetInputInteractable(bool isInteractable)
        {
            _inputView.SetInputInteractable(isInteractable);
        }
    }
}
