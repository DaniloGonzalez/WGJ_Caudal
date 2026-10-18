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
                double deadline = EditorApplication.timeSinceStartup + 10;
                while (game == null || game.State != GameState.CharacterSelection)
                {
                    if (EditorApplication.timeSinceStartup > deadline) throw new Exception("La selección no apareció tras la intro.");
                    await Frames();
                    game = Object.FindAnyObjectByType<GameManager>();
                }

                game.ConfirmCharacter(false);
                game.ConfirmCharacter(true);
                Check(game.State == GameState.Playing, "Ambos perfiles pueden iniciar una jornada para la prueba.");
                checks.Add("Main inicia una jornada con ambos roles");

                var task = game.Tasks.Tasks.First(t => t.Status == Caudal.TaskStatus.Available);
                int taskIndex = -1;
                for (int i = 0; i < game.Tasks.Tasks.Count; i++)
                    if (game.Tasks.Tasks[i] == task) { taskIndex = i; break; }
                Check(game.Tasks.Assign(taskIndex, TaskStrategy.Normal), "No se pudo iniciar la asignación de prueba.");
                await Frames();
                var popup = Object.FindAnyObjectByType<PopUpController>();
                Check(popup != null && popup.gameObject.activeInHierarchy, "El spawner no abrió la ventana para el jefe.");
                var bossRect = (RectTransform)popup.transform;
                var bossTexts = popup.GetComponentsInChildren<Text>(true).Select(t => t.text).ToArray();
                Check(bossRect.anchorMin.x == .5f && bossRect.anchorMax.x == 1 && bossTexts.Any(t => t.Contains("JEFE")),
                    "La ventana de asignación no está en la mitad del jefe.");
                checks.Add("La asignación abre una ventana del jefe en la mitad derecha");
                Check(game.Tasks.Resolve(task.Id, task.Day, InteractionKind.Assignment, InteractionResult.Success), "No se pudo resolver la asignación de prueba.");
                await Frames();
                Check(Object.FindAnyObjectByType<PopUpController>() == null, "La ventana del jefe no se cerró al resolver la asignación.");
                game.Tasks.Accept();
                await Frames();

                popup = Object.FindAnyObjectByType<PopUpController>();
                Check(popup != null && popup.gameObject.activeInHierarchy, "El spawner no abrió la ventana al aceptar la tarea.");
                checks.Add("La ejecución abre una ventana del trabajador al aceptar la tarea");
                var workerRect = (RectTransform)popup.transform;
                Check(workerRect.anchorMin.x == 0 && workerRect.anchorMax.x == .5f, "La ventana de ejecución no está en la mitad del trabajador.");
                var texts = popup.GetComponentsInChildren<Text>(true).Select(t => t.text).ToArray();
                Check(texts.Contains(task.Data.taskName) && texts.Contains(task.Data.description), "La ventana no muestra nombre y descripción.");
                checks.Add("La ventana presenta título y descripción de TaskData");
                bool hasMinigame = task.Data.GetMinigamePrefab(InteractionKind.Execution) != null;
                Check(texts.Any(t => t.Contains("TRABAJADOR: WASD")) &&
                    (hasMinigame ? popup.GetComponentInChildren<TimedMinigame>(true) != null : texts.Any(t => t.Contains("MINIJUEGO EN PREPARACIÓN"))),
                    "La ventana no presenta controles o contenido.");
                checks.Add("La ventana muestra rol, controles y área de minijuego");
                Check(popup.GetComponentsInChildren<Button>(true).Length == 1 && Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null,
                    "El botón de cancelar o EventSystem no está disponible.");
                checks.Add("Cancelar dispone de botón opcional y EventSystem");

                popup.GetComponentsInChildren<Button>(true).Single().onClick.Invoke();
                await Frames();
                Check(task.Status == Caudal.TaskStatus.Failed && Object.FindAnyObjectByType<PopUpController>() == null,
                    "Cancelar no resolvió una sola vez y cerró la ventana.");
                checks.Add("Cancelar resuelve una vez y cierra la ventana");

                var second = game.Tasks.Tasks.First(t => t.Status == Caudal.TaskStatus.Available);
                taskIndex = -1;
                for (int i = 0; i < game.Tasks.Tasks.Count; i++)
                    if (game.Tasks.Tasks[i] == second) { taskIndex = i; break; }
                game.Tasks.Assign(taskIndex, TaskStrategy.Normal);
                game.Tasks.Resolve(second.Id, second.Day, InteractionKind.Assignment, InteractionResult.Success);
                game.Tasks.Accept();
                await Frames();
                Check(Object.FindAnyObjectByType<PopUpController>() != null, "No abrió la segunda ventana de prueba.");
                game.Tasks.EndDay();
                await Frames();
                Check(second.Status == Caudal.TaskStatus.Expired && Object.FindAnyObjectByType<PopUpController>() == null,
                    "La ventana no se cerró al expirar la tarea.");
                checks.Add("La ventana se limpia al expirar la tarea");

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
    }
}
