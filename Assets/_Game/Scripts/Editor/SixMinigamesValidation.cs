using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Scripting.LifecycleManagement;

namespace Caudal.Editor
{
    [InitializeOnLoad, NoAutoStaticsCleanup]
    public static class SixMinigamesValidation
    {
        const string PendingKey = "Caudal.SixMinigames.Pending";
        const string ResultKey = "Caudal.SixMinigames.Result";
        static Keyboard keyboard;
        static Canvas canvas;
        static GameManager gameManager;
        static bool gameManagerWasEnabled;

        static readonly string[] PrefabPaths =
        {
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_01.prefab",
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_02.prefab",
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_03.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_01.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_02.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_03.prefab"
        };

        static SixMinigamesValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Caudal/Validar seis minijuegos")]
        public static void Begin()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
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

        static async void Run()
        {
            bool oldBackground = Application.runInBackground;
            try
            {
                Application.runInBackground = true;
                canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                gameManagerWasEnabled = gameManager != null && gameManager.enabled;
                if (canvas == null || gameManager == null || gameManager.Input == null)
                    throw new Exception("Faltan Canvas, GameManager o InputRouter.");
                gameManager.enabled = false;
                typeof(GameManager).GetProperty("State").GetSetMethod(true).Invoke(gameManager, new object[] { GameState.DayStart });
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                EditorApplication.update += KeepKeyboardCurrent;

                string[] names = { "MG_BOSS_01", "MG_BOSS_02", "MG_BOSS_03", "MG_WORKER_01", "MG_WORKER_02", "MG_WORKER_03" };
                for (int i = 0; i < PrefabPaths.Length; i++)
                {
                    await VerifySuccess(PrefabPaths[i], i);
                    await VerifyFailure(PrefabPaths[i], i);
                }
                await VerifyIndependentInput();
                await VerifyPopupsTogether();
                await VerifyTaskFlow(gameManager);

                string report = "CAUDAL_SIX_MINIGAMES_PASSED (" + Screen.width + "x" + Screen.height + "):\n" +
                    string.Join("\n", names) +
                    "\nÉxito y fracaso comprobados por mecánica.\nLos dos minijuegos completaron en popups simultáneos.\nTaskManager asignó y aceptó la tarea con los prefabs por rol.";
                SessionState.SetString(ResultKey, report);
                string path = Path.Combine(Application.dataPath, "../Docs/UI-Verification/six-minigames-play.txt");
                File.WriteAllText(path, report);
                Debug.Log(report);
            }
            catch (Exception exception)
            {
                SessionState.SetString(ResultKey, "FAILED: " + exception);
                Debug.LogException(exception);
            }
            finally
            {
                EditorApplication.update -= KeepKeyboardCurrent;
                if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
                keyboard = null;
                if (gameManager != null) gameManager.enabled = gameManagerWasEnabled;
                gameManager = null;
                Application.runInBackground = oldBackground;
                EditorApplication.isPlaying = false;
            }
        }

        static void KeepKeyboardCurrent()
        {
            if (keyboard != null && keyboard.added) keyboard.MakeCurrent();
        }

