using DevAndrew.Calculator.Core.Presenters;
using DevAndrew.Calculator.Core.Interfaces;
using DevAndrew.Calculator.Infrastructure;
using DevAndrew.Calculator.Presentation;
using DevAndrew.Dialogs.Presentation;
using DevAndrew.SaveLoad.Infrastructure;
using UnityEngine;

namespace DevAndrew.Calculator.App
{
    public sealed class AppBootstrapper : MonoBehaviour
    {
        [SerializeField] private CalculatorInputView _calculatorInputView;
        [SerializeField] private CalculatorHistoryView _calculatorHistoryView;
        [SerializeField] private MessageBoxView _messageBoxView;

        private CalculatorPresenter _presenter;
        private ICalculatorView _calculatorView;
        private bool _isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
            _presenter = null;
            _calculatorView = null;
            _isInitialized = false;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _presenter?.PersistIfNeeded();
            }
        }

        private void OnApplicationQuit()
        {
            _presenter?.PersistIfNeeded();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_calculatorInputView == null)
            {
                Debug.LogError("AppBootstrapper: Calculator input view reference is missing.");
                return;
            }

            if (_calculatorHistoryView == null)
            {
                Debug.LogError("AppBootstrapper: Calculator history view reference is missing.");
                return;
            }

            var saveLoadService = new JsonSaveLoadService();
            var stateRepository = new FileStateRepository(saveLoadService);
            var messageBoxService = new MessageBoxService(_messageBoxView);
            var errorHandler = new DialogErrorHandler(messageBoxService);
            _calculatorView = new CalculatorScreenView(_calculatorInputView, _calculatorHistoryView);

            _presenter = new CalculatorPresenter(
                _calculatorView,
                stateRepository,
                errorHandler);

            _presenter.Initialize();
            _isInitialized = true;
        }
    }
}
