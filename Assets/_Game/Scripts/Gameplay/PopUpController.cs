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
        bool bossRole = kind == InteractionKind.Assignment;
        rootRect.anchorMin = new Vector2(.665f, bossRole ? .39f : .015f);
        rootRect.anchorMax = new Vector2(.985f, bossRole ? .765f : .365f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        var overlay = GetComponent<Image>();
        if (overlay != null) overlay.color = new Color(0, 0, 0, 0);
        var window = transform.Find("Window");
        if (window != null)
        {
            var windowRect = window as RectTransform;
            var panel = window.GetComponent<Image>();
            if (panel != null)
            {
                panel.sprite = task.Data.minigameWindowBackground;
                panel.type = Image.Type.Simple;
                panel.preserveAspect = panel.sprite != null;
                float windowHeight = 400f;
                float aspect = panel.sprite != null
                    ? panel.sprite.rect.width / panel.sprite.rect.height
                    : 410f / 250f;
                windowRect.sizeDelta = new Vector2(windowHeight * aspect, windowHeight);
                panel.color = panel.sprite == null
                    ? bossRole ? new Color(.22f, .25f, .43f, .96f) : new Color(.16f, .34f, .31f, .96f)
                    : bossRole ? new Color(.96f, .96f, 1f, 1f) : new Color(.94f, 1f, .94f, 1f);
            }
        }
        SetImageBackgroundAlpha("Header", 0f);
        SetImageBackgroundAlpha("MinigameContainer", 0f);
        SetImageBackgroundAlpha("MinigameContainer/EmptyMinigameState", 0f);
        ConfigureCompactLayout();
        if (titleText != null) titleText.text = task.Data.taskName;
        if (taskDescriptionText != null) taskDescriptionText.text = task.Data.description;
        if (roleText != null) roleText.text = kind == InteractionKind.Assignment ? "JEFE · ASIGNACIÓN" : "TRABAJADOR · " + (kind == InteractionKind.Refusal ? "CONVERSACIÓN" : "TAREA");
        if (controlsText != null) controlsText.text = GetCompactControls(kind);
        if (closeText != null) closeText.text = "CANCELAR";
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

    private void SetImageBackgroundAlpha(string childPath, float alpha)
    {
        var child = transform.Find("Window/" + childPath);
        var image = child != null ? child.GetComponent<Image>() : null;
        if (image == null) return;
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void ConfigureCompactLayout()
    {
        SetRect(titleText != null ? titleText.rectTransform : null, new Vector2(0, 1), new Vector2(0, 1), new Vector2(25, -22), new Vector2(220, 30));
        if (titleText != null)
        {
            titleText.rectTransform.pivot = new Vector2(0, .5f);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 12;
            titleText.resizeTextMaxSize = 19;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Truncate;
        }
        SetRect(taskDescriptionText != null ? taskDescriptionText.rectTransform : null, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -104), new Vector2(310, 38));
        SetRect(roleText != null ? roleText.rectTransform : null, new Vector2(0, 1), new Vector2(0, 1), new Vector2(25, -65), new Vector2(190, 26));
        SetRect(timerText != null ? timerText.rectTransform : null, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -65), new Vector2(82, 26));
        if (roleText != null)
        {
            roleText.rectTransform.pivot = new Vector2(0, .5f);
            roleText.alignment = TextAnchor.MiddleLeft;
            roleText.resizeTextForBestFit = true;
            roleText.resizeTextMinSize = 10;
            roleText.resizeTextMaxSize = 14;
            roleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
        if (timerText != null)
        {
            timerText.rectTransform.pivot = new Vector2(1, .5f);
            timerText.alignment = TextAnchor.MiddleRight;
        }
        SetRect(minigameContainer, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -18), new Vector2(320, 112));
        if (placeholderContent != null)
            SetRect(placeholderContent.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetRect(controlsText != null ? controlsText.rectTransform : null, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-48, 22), new Vector2(220, 34));
        if (closeButton != null)
            SetRect(closeButton.transform as RectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-45, 22), new Vector2(82, 34));
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
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

    private static string GetCompactControls(InteractionKind kind) => kind == InteractionKind.Assignment
        ? "Flechas + Enter: asignar"
        : "WASD + F: responder · G: salir";

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
