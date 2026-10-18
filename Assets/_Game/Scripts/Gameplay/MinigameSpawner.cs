using UnityEngine;
using Caudal;

public class MinigameSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject popupBasePrefab;
    [SerializeField] private GameObject fallbackMinigamePrefab;

    private PopUpController currentPopup;

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
        if (currentPopup != null)
        {
            currentPopup.ClosePopUp();
            currentPopup = null;
        }
    }

    public void HandleInteractionStarted(TaskInstance task, InteractionKind kind) {
        if (currentPopup != null) return;
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

        if (task == null || task.Data == null)
        {
            Debug.LogError("MinigameSpawner: la interacción no tiene una tarea válida.", this);
            return;
        }

        GameObject prefabToSpawn = task.Data.minigamePrefab != null ? task.Data.minigamePrefab : fallbackMinigamePrefab;

        GameObject popupInstance = Instantiate(popupBasePrefab, canvasObj.transform, false);

        if (popupInstance.TryGetComponent<PopUpController>(out currentPopup)) {
            currentPopup.Setup(task, kind, prefabToSpawn, (result) =>
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

    private void HandleInteractionResolved(TaskInstance task, InteractionResult result)
    {
        if (currentPopup != null)
        {
            currentPopup.ClosePopUp();
            currentPopup = null;
        }
    }

}
