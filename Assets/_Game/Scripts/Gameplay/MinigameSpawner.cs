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

        GameObject prefabToSpawn = task.Data.minigamePrefab != null ? task.Data.minigamePrefab : fallbackMinigamePrefab;

        GameObject popupInstance = Instantiate(popupBasePrefab, canvasObj.transform, false);

        if (popupInstance.TryGetComponent<PopUpController>(out currentPopup)) {
            currentPopup.Setup(task, kind, prefabToSpawn, (result) => taskManager.Resolve(task.Id, task.Day, kind, result));
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
