using UnityEngine;
using System.Collections.Generic;
using Caudal;

public class MinigameSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject popupBasePrefab;

    private readonly Dictionary<int, PopUpController> openPopups = new Dictionary<int, PopUpController>();

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.OnInteractionStarted += HandleInteractionStarted;
            taskManager.OnInteractionResolved += HandleInteractionResolved;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.OnInteractionStarted -= HandleInteractionStarted;
            taskManager.OnInteractionResolved -= HandleInteractionResolved;
        }
        foreach (var popup in openPopups.Values)
        {
            if (popup != null)
                popup.ClosePopUp();
        }
        openPopups.Clear();
    }

    public void HandleInteractionStarted(TaskInstance task, InteractionKind kind) {
        if (task == null || task.Data == null)
        {
            Debug.LogError("MinigameSpawner: la interacción no tiene una tarea válida.", this);
            return;
        }
        if (openPopups.ContainsKey(task.Id)) return;
        GameObject prefabToSpawn = task.Data.GetMinigamePrefab(task.Owner);
        if (popupBasePrefab == null)
        {
            Debug.LogWarning("MinigameSpawner: falta asignar popupBasePrefab en el Inspector.");
            return;
        }

        GameObject canvasObj = GameObject.Find("Game UI");

        if (canvasObj == null) {
            Debug.LogError("MinigameSpawner: No se encontro Game UI dónde abrir el minijuego");
            return;
        }

        GameObject popupInstance = Instantiate(popupBasePrefab, canvasObj.transform, false);

        if (popupInstance.TryGetComponent<PopUpController>(out var popup)) {
            openPopups.Add(task.Id, popup);
            popup.Setup(task, kind, prefabToSpawn, (result) =>
            {
                if (taskManager != null)
                    taskManager.Resolve(task.Id, task.Day, kind, result);
            });
        }
        else
        {
            Destroy(popupInstance);
            Debug.LogError("MinigameSpawner: popupBasePrefab no tiene PopUpController.", this);
        }
    }

    private void HandleInteractionResolved(TaskInstance task, InteractionKind kind, InteractionResult result)
    {
        if (task != null && openPopups.TryGetValue(task.Id, out var popup))
        {
            if (popup != null)
                popup.ClosePopUp();
            openPopups.Remove(task.Id);
        }
    }

}
