using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Caudal.Editor
{
    [InitializeOnLoad]
    public static class MinigamePopupValidation
    {
        const string PendingKey = "Caudal.MinigamePopup.Pending";
        const string ResultKey = "Caudal.MinigamePopup.Result";

        static MinigamePopupValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Caudal/Validar ventana de minijuegos")]
        public static void Begin()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != "Assets/_Game/Scenes/Main.unity" || scene.isDirty)
                throw new Exception("Abre y guarda Main fuera de Play antes de validar.");
            SessionState.SetBool(PendingKey, true);
            SessionState.SetString(ResultKey, "running");
            EditorApplication.isPlaying = true;
        }

        public static string Status() => SessionState.GetString(ResultKey, "not run");

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
            {
                SessionState.SetBool(PendingKey, false);
                Run();
            }
        }

        static async Task Frames(int count = 2)
        {
            int target = Time.frameCount + count;
            double deadline = EditorApplication.timeSinceStartup + 5;
            while (Time.frameCount < target)
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
                    throw new Exception("El player loop se detuvo durante la prueba.");
                await Task.Delay(15);
            }
        }

        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        static async void Run()
        {
            var checks = new System.Collections.Generic.List<string>();
            bool background = Application.runInBackground;
            try
            {
                Application.runInBackground = true;
                var game = Object.FindAnyObjectByType<GameManager>();
                Check(game != null, "GameManager presente en Main");
                var canvas = Object.FindAnyObjectByType<Canvas>();
                var spawner = Object.FindAnyObjectByType<MinigameSpawner>();
                Check(canvas != null && spawner != null, "Canvas y MinigameSpawner presentes");
                var popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/PopUp.prefab");
                Check(popupPrefab != null, "PopUp.prefab presente");

                foreach (CharacterRole role in new[] { CharacterRole.Boss, CharacterRole.Worker })
                {
                    game.Restart();
                    game.ConfirmCharacter(role == CharacterRole.Boss);
                    Check(game.State == GameState.Playing && game.ActiveRole == role, "Una confirmación inicia la perspectiva " + role);
                    Check(game.Tasks.StartOwnTask(0), "TaskManager inicia tarea de " + role);
                    await Frames();
                    var task = game.Tasks.Active;
                    var popup = Object.FindAnyObjectByType<PopUpController>();
                    Check(task != null && popup != null && popup.gameObject.activeInHierarchy, "Spawner abre popup de " + role);
                    var rect = (RectTransform)popup.transform;
                    var texts = popup.GetComponentsInChildren<Text>(true).Select(t => t.text).ToArray();
                    Check(texts.Contains(task.Data.taskName) && texts.Contains(task.Data.description), "Popup muestra título y descripción");
                    Check(rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one, "Popup cubre el Canvas");
                    var window = popup.transform.Find("Window") as RectTransform;
                    Check(window != null && window.anchorMin == new Vector2(.5f, .5f) &&
                        window.anchorMax == new Vector2(.5f, .5f) && window.anchoredPosition == Vector2.zero,
                        "Ventana de minijuego centrada en el popup");
                    Check(popup.GetComponentsInChildren<Button>(true).Length == 1 && Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null,
                        "Popup conserva cancelar opcional y EventSystem");
                    var minigame = popup.GetComponentInChildren<TimedMinigame>(true);
                    Check(minigame != null && task.Data.GetMinigamePrefab(role) != null, "Popup carga minijuego propio de " + role);
                    Check(minigame.gameObject.activeInHierarchy, "Minijuego queda activo dentro del popup");
                    Finish(minigame, InteractionResult.Success);
                    await Frames();
                    Check(task.Status == Caudal.TaskStatus.Completed && Object.FindAnyObjectByType<PopUpController>() == null,
                        "Resultado cierra popup y completa tarea de " + role);
                    checks.Add("Popup y resultado funcional para " + role);
                }

                string report = "CAUDAL_MINIGAME_POPUP_PASSED (" + Screen.width + "x" + Screen.height + "):\n" + string.Join("\n", checks);
                SessionState.SetString(ResultKey, report);
                Debug.Log(report);
            }
            catch (Exception exception)
            {
                SessionState.SetString(ResultKey, "FAILED: " + exception);
                Debug.LogException(exception);
            }
            finally
            {
                Application.runInBackground = background;
                EditorApplication.isPlaying = false;
            }
        }

        static void Finish(TimedMinigame minigame, InteractionResult result)
        {
            typeof(TimedMinigame).GetMethod("Finish", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(minigame, new object[] { result });
        }
    }
}
