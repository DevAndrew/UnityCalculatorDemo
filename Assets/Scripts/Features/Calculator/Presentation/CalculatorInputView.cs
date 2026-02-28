using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevAndrew.Calculator.Presentation
{
    public class CalculatorInputView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _resultButton;

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
    }
}
