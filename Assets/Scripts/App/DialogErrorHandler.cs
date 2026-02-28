using System;
using DevAndrew.Calculator.Core.Interfaces;
using DevAndrew.Dialogs.Contracts;

namespace DevAndrew.Calculator.App
{
    public sealed class DialogErrorHandler : ICalculatorErrorHandler
    {
        private readonly IDialogService _dialogService;

        public DialogErrorHandler(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void ShowError(string title, string message, Action onClosed)
        {
            _dialogService.Show(title, message, onClosed);
        }
    }
}
