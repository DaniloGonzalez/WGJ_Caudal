using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Scripting.LifecycleManagement;

namespace Caudal.Editor
{
    [InitializeOnLoad, NoAutoStaticsCleanup]
    public static class SixMinigamesValidation
    {
        const string PendingKey = "Caudal.SixMinigames.Pending";
        const string ResultKey = "Caudal.SixMinigames.Result";
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
                string[] names = { "MG_BOSS_01", "MG_BOSS_02", "MG_BOSS_03", "MG_WORKER_01", "MG_WORKER_02", "MG_WORKER_03" };
                for (int i = 0; i < PrefabPaths.Length; i++)
                {
                    await VerifySuccess(PrefabPaths[i], i);
                    await VerifyFailure(PrefabPaths[i], i);
                }
                await VerifyTaskFlow(gameManager);

                string report = "CAUDAL_SIX_MINIGAMES_PASSED (" + Screen.width + "x" + Screen.height + "):\n" +
                    string.Join("\n", names) +
                    "\nÉxito y fracaso comprobados por mecánica.\nControles: mouse y clic.\n" +
                    "TaskManager abrió y resolvió una tarea propia por rol en secuencia.";
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
                if (gameManager != null) gameManager.enabled = gameManagerWasEnabled;
                gameManager = null;
                Application.runInBackground = oldBackground;
                EditorApplication.isPlaying = false;
            }
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
                    Image[] documents = Field<Image[]>(minigame, "documents");
                    Image[] trays = Field<Image[]>(minigame, "trays");
                    for (int n = 0; n < 3; n++)
                    {
                        await Click(minigame, documents[n]);
                        await Click(minigame, trays[documentColors[n]]);
                    }
                    break;
                case 1:
                    int[] priorities = Field<int[]>(minigame, "priorities");
                    Image[] cards = Field<Image[]>(minigame, "cards");
                    Text[] slots = Field<Text[]>(minigame, "slots");
                    for (int slot = 0; slot < 3; slot++)
                    {
                        int card = Array.IndexOf(priorities, slot);
                        await Click(minigame, cards[card]);
                        await Click(minigame, slots[slot].transform.parent.GetComponent<Image>());
                    }
                    break;
                case 2:
                    bool[] approvals = Field<bool[]>(minigame, "shouldApprove");
                    Image approve = Field<Image>(minigame, "approveCard");
                    Image reject = Field<Image>(minigame, "rejectCard");
                    for (int n = 0; n < 5; n++)
                    {
                        await Click(minigame, approvals[n] ? approve : reject);
                    }
                    break;
                case 3:
                    int[] connectorColors = Field<int[]>(minigame, "connectorColors");
                    Image[] cables = Field<Image[]>(minigame, "cables");
                    Image[] connectors = Field<Image[]>(minigame, "connectors");
                    for (int cable = 0; cable < 3; cable++)
                    {
                        await Click(minigame, cables[cable]);
                        int connector = Array.IndexOf(connectorColors, cable);
                        await Click(minigame, connectors[connector]);
                    }
                    break;
                case 4:
                    bool[] spam = Field<bool[]>(minigame, "isSpam");
                    Image[] emailCards = Field<Image[]>(minigame, "cards");
                    for (int i = 0; i < spam.Length; i++)
                    {
                        if (!spam[i]) continue;
                        await Click(minigame, emailCards[i]);
                    }
                    break;
                case 5:
                    for (int n = 0; n < 3; n++)
                    {
                        SetField(minigame, "position", .5f);
                        await ClickAt(minigame, Field<RectTransform>(minigame, "track"));
                    }
                    break;
            }

            string detail = index == 4
                ? " selected=" + Field<int>(minigame, "selected") + " removed=" + Field<int>(minigame, "removedSpam") + " result=" + result
                : index == 2
                    ? " request=" + Field<int>(minigame, "requestIndex") + " errors=" + Field<int>(minigame, "ErrorCount") + " decisions=" + string.Join(",", Field<bool[]>(minigame, "shouldApprove")) + " result=" + result
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
                    Image[] documents = Field<Image[]>(minigame, "documents");
                    Image[] trays = Field<Image[]>(minigame, "trays");
                    await Click(minigame, documents[0]);
                    int wrong = (colors[0] + 1) % 3;
                    for (int j = 0; j < 3; j++) await Click(minigame, trays[wrong]);
                }
            }
            else if (index == 2)
            {
                    bool[] approvals = Field<bool[]>(minigame, "shouldApprove");
                    Image approve = Field<Image>(minigame, "approveCard");
                    Image reject = Field<Image>(minigame, "rejectCard");
                for (int i = 0; i < 3; i++)
                {
                        await Click(minigame, approvals[i] ? reject : approve);
                }
            }
            else if (index == 3)
            {
                    int[] colors = Field<int[]>(minigame, "connectorColors");
                    Image[] cables = Field<Image[]>(minigame, "cables");
                    Image[] connectors = Field<Image[]>(minigame, "connectors");
                    await Click(minigame, cables[0]);
                    int wrong = Array.FindIndex(colors, value => value != 0);
                    for (int i = 0; i < 3; i++) await Click(minigame, connectors[wrong]);
            }
            else if (index == 4)
            {
                bool[] spam = Field<bool[]>(minigame, "isSpam");
                int firstImportant = Array.FindIndex(spam, value => !value);
                int secondImportant = Array.FindIndex(spam, firstImportant + 1, value => !value);
                    Image[] cards = Field<Image[]>(minigame, "cards");
                    await Click(minigame, cards[firstImportant]);
                    await Click(minigame, cards[secondImportant]);
            }
            else if (index == 5)
            {
                for (int i = 0; i < 3; i++)
                {
                    SetField(minigame, "position", 0f);
                    await ClickAt(minigame, Field<RectTransform>(minigame, "track"));
                }
            }
            else
            {
                SetField(minigame, "remaining", .001f);
                await Frames();
            }

            string failureDetail = index == 2
                ? " request=" + Field<int>(minigame, "requestIndex") + " errors=" + Field<int>(minigame, "ErrorCount") + " result=" + result
                : " result=" + result;
            Check(result == InteractionResult.Failure, "No produjo fracaso: " + path + failureDetail);
            UnityEngine.Object.Destroy(instance);
            await Frames();
        }

        static async Task VerifyTaskFlow(GameManager game)
        {
            var bossData = AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/informe.asset");
            var workerData = AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/correo.asset");
            try
            {
                Check(bossData != null && workerData != null, "Faltan tareas de prueba de ambos roles.");
                foreach (TaskData data in new[] { bossData, workerData })
                {
                    var role = data.owner;
                    game.Restart();
                    game.ConfirmCharacter(role == CharacterRole.Boss);
                    Check(game.State == GameState.Playing, "La perspectiva diaria no inició para " + role + ".");
                    int taskIndex = -1;
                    for (int i = 0; i < game.Tasks.Tasks.Count; i++)
                        if (game.Tasks.Tasks[i].Data == data) { taskIndex = i; break; }
                    Check(taskIndex >= 0 && game.Tasks.Tasks.Count == 3, "La tarea de prueba forma parte de su oleada para " + role + ".");
                    var task = game.Tasks.Tasks[taskIndex];
                    float happiness = game.Stats.Happiness;
                    float productivity = game.Stats.Productivity;
                    Check(game.Tasks.StartOwnTask(taskIndex), "Se puede iniciar una tarea de " + role + ".");
                    await Frames(2);
                    var popup = UnityEngine.Object.FindAnyObjectByType<PopUpController>();
                    var minigame = popup != null ? popup.GetComponentInChildren<TimedMinigame>(true) : null;
                    Check(minigame != null, "TaskManager no abrió el minijuego de " + role + " dentro del popup.");
                    FinishMinigame(minigame, InteractionResult.Success);
                    await Frames(3);
                    float expectedHappiness = happiness + (data.affectedStat == TaskStat.Happiness ? 5 : 0);
                    float expectedProductivity = productivity + (data.affectedStat == TaskStat.Productivity ? 5 : 0);
                    Check(task.Status == TaskStatus.Completed && Mathf.Approximately(game.Stats.Happiness, expectedHappiness) &&
                        Mathf.Approximately(game.Stats.Productivity, expectedProductivity) && game.Days.QuotaProgress == 0 &&
                        UnityEngine.Object.FindAnyObjectByType<PopUpController>() == null,
                        "El resultado de " + role + " no resolvió la tarea o dejó abierto el popup.");
                    game.Tasks.EndWaveDay();
                }
            }
            finally
            {
                game.Tasks.EndDay();
            }
        }

        static GameObject Create(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Check(prefab != null, "Prefab no encontrado: " + path);
            return UnityEngine.Object.Instantiate(prefab, canvas.transform, false);
        }

        static async Task Click(TimedMinigame minigame, Graphic target)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, target.rectTransform.position)
            };
            eventData.pointerPressRaycast = new RaycastResult { gameObject = target.gameObject };
            eventData.pointerCurrentRaycast = eventData.pointerPressRaycast;
            ExecuteEvents.Execute<IPointerClickHandler>(minigame.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            await Frames(1);
        }

        static async Task ClickAt(TimedMinigame minigame, RectTransform target)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, target.position)
            };
            eventData.pointerPressRaycast = new RaycastResult { gameObject = target.gameObject };
            eventData.pointerCurrentRaycast = eventData.pointerPressRaycast;
            ExecuteEvents.Execute<IPointerClickHandler>(minigame.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            await Frames(1);
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