        static async Task VerifySuccess(string path, int index)
        {
            var instance = Create(path);
            var minigame = instance.GetComponent<TimedMinigame>();
            InteractionResult? result = null;
            minigame.OnFinished += value => result = value;
            await Frames(2);

            switch (index)
            {
                case 0:
                    int[] documentColors = Field<int[]>(minigame, "documentColors");
                    for (int n = 0; n < 3; n++)
                    {
                        await Press(Key.Enter);
                        for (int j = 0; j < documentColors[n]; j++) await Press(Key.DownArrow);
                        await Press(Key.Enter);
                    }
                    break;
                case 1:
                    int[] priorities = Field<int[]>(minigame, "priorities");
                    for (int slot = 0; slot < 3; slot++)
                    {
                        int card = Array.IndexOf(priorities, slot);
                        for (int j = 0; j < card; j++) await Press(Key.DownArrow);
                        await Press(Key.Enter);
                        for (int j = 0; j < slot; j++) await Press(Key.DownArrow);
                        await Press(Key.Enter);
                    }
                    break;
                case 2:
                    bool[] approvals = Field<bool[]>(minigame, "shouldApprove");
                    for (int n = 0; n < 5; n++)
                    {
                        if (!approvals[n]) await Press(Key.DownArrow);
                        await Press(Key.Enter);
                    }
                    break;
                case 3:
                    int[] connectorColors = Field<int[]>(minigame, "connectorColors");
                    for (int cable = 0; cable < 3; cable++)
                    {
                        for (int j = 0; j < cable; j++) await Press(Key.S);
                        await Press(Key.F);
                        int connector = Array.IndexOf(connectorColors, cable);
                        for (int j = 0; j < connector; j++) await Press(Key.S);
                        await Press(Key.F);
                    }
                    break;
                case 4:
                    bool[] spam = Field<bool[]>(minigame, "isSpam");
                    for (int i = 0; i < spam.Length; i++)
                    {
                        if (!spam[i]) continue;
                        await MoveEmail((EmailCleaningMinigame)minigame, i);
                        await Press(Key.F);
                    }
                    break;
                case 5:
                    for (int n = 0; n < 3; n++)
                    {
                        SetField(minigame, "position", .5f);
                        await Press(Key.F);
                    }
                    break;
            }

            string detail = index == 4
                ? " selected=" + Field<int>(minigame, "selected") + " removed=" + Field<int>(minigame, "removedSpam") + " result=" + result
                : " result=" + result;
            Check(result == InteractionResult.Success, "No completó con éxito: " + path + detail);
            UnityEngine.Object.Destroy(instance);
            await Frames();
        }

        static async Task VerifyFailure(string path, int index)
        {
            var instance = Create(path);
            var minigame = instance.GetComponent<TimedMinigame>();
            InteractionResult? result = null;
            minigame.OnFinished += value => result = value;
            await Frames(2);

            if (index == 0)
            {
                int[] colors = Field<int[]>(minigame, "documentColors");
                for (int i = 0; i < 3; i++)
                {
                    await Press(Key.Enter);
                    int wrong = (colors[0] + 1) % 3;
                    for (int j = 0; j < wrong; j++) await Press(Key.DownArrow);
                    await Press(Key.Enter);
                }
            }
            else if (index == 2)
            {
                bool[] approvals = Field<bool[]>(minigame, "shouldApprove");
                if (approvals[0]) await Press(Key.DownArrow);
                for (int i = 0; i < 3; i++) await Press(Key.Enter);
            }
            else if (index == 3)
            {
                int[] colors = Field<int[]>(minigame, "connectorColors");
                int wrong = Array.FindIndex(colors, value => value != 0);
                for (int i = 0; i < 3; i++)
                {
                    await Press(Key.F);
                    for (int j = 0; j < wrong; j++) await Press(Key.S);
                    await Press(Key.F);
                }
            }
            else if (index == 4)
            {
                bool[] spam = Field<bool[]>(minigame, "isSpam");
                int firstImportant = Array.FindIndex(spam, value => !value);
                int secondImportant = Array.FindIndex(spam, firstImportant + 1, value => !value);
                await MoveEmail((EmailCleaningMinigame)minigame, firstImportant);
                await Press(Key.F);
                await MoveEmail((EmailCleaningMinigame)minigame, secondImportant);
                await Press(Key.F);
            }
            else if (index == 5)
            {
                for (int i = 0; i < 3; i++)
                {
                    SetField(minigame, "position", 0f);
                    await Press(Key.F);
                }
            }
            else
            {
                SetField(minigame, "remaining", .001f);
                await Frames();
            }

            Check(result == InteractionResult.Failure, "No produjo fracaso: " + path);
            UnityEngine.Object.Destroy(instance);
            await Frames();
        }

        static async Task VerifyIndependentInput()
        {
            var boss = Create(PrefabPaths[2]);
            var worker = Create(PrefabPaths[3]);
            await Frames(2);
            await Press(Key.Enter, Key.F);
            int cable = Field<int>(worker.GetComponent<WireConnectionMinigame>(), "selectedCable");
            int request = Field<int>(boss.GetComponent<ApprovalMinigame>(), "requestIndex");
            int errors = Field<int>(boss.GetComponent<ApprovalMinigame>(), "ErrorCount");
            Check(cable == 0 && (request > 0 || errors > 0), "Los controles simultáneos de ambos roles se interfirieron.");
            UnityEngine.Object.Destroy(boss);
            UnityEngine.Object.Destroy(worker);
            await Frames();
        }

