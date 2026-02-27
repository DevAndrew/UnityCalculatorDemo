using System;

namespace DevAndrew.Dialogs.Contracts
{
    public interface IDialogService
    {
        void Show(string title, string message, Action onClosed);

        void Hide();
    }
}
