using System;
using System.Collections.Generic;
using System.Text;
using DevAndrew.Calculator.Core.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevAndrew.Calculator.Presentation
{
    public sealed class CalculatorScreenView : MonoBehaviour, ICalculatorView
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _resultButton;
        [SerializeField] private TMP_Text _historyText;
        [SerializeField] private ScrollRect _historyScrollRect;

        public event Action ResultClicked;
        public event Action<string> InputChanged;

        public string InputText => _inputField == null ? string.Empty : _inputField.text;

        private void Awake()
        {
            if (_resultButton != null)
            {
                _resultButton.onClick.AddListener(HandleResultClick);
            }

            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(HandleInputChanged);
            }
        }

        private void Start()
        {
            RefreshHistoryLayoutAndScrollToBottom();
        }

        private void OnDestroy()
        {
            if (_resultButton != null)
            {
                _resultButton.onClick.RemoveListener(HandleResultClick);
            }

            if (_inputField != null)
            {
                _inputField.onValueChanged.RemoveListener(HandleInputChanged);
            }
        }

        public void SetInputText(string value)
        {
            if (_inputField == null)
            {
                return;
            }

            _inputField.text = value ?? string.Empty;
        }

        public void SetHistory(IReadOnlyList<string> lines)
        {
            if (_historyText == null)
            {
                return;
            }

            if (lines == null || lines.Count == 0)
            {
                _historyText.text = string.Empty;
            }
            else
            {
                var builder = new StringBuilder();
                for (var i = 0; i < lines.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append('\n');
                    }

                    builder.Append(lines[i]);
                }

                _historyText.text = builder.ToString();
            }

            RefreshHistoryLayoutAndScrollToBottom();
        }

        public void SetResultInteractable(bool isInteractable)
        {
            if (_resultButton != null)
            {
                _resultButton.interactable = isInteractable;
            }
        }

        public void SetInputInteractable(bool isInteractable)
        {
            if (_inputField != null)
            {
                _inputField.interactable = isInteractable;
            }
        }

        private void HandleResultClick()
        {
            ResultClicked?.Invoke();
        }

        private void HandleInputChanged(string value)
        {
            InputChanged?.Invoke(value ?? string.Empty);
        }

        private void RefreshHistoryLayoutAndScrollToBottom()
        {
            if (_historyText == null || _historyScrollRect == null)
            {
                return;
            }

            _historyText.ForceMeshUpdate();
            ResizeHistoryContentToFitText();

            if (_historyScrollRect.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_historyScrollRect.content);
            }

            _historyScrollRect.verticalNormalizedPosition = 0f;
        }

        private void ResizeHistoryContentToFitText()
        {
            if (_historyScrollRect.content == null)
            {
                return;
            }

            var contentRect = _historyScrollRect.content;
            var viewportHeight = _historyScrollRect.viewport == null ? 0f : _historyScrollRect.viewport.rect.height;
            var requiredHeight = Mathf.Max(viewportHeight, _historyText.preferredHeight + 24f);

            var sizeDelta = contentRect.sizeDelta;
            if (Mathf.Approximately(sizeDelta.y, requiredHeight))
            {
                return;
            }

            sizeDelta.y = requiredHeight;
            contentRect.sizeDelta = sizeDelta;
        }
    }
}