        static async Task VerifyPopupsTogether()
        {
            var informe = AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/informe.asset");
            var popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/PopUp.prefab");
            Check(informe != null && popupPrefab != null, "Falta un TaskData o PopUp.prefab.");

            var bossTask = new TaskInstance(1001, 1, informe);
            var workerTask = new TaskInstance(1002, 1, informe);
            SetTaskRemaining(bossTask, 10);
            SetTaskRemaining(workerTask, 10);
            var spawner = UnityEngine.Object.FindAnyObjectByType<MinigameSpawner>();
            Check(spawner != null, "No se encontró MinigameSpawner en Main.");
            spawner.HandleInteractionStarted(bossTask, InteractionKind.Assignment);
            spawner.HandleInteractionStarted(workerTask, InteractionKind.Execution);
            var spawnedPopups = UnityEngine.Object.FindObjectsByType<PopUpController>();
            Check(spawnedPopups.Length == 2, "MinigameSpawner no mantuvo un popup por jugador.");
            var resolved = typeof(MinigameSpawner).GetMethod("HandleInteractionResolved", BindingFlags.Instance | BindingFlags.NonPublic);
            resolved.Invoke(spawner, new object[] { bossTask, InteractionResult.Success });
            resolved.Invoke(spawner, new object[] { workerTask, InteractionResult.Success });
            await Frames();

            int bossResult = -1, workerResult = -1;
            var bossObject = UnityEngine.Object.Instantiate(popupPrefab, canvas.transform, false);
            var workerObject = UnityEngine.Object.Instantiate(popupPrefab, canvas.transform, false);
            var bossPopup = bossObject.GetComponent<PopUpController>();
            var workerPopup = workerObject.GetComponent<PopUpController>();
            bossPopup.Setup(bossTask, InteractionKind.Assignment, informe.GetMinigamePrefab(InteractionKind.Assignment), value => bossResult = (int)value);
            workerPopup.Setup(workerTask, InteractionKind.Execution, informe.GetMinigamePrefab(InteractionKind.Execution), value => workerResult = (int)value);
            Check(bossObject.GetComponentInChildren<ApprovalMinigame>(true) != null && workerObject.GetComponentInChildren<CopierTimingMinigame>(true) != null,
                "Cada rol no recibió su prefab específico dentro del popup.");
            Check(((RectTransform)bossObject.transform).anchorMin.x == .5f && ((RectTransform)workerObject.transform).anchorMax.x == .5f,
                "Los popups concurrentes no respetan su mitad de pantalla.");
            await Frames(2);
            Capture("six-minigames-both-roles");

            var approval = bossObject.GetComponentInChildren<ApprovalMinigame>(true);
            bool[] decisions = Field<bool[]>(approval, "shouldApprove");
            for (int i = 0; i < 5; i++)
            {
                if (!decisions[i]) await Press(Key.DownArrow);
                await Press(Key.Enter);
            }

            var copier = workerObject.GetComponentInChildren<CopierTimingMinigame>(true);
            for (int i = 0; i < 3; i++)
            {
                SetField(copier, "position", .5f);
                await Press(Key.F);
            }
            Check(bossResult == (int)InteractionResult.Success && workerResult == (int)InteractionResult.Success,
                "Los resultados simultáneos no llegaron a los callbacks de sus popups.");
            await Frames(3);
            Check(bossObject == null && workerObject == null,
                "Alguno de los popups no cerró al resolver el minijuego.");
        }

