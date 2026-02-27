using DevAndrew.Calculator.Core.Presenters;
using DevAndrew.Calculator.Infrastructure;
using DevAndrew.Calculator.Presentation;
using DevAndrew.Dialogs.Presentation;
using DevAndrew.SaveLoad.Infrastructure;
using UnityEngine;

namespace CalculatorDemoTask.App
{
    public sealed class AppBootstrapper : MonoBehaviour
    {
        [SerializeField] private CalculatorScreenView _calculatorScreenView;
        [SerializeField] private MessageBoxView _messageBoxView;

        private CalculatorPresenter _presenter;
        private bool _isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
            _presenter = null;
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

            if (_calculatorScreenView == null)
            {
                Debug.LogError("AppBootstrapper: CalculatorScreenView reference is missing.");
                return;
            }

            var saveLoadService = new JsonSaveLoadService();
            var stateRepository = new FileStateRepository(saveLoadService);
            var messageBoxService = new MessageBoxService(_messageBoxView);
            var errorHandler = new DialogErrorHandler(messageBoxService);

            _presenter = new CalculatorPresenter(
                _calculatorScreenView,
                stateRepository,
                errorHandler);

            _presenter.Initialize();
            _isInitialized = true;
        }
    }
}
