using System;
using System.Collections.Generic;
using DevAndrew.Calculator.Core.Interfaces;
using DevAndrew.Calculator.Core.Logic;
using DevAndrew.Calculator.Core.Models;

namespace DevAndrew.Calculator.Core.Presenters
{
    public sealed class CalculatorPresenter : IDisposable
    {
        private const string ErrorTitle = "Error";
        private const string ErrorMessage = "Проверьте введенную информацию.";

        private readonly ICalculatorView _view;
        private readonly ICalculatorErrorHandler _errorHandler;
        private readonly IStateRepository _stateRepository;

        private CalculatorState _state;
        private bool _dirty;
        private bool _isErrorDialogVisible;

        public CalculatorPresenter(
            ICalculatorView view,
            IStateRepository stateRepository,
            ICalculatorErrorHandler errorHandler)
        {
            _view = view;
            _stateRepository = stateRepository;
            _errorHandler = errorHandler;
        }

        public void Initialize()
        {
            _state = _stateRepository.Load() ?? CalculatorState.CreateDefault();
            _view.SetInputText(_state.InputExpression);
            RebuildHistoryOnView();
            _view.ResultClicked += OnResultClicked;
            _view.InputChanged += OnInputChanged;
            _dirty = false;
        }

        public void Dispose()
        {
            PersistIfNeeded();
            _view.ResultClicked -= OnResultClicked;
            _view.InputChanged -= OnInputChanged;
        }

        public void PersistIfNeeded()
        {
            if (_state == null || !_dirty)
            {
                return;
            }

            if (_stateRepository.TrySave(_state))
            {
                _dirty = false;
            }
        }

        private void OnResultClicked()
        {
            if (_isErrorDialogVisible)
            {
                return;
            }

            var expressionAtClick = _view.InputText ?? string.Empty;
            OnInputChanged(expressionAtClick);

            if (ExpressionEvaluator.TryEvaluate(expressionAtClick, out var sum))
            {
                var entry = HistoryEntry.Success(expressionAtClick, sum);
                _state.AddHistoryEntry(entry);
                _dirty = true;
                AppendHistoryOnView(entry);
                PersistIfNeeded();
                return;
            }

            var errorEntry = HistoryEntry.Error(expressionAtClick);
            _state.AddHistoryEntry(errorEntry);
            _dirty = true;
            AppendHistoryOnView(errorEntry);
            PersistIfNeeded();

            _isErrorDialogVisible = true;
            SetUiInteractable(false);

            try
            {
                _errorHandler.ShowError(ErrorTitle, ErrorMessage, () =>
                {
                    _view.SetInputText(expressionAtClick);
                    SetUiInteractable(true);
                    _isErrorDialogVisible = false;
                });
            }
            catch
            {
                SetUiInteractable(true);
                _isErrorDialogVisible = false;
                throw;
            }
        }

        private void OnInputChanged(string value)
        {
            if (_state == null)
            {
                return;
            }

            var currentInput = value ?? string.Empty;
            if (!_state.TrySetInputExpression(currentInput))
            {
                return;
            }
            _dirty = true;
        }

        private void RebuildHistoryOnView()
        {
            var historyLines = new List<string>(_state.History.Count);
            foreach (var entry in _state.History)
            {
                historyLines.Add(FormatHistoryEntry(entry));
            }

            _view.SetHistory(historyLines);
        }

        private void AppendHistoryOnView(HistoryEntry entry)
        {
            var line = FormatHistoryEntry(entry);
            _view.AppendHistoryLine(line);
        }

        private static string FormatHistoryEntry(HistoryEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            return entry.IsError
                ? $"{entry.Expression}=ERROR"
                : $"{entry.Expression} = {entry.Result}";
        }

        private void SetUiInteractable(bool isInteractable)
        {
            _view.SetInputInteractable(isInteractable);
            _view.SetResultInteractable(isInteractable);
        }
    }
}
