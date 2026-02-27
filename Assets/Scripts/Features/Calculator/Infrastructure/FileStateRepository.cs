using System;
using System.Collections.Generic;
using DevAndrew.SaveLoad.Contracts;
using DevAndrew.Calculator.Core.Interfaces;
using DevAndrew.Calculator.Core.Models;

namespace DevAndrew.Calculator.Infrastructure
{
    public sealed class FileStateRepository : IStateRepository
    {
        private const string FileName = "CalculatorState.json";

        private readonly ISaveLoadService _saveLoadService;

        public FileStateRepository(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
        }

        public CalculatorState Load()
        {
            if (_saveLoadService.TryLoad(FileName, out SaveStateDto dto))
            {
                return dto.ToDomain();
            }

            return CalculatorState.CreateDefault();
        }

        public bool TrySave(CalculatorState state)
        {
            if (state == null)
            {
                return false;
            }

            var dto = SaveStateDto.FromDomain(state);
            return _saveLoadService.TrySave(FileName, dto);
        }

        [Serializable]
        private class SaveStateDto
        {
            public int version = 1;
            public string inputExpression = string.Empty;
            public List<HistoryEntryDto> history = new List<HistoryEntryDto>();

            public static SaveStateDto FromDomain(CalculatorState state)
            {
                var dto = new SaveStateDto
                {
                    inputExpression = state.InputExpression,
                    history = new List<HistoryEntryDto>()
                };

                foreach (var item in state.History)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    dto.history.Add(new HistoryEntryDto
                    {
                        expression = item.Expression,
                        isError = item.IsError,
                        result = item.Result
                    });
                }

                return dto;
            }

            public CalculatorState ToDomain()
            {
                var state = CalculatorState.CreateDefault();
                state.TrySetInputExpression(inputExpression);

                if (history != null)
                {
                    foreach (var item in history)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        state.AddHistoryEntry(HistoryEntry.Restore(
                            item.expression ?? string.Empty,
                            item.isError,
                            item.result));
                    }
                }

                return state;
            }
        }

        [Serializable]
        private class HistoryEntryDto
        {
            public string expression;
            public bool isError;
            public long result;
        }
    }
}