        static void Capture(string name)
        {
            var type = Type.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands, Unity.Pipeline.Editor");
            if (type == null) return;
            var result = type.GetMethod("CaptureGameView", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { Screen.width, Screen.height, null, null, false, 0, "screen" });
            string base64 = (string)result.GetType().GetProperty("Base64").GetValue(result);
            string directory = Path.Combine(Application.dataPath, "../Docs/UI-Verification");
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, name + "-" + Screen.width + "x" + Screen.height + ".png"), Convert.FromBase64String(base64));
        }

        static async Task VerifyTaskFlow(GameManager game)
        {
            var data = AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/informe.asset");
            var catalog = Field<List<TaskData>>(game.Tasks, "catalog");
            var savedCatalog = new List<TaskData>(catalog);
            int savedTaskCount = Field<int>(game.Tasks, "tasksPerDay");
            try
            {
                catalog.Clear();
                catalog.Add(data);
                SetField(game.Tasks, "tasksPerDay", 1);
                game.Tasks.StartDay();
                Check(game.Tasks.Assign(0, TaskStrategy.Normal), "TaskManager no asignó la tarea de prueba.");
                var task = game.Tasks.Assigning;
                await Frames(2);
                var bossPopup = UnityEngine.Object.FindAnyObjectByType<PopUpController>();
                Check(bossPopup != null && bossPopup.GetComponentInChildren<ApprovalMinigame>(true) != null,
                    "La asignación real no abrió MG_BOSS_03 dentro del popup.");
                FinishMinigame(bossPopup.GetComponentInChildren<ApprovalMinigame>(true), InteractionResult.Success);
                await Frames(3);
                Check(game.Tasks.Pending == task && UnityEngine.Object.FindAnyObjectByType<PopUpController>() == null,
                    "TaskManager no recibió el éxito de asignación del jefe.");

                game.Tasks.Accept();
                await Frames(2);
                var workerPopup = UnityEngine.Object.FindAnyObjectByType<PopUpController>();
                Check(workerPopup != null && workerPopup.GetComponentInChildren<CopierTimingMinigame>(true) != null,
                    "La aceptación real no abrió MG_WORKER_03 dentro del popup.");
                FinishMinigame(workerPopup.GetComponentInChildren<CopierTimingMinigame>(true), InteractionResult.Success);
                await Frames(3);
                Check(task.Status == TaskStatus.Completed && game.Days.QuotaProgress > 0 &&
                    UnityEngine.Object.FindAnyObjectByType<PopUpController>() == null,
                    "TaskManager no recibió el éxito del trabajador o dejó el popup abierto.");
            }
            finally
            {
                catalog.Clear();
                catalog.AddRange(savedCatalog);
                SetField(game.Tasks, "tasksPerDay", savedTaskCount);
                game.Tasks.EndDay();
            }
        }

        static GameObject Create(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Check(prefab != null, "Prefab no encontrado: " + path);
            return UnityEngine.Object.Instantiate(prefab, canvas.transform, false);
        }

        static async Task MoveEmail(EmailCleaningMinigame game, int target)
        {
            int current = Field<int>(game, "selected");
            int currentRow = current / 3, targetRow = target / 3;
            if (currentRow != targetRow) await Press(targetRow > currentRow ? Key.S : Key.W);
            int currentColumn = current % 3, targetColumn = target % 3;
            int right = (targetColumn - currentColumn + 3) % 3;
            int left = (currentColumn - targetColumn + 3) % 3;
            Key key = right <= left ? Key.D : Key.A;
            for (int i = 0; i < Mathf.Min(right, left); i++) await Press(key);
        }

        static async Task Press(params Key[] keys)
        {
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            await Frames(2);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            await Frames(2);
        }

        static async Task Frames(int count = 1)
        {
            int target = Time.frameCount + count;
            double deadline = EditorApplication.timeSinceStartup + 5;
            while (Time.frameCount < target)
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
                    throw new Exception("El player loop se detuvo durante la validación.");
                await Task.Delay(15);
            }
        }

        static T Field<T>(object target, string name)
        {
            if (name == "ErrorCount")
                return (T)typeof(TimedMinigame).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?? typeof(TimedMinigame).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)field.GetValue(target);
        }

        static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? typeof(TimedMinigame).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        static void SetTaskRemaining(TaskInstance task, float value)
        {
            typeof(TaskInstance).GetProperty("Remaining").GetSetMethod(true).Invoke(task, new object[] { value });
        }

        static void FinishMinigame(TimedMinigame minigame, InteractionResult result)
        {
            typeof(TimedMinigame).GetMethod("Finish", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(minigame, new object[] { result });
        }

        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
