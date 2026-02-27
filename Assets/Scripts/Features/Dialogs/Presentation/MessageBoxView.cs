using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevAndrew.Dialogs.Presentation
{
    public sealed class MessageBoxView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _okButton;

        private Action _onClosed;

        private void Awake()
        {
            if (_okButton != null)
            {
                _okButton.onClick.RemoveListener(HandleOkClicked);
                _okButton.onClick.AddListener(HandleOkClicked);
            }
        }

        private void OnDestroy()
        {
            if (_okButton != null)
            {
                _okButton.onClick.RemoveListener(HandleOkClicked);
            }
        }

        public void Show(string title, string message, Action onClosed)
        {
            _onClosed = onClosed;
            if (_titleText != null)
            {
                _titleText.text = title ?? string.Empty;
            }

            if (_messageText != null)
            {
                _messageText.text = message ?? string.Empty;
            }

            SetVisible(true);
        }

        private void HandleOkClicked()
        {
            var callback = _onClosed;
            _onClosed = null;
            SetVisible(false);
            callback?.Invoke();
        }

        public void Hide()
        {
            _onClosed = null;
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
                return;
            }

            gameObject.SetActive(visible);
        }
    }
}
