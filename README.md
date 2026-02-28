# Unity Calculator Demo

## Environment

- Unity Editor: `2022.3.62f3`

## Runbook

- Main scene: `Assets/Scenes/CalculatorMain.unity`

## Что реализовано

- Поддерживается ввод только целых неотрицательных чисел и знака `+`.
- Корректные выражения вычисляются и добавляются в историю (пример: `54+21 = 75`).
- Некорректные выражения добавляются в историю как ошибка (`<выражение>=ERROR`), а пользователю показывается диалог с просьбой проверить ввод.
- После закрытия диалога в поле ввода восстанавливается последнее введенное выражение.
- Введенное выражение и история сохраняются между сессиями приложения.
- Опционально можно очищать поле ввода после успешного `Result`: флаг `Сlear Input On Success` в `AppBootstrapper` (Inspector), по умолчанию выключен.

## Соответствие ТЗ (кратко)

- **Clean Architecture + MVP**: доменная логика отделена от UI, экран работает через Presenter.
- **Модульность через assembly**: проект разбит на отдельные `asmdef` (Core, Infrastructure, Presentation, Dialogs, SaveLoad и тесты).
- **Сценарий использования**: поведение экрана и ошибки соответствует требованиям и примерам из приложения А.

## Архитектура

### Core

- `ExpressionEvaluator` — валидация и вычисление выражения.
- `CalculatorPresenter` — сценарии экрана, работа с ошибками, сохранение состояния.
- Модели состояния и истории (`CalculatorState`, `HistoryEntry`) и контракты (`ICalculatorView`, `IStateRepository`, `ICalculatorErrorHandler`).

### Infrastructure

- `FileStateRepository` — загрузка/сохранение состояния калькулятора.
- `JsonSaveLoadService` — JSON-сохранение в файл с резервной копией (`.bak`) и временным файлом (`.tmp`) для безопасной записи.

### Presentation

- `CalculatorInputView` — Unity UI слой ввода (input + кнопка Result).
- `CalculatorHistoryView` — Unity UI слой истории (виртуализированный список).
- `CalculatorScreenView` — объединяет input/history view в единый `ICalculatorView` для Presenter.
- `MessageBoxView` + `MessageBoxService` — отображение диалога ошибки.
- `AppBootstrapper` — композиция зависимостей и запуск приложения.

## Сохранение состояния

Сохраняется:

- текущее введенное выражение;
- история вычислений.

Сохранение выполняется при изменениях состояния, а также при паузе/закрытии приложения.  
При проблемах чтения основного файла используется резервная копия.

## Тесты

Добавлены Unit-тесты (EditMode):

- `ExpressionEvaluatorTests` — валидные/невалидные выражения, переполнение.
- `CalculatorPresenterTests` — успешный результат, ошибка, блокировка UI во время диалога, сохранение ввода.
- `FileStateRepositoryTests` — round-trip сохранения, восстановление из backup, поведение при поврежденных файлах.

Запуск:

- через Unity Test Runner в режиме EditMode.

## Virtualized history

Для истории добавлен переиспользуемый вертикальный скроллер: `Assets/Scripts/Core/UI/VirtualizedList/VerticalScroller.cs`
