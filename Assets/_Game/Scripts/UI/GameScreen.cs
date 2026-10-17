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

        public static Text Label(Transform parent, string value, int size, Color color,
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
            return text;
        }

        public static void Build(Transform parent, GameManager game, Texture workerOffice, Texture bossOffice, Texture worker, Texture boss)
        {
            var left = Panel(parent, "Trabajador", new Color(.06f, .13f, .16f), new Vector2(0, .29f), new Vector2(.5f, .88f));
            var right = Panel(parent, "Jefe", new Color(.16f, .12f, .12f), new Vector2(.5f, .29f), new Vector2(1, .88f));
            Picture(left, workerOffice, new Vector2(0, .35f), new Vector2(.45f, .9f));
            var workerPicture = Picture(left, worker, new Vector2(.45f, .35f), new Vector2(1, .9f));
            Picture(right, bossOffice, new Vector2(0, .35f), new Vector2(.45f, .9f));
            var bossPicture = Picture(right, boss, new Vector2(.45f, .35f), new Vector2(1, .9f));
            var workerTitle = Label(left, "TRABAJADOR", 26, Color.white, new Vector2(0, .9f), Vector2.one);
            var bossTitle = Label(right, "JEFE", 26, Color.white, new Vector2(0, .9f), Vector2.one);
            var workerText = Label(left, "", 22, Color.white, new Vector2(.03f, .02f), new Vector2(.97f, .36f));
            var bossText = Label(right, "", 20, Color.white, new Vector2(.03f, .02f), new Vector2(.97f, .36f));
            left.gameObject.AddComponent<TrabajadorHUD>().Initialize(game, workerText);
            right.gameObject.AddComponent<JefeHUD>().Initialize(game, bossText);
            var shared = Label(parent, "", 23, Color.white, new Vector2(.02f, .89f), new Vector2(.98f, .99f));
            shared.gameObject.AddComponent<SharedHUD>().Initialize(game, shared);
            var graph = Panel(parent, "Plano cartesiano", new Color(.1f, .16f, .2f), new Vector2(.02f, .04f), new Vector2(.25f, .26f));
            Panel(graph, "X", Color.gray, new Vector2(0, .495f), new Vector2(1, .505f));
            Panel(graph, "Y", Color.gray, new Vector2(.495f, 0), new Vector2(.505f, 1));
            Label(graph, "Productividad ↑", 14, Color.white, new Vector2(0, .8f), new Vector2(.9f, 1));
            Label(graph, "Felicidad →", 14, Color.white, Vector2.zero, new Vector2(.9f, .2f));
            var marker = Panel(graph, "Estado de oficina", new Color(.3f, 1, .7f), Vector2.zero, Vector2.zero);
            marker.sizeDelta = new Vector2(12, 12);
            var stats = Label(parent, "", 21, Color.white, new Vector2(.28f, .04f), new Vector2(.98f, .26f));
            graph.gameObject.AddComponent<StatsGraphUI>().Initialize(game, marker, stats);
            var result = Panel(parent, "Resultados", new Color(.025f, .05f, .07f, .98f), new Vector2(.08f, .12f), new Vector2(.92f, .88f));
            var resultText = Label(result, "", 30, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            parent.gameObject.AddComponent<ResultsUI>().Initialize(game, result.gameObject, resultText);
            result.gameObject.SetActive(false);
            parent.gameObject.AddComponent<CharacterSelectionUI>().Initialize(
                game, parent, workerPicture, bossPicture, workerTitle, bossTitle);
        }
        public static RawImage Picture(Transform parent, Texture texture, Vector2 min, Vector2 max)
        {
            string imageName = texture != null ? texture.name : "Retrato";
            var wrapper = new GameObject("Marco " + imageName, typeof(RectTransform));
            var frame = wrapper.GetComponent<RectTransform>();
            frame.SetParent(parent, false);
            frame.anchorMin = min;
            frame.anchorMax = max;
            frame.offsetMin = frame.offsetMax = Vector2.zero;

            var go = new GameObject(imageName, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(frame, false);
            var picture = go.GetComponent<RawImage>();
            picture.texture = texture;
            picture.raycastTarget = false;

            // Encajar sin estirar ni recortar el PNG.
            var aspect = go.AddComponent<AspectRatioFitter>();
            aspect.aspectRatio = texture != null ? (float)texture.width / texture.height : 1;
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            return picture;
        }
    }
}
