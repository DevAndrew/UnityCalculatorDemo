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
        private bool _isConfigured;

        public event Action ResultClicked;
        public event Action<string> InputChanged;

        public string InputText => _isConfigured ? _inputField.text : string.Empty;

        private void Awake()
        {
            if (_inputField == null || _resultButton == null)
            {
                Debug.LogError("CalculatorInputView: assign InputField and ResultButton in Inspector.");
                enabled = false;
                return;
            }

            _resultButton.onClick.AddListener(HandleResultClick);
            _inputField.onValueChanged.AddListener(HandleInputChanged);
            _isConfigured = true;
        }

        private void OnDestroy()
        {
            if (!_isConfigured)
            {
                return;
            }

            _resultButton.onClick.RemoveListener(HandleResultClick);
            _inputField.onValueChanged.RemoveListener(HandleInputChanged);
        }

        public void SetInputText(string value)
        {
            if (!_isConfigured)
            {
                return;
            }

            _inputField.text = value ?? string.Empty;
        }

        public void SetResultInteractable(bool isInteractable)
        {
            if (!_isConfigured)
            {
                return;
            }

            _resultButton.interactable = isInteractable;
        }

        public void SetInputInteractable(bool isInteractable)
        {
            if (!_isConfigured)
            {
                return;
            }

            _inputField.interactable = isInteractable;
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
