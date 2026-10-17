using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    // Pantalla dividida con un texto centrado por personaje.
    public static class GameScreen
    {
        public static RectTransform Panel(Transform parent, string name, Color color,
            Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        public static void Label(Transform parent, string value, int size, Color color,
            Vector2 min, Vector2 max)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
        }

        public static void Build(Transform parent, string playerOneName, string playerTwoName)
        {
            var left = Panel(parent, "Panel izquierdo", new Color(0.08f, 0.22f, 0.25f),
                Vector2.zero, new Vector2(0.5f, 1));
            var right = Panel(parent, "Panel derecho", new Color(0.25f, 0.17f, 0.18f),
                new Vector2(0.5f, 0), Vector2.one);
            Label(left, playerOneName, 52, Color.white,
                new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.6f));
            Label(right, playerTwoName, 52, Color.white,
                new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.6f));
            Panel(parent, "Division", new Color(0.85f, 0.84f, 0.76f),
                new Vector2(0.499f, 0), new Vector2(0.501f, 1));
        }
    }
}
