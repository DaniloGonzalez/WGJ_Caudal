using UnityEngine;
using UnityEngine.UI;
using System;
using Caudal;

public class PopUpController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text taskDescriptionText;
    [SerializeField] private Text roleText;
    [SerializeField] private Text controlsText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text closeText;
    [SerializeField] private Image timerFill;
    [SerializeField] private RectTransform minigameContainer;
    [SerializeField] private GameObject placeholderContent;
    [SerializeField] private Button closeButton;

    private Action<InteractionResult> onCompleteCallback;
    private GameObject activeMinigameInstance;
    private IMinigame activeMinigame;
    private TaskInstance activeTask;
    private InteractionKind activeKind;
    private bool finished;

    public void Setup(TaskInstance task, InteractionKind kind, GameObject minigamePrefab, Action<InteractionResult> onComplete)
    {
        activeTask = task;
        activeKind = kind;
        var rootRect = (RectTransform)transform;
        rootRect.anchorMin = kind == InteractionKind.Assignment ? new Vector2(.5f, 0) : Vector2.zero;
        rootRect.anchorMax = kind == InteractionKind.Assignment ? Vector2.one : new Vector2(.5f, 1);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        if (titleText != null) titleText.text = task.Data.taskName;
        if (taskDescriptionText != null) taskDescriptionText.text = task.Data.description;
        if (roleText != null) roleText.text = kind == InteractionKind.Assignment ? "JEFE · ASIGNACIÓN" : "TRABAJADOR · " + (kind == InteractionKind.Refusal ? "CONVERSACIÓN" : "TAREA");
        if (controlsText != null) controlsText.text = GetControls(kind);
        if (closeText != null) closeText.text = kind == InteractionKind.Assignment ? "CANCELAR · SHIFT DER." : "CANCELAR · G";
        onCompleteCallback = onComplete;

        bool hasMinigame = false;
        if (minigamePrefab != null && minigameContainer != null) {
            activeMinigameInstance = Instantiate(minigamePrefab, minigameContainer, false);

            if (activeMinigameInstance.TryGetComponent<IMinigame>(out var minigame))
            {
                activeMinigame = minigame;
                activeMinigame.OnFinished += FinishMinigame;
                hasMinigame = true;
            }
            else
                Debug.LogWarning("PopUpController: el prefab asignado no implementa IMinigame en su objeto raíz.", activeMinigameInstance);
        }
        if (placeholderContent != null) placeholderContent.SetActive(!hasMinigame);

        if (closeButton != null) {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => FinishMinigame(InteractionResult.Cancelled));
        }
        RefreshTimer();
    }

    private void Update()
    {
        if (activeTask != null && activeTask.IsFinal)
        {
            ClosePopUp();
            return;
        }
        RefreshTimer();
    }

    private void RefreshTimer()
    {
        if (activeTask == null) return;
        float limit = activeKind == InteractionKind.Assignment ? activeTask.Data.assignmentTimeLimit : activeTask.Data.executionTimeLimit;
        if (activeKind == InteractionKind.Refusal) limit = 10f;
        float remaining = Mathf.Max(0, activeTask.Remaining);
        if (timerText != null) timerText.text = $"{remaining:0.0} s";
        if (timerFill != null) timerFill.fillAmount = limit > 0 ? Mathf.Clamp01(remaining / limit) : 0;
    }

    private static string GetControls(InteractionKind kind)
    {
        if (kind == InteractionKind.Assignment)
            return "JEFE: flechas para elegir · Enter para confirmar · Shift derecho para cancelar";
        return "TRABAJADOR: WASD para elegir · F para confirmar · G para cancelar";
    }

    public void FinishMinigame(InteractionResult result)
    {
        if (finished) return;
        finished = true;
        onCompleteCallback?.Invoke(result);
        ClosePopUp();
    }

    private void OnDestroy()
    {
        if (activeMinigame != null) activeMinigame.OnFinished -= FinishMinigame;
    }

    public void ClosePopUp() {
        finished = true;
        if (activeMinigame != null) activeMinigame.OnFinished -= FinishMinigame;
        Destroy(gameObject);
    }
}
