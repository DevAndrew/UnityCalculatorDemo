using System.Collections;
using System.Collections.Generic;
using DevAndrew.Core.UI.VirtualizedList;
using TMPro;
using UnityEngine;

namespace DevAndrew.Calculator.Presentation
{
    public sealed class CalculatorHistoryView : MonoBehaviour
    {
        [SerializeField] private VerticalScroller _historyVirtualScroller;
        [SerializeField] private int _minRowHeight = 48;
        [SerializeField] private int _rowVerticalPadding = 8;

        private readonly List<string> _historyLines = new List<string>();
        private readonly List<int> _cachedHeights = new List<int>();
        private TMP_Text _measurementText;
        private float _cachedMeasureWidth = -1f;
        private float _lastAppliedMeasureWidth = -1f;
        private bool _pendingKeepBottom = true;
        private Coroutine _pendingRefreshRoutine;
        private bool _isReady;

        private void Awake()
        {
            if (_historyVirtualScroller == null)
            {
                Debug.LogError("CalculatorHistoryView: VerticalScroller reference is missing.");
                enabled = false;
                return;
            }

            _measurementText = ResolveMeasurementText();
            _historyVirtualScroller.OnFill += FillVirtualizedRow;
            _historyVirtualScroller.OnHeight += GetVirtualizedRowHeight;
            _isReady = true;
        }

        private void Start()
        {
            if (_isReady)
            {
                RequestVirtualRefresh(true);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!_isReady)
            {
                return;
            }

            var width = GetMeasureWidth();
            if (width <= 1f || Mathf.Approximately(_lastAppliedMeasureWidth, width))
            {
                return;
            }

            _cachedMeasureWidth = -1f;
            InvalidateCachedHeights();
            var keepBottom = _historyVirtualScroller.IsAtBottom();
            RequestVirtualRefresh(keepBottom);
        }

        private void OnDestroy()
        {
            if (_pendingRefreshRoutine != null)
            {
                StopCoroutine(_pendingRefreshRoutine);
                _pendingRefreshRoutine = null;
            }

            if (_historyVirtualScroller != null)
            {
                _historyVirtualScroller.OnFill -= FillVirtualizedRow;
                _historyVirtualScroller.OnHeight -= GetVirtualizedRowHeight;
            }
        }

        public void SetHistory(IReadOnlyList<string> lines)
        {
            _historyLines.Clear();
            _cachedHeights.Clear();
            if (lines != null)
            {
                _historyLines.AddRange(lines);
                for (var i = 0; i < lines.Count; i++)
                {
                    _cachedHeights.Add(-1);
                }
            }

            _cachedMeasureWidth = -1f;
            if (_isReady)
            {
                RequestVirtualRefresh(true);
            }
        }

        public void AppendHistoryLine(string line)
        {
            var normalized = line ?? string.Empty;
            _historyLines.Add(normalized);
            _cachedHeights.Add(-1);

            if (_isReady)
            {
                var keepBottom = _historyVirtualScroller.IsAtBottom();
                RequestVirtualRefresh(keepBottom);
            }
        }

        private void FillVirtualizedRow(int index, GameObject item)
        {
            if (index < 0 || index >= _historyLines.Count || item == null)
            {
                return;
            }

            var text = _historyLines[index];
            if (item.TryGetComponent<HistoryRowView>(out var rowView))
            {
                rowView.SetText(text);
                return;
            }

            var tmp = item.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = text;
            }
        }

        private int GetVirtualizedRowHeight(int index)
        {
            if (index < 0 || index >= _historyLines.Count)
            {
                return _minRowHeight;
            }

            var width = GetMeasureWidth();
            if (width <= 1f || _measurementText == null)
            {
                return _minRowHeight;
            }

            if (!Mathf.Approximately(_cachedMeasureWidth, width))
            {
                _cachedMeasureWidth = width;
                InvalidateCachedHeights();
            }

            if (_cachedHeights[index] > 0)
            {
                return _cachedHeights[index];
            }

            var preferred = _measurementText.GetPreferredValues(_historyLines[index], width, Mathf.Infinity);
            var calculated = Mathf.CeilToInt(preferred.y) + Mathf.Max(0, _rowVerticalPadding);
            var height = Mathf.Max(_minRowHeight, calculated);
            _cachedHeights[index] = height;
            return height;
        }

        private TMP_Text ResolveMeasurementText()
        {
            var prefab = _historyVirtualScroller.ItemPrefab;
            if (prefab == null)
            {
                return null;
            }

            if (prefab.TryGetComponent<HistoryRowView>(out var row))
            {
                var rowText = row.GetComponentInChildren<TMP_Text>(true);
                if (rowText != null)
                {
                    return rowText;
                }
            }

            return prefab.GetComponentInChildren<TMP_Text>(true);
        }

        private float GetMeasureWidth()
        {
            var rowWidth = _historyVirtualScroller.ViewportWidth;
            if (rowWidth <= 1f)
            {
                return 0f;
            }

            if (_measurementText == null)
            {
                return rowWidth;
            }

            var rect = _measurementText.rectTransform;
            if (rect == null)
            {
                return rowWidth;
            }

            // Account for left/right offsets when text is stretched inside row container.
            var left = Mathf.Max(0f, rect.offsetMin.x);
            var right = Mathf.Max(0f, -rect.offsetMax.x);
            var textWidth = rowWidth - left - right;
            return Mathf.Max(1f, textWidth);
        }

        private void InvalidateCachedHeights()
        {
            for (var i = 0; i < _cachedHeights.Count; i++)
            {
                _cachedHeights[i] = -1;
            }
        }

        private void RequestVirtualRefresh(bool keepBottom)
        {
            _pendingKeepBottom = keepBottom;
            if (_pendingRefreshRoutine == null)
            {
                _pendingRefreshRoutine = StartCoroutine(RefreshWhenLayoutReady());
            }
        }

        private IEnumerator RefreshWhenLayoutReady()
        {
            while (_isReady)
            {
                var width = GetMeasureWidth();
                if (width > 1f)
                {
                    _cachedMeasureWidth = width;
                    _lastAppliedMeasureWidth = width;
                    _historyVirtualScroller.InitData(_historyLines.Count, _pendingKeepBottom);
                    _pendingRefreshRoutine = null;
                    yield break;
                }

                // Wait until layout settles and viewport width is available.
                yield return null;
            }

            _pendingRefreshRoutine = null;
        }
    }
}
