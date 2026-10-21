using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_03.prefab",
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_04.prefab",
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_05.prefab",
            "Assets/_Game/Prefabs/Minigames/Boss/MG_BOSS_06.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_04.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_05.prefab",
            "Assets/_Game/Prefabs/Minigames/Worker/MG_WORKER_06.prefab"
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
                string[] names = { "MG_BOSS_01", "MG_BOSS_02", "MG_BOSS_03", "MG_WORKER_01", "MG_WORKER_02", "MG_WORKER_03",
                    "MG_BOSS_04", "MG_BOSS_05", "MG_BOSS_06", "MG_WORKER_04", "MG_WORKER_05", "MG_WORKER_06" };
                for (int i = 0; i < PrefabPaths.Length; i++)
                {
                    for (int level = 1; level <= 3; level++)
                        await VerifySuccess(PrefabPaths[i], i, level);
                    await VerifyFailure(PrefabPaths[i], i);
                }
                await VerifyTaskFlow(gameManager);

                string report = "CAUDAL_SIX_MINIGAMES_PASSED (" + Screen.width + "x" + Screen.height + "):\n" +
                    string.Join("\n", names) +
                    "\nÉxito en las tres dificultades y fracaso por mecánica. Arrastre con raycast y seguimiento del cursor.\nControles: mouse y clic.\n" +
                    "TaskManager abrió tareas de ambos roles y la fotocopiadora se completó con clics desde el popup.";
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

        static async Task VerifySuccess(string path, int index, int level)
        {
            var instance = Create(path, level);
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
                    for (int n = 0; n < Field<int>(minigame, "activeDocumentCount"); n++)
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
                    for (int n = 0; n < Field<int>(minigame, "requestCount"); n++)
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
                    for (int i = 0; i < Field<int>(minigame, "cardCount"); i++)
                    {
                        if (!spam[i]) continue;
                        await Click(minigame, emailCards[i]);
                    }
                    break;
                case 5:
                    RectTransform targetZone = Field<RectTransform>(minigame, "targetZone");
                    for (int n = 0; n < Field<int>(minigame, "pagesRequired"); n++)
                    {
                        SetField(minigame, "position", .52f);
                        await ClickAt(minigame, targetZone);
                    }
                    break;
                case 6:
                    Button[] operationButtons = Field<Button[]>(minigame, "operationButtons");
                    await Click(minigame, operationButtons[Field<int>(minigame, "incorrectIndex")]);
                    break;
                case 7:
                    RectTransform stamp = Field<RectTransform>(minigame, "stamp");
                    Image[] zones = Field<Image[]>(minigame, "validZones");
                    // Any contract can be sealed first, and reopening resets its sprite and target.
                    int contractCount = Field<int>(minigame, "contractCount");
                    await Drag(minigame, stamp, zones[Mathf.Min(2, contractCount - 1)].rectTransform);
                    instance.SetActive(false); instance.SetActive(true);
                    Check(Field<int>(minigame, "sealedCount") == 0, "Contracts reset on reopen");
                    for (int i = contractCount - 1; i >= 0; i--) await Drag(minigame, stamp, zones[i].rectTransform);
                    break;
                case 8:
                    RectTransform[] employees = Field<RectTransform[]>(minigame, "employees");
                    RectTransform[] breaks = Field<RectTransform[]>(minigame, "slots");
                    int[] destinations = { 0, 2, 4, 1 };
                    await Drag(minigame, employees[0], breaks[0]);
                    var originalParent = employees[0].parent;
                    employees[0].GetComponent<MinigameDragHandle>().OnBeginDrag(new PointerEventData(EventSystem.current));
                    Check(employees[0].parent == originalParent && Field<int>(minigame, "assignedCount") == 1, "Assigned employee cannot be counted twice");
                    instance.SetActive(false); instance.SetActive(true);
                    Check(Field<int>(minigame, "assignedCount") == 0 && employees.All(e => e.parent == minigame.transform), "Employees reset on reopen");
                    for (int i = 0; i < Field<int>(minigame, "employeeCount"); i++) await Drag(minigame, employees[i], breaks[destinations[i]]);
                    break;
                case 9:
                    SetField(minigame, "acceptingInput", true);
                    Button[] colors = Field<Button[]>(minigame, "colorButtons");
                    foreach (var button in colors) button.interactable = true;
                    foreach (int color in Field<int[]>(minigame, "sequence")) await Click(minigame, colors[color]);
                    break;
                case 10:
                    RectTransform[] items = Field<RectTransform[]>(minigame, "items");
                    RectTransform[] categories = Field<RectTransform[]>(minigame, "categories");
                    int[] correct = Field<int[]>(minigame, "correctCategories");
                    await Drag(minigame, items[0], categories[correct[0]]);
                    Check(items[0].parent == categories[correct[0]], "Sorted object snaps into its category");
                    instance.SetActive(false); instance.SetActive(true);
                    Check(Field<int>(minigame, "sortedCount") == 0 && items.All(item => item.parent == minigame.transform), "Desk resets on reopen");
                    for (int i = 0; i < Field<int>(minigame, "itemCount"); i++) await Drag(minigame, items[i], categories[correct[i]]);
                    break;
                case 11:
                    Button[] closeButtons = Field<Button[]>(minigame, "closeButtons");
                    bool[] shownImportant = Field<bool[]>(minigame, "shownImportant");
                    for (int i = 0; i < shownImportant.Length; i++)
                        if (!shownImportant[i]) await Click(minigame, closeButtons[i]);
                    break;
            }

            string detail = index == 4
                ? " selected=" + Field<int>(minigame, "selected") + " removed=" + Field<int>(minigame, "removedSpam") + " result=" + result
                : index == 2
                    ? " request=" + Field<int>(minigame, "requestIndex") + " errors=" + Field<int>(minigame, "ErrorCount") + " decisions=" + string.Join(",", Field<bool[]>(minigame, "shouldApprove")) + " result=" + result
                    : index == 7
                        ? " count=" + Field<int>(minigame, "contractCount") + " cards=" + Field<Image[]>(minigame, "contracts").Length + " zones=" + Field<Image[]>(minigame, "validZones").Length + " sealed=" + Field<int>(minigame, "sealedCount") + " result=" + result
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
                int visibleCards = Field<int>(minigame, "cardCount");
                int firstImportant = Array.FindIndex(spam, 0, visibleCards, value => !value);
                int secondImportant = Array.FindIndex(spam, firstImportant + 1, visibleCards - firstImportant - 1, value => !value);
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
            else if (index >= 6)
            {
                SetField(minigame, "remaining", .001f);
                await Frames();
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
            TaskData copierData = null;
            foreach (string guid in AssetDatabase.FindAssets("t:TaskData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<TaskData>(path);
                var prefab = candidate != null ? candidate.GetMinigamePrefab(CharacterRole.Worker) : null;
                if (prefab != null && prefab.name == "MG_WORKER_03")
                {
                    copierData = candidate;
                    break;
                }
            }
            List<TaskData> savedCatalog = null;
            try
            {
                Check(bossData != null && workerData != null && copierData != null, "Faltan tareas de prueba de ambos roles o de la fotocopiadora.");
                var tasks = game.Tasks;
                var originalCatalog = (List<TaskData>)typeof(TaskManager).GetField("catalog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tasks);
                savedCatalog = new List<TaskData>(originalCatalog);
                foreach (TaskData data in new[] { bossData, workerData, copierData })
                {
                    var role = data.owner;
                    game.Restart();
                    SetField(tasks, "catalog", BuildBalancedCatalog(savedCatalog, data));
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
                    if (minigame is CopierTimingMinigame)
                    {
                        var targetZone = Field<RectTransform>(minigame, "targetZone");
                        for (int page = 0; page < Field<int>(minigame, "pagesRequired"); page++)
                        {
                            SetField(minigame, "position", .52f);
                            await ClickAt(minigame, targetZone);
                        }
                    }
                    else
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
                if (savedCatalog != null)
                    SetField(game.Tasks, "catalog", savedCatalog);
                game.Tasks.EndDay();
            }
        }

        static List<TaskData> BuildBalancedCatalog(List<TaskData> catalog, TaskData requested)
        {
            var candidates = catalog.Where(data => data != null && data.owner == requested.owner &&
                data.GetMinigamePrefab(requested.owner) != null).GroupBy(data => data.GetMinigamePrefab(requested.owner))
                .Select(group => group.First()).ToList();
            int productivityNeeded = requested.owner == CharacterRole.Boss ? 2 : 1;
            int happinessNeeded = 3 - productivityNeeded;
            var chosen = new List<TaskData> { requested };
            foreach (TaskStat stat in new[] { TaskStat.Productivity, TaskStat.Happiness })
            {
                int needed = stat == TaskStat.Productivity ? productivityNeeded : happinessNeeded;
                int have = chosen.Count(data => data.affectedStat == stat);
                foreach (var candidate in candidates)
                {
                    if (have >= needed) break;
                    if (candidate.affectedStat != stat || chosen.Contains(candidate)) continue;
                    chosen.Add(candidate);
                    have++;
                }
                Check(have == needed, "No se pudo preparar catálogo equilibrado para " + requested.taskId);
            }
            return chosen;
        }

        static GameObject Create(string path, int level = 1)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Check(prefab != null, "Prefab no encontrado: " + path);
            var staging = new GameObject("Validation staging", typeof(RectTransform));
            staging.SetActive(false);
            var instance = UnityEngine.Object.Instantiate(prefab, staging.transform, false);
            instance.GetComponent<TimedMinigame>().SetWaveDifficulty(level);
            instance.transform.SetParent(canvas.transform, false);
            UnityEngine.Object.Destroy(staging);
            return instance;
        }

        static async Task Click(TimedMinigame minigame, Graphic target)
        {
            await ClickAt(minigame, target.rectTransform);
        }

        static async Task Click(TimedMinigame minigame, Button target)
        {
            Check(target != null && target.targetGraphic != null, "El botón no tiene Graphic objetivo.");
            await ClickAt(minigame, target.targetGraphic.rectTransform);
        }

        static async Task ClickAt(TimedMinigame minigame, RectTransform target)
        {
            var eventSystem = EventSystem.current;
            Check(eventSystem != null, "No hay EventSystem para probar el clic de " + minigame.GetType().Name + ".");
            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Camera eventCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? targetCanvas.worldCamera
                : null;
            var eventData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(eventCamera, target.position)
            };
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(eventData, hits);
            RaycastResult hit = default;
            bool foundHit = false;
            foreach (var candidate in hits)
            {
                if (candidate.gameObject.transform == minigame.transform ||
                    candidate.gameObject.transform.IsChildOf(minigame.transform))
                {
                    hit = candidate;
                    foundHit = true;
                    break;
                }
            }
            Check(foundHit, "El mouse no alcanza " + target.name + " en " + minigame.GetType().Name + ".");
            eventData.pointerPressRaycast = hit;
            eventData.pointerCurrentRaycast = eventData.pointerPressRaycast;
            ExecuteEvents.ExecuteHierarchy(hit.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            await Frames(1);
        }

        static async Task Drag(TimedMinigame minigame, RectTransform source, RectTransform target)
        {
            var eventSystem = EventSystem.current;
            Check(eventSystem != null, "No hay EventSystem para arrastrar en " + minigame.GetType().Name + ".");
            var data = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                pointerDrag = source.gameObject,
                position = RectTransformUtility.WorldToScreenPoint(null, source.position)
            };
            data.pressPosition = data.position;
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(data, hits);
            Check(hits.Count > 0 && (hits[0].gameObject.transform == source || hits[0].gameObject.transform.IsChildOf(source)),
                "El arrastre no alcanza el objeto visible: " + source.name);
            ExecuteEvents.Execute(source.gameObject, data, ExecuteEvents.beginDragHandler);
            data.position = RectTransformUtility.WorldToScreenPoint(null, target.position);
            ExecuteEvents.Execute(source.gameObject, data, ExecuteEvents.dragHandler);
            Check(Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, source.position), data.position) < 2,
                "El objeto arrastrado se separa del cursor: " + source.name);
            ExecuteEvents.Execute(source.gameObject, data, ExecuteEvents.endDragHandler);
            await Frames();
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
