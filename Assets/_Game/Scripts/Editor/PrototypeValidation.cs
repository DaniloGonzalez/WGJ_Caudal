using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Caudal.Editor
{
    public static class PrototypeValidation
    {
        static void Check(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
        }
        static void Call(object target, string method)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
        static void StartSelectedGame(GameManager game)
        {
            game.Restart();
            game.ConfirmCharacter(false);
            game.ConfirmCharacter(true);
        }

        static void ValidateCharacters(GameManager game)
        {
            var serialized = new SerializedObject(game);
            foreach (var field in new[] { "workerCharacters", "bossCharacters" })
            {
                var profiles = serialized.FindProperty(field);
                Check(profiles.arraySize == 3, "Three profiles per role");
                for (int i = 0; i < profiles.arraySize; i++)
                {
                    var profile = profiles.GetArrayElementAtIndex(i).objectReferenceValue as CharacterData;
                    Check(profile != null && profile.portrait != null && !string.IsNullOrEmpty(profile.biography), "Profile content");
                }
            }

            game.Restart();
            Check(game.State == GameState.CharacterSelection && game.Days.CurrentDay == 0, "Selection before day one");
            Call(game, "Update");
            Check(game.Days.TimeRemaining == 0, "No clock during selection");
            game.SelectCharacter(false, -1);
            var last = game.WorkerCharacter;
            game.ConfirmCharacter(false);
            game.SelectCharacter(false, 1);
            Check(game.WorkerCharacter == last && game.State == GameState.CharacterSelection, "Ready role locked; wait for partner");
            game.CancelCharacter(false);
            game.SelectCharacter(false, 1);
            Check(game.WorkerCharacter != last && !game.WorkerReady, "Cancel and wrap selection");

            // Nine combinations: real task rewards, not just displayed numbers.
            for (int worker = 0; worker < 3; worker++)
            for (int boss = 0; boss < 3; boss++)
            {
                game.Restart();
                game.SelectCharacter(false, worker);
                game.SelectCharacter(true, boss);
                game.ConfirmCharacter(false);
                game.ConfirmCharacter(true);
                Check(game.State == GameState.Playing && game.Days.CurrentDay == 1, "Both ready starts day");
                var w = game.WorkerCharacter;
                var b = game.BossCharacter;
                game.SelectCharacter(false, 1);
                Check(game.WorkerCharacter == w, "No changing profile mid-game");
                bool pressure = (worker + boss) % 2 == 1;
                var tasks = game.Tasks;
                tasks.Assign(0, pressure ? TaskStrategy.Pressure : TaskStrategy.Normal);
                var task = tasks.Assigning;
                tasks.Resolve(task.Id, task.Day, InteractionKind.Assignment, InteractionResult.Success);
                tasks.Accept();
                Check(game.Days.QuotaProgress == 0 && game.Stats.Productivity == 50, "Bonuses do not reward assignment/acceptance");
                tasks.Resolve(task.Id, task.Day, InteractionKind.Execution, InteractionResult.Success);
                float reward = pressure ? 1.5f : 1;
                float stress = pressure ? 2 : 1;
                float cost = (1-w.happinessCostReductionPercent/100)*(1-b.happinessCostReductionPercent/100);
                float expectedQuota = task.Data.quotaReward * reward * (1+(w.quotaBonusPercent+b.quotaBonusPercent)/100);
                float expectedProductivity = 50+task.Data.productivityReward*reward*(1+(w.productivityBonusPercent+b.productivityBonusPercent)/100);
                Check(Mathf.Abs(game.Days.QuotaProgress-expectedQuota)<.001f, "Combined quota bonuses");
                Check(Mathf.Abs(game.Stats.Productivity-expectedProductivity)<.001f, "Combined productivity bonuses");
                Check(Mathf.Abs(game.Stats.Happiness-(50-task.Data.happinessCost*stress*cost))<.001f, "Combined happiness reductions");
                Check(!tasks.Resolve(task.Id,task.Day,InteractionKind.Execution,InteractionResult.Success), "Bonus only once");
                tasks.Assign(1, TaskStrategy.Normal);
                task=tasks.Assigning;
                tasks.Resolve(task.Id,task.Day,InteractionKind.Assignment,InteractionResult.Success);
                tasks.Accept();
                float happiness=game.Stats.Happiness;
                tasks.Resolve(task.Id,task.Day,InteractionKind.Execution,InteractionResult.Failure);
                Check(Mathf.Abs(game.Stats.Happiness-(happiness-2*cost))<.001f && Mathf.Abs(game.Days.QuotaProgress-expectedQuota)<.001f, "Failure reduces stress but grants no quota");
                Call(game,"CloseDay");
                Call(game,"BeginDay");
                Check(game.WorkerCharacter==w && game.BossCharacter==b,"Profiles persist across days");
            }
            game.Restart();
            Check(!game.WorkerReady && !game.BossReady && game.Days.QuotaProgress==0,"Restart clears selection readiness and quota");
            Debug.Log("CAUDAL_CHARACTERS_PASSED: selection, nine combinations, pressure, failure, persistence and restart.");
        }

        [MenuItem("Caudal/Validar lógica del prototipo")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity", OpenSceneMode.Additive);
            try
            {
                var go = scene.GetRootGameObjects()[0];
                var game = go.GetComponent<GameManager>();
                var stats = go.GetComponent<StatsManager>();
                var days = go.GetComponent<DayManager>();
                var tasks = go.GetComponent<TaskManager>();
                var input = go.GetComponent<InputRouter>();
                var bootstrap = new SerializedObject(go.GetComponent<GameBootstrap>());
                foreach (var field in new[] { "workerOffice", "bossOffice", "workerPortrait", "bossPortrait", "gameManager", "statsManager", "dayManager", "taskManager", "inputRouter" })
                    Check(bootstrap.FindProperty(field).objectReferenceValue != null, "Missing scene reference: " + field);
                game.Initialize(stats, days, tasks, input);
                ValidateCharacters(game);
                StartSelectedGame(game);
                Check(tasks.Tasks.Count == 6, "Daily catalog");
                var ids = new System.Collections.Generic.HashSet<string>();
                foreach (var task in tasks.Tasks)
                    Check(ids.Add(task.Data.taskId), "Duplicate task");
                int normal = -1;
                for (int i = 0; i < tasks.Tasks.Count; i++)
                if (!tasks.Tasks[i].Data.requiresRefusalMinigame)
                {
                    normal = i;
                    break;
                }
                Check(tasks.Assign(normal, TaskStrategy.Normal), "Assign normal");
                var assignment = tasks.Assigning;
                Check(days.QuotaProgress == 0, "Assignment grants no production");
                tasks.Resolve(assignment.Id, assignment.Day, InteractionKind.Assignment, InteractionResult.Success);
                Check(!tasks.Assign((normal + 1) % tasks.Tasks.Count, TaskStrategy.Normal), "Pending limit");
                tasks.Reject();
                Check(assignment.Status == TaskStatus.Rejected && days.QuotaProgress == 0, "Direct rejection");
                int index = (normal + 1) % tasks.Tasks.Count;
                tasks.Assign(index, TaskStrategy.Pressure);
                var pressure = tasks.Assigning;
                tasks.Resolve(pressure.Id, pressure.Day, InteractionKind.Assignment, InteractionResult.Success);
                tasks.Reject();
                Check(pressure.Refusing, "Pressure refusal");
                tasks.Resolve(pressure.Id, pressure.Day, InteractionKind.Refusal, InteractionResult.Failure);
                Check(tasks.Active == null && tasks.Pending == pressure, "No forced acceptance");
                tasks.Accept();
                Check(days.QuotaProgress == 0, "Acceptance grants no production");
                tasks.Resolve(pressure.Id, pressure.Day, InteractionKind.Execution, InteractionResult.Success);
                float quota = days.QuotaProgress;
                Check(quota > 0, "Completion rewards");
                Check(!tasks.Resolve(pressure.Id, pressure.Day, InteractionKind.Execution, InteractionResult.Success) && quota == days.QuotaProgress, "Duplicate result");
                index = (normal + 2) % tasks.Tasks.Count;
                tasks.Assign(index, TaskStrategy.Normal);
                var expired = tasks.Assigning;
                tasks.EndDay();
                Check(expired.Status == TaskStatus.Expired, "Expiry");
                Check(!tasks.Resolve(expired.Id, expired.Day, InteractionKind.Assignment, InteractionResult.Success), "Late result");
                stats.Apply(999, -999);
                Check(stats.Happiness == 100 && stats.Productivity == 0, "Clamp");
                StartSelectedGame(game);
                for (int day = 1; day <= 5; day++)
                {
                    Check(days.CurrentDay == day, "Day sequence");
                    Check(days.Tick(1000), "Clock ends");
                    Call(game, "CloseDay");
                    Check(game.State == GameState.DaySummary, "Summary");
                    if (day < 5)
                        Call(game, "BeginDay");
                }
                Call(game, "ShowEnding");
                Check(game.State == GameState.GameOver && game.ResultText.Length > 0, "Ending");
                StartSelectedGame(game);
                Check(days.CurrentDay == 1 && days.QuotaProgress == 0 && stats.Happiness == 50 && stats.Productivity == 50, "Restart");
                var canvas = new GameObject("Validation Canvas", typeof(Canvas));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvas, scene);
                GameScreen.Build(canvas.transform, game, (Texture)bootstrap.FindProperty("workerOffice").objectReferenceValue, (Texture)bootstrap.FindProperty("bossOffice").objectReferenceValue, (Texture)bootstrap.FindProperty("workerPortrait").objectReferenceValue, (Texture)bootstrap.FindProperty("bossPortrait").objectReferenceValue);
                Check(canvas.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).Length == 6, "Four supplied placeholders and two character previews");
                foreach (var component in canvas.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    var method = component.GetType().GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method != null)
                        method.Invoke(component, null);
                }
                Debug.Log("CAUDAL_VALIDATION_PASSED: scene references, catalog, concurrency, decisions, rewards, expiry, five days, restart and UI.");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
