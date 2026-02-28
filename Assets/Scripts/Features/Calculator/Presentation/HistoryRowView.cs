using TMPro;
using UnityEngine;

namespace DevAndrew.Calculator.Presentation
{
    public sealed class HistoryRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public void SetText(string value)
        {
            if (_text == null)
            {
                return;
            }

            _text.text = value ?? string.Empty;
        }
    }
}
