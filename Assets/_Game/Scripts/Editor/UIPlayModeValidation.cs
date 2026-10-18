using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Scripting.LifecycleManagement;
using Object = UnityEngine.Object;

namespace Caudal.Editor
{
    // Prueba de integración del montaje real. No cambia assets ni reglas del juego.
    [InitializeOnLoad]
    [NoAutoStaticsCleanup]
    public static class UIPlayModeValidation
    {
        const string Pending = "Caudal.UIPlay.Pending";
        const string Result = "Caudal.UIPlay.Result";
        static Keyboard keyboard;
        static Keyboard[] physicalKeyboards;
        static readonly List<string> checks = new List<string>();

        static UIPlayModeValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Caudal/Validar UI en Play")]
        public static void Begin()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != "Assets/_Game/Scenes/Main.unity" || scene.isDirty)
                throw new Exception("Abre y guarda Main fuera de Play antes de validar.");
            SessionState.SetBool(Pending, true);
            SessionState.SetString(Result, "running");
            EditorApplication.isPlaying = true;
        }

        public static string Status() => SessionState.GetString(Result, "not run");

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            Run();
        }

        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            checks.Add(message);
        }

        static async Task Frames(int count = 3)
        {
            int target = Time.frameCount + count;
            double deadline = EditorApplication.timeSinceStartup + 5;
            while (Time.frameCount < target)
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
                    throw new Exception("Player loop stopped while waiting for frames.");
                await Task.Delay(15);
            }
        }

        static async Task Press(params Key[] keys)
        {
            keyboard.MakeCurrent();
            if (Keyboard.current != keyboard)
                throw new Exception("Virtual keyboard lost current-device status before input: " + Keyboard.current?.name);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            await Frames();
            if (Keyboard.current != keyboard)
                throw new Exception("Virtual keyboard lost current-device status after input: " + Keyboard.current?.name);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            await Frames();
        }

        static void KeepVirtualKeyboardCurrent()
        {
            if (keyboard != null && keyboard.added) keyboard.MakeCurrent();
        }

        static T Find<T>() where T : Object =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        static void ProfileVisible(CharacterCard card, CharacterData data)
        {
            Check(card.GetComponentsInChildren<RawImage>(true).Single().texture == data.portrait,
                "Character portrait: " + data.characterName);
            var texts = card.GetComponentsInChildren<Text>(true).Select(t => t.text).ToArray();
            Check(texts.Contains(data.characterName) && texts.Contains(data.biography),
                "Character name and biography: " + data.characterName);
        }

        static async Task SelectTask(GameManager game, int index)
        {
            while (game.Selected < index) await Press(game.Input.boss.down);
            while (game.Selected > index) await Press(game.Input.boss.up);
        }

        static async Task ResolveOrPress(GameManager game, TaskInstance task, InteractionKind kind, InteractionResult result, Key key)
        {
            if (task.Data.GetMinigamePrefab(kind) != null)
            {
                Check(game.Tasks.Resolve(task.Id, task.Day, kind, result), "TaskManager accepted minigame result for " + task.Data.taskId);
                await Frames();
            }
            else
                await Press(key);
        }

        static async void Run()
        {
            bool background = Application.runInBackground;
            var previousKeyboard = Keyboard.current;
            checks.Clear();
            try
            {
                Application.runInBackground = true;
                keyboard = InputSystem.AddDevice<Keyboard>("Caudal UI verification");
                physicalKeyboards = InputSystem.devices.OfType<Keyboard>()
                    .Where(device => device != keyboard && device.enabled).ToArray();
                foreach (var device in physicalKeyboards) InputSystem.DisableDevice(device);
                InputSystem.onAfterUpdate += KeepVirtualKeyboardCurrent;
                await Frames();
                var game = Find<GameManager>();
                var canvas = Find<GameScreen>();
                Check(game != null && canvas != null, "Scene systems present");
                Check(Find<IntroPanel>().gameObject.activeInHierarchy && game.State == GameState.DayStart, "Intro visible");
                Check(Object.FindObjectsByType<Camera>(FindObjectsInactive.Include).Length == 1, "One camera in Play");
                Check(Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).Length == 1, "One Canvas in Play");
                Check(canvas.transform.childCount == 7, "Seven panel instances, no duplicates");
                double deadline = EditorApplication.timeSinceStartup + 10;
                while (game.State == GameState.DayStart && EditorApplication.timeSinceStartup < deadline) await Frames();
                await Frames();
                Check(game.State == GameState.CharacterSelection && !Find<IntroPanel>().gameObject.activeSelf, "Intro transitions to selection");
                Check(game.Days.CurrentDay == 0 && game.Days.TimeRemaining == 0, "Selection clock stopped");
                var selection = Find<CharacterSelectionUI>();
                var workerCard = selection.transform.Find("TrabajadorCharacterCard").GetComponent<CharacterCard>();
                var bossCard = selection.transform.Find("JefeCharacterCard").GetComponent<CharacterCard>();
                var workers = new HashSet<CharacterData>();
                var bosses = new HashSet<CharacterData>();
                for (int i = 0; i < 3; i++)
                {
                    workers.Add(game.WorkerCharacter); bosses.Add(game.BossCharacter);
                    ProfileVisible(workerCard, game.WorkerCharacter);
                    ProfileVisible(bossCard, game.BossCharacter);
                    await Press(game.Input.worker.down, game.Input.boss.down);
                }
                Check(workers.Count == 3 && bosses.Count == 3,
                    "Three independent profiles per role using simultaneous input (worker: " + string.Join(", ", workers.Select(c => c.characterName)) +
                    "; boss: " + string.Join(", ", bosses.Select(c => c.characterName)) + ")");
                await Press(game.Input.worker.confirm);
                Check(game.WorkerReady && !game.BossReady && game.State == GameState.CharacterSelection, "Worker ready waits for boss");
                Check(workerCard.transform.Find("Ready").gameObject.activeSelf, "Ready indicator visible");
                await Press(game.Input.worker.cancel);
                Check(!game.WorkerReady, "Worker cancels readiness");
                await Press(game.Input.boss.confirm);
                Check(game.BossReady && !game.WorkerReady && game.State == GameState.CharacterSelection, "Boss ready waits for worker");
                await Press(game.Input.worker.confirm);
                Check(game.State == GameState.Playing && game.Days.CurrentDay == 1, "Both confirmations start day one");
                Check(!selection.gameObject.activeSelf && Find<TrabajadorHUD>().gameObject.activeSelf, "Gameplay panels visible");
                float clock = game.Days.TimeRemaining;
                await Frames(5);
                Check(game.Days.TimeRemaining < clock && Time.frameCount > 10, "Player loop and day clock advance");
                int uiCount = canvas.GetComponentsInChildren<Transform>(true).Length;
                Check(canvas.GetComponentsInChildren<TaskCard>(true).Length == 3, "Three task cards reused");

                // Normal assignment, acceptance and successful simulator execution.
                await Press(game.Input.boss.confirm);
                var first = game.Tasks.Assigning;
                Check(first != null, "Assignment started with boss input");
                var bossPopup = Find<PopUpController>();
                Check(bossPopup != null && ((RectTransform)bossPopup.transform).anchorMin.x == .5f &&
                    bossPopup.GetComponentsInChildren<Text>(true).Any(t => t.text.Contains("JEFE")),
                    "Boss assignment minigame window opens on the right half");
                await ResolveOrPress(game, first, InteractionKind.Assignment, InteractionResult.Success, game.Input.boss.confirm);
                Check(game.Tasks.Pending == first, "Assignment simulator success");
                Check(Find<PopUpController>() == null, "Boss assignment window closes after resolution");
                await Press(game.Input.worker.confirm);
                Check(game.Tasks.Active == first, "Explicit acceptance with worker input");
                var minigameWindow = Find<PopUpController>();
                Check(minigameWindow != null && minigameWindow.gameObject.activeInHierarchy &&
                    ((RectTransform)minigameWindow.transform).anchorMax.x == .5f, "Worker execution minigame window opens on the left half");
                var popupTexts = minigameWindow.GetComponentsInChildren<Text>(true).Select(t => t.text).ToArray();
                Check(popupTexts.Contains(first.Data.taskName) && popupTexts.Contains(first.Data.description), "Minigame window shows task title and description");
                bool hasWorkerMinigame = first.Data.GetMinigamePrefab(InteractionKind.Execution) != null;
                Check(popupTexts.Any(t => t.Contains("TRABAJADOR: WASD")) &&
                    (hasWorkerMinigame ? minigameWindow.GetComponentInChildren<TimedMinigame>(true) != null : popupTexts.Any(t => t.Contains("MINIJUEGO EN PREPARACIÓN"))),
                    "Minigame window shows role controls and playable content");
                await ResolveOrPress(game, first, InteractionKind.Execution, InteractionResult.Success, game.Input.worker.confirm);
                Check(first.Status == Caudal.TaskStatus.Completed && game.Days.QuotaProgress > 0, "Execution simulator success and quota");
                Check(Find<PopUpController>() == null, "Minigame window closes after its result resolves");
                var graph = Find<StatsGraphUI>();
                var marker = (RectTransform)graph.transform.Find("Plot/Marker");
                Check(Vector2.Distance(marker.anchorMin, new Vector2(game.Stats.Happiness, game.Stats.Productivity) / 100) < .001f,
                    "Graph marker follows statistics");
                Check(Find<SharedHUD>().transform.Find("Counters/Quota").GetComponent<Text>().text ==
                    $"CUOTA {game.Days.QuotaProgress:0}/{game.Days.QuotaTarget:0}", "Shared HUD shows actual quota");

                // Pressure refusal can fail without forcing acceptance.
                await SelectTask(game, 1);
                await Press(game.Input.boss.alternate);
                Check(game.Strategy == TaskStrategy.Pressure, "Strategy input preserved");
                await Press(game.Input.boss.confirm);
                var pressure = game.Tasks.Assigning;
                await ResolveOrPress(game, pressure, InteractionKind.Assignment, InteractionResult.Success, game.Input.boss.confirm);
                await Press(game.Input.worker.cancel);
                Check(game.Tasks.Pending == pressure && pressure.Refusing, "Pressure refusal interaction");
                await Press(game.Input.worker.cancel);
                Check(game.Tasks.Active == null && game.Tasks.Pending == pressure && !pressure.Refusing, "Refusal failure preserves decision");
                await Press(game.Input.worker.confirm);
                await Press(game.Input.worker.cancel);
                Check(pressure.Status == Caudal.TaskStatus.Failed, "Execution simulator failure");
                await SelectTask(game, 2);
                await Press(game.Input.boss.alternate);
                await Press(game.Input.boss.confirm);
                var rejected = game.Tasks.Assigning;
                await ResolveOrPress(game, rejected, InteractionKind.Assignment, InteractionResult.Success, game.Input.boss.confirm);
                await Press(game.Input.worker.cancel);
                if (rejected.Refusing)
                    await ResolveOrPress(game, rejected, InteractionKind.Refusal, InteractionResult.Success, game.Input.worker.confirm);
                Check(rejected.Status == Caudal.TaskStatus.Rejected, "Rejection completed");
                Check(canvas.GetComponentsInChildren<Transform>(true).Length == uiCount, "UI not rebuilt during interactions");

                // Advance elapsed time through the existing day API; do not alter duration or rules.
                for (int day = 1; day <= game.Days.TotalDays; day++)
                {
                    game.Days.Tick(1000);
                    await Frames();
                    Check(game.State == GameState.DaySummary && Find<ResultsUI>().gameObject.activeSelf, "Day summary " + day);
                    await Press(game.Input.worker.confirm);
                    if (day < game.Days.TotalDays)
                        Check(game.State == GameState.Playing && game.Days.CurrentDay == day + 1, "Next day " + (day + 1));
                }
                Check(game.State == GameState.GameOver && Find<ResultsUI>().gameObject.activeSelf, "Final displayed");
                await Press(game.Input.boss.confirm);
                Check(game.State == GameState.CharacterSelection && !game.WorkerReady && !game.BossReady, "Restart returns to independent selection");
                Check(game.Stats.Happiness == 50 && game.Stats.Productivity == 50 && game.Days.CurrentDay == 0, "Restart resets game values");
                Check(canvas.GetComponentsInChildren<Transform>(true).Length == uiCount, "No UI duplicates after full cycle");
                var report = "CAUDAL_UI_PLAY_PASSED (" + Screen.width + "x" + Screen.height + "):\n" + string.Join("\n", checks);
                SessionState.SetString(Result, report);
                Debug.Log(report);
            }
            catch (Exception exception)
            {
                SessionState.SetString(Result, "FAILED: " + exception);
                Debug.LogException(exception);
            }
            finally
            {
                if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
                InputSystem.onAfterUpdate -= KeepVirtualKeyboardCurrent;
                keyboard = null;
                if (physicalKeyboards != null)
                    foreach (var device in physicalKeyboards)
                        if (device != null && device.added && !device.enabled) InputSystem.EnableDevice(device);
                physicalKeyboards = null;
                if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
                Application.runInBackground = background;
            }
        }
    }
}
