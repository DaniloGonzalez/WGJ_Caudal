using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string teamName = "Caudal";
        [SerializeField] private string playerOneName = "Jefe";
        [SerializeField] private string playerTwoName = "Trabajador";
        [SerializeField, Min(0)] private float introDuration = 2f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;

        private IEnumerator Start()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.07f, 0.09f);

            var canvasObject = new GameObject("Game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            GameScreen.Build(canvasObject.transform, playerOneName, playerTwoName);

            var intro = GameScreen.Panel(canvasObject.transform, "Intro Caudal",
                camera.backgroundColor, Vector2.zero, Vector2.one);
            GameScreen.Label(intro, "UN JUEGO DE", 22, new Color(0.36f, 0.86f, 0.77f),
                new Vector2(0.15f, 0.57f), new Vector2(0.85f, 0.64f));
            GameScreen.Label(intro, teamName, 100, Color.white,
                new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.57f));
            GameScreen.Panel(intro, "Linea", new Color(0.36f, 0.86f, 0.77f),
                new Vector2(0.44f, 0.33f), new Vector2(0.56f, 0.335f));
            var group = intro.gameObject.AddComponent<CanvasGroup>();
            yield return new WaitForSecondsRealtime(Mathf.Max(0, introDuration));
            float duration = Mathf.Max(0.01f, fadeDuration);
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - elapsed / duration;
                yield return null;
            }
            Destroy(intro.gameObject);
        }
    }
}
