using System;
using DevAndrew.Dialogs.Contracts;
using UnityEngine;

namespace DevAndrew.Dialogs.Presentation
{
    public sealed class MessageBoxService : IDialogService
    {
        private readonly MessageBoxView _view;

        public MessageBoxService(MessageBoxView view)
        {
            _view = view;
        }

        public void Show(string title, string message, Action onClosed)
        {
            if (_view == null)
            {
                Debug.LogError("MessageBoxService: MessageBoxView reference is missing.");
                onClosed?.Invoke();
                return;
            }

            _view.Show(title, message, onClosed);
        }

        public void Hide()
        {
            if (_view == null)
            {
                return;
            }

            _view.Hide();
        }
    }
}
