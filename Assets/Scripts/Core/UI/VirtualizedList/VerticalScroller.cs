// MIT License
// Copyright (c) 2019-2024 Mopsicus
//
// This file is an adapted vertical-only subset inspired by:
// https://github.com/mopsicus/uis/blob/26627ae362dc7c4873d700449ccf065122fa4aa1/Runtime/Scroller.cs
//
// Modifications:
// - Kept only vertical list virtualization needed for calculator history.
// - Removed pull-to-refresh labels and horizontal logic.
// - Simplified API for project-specific usage.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevAndrew.Core.UI.VirtualizedList
{
    public sealed class VerticalScroller : MonoBehaviour
    {
        public delegate int HeightItem(int index);

        [Header("References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _itemPrefab;
        [SerializeField] private RectTransform _parentContainer;

        [Header("Layout")]
        [SerializeField] private int _topPadding = 10;
        [SerializeField] private int _bottomPadding = 10;
        [SerializeField] private int _itemSpacing = 6;
        [SerializeField] private int _addonViewsCount = 4;

        public event HeightItem OnHeight;
        public Action<int, GameObject> OnFill = delegate { };

        private RectTransform _content;
        private float _lastContainerHeight = -1f;

        private GameObject[] _views;
        private RectTransform[] _rects;
        private int[] _boundIndexes;

        private int[] _heights;
        private float[] _offsets;
        private int _count;
        private int _firstRenderedIndex = -1;

        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;
        public int ItemsCount => _count;
        public float ViewportWidth => _scrollRect != null && _scrollRect.viewport != null
            ? Mathf.Abs(_scrollRect.viewport.rect.width)
            : 0f;
        public RectTransform ItemPrefab => _itemPrefab;

        private void Awake()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            if (_scrollRect == null)
            {
                Debug.LogError("VerticalScroller: ScrollRect reference is missing.");
                return;
            }

            _content = _scrollRect.content;
            if (_content == null)
            {
                Debug.LogError("VerticalScroller: ScrollRect.content reference is missing.");
                return;
            }

            _scrollRect.onValueChanged.AddListener(HandleScrollChanged);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(HandleScrollChanged);
            }
        }

        private void Update()
        {
            if (!_isInitialized || _count == 0)
            {
                return;
            }

            var currentHeight = GetContainerHeight();
            if (!Mathf.Approximately(currentHeight, _lastContainerHeight))
            {
                _lastContainerHeight = currentHeight;
                RebuildViewsIfNeeded();
                _firstRenderedIndex = -1;
            }

            UpdateVisible(false);
        }

        public void InitData(int count, bool scrollToBottom = false)
        {
            if (!_isInitialized)
            {
                return;
            }

            _count = Mathf.Max(0, count);
            _firstRenderedIndex = -1;

            if (_count == 0)
            {
                EnsurePools(0);
                if (_content != null)
                {
                    _content.sizeDelta = new Vector2(_content.sizeDelta.x, _topPadding + _bottomPadding);
                    _content.anchoredPosition = Vector2.zero;
                }

                return;
            }

            BuildOffsetsAndHeights(_count);
            RebuildViewsIfNeeded();
            UpdateContentHeight();

            if (scrollToBottom)
            {
                ScrollToBottom();
            }

            UpdateVisible(true);
        }

        public void ScrollToBottom()
        {
            if (!_isInitialized || _count == 0)
            {
                return;
            }

            _scrollRect.verticalNormalizedPosition = 0f;
            UpdateVisible(true);
        }

        public bool IsAtBottom(float epsilon = 0.001f)
        {
            if (!_isInitialized || _count == 0)
            {
                return true;
            }

            return _scrollRect.verticalNormalizedPosition <= epsilon;
        }

        private void HandleScrollChanged(Vector2 _)
        {
            if (!_isInitialized || _count == 0)
            {
                return;
            }

            UpdateVisible(false);
        }

        private void BuildOffsetsAndHeights(int count)
        {
            EnsureLayoutBuffers(count);

            var currentOffset = (float)_topPadding;
            for (var i = 0; i < count; i++)
            {
                var fallbackHeight = GetFallbackItemHeight();
                var requestedHeight = OnHeight != null ? OnHeight(i) : fallbackHeight;
                var itemHeight = Mathf.Max(1, requestedHeight);

                _heights[i] = itemHeight;
                _offsets[i] = currentOffset;
                currentOffset += itemHeight + _itemSpacing;
            }
        }

        private void UpdateContentHeight()
        {
            if (_count == 0 || _content == null)
            {
                return;
            }

            var lastIndex = _count - 1;
            var contentHeight = _offsets[lastIndex] + _heights[lastIndex] + _bottomPadding;

            var size = _content.sizeDelta;
            size.y = contentHeight;
            _content.sizeDelta = size;
        }

        private void RebuildViewsIfNeeded()
        {
            var desiredCount = CalculateRequiredViewsCount();
            desiredCount = Mathf.Clamp(desiredCount, 0, _count);

            if (_views == null || _views.Length != desiredCount)
            {
                EnsurePools(desiredCount);
            }
        }

        private int CalculateRequiredViewsCount()
        {
            if (_count == 0)
            {
                return 0;
            }

            var averageHeight = GetFallbackItemHeight() + Mathf.Max(0, _itemSpacing);
            var needed = Mathf.CeilToInt(GetContainerHeight() / Mathf.Max(1f, averageHeight));
            return Mathf.Max(1, needed + Mathf.Max(0, _addonViewsCount));
        }

        private float GetContainerHeight()
        {
            if (_parentContainer != null)
            {
                return Mathf.Abs(_parentContainer.rect.height);
            }

            if (_scrollRect != null && _scrollRect.viewport != null)
            {
                return Mathf.Abs(_scrollRect.viewport.rect.height);
            }

            var selfRect = GetComponent<RectTransform>();
            return selfRect == null ? 0f : Mathf.Abs(selfRect.rect.height);
        }

        private int GetFallbackItemHeight()
        {
            if (_itemPrefab == null)
            {
                return 48;
            }

            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(_itemPrefab.rect.height)));
        }

        private void EnsurePools(int viewsCount)
        {
            DestroyPools();

            _views = new GameObject[viewsCount];
            _rects = new RectTransform[viewsCount];
            _boundIndexes = new int[viewsCount];

            for (var i = 0; i < viewsCount; i++)
            {
                _boundIndexes[i] = -1;
            }

            if (_itemPrefab == null || _content == null)
            {
                if (_itemPrefab == null)
                {
                    Debug.LogWarning("VerticalScroller: Item prefab is missing.");
                }

                return;
            }

            for (var i = 0; i < viewsCount; i++)
            {
                var clone = Instantiate(_itemPrefab.gameObject, _content);
                clone.name = $"Item_{i}";
                clone.SetActive(false);

                var rect = clone.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                }

                _views[i] = clone;
                _rects[i] = rect;
            }
        }

        private void DestroyPools()
        {
            if (_views == null)
            {
                return;
            }

            for (var i = 0; i < _views.Length; i++)
            {
                if (_views[i] != null)
                {
                    Destroy(_views[i]);
                }
            }

            _views = null;
            _rects = null;
            _boundIndexes = null;
        }

        private void EnsureLayoutBuffers(int count)
        {
            if (_heights == null || _heights.Length != count)
            {
                _heights = new int[count];
            }

            if (_offsets == null || _offsets.Length != count)
            {
                _offsets = new float[count];
            }
        }

        private void UpdateVisible(bool forceRebind)
        {
            if (_views == null || _views.Length == 0 || _count == 0)
            {
                return;
            }

            var scrollTop = Mathf.Max(0f, _content.anchoredPosition.y);
            var firstVisible = FindFirstVisibleIndex(scrollTop);
            var firstToRender = Mathf.Max(0, firstVisible - Mathf.Max(0, _addonViewsCount / 2));

            if (!forceRebind && firstToRender == _firstRenderedIndex)
            {
                return;
            }

            _firstRenderedIndex = firstToRender;

            for (var slot = 0; slot < _views.Length; slot++)
            {
                var index = firstToRender + slot;
                var view = _views[slot];
                var rect = _rects[slot];
                if (view == null || rect == null)
                {
                    continue;
                }

                if (index >= _count)
                {
                    _boundIndexes[slot] = -1;
                    view.SetActive(false);
                    continue;
                }

                view.SetActive(true);

                var anchored = rect.anchoredPosition;
                anchored.x = 0f;
                anchored.y = -_offsets[index];
                rect.anchoredPosition = anchored;

                var size = rect.sizeDelta;
                size.y = _heights[index];
                rect.sizeDelta = size;

                if (_boundIndexes[slot] != index || forceRebind)
                {
                    _boundIndexes[slot] = index;
                    OnFill(index, view);
                }
            }
        }

        private int FindFirstVisibleIndex(float scrollTop)
        {
            if (_count <= 1)
            {
                return 0;
            }

            var low = 0;
            var high = _count - 1;

            while (low < high)
            {
                var mid = low + ((high - low) / 2);
                var endOffset = _offsets[mid] + _heights[mid];
                if (endOffset < scrollTop)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            return Mathf.Clamp(low, 0, _count - 1);
        }
    }
}
