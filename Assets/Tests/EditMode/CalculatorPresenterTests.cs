using System;
using System.Collections.Generic;
using DevAndrew.Calculator.Core.Interfaces;
using DevAndrew.Calculator.Core.Models;
using DevAndrew.Calculator.Core.Presenters;
using NUnit.Framework;

public class CalculatorPresenterTests
{
    [Test]
    public void ResultClicked_AddsSuccessLine_ForValidExpression()
    {
        var view = new FakeView();
        var repository = new InMemoryRepository();
        var errorHandler = new FakeErrorHandler();
        var presenter = new CalculatorPresenter(view, repository, errorHandler);

        presenter.Initialize();
        view.SetInputText("54+21");
        view.EmitResultClick();

        Assert.AreEqual("54+21 = 75", view.HistoryLines[0]);
        Assert.AreEqual(1, repository.LastSavedState.History.Count);
        Assert.IsFalse(errorHandler.WasShown);
    }

    [Test]
    public void ResultClicked_ShowsErrorAndRestoresInput_ForInvalidExpression()
    {
        var view = new FakeView();
        var repository = new InMemoryRepository();
        var errorHandler = new FakeErrorHandler();
        var presenter = new CalculatorPresenter(view, repository, errorHandler);

        presenter.Initialize();
        view.SetInputText("1++2");
        view.EmitResultClick();

        Assert.AreEqual("1++2=ERROR", view.HistoryLines[0]);
        Assert.IsTrue(errorHandler.WasShown);
        Assert.AreEqual("1++2", view.LastSetInputText);
        Assert.IsTrue(view.ResultInteractableState);
    }

    [Test]
    public void ResultClicked_DisablesUi_WhileErrorDialogIsOpen()
    {
        var view = new FakeView();
        var repository = new InMemoryRepository();
        var errorHandler = new FakeErrorHandler { AutoClose = false };
        var presenter = new CalculatorPresenter(view, repository, errorHandler);

        presenter.Initialize();
        view.SetInputText("5/5");
        view.EmitResultClick();

        Assert.IsFalse(view.InputInteractableState);
        Assert.IsFalse(view.ResultInteractableState);
        Assert.IsTrue(errorHandler.WasShown);

        errorHandler.Close();
        Assert.IsTrue(view.InputInteractableState);
        Assert.IsTrue(view.ResultInteractableState);
    }

    [Test]
    public void PersistIfNeeded_SavesCurrentInput_WithoutResultClick()
    {
        var view = new FakeView();
        var repository = new InMemoryRepository();
        var errorHandler = new FakeErrorHandler();
        var presenter = new CalculatorPresenter(view, repository, errorHandler);

        presenter.Initialize();
        view.SetInputText("34+47");
        presenter.PersistIfNeeded();

        Assert.NotNull(repository.LastSavedState);
        Assert.AreEqual("34+47", repository.LastSavedState.InputExpression);
    }

    [Test]
    public void PersistIfNeeded_KeepsLatestInput_WhenViewInputBecomesUnavailable()
    {
        var view = new FakeView();
        var repository = new InMemoryRepository();
        var errorHandler = new FakeErrorHandler();
        var presenter = new CalculatorPresenter(view, repository, errorHandler);

        presenter.Initialize();
        view.SetInputText("5+5");
        view.SimulateInputUnavailable();
        presenter.PersistIfNeeded();

        Assert.NotNull(repository.LastSavedState);
        Assert.AreEqual("5+5", repository.LastSavedState.InputExpression);
    }

    private sealed class FakeView : ICalculatorView
    {
        public event Action ResultClicked;
        public event Action<string> InputChanged;

        public string InputText => IsInputUnavailable ? string.Empty : InputTextValue;

        public string InputTextValue { get; set; }
        public bool IsInputUnavailable { get; private set; }

        public List<string> HistoryLines { get; } = new List<string>();

        public string LastSetInputText { get; private set; } = string.Empty;

        public bool ResultInteractableState { get; private set; } = true;
        public bool InputInteractableState { get; private set; } = true;

        public void SetInputText(string value)
        {
            LastSetInputText = value ?? string.Empty;
            InputTextValue = LastSetInputText;
            InputChanged?.Invoke(InputTextValue);
        }

        public void SetHistory(IReadOnlyList<string> lines)
        {
            HistoryLines.Clear();
            if (lines == null)
            {
                return;
            }

            HistoryLines.AddRange(lines);
        }

        public void SetResultInteractable(bool isInteractable)
        {
            ResultInteractableState = isInteractable;
        }

        public void SetInputInteractable(bool isInteractable)
        {
            InputInteractableState = isInteractable;
        }

        public void EmitResultClick()
        {
            ResultClicked?.Invoke();
        }

        public void SimulateInputUnavailable()
        {
            IsInputUnavailable = true;
        }
    }

    private sealed class FakeErrorHandler : ICalculatorErrorHandler
    {
        public bool WasShown { get; private set; }
        public bool AutoClose { get; set; } = true;
        private Action _onClosed;

        public void ShowError(string title, string message, Action onClosed)
        {
            WasShown = true;
            _onClosed = onClosed;
            if (AutoClose)
            {
                _onClosed?.Invoke();
                _onClosed = null;
            }
        }

        public void Close()
        {
            var callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }
    }

    private sealed class InMemoryRepository : IStateRepository
    {
        public CalculatorState LastSavedState { get; private set; }

        public CalculatorState Load()
        {
            return CalculatorState.CreateDefault();
        }

        public bool TrySave(CalculatorState state)
        {
            LastSavedState = state;
            return true;
        }
    }
}
