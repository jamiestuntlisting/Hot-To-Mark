using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HotToMark.UI
{
    /// <summary>
    /// Static helpers for building Unity UI elements at runtime.
    /// Used by all UI scripts that self-build their hierarchy.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Create a full-screen panel with background color.
        /// </summary>
        public static GameObject CreatePanel(string name, Transform parent, Color bgColor)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor;

            return panel;
        }

        /// <summary>
        /// Create an Image element.
        /// </summary>
        public static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            var img = obj.AddComponent<Image>();
            img.color = color;

            return obj;
        }

        /// <summary>
        /// Create a TextMeshProUGUI element.
        /// </summary>
        public static GameObject CreateText(string name, Transform parent,
            string content, float fontSize, Color color,
            FontStyles style = FontStyles.Normal,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            tmp.richText = true;

            return obj;
        }

        /// <summary>
        /// Set anchors on a RectTransform using anchor min/max.
        /// </summary>
        public static void SetAnchors(GameObject obj,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot = default, Vector2 anchoredPos = default,
            Vector2 sizeDelta = default)
        {
            var rect = obj.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (pivot != default) rect.pivot = pivot;
            if (anchoredPos != default) rect.anchoredPosition = anchoredPos;
            if (sizeDelta != default) rect.sizeDelta = sizeDelta;
        }
    }
}
