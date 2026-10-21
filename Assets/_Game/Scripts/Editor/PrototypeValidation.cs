using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Caudal.Editor
{
    public static class PrototypeValidation
    {
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        static void Call(object target, string method)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }

        static void SetField(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        static int FindTask(TaskManager tasks, TaskCategory category)
        {
            for (int i = 0; i < tasks.Tasks.Count; i++)
                if (tasks.Tasks[i].Data.category == category) return i;
            return -1;
        }

        [MenuItem("Caudal/Validar oleadas sincronizadas")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/_Game/Scenes/Main.unity");
            try
            {
                var root = Array.Find(scene.GetRootGameObjects(), go => go.GetComponent<GameManager>() != null);
                Check(root != null, "GameManager root");
                var game = root.GetComponent<GameManager>();
                var stats = root.GetComponent<StatsManager>();
                var days = root.GetComponent<DayManager>();
                var tasks = root.GetComponent<TaskManager>();
                var spawner = root.GetComponent<MinigameSpawner>();
                var results = scene.GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<ResultsUI>(true)).FirstOrDefault();
                Check(results != null, "ResultsUI is connected in Main");
                if (spawner != null) spawner.enabled = false;

                game.Initialize(stats, days, tasks, root.GetComponent<InputRouter>());
                game.Restart();
                var dayConfig = new SerializedObject(days);
                Check(Mathf.Approximately(dayConfig.FindProperty("dayDuration").floatValue, 30), "Global day lasts 30 seconds");
                Check(Mathf.Approximately((float)typeof(TaskManager).GetField("waveDuration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tasks), 15),
                    "Each wave lasts 15 seconds");

                ValidateWaveRole(game, stats, days, tasks, CharacterRole.Worker);
                game.Restart();
                ValidateWaveRole(game, stats, days, tasks, CharacterRole.Boss);

                ValidateEndings(game, stats, days, results);
                Debug.Log("CAUDAL_WAVES_AND_ENDINGS_PASSED: waves and all four Friday endings verified at the 50 threshold.");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        static void ValidateWaveRole(GameManager game, StatsManager stats, DayManager days, TaskManager tasks,
            CharacterRole role)
        {
            game.ConfirmCharacter(role == CharacterRole.Boss);
            Check(game.State == GameState.Playing && game.ActiveRole == role && days.CurrentDay == 1,
                role + " begins its selected day");
            Check(tasks.Tasks.Count == 3 && tasks.WaveNumber == 1 && Mathf.Approximately(tasks.WaveTimeRemaining, 15),
                role + " starts with one three-task wave");
            Check(tasks.BacklogCount(CharacterRole.Boss) == 0 && tasks.BacklogCount(CharacterRole.Worker) == 0,
                "Wave loop has no cross-day backlog");

            var minigames = new HashSet<GameObject>();
            var firstWaveTasks = new HashSet<TaskData>();
            int productivityCount = 0;
            int happinessCount = 0;
            for (int i = 0; i < 3; i++)
            {
                var task = tasks.Tasks[i];
                Check(task.Owner == role, role + " only receives its own tasks");
                firstWaveTasks.Add(task.Data);
                if (task.Data.affectedStat == TaskStat.Productivity) productivityCount++;
                else happinessCount++;
                Check(task.Data.GetMinigamePrefab(role) != null && minigames.Add(task.Data.GetMinigamePrefab(role)),
                    role + " has one distinct existing minigame per task");
            }
            Check(productivityCount == (role == CharacterRole.Boss ? 2 : 1) && happinessCount == (role == CharacterRole.Boss ? 1 : 2),
                role + " wave has the required stat balance");

            float happiness = stats.Happiness;
            float productivity = stats.Productivity;
            var successTask = tasks.Tasks[0];
            Check(tasks.StartOwnTask(0) && tasks.Resolve(successTask.Id, successTask.Day,
                InteractionKind.Execution, InteractionResult.Success), "Minigame success resolves once");
            CheckStatChange(stats, successTask.Data.affectedStat, 5, happiness, productivity, "Success gives +5 only to its assigned stat");
            float successHappiness = stats.Happiness, successProductivity = stats.Productivity;
            Check(!tasks.Resolve(successTask.Id, successTask.Day, InteractionKind.Execution, InteractionResult.Success) &&
                stats.Happiness == successHappiness && stats.Productivity == successProductivity, "Duplicate result is ignored");

            var failureTask = tasks.Tasks[1];
            happiness = stats.Happiness;
            productivity = stats.Productivity;
            Check(tasks.StartOwnTask(1) && tasks.Resolve(failureTask.Id, failureTask.Day,
                InteractionKind.Execution, InteractionResult.Failure), "Minigame failure resolves");
            CheckStatChange(stats, failureTask.Data.affectedStat, -5, happiness, productivity, "Failure gives -5 only to its assigned stat");

            var rejectedTask = tasks.Tasks[2];
            happiness = stats.Happiness;
            productivity = stats.Productivity;
            Check(tasks.RejectOwnTask(2), "Task can be rejected");
            CheckStatChange(stats, rejectedTask.Data.affectedStat, -5, happiness, productivity, "Rejection gives -5 only to its assigned stat");
            Check(tasks.CompletedThisDay == 1 && tasks.FailedThisDay == 1 && tasks.RejectedThisDay == 1 &&
                tasks.WaveTransitionPending && days.QuotaProgress == 0, "Resolved wave advances without quota");

            Check(tasks.BeginNextWave() && tasks.WaveNumber == 2 && tasks.Tasks[0].Difficulty == 2 &&
                Mathf.Approximately(tasks.WaveTimeRemaining, 15), "Second wave uses medium difficulty");
            foreach (var task in tasks.Tasks)
                Check(!firstWaveTasks.Contains(task.Data), role + " avoids repeating previous-wave tasks when alternatives exist");
            tasks.FinishWaveTransition();
            for (int i = 0; i < 3; i++) Check(tasks.RejectOwnTask(i), "Wave two tasks resolve");
            Check(tasks.BeginNextWave() && tasks.WaveNumber == 3 && tasks.Tasks[0].Difficulty == 3,
                "Third wave and later use hard difficulty");
            tasks.FinishWaveTransition();

            Check(tasks.StartOwnTask(0), "Can start task before wave timeout");
            SetField(tasks, "waveTimeRemaining", .01f);
            tasks.Tick(.02f);
            Check(tasks.ExpiredThisDay == 2 && tasks.HasActiveMinigame &&
                !tasks.WaveTransitionPending, "Wave timeout preserves the opened minigame");
            var finishingTask = tasks.Active;
            happiness = stats.Happiness;
            productivity = stats.Productivity;
            tasks.Tick(20);
            Check(tasks.Resolve(finishingTask.Id, finishingTask.Day, InteractionKind.Execution, InteractionResult.Success),
                "Opened minigame can finish after wave timeout");
            CheckStatChange(stats, finishingTask.Data.affectedStat, 5, happiness, productivity, "Late wave success rewards once");
            Check(!tasks.Resolve(finishingTask.Id, finishingTask.Day, InteractionKind.Execution, InteractionResult.Success) &&
                tasks.WaveTransitionPending, "Duplicate result ignored before next wave");

            Check(tasks.BeginNextWave(), "Unlimited waves can continue");
            tasks.FinishWaveTransition();
            var activeTask = tasks.Tasks[0];
            Check(tasks.StartOwnTask(0), "Can run a minigame near global day end");
            tasks.EndWaveDay();
            Check(tasks.DayEnding && tasks.HasActiveMinigame && !tasks.WaveTransitionPending,
                "Global day end waits for the active minigame");
            happiness = stats.Happiness;
            productivity = stats.Productivity;
            Check(tasks.Resolve(activeTask.Id, activeTask.Day, InteractionKind.Execution, InteractionResult.Success),
                "Active minigame can finish after global time reaches zero");
            CheckStatChange(stats, activeTask.Data.affectedStat, 5, happiness, productivity,
                "Deferred active result is registered once");
            Check(tasks.DayEnding && !tasks.HasActiveMinigame && !tasks.WaveTransitionPending,
                "Day closes without creating another wave");
            Call(game, "CloseDay");
            Check(game.State == GameState.DaySummary && game.ResultText.Contains("Oleadas:") &&
                !game.ResultText.Contains("Cuota"), "Summary reports waves and no quota dependency");
        }

        static void CheckStatChange(StatsManager stats, TaskStat stat, float amount,
            float previousHappiness, float previousProductivity, string message)
        {
            float expectedHappiness = previousHappiness + (stat == TaskStat.Happiness ? amount : 0);
            float expectedProductivity = previousProductivity + (stat == TaskStat.Productivity ? amount : 0);
            Check(Mathf.Approximately(stats.Happiness, Mathf.Clamp(expectedHappiness, 0, 100)) &&
                Mathf.Approximately(stats.Productivity, Mathf.Clamp(expectedProductivity, 0, 100)), message);
        }

        static void RunLegacy()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/_Game/Scenes/Main.unity");
            try
            {
                var root = Array.Find(scene.GetRootGameObjects(), go => go.GetComponent<GameManager>() != null);
                Check(root != null, "GameManager root");
                var game = root.GetComponent<GameManager>();
                var stats = root.GetComponent<StatsManager>();
                var days = root.GetComponent<DayManager>();
                var tasks = root.GetComponent<TaskManager>();
                var input = root.GetComponent<InputRouter>();
                var bootstrap = new SerializedObject(root.GetComponent<GameBootstrap>());
                foreach (var field in new[] { "sceneCamera", "sceneCanvas", "screen", "intro", "gameManager", "statsManager", "dayManager", "taskManager", "inputRouter" })
                    Check(bootstrap.FindProperty(field).objectReferenceValue != null, "Missing scene reference: " + field);
                Check(input.worker.confirm == input.boss.confirm && input.worker.up == input.boss.up && input.worker.down == input.boss.down,
                    "Roles share the current arrow and Enter controls");

                var serialized = new SerializedObject(game);
                foreach (var field in new[] { "workerCharacters", "bossCharacters" })
                {
                    var profiles = serialized.FindProperty(field);
                    Check(profiles.arraySize == 1, "One fixed protagonist per role");
                    for (int i = 0; i < profiles.arraySize; i++)
                    {
                        var profile = profiles.GetArrayElementAtIndex(i).objectReferenceValue as CharacterData;
                        string expectedName = field == "bossCharacters" ? "Josefa" : "Tomás";
                        Check(profile != null && profile.characterName == expectedName && profile.portrait != null &&
                            !string.IsNullOrEmpty(profile.biography), "Fixed protagonist content: " + expectedName);
                        foreach (var trigger in new[] { DialogueTrigger.TaskSuccess, DialogueTrigger.TaskFailure, DialogueTrigger.TaskRejected })
                            Check(profile.dialogueLines != null && Array.Exists(profile.dialogueLines,
                                line => line != null && line.trigger == trigger && !string.IsNullOrWhiteSpace(line.text)),
                                expectedName + " has dialogue for " + trigger);
                    }
                }

                game.Initialize(stats, days, tasks, input);
                game.Restart();
                Check(game.State == GameState.CharacterSelection && days.CurrentDay == 0, "Selection before day one");
                Call(game, "Update");
                Check(days.TimeRemaining == 0, "No clock during selection");
                game.CancelCharacter(false);
                game.ConfirmCharacter(false);
                Check(game.State == GameState.Playing && days.CurrentDay == 1 && game.ActiveRole == CharacterRole.Worker,
                    "One confirmation starts the chosen daily perspective");
                Check(tasks.Tasks.Count == 3, "Worker board respects the three-task backlog limit");
                foreach (var task in tasks.Tasks) Check(task.Owner == CharacterRole.Worker, "Worker sees only worker tasks");

                var originalCatalog = (List<TaskData>)typeof(TaskManager)
                    .GetField("catalog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tasks);
                var savedCatalog = new List<TaskData>(originalCatalog);
                SetField(tasks, "catalog", new List<TaskData>
                {
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/agenda.asset"),
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/reunion.asset"),
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/urgente.asset"),
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/inventario.asset")
                });
                game.Restart();
                game.ConfirmCharacter(true);
                Check(game.State == GameState.Playing && days.CurrentDay == 1 && game.ActiveRole == CharacterRole.Boss,
                    "Boss perspective starts with one confirmation");
                Check(tasks.Tasks.Count == 3, "Boss board respects the three-task backlog limit");
                foreach (var task in tasks.Tasks) Check(task.Owner == CharacterRole.Boss, "Boss sees only boss tasks");
                var ids = new HashSet<string>();
                foreach (var task in tasks.Tasks) Check(ids.Add(task.Data.taskId), "Duplicate task");

                int taskIndex = 0;
                var first = tasks.Tasks[taskIndex];
                Check(tasks.StartOwnTask(taskIndex), "Can start a task from the active role board");
                Check(tasks.Resolve(first.Id, first.Day, InteractionKind.Execution, InteractionResult.Success), "Successful minigame result resolves task");
                float quota = days.QuotaProgress;
                Check(first.Status == TaskStatus.Completed && quota > 0, "Success records quota once");
                Check(!tasks.Resolve(first.Id, first.Day, InteractionKind.Execution, InteractionResult.Success) && days.QuotaProgress == quota,
                    "Duplicate minigame result is ignored");

                int failedIndex = 1;
                var failed = tasks.Tasks[failedIndex];
                float happiness = stats.Happiness;
                Check(tasks.StartOwnTask(failedIndex), "Can start a second task");
                Check(tasks.Resolve(failed.Id, failed.Day, InteractionKind.Execution, InteractionResult.Failure), "Failure result resolves task");
                Check(days.QuotaProgress == quota && stats.Happiness < happiness, "Failure adds no quota and applies stress");
                Call(game, "CloseDay");
                Check(game.State == GameState.DaySummary, "Day summary follows the active day");
                Call(game, "BeginDay");
                Check(days.CurrentDay == 2 && game.State == GameState.Playing, "Next day starts cleanly");

                game.Restart();
                game.ConfirmCharacter(true);
                int necessaryIndex = FindTask(tasks, TaskCategory.Required);
                Check(necessaryIndex >= 0, "Necessary task exists for the boss");
                var necessary = tasks.Tasks[necessaryIndex];
                float happinessBeforeReject = stats.Happiness;
                Check(tasks.RejectOwnTask(necessaryIndex) && necessary.Status == TaskStatus.Available &&
                    stats.Happiness == happinessBeforeReject, "Rejecting a necessary task leaves it pending without a direct penalty");
                int overloadIndex = FindTask(tasks, TaskCategory.Overload);
                Check(overloadIndex >= 0, "Overload task exists for the boss");
                var overload = tasks.Tasks[overloadIndex];
                Check(tasks.RejectOwnTask(overloadIndex) && overload.Status == TaskStatus.Rejected &&
                    Mathf.Approximately(stats.Happiness, happinessBeforeReject + overload.Data.rejectionHappinessChange),
                    "Rejecting overload applies only its configured wellbeing effect");

                game.Restart();
                game.ConfirmCharacter(false);
                int optionalIndex = FindTask(tasks, TaskCategory.Optional);
                Check(optionalIndex >= 0, "Optional task exists for the worker");
                var optional = tasks.Tasks[optionalIndex];
                happinessBeforeReject = stats.Happiness;
                Check(tasks.RejectOwnTask(optionalIndex) && optional.Status == TaskStatus.Rejected &&
                    stats.Happiness == happinessBeforeReject, "Rejecting optional work discards it without a direct penalty");

                // Controlled catalog makes backlog aging and the one-time expiry cost deterministic.
                var controlledCatalog = new List<TaskData>
                {
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/agenda.asset"),
                    AssetDatabase.LoadAssetAtPath<TaskData>("Assets/_Game/Data/Tasks/correo.asset")
                };
                Check(controlledCatalog[0] != null && controlledCatalog[1] != null, "Required tasks for backlog test");
                SetField(tasks, "catalog", controlledCatalog);
                game.Restart();
                stats.Apply(0, -50); // Inactive role always takes its failure path.
                TaskInstance bossPending = null, workerPending = null;
                for (int day = 1; day <= 3; day++)
                {
                    if (day == 1) game.ConfirmCharacter(true);
                    else Call(game, "BeginDay");
                    bossPending = tasks.Tasks[0];
                    Check(bossPending.Owner == CharacterRole.Boss, "Active role keeps its own backlog");
                    if (day > 1) Check(bossPending.Age == day - 1, "Active task ages between days");
                    Call(game, "CloseDay");
                    workerPending = FindBoardTask(tasks, CharacterRole.Worker);
                    if (day < 3)
                    {
                        Check(bossPending.Age == day && workerPending != null && workerPending.Age == day,
                            "Both role boards age once per day");
                        Check(game.ResultText.Contains(day == 1 ? " (urgente)" : " (crítica)"),
                            "Daily summary shows task urgency");
                    }
                }
                Check(tasks.ExpiredThisDay == 2 && tasks.BacklogCount(CharacterRole.Boss) == 0 &&
                    tasks.BacklogCount(CharacterRole.Worker) == 0, "Both critical obligations expire on their third missed day");
                Check(Mathf.Approximately(stats.Happiness, 40), "Required-task expiry applies one provisional five-point cost per role");
                Check(tasks.ExpiredSummary.Contains("Josefa") && tasks.ExpiredSummary.Contains("Tomás") &&
                    game.ResultText.Contains("Incumplidas: 2"), "Summary names expired obligations and owners");
                float afterExpiry = stats.Happiness;
                tasks.EndDay();
                Check(stats.Happiness == afterExpiry && tasks.ExpiredThisDay == 2, "Expiry consequence cannot apply twice");
                SetField(tasks, "catalog", savedCatalog);

                stats.Apply(999, -999);
                Check(stats.Happiness == 100 && stats.Productivity == 0, "Stats clamp to 0–100");
                game.Restart();
                Check(days.CurrentDay == 0 && days.QuotaProgress == 0 && stats.Happiness == 50 && stats.Productivity == 50,
                    "Restart resets days, quota, and stats");

                ValidateFiveDayRoute(game, stats, days, tasks, CharacterRole.Boss, CharacterRole.Boss,
                    CharacterRole.Boss, CharacterRole.Boss, CharacterRole.Boss);
                ValidateFiveDayRoute(game, stats, days, tasks, CharacterRole.Worker, CharacterRole.Worker,
                    CharacterRole.Worker, CharacterRole.Worker, CharacterRole.Worker);
                ValidateFiveDayRoute(game, stats, days, tasks, CharacterRole.Boss, CharacterRole.Worker,
                    CharacterRole.Boss, CharacterRole.Worker, CharacterRole.Boss);
                var results = scene.GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<ResultsUI>(true)).FirstOrDefault();
                Check(results != null, "ResultsUI is connected for legacy route validation");
                ValidateEndings(game, stats, days, results);

                game.Restart();

                game.ConfirmCharacter(true);
                var canvas = (Canvas)bootstrap.FindProperty("sceneCanvas").objectReferenceValue;
                var screen = (GameScreen)bootstrap.FindProperty("screen").objectReferenceValue;
                screen.Initialize(game);
                screen.Refresh();
                var activeHud = canvas.GetComponentInChildren<TrabajadorHUD>(true);
                var selectedTask = tasks.Tasks[game.Selected];
                Check(activeHud.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                    .Any(text => text.text.Contains(selectedTask.Data.description)), "Selected task shows narrative description before acceptance");
                UIPrefabValidation.CheckScene(scene);
                Debug.Log("CAUDAL_VALIDATION_PASSED: scene references, profiles, daily role selection, task resolution, backlog aging/expiry, five-day routes for each role and alternation, four endings, quota, stats, restart and UI.");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        static TaskInstance FindBoardTask(TaskManager tasks, CharacterRole role)
        {
            var field = typeof(TaskManager).GetField("taskBoards", BindingFlags.Instance | BindingFlags.NonPublic);
            var boards = (Dictionary<CharacterRole, List<TaskInstance>>)field.GetValue(tasks);
            return boards.TryGetValue(role, out var board) ? board.FirstOrDefault(task => !task.IsFinal) : null;
        }

        static void ValidateFiveDayRoute(GameManager game, StatsManager stats, DayManager days,
            TaskManager tasks, params CharacterRole[] roles)
        {
            game.Restart();
            stats.Apply(0, -50); // Makes the inactive character's result deterministic.
            for (int i = 0; i < roles.Length; i++)
            {
                if (i == 0)
                {
                    game.ConfirmCharacter(roles[i] == CharacterRole.Boss);
                }
                else
                {
                    SetField(game, "<State>k__BackingField", GameState.CharacterSelection);
                    game.CancelCharacter(roles[i] == CharacterRole.Boss);
                    game.ConfirmCharacter(roles[i] == CharacterRole.Boss);
                }

                Check(days.CurrentDay == i + 1 && game.State == GameState.Playing && game.ActiveRole == roles[i],
                    "Daily role selection starts day " + (i + 1));
                foreach (var task in tasks.Tasks)
                    Check(task.Owner == roles[i], "Daily route shows only the selected role's board");
                Call(game, "CloseDay");
                Check(game.State == GameState.DaySummary && game.ResultText.Contains("Pendientes — Josefa:") &&
                    game.ResultText.Contains("Tomás:"), "Daily route summarizes both role boards");
            }
            Check(days.CurrentDay == 5 && days.RecordedClosingDays == 5,
                "Route reaches Friday and records one statistics sample per day");
        }

        static void ValidateEndings(GameManager game, StatsManager stats, DayManager days, ResultsUI results)
        {
            results.Initialize(game);
            var cases = new[]
            {
                (50f, 50f, "Best Place to Work", "Todo bueno.PNG"),
                (49f, 50f, "Éxito miserable", "Productividad buena.PNG"),
                (50f, 49f, "Oficina feliz en crisis", "Bienestar bueno.PNG"),
                (49f, 49f, "Colapso de oficina", "Todo malo.PNG")
            };
            foreach (var test in cases)
            {
                days.ResetDays();
                for (int day = 0; day < days.TotalDays; day++)
                {
                    days.StartDay();
                    stats.ResetStats();
                    if (day == days.TotalDays - 1)
                        stats.Apply(test.Item1 - stats.Happiness, test.Item2 - stats.Productivity);
                    days.RecordClosingStats(stats.Happiness, stats.Productivity);
                    days.RecordClosingStats(0, 0); // Duplicate close must not change the weekly average.
                }
                typeof(GameManager).GetProperty("State").GetSetMethod(true)
                    .Invoke(game, new object[] { GameState.DaySummary });
                game.ContinueFromDaySummary();
                results.Refresh(game);
                var image = (UnityEngine.UI.Image)typeof(ResultsUI)
                    .GetField("endingArtwork", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(results);
                var button = (UnityEngine.UI.Button)typeof(ResultsUI)
                    .GetField("continueButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(results);
                string spritePath = "Assets/_Game/Art/Screens/Endings/" + test.Item4;
                var expectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                Check(game.State == GameState.GameOver && game.ResultText.StartsWith(test.Item3) &&
                    game.ResultText.Contains("Valores finales del viernes") &&
                    game.ResultText.Contains("Bienestar: " + test.Item1.ToString("0")) &&
                    game.ResultText.Contains("Productividad: " + test.Item2.ToString("0")) &&
                    days.CurrentDay == days.TotalDays && image.gameObject.activeSelf && image.sprite == expectedSprite &&
                    button.interactable && days.RecordedClosingDays == days.TotalDays,
                    "Friday ending, image, values, and restart button: " + test.Item3);
                if (test.Item3 == "Colapso de oficina")
                {
                    button.onClick.Invoke();
                    Check(game.State == GameState.CharacterSelection && days.CurrentDay == 0 &&
                        Mathf.Approximately(stats.Happiness, 50) && Mathf.Approximately(stats.Productivity, 50),
                        "Restart button starts a new game and resets the week");
                }
            }
        }
    }
}
