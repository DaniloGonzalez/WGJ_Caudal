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
                game.Restart();
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
                game.Restart();
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
                game.Restart();
                Check(days.CurrentDay == 1 && days.QuotaProgress == 0 && stats.Happiness == 50 && stats.Productivity == 50, "Restart");
                var canvas = new GameObject("Validation Canvas", typeof(Canvas));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvas, scene);
                GameScreen.Build(canvas.transform, game, (Texture)bootstrap.FindProperty("workerOffice").objectReferenceValue, (Texture)bootstrap.FindProperty("bossOffice").objectReferenceValue, (Texture)bootstrap.FindProperty("workerPortrait").objectReferenceValue, (Texture)bootstrap.FindProperty("bossPortrait").objectReferenceValue);
                Check(canvas.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).Length == 4, "Four supplied placeholders");
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
