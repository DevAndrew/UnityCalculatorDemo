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
        private SaveStateDto _cachedDto;

        public FileStateRepository(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
        }

        public CalculatorState Load()
        {
            _cachedDto = null;

            if (_saveLoadService.TryLoad(FileName, out SaveStateDto dto))
            {
                _cachedDto = dto;
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

            if (_cachedDto == null)
            {
                _cachedDto = SaveStateDto.FromDomain(state);
            }
            else
            {
                _cachedDto.SyncFrom(state);
            }

            return _saveLoadService.TrySave(FileName, _cachedDto);
        }

        [Serializable]
        private class SaveStateDto
        {
            public int version = 1;
            public string inputExpression = string.Empty;
            public List<HistoryEntryDto> history = new List<HistoryEntryDto>();

            public static SaveStateDto FromDomain(CalculatorState state)
            {
                var dto = new SaveStateDto();
                dto.SyncFrom(state);
                return dto;
            }

            public void SyncFrom(CalculatorState state)
            {
                EnsureHistoryInitialized();
                inputExpression = state.InputExpression;

                var source = state.History;

                if (source.Count == history.Count)
                {
                    SyncSameCount(source);
                    return;
                }

                if (source.Count > history.Count
                    && (history.Count == 0 || MatchesEntry(history[0], source[0])))
                {
                    AppendNewEntries(source, history.Count);
                    return;
                }

                RebuildHistory(source);
            }

            public CalculatorState ToDomain()
            {
                EnsureHistoryInitialized();
                var state = CalculatorState.CreateDefault();
                state.TrySetInputExpression(inputExpression);

                if (history != null)
                {
                    var startIndex = Math.Max(0, history.Count - CalculatorState.MaxHistoryEntries);
                    for (var i = startIndex; i < history.Count; i++)
                    {
                        var item = history[i];
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

            private void EnsureHistoryInitialized()
            {
                if (history == null)
                {
                    history = new List<HistoryEntryDto>();
                }
            }

            private void SyncSameCount(IReadOnlyList<HistoryEntry> source)
            {
                if (history.Count == 0)
                {
                    return;
                }

                var lastSource = source[source.Count - 1];
                var lastCached = history[history.Count - 1];

                if (MatchesEntry(lastCached, lastSource))
                {
                    return;
                }

                // Invariant: CalculatorState history is append-only and capped by removing
                // from the start, so same-count change means one-item sliding window.
                history.RemoveAt(0);
                history.Add(CreateDto(lastSource));
            }

            private void AppendNewEntries(IReadOnlyList<HistoryEntry> source, int fromIndex)
            {
                for (var i = fromIndex; i < source.Count; i++)
                {
                    var item = source[i];
                    if (item == null)
                    {
                        continue;
                    }

                    history.Add(CreateDto(item));
                }
            }

            private void RebuildHistory(IReadOnlyList<HistoryEntry> source)
            {
                history.Clear();

                if (history.Capacity < source.Count)
                {
                    history.Capacity = source.Count;
                }

                AppendNewEntries(source, 0);
            }

            private static bool MatchesEntry(HistoryEntryDto dto, HistoryEntry entry)
            {
                if (dto == null || entry == null)
                {
                    return dto == null && entry == null;
                }

                return string.Equals(dto.expression, entry.Expression, StringComparison.Ordinal)
                    && dto.isError == entry.IsError
                    && dto.result == entry.Result;
            }

            private static HistoryEntryDto CreateDto(HistoryEntry entry)
            {
                return new HistoryEntryDto
                {
                    expression = entry.Expression,
                    isError = entry.IsError,
                    result = entry.Result
                };
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
