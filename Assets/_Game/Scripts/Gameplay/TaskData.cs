using UnityEngine;
namespace Caudal
{
    public enum TaskCategory { Required, Optional, Overload }
    public enum TaskStat { Happiness, Productivity }

    [CreateAssetMenu(menuName = "Caudal/Tarea", fileName = "NuevaTarea")]
    public sealed class TaskData : ScriptableObject
    {
        public string taskId, taskName;
        public CharacterRole owner;
        public TaskStat affectedStat = TaskStat.Productivity;
        public TaskCategory category = TaskCategory.Required;
        [TextArea(2, 4)] public string description;
        [TextArea(2, 4)] public string instruction;
        [Min(1)] public int difficulty = 1;
        [Min(0)] public float happinessCost = 5, productivityReward = 5, quotaReward = 15;
        [Min(1)] public float assignmentTimeLimit = 10, executionTimeLimit = 20;
        public float rejectionHappinessChange;
        public bool keepAfterFailure = true;
        public bool requiresRefusalMinigame;

        [Header("Presentación")]
        public Sprite taskArtwork;
        [Tooltip("Fondo del popup de minijuego para esta tarea.")]
        public Sprite minigameWindowBackground;

        [Header("Minijuego")]
        public GameObject bossMinigamePrefab;
        public GameObject workerMinigamePrefab;
        [Tooltip("Referencia antigua, usada solo si no hay un prefab asignado al rol.")]
        public GameObject minigamePrefab;

        public GameObject GetMinigamePrefab(CharacterRole role)
        {
            GameObject rolePrefab = role == CharacterRole.Boss ? bossMinigamePrefab : workerMinigamePrefab;
            return rolePrefab != null ? rolePrefab : minigamePrefab;
        }

        // Compatibilidad temporal mientras se migran los validadores del loop antiguo.
        public GameObject GetMinigamePrefab(InteractionKind kind) =>
            GetMinigamePrefab(kind == InteractionKind.Assignment ? CharacterRole.Boss : CharacterRole.Worker);
    }
}
