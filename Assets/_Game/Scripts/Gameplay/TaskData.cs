using UnityEngine;
namespace Caudal
{
    [CreateAssetMenu(menuName = "Caudal/Tarea", fileName = "NuevaTarea")]
    public sealed class TaskData : ScriptableObject
    {
        public string taskId, taskName;
        [TextArea] public string description;
        [Min(1)] public int difficulty = 1;
        [Min(0)] public float happinessCost = 5, productivityReward = 5, quotaReward = 15;
        [Min(1)] public float assignmentTimeLimit = 10, executionTimeLimit = 20;
        public bool requiresRefusalMinigame;

        [Header("Minijuego")]
        public GameObject minigamePrefab;
    }
}
