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
        bool bossRole = task.Owner == CharacterRole.Boss;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        var overlay = GetComponent<Image>();
        if (overlay != null) overlay.color = new Color(0, 0, 0, .38f);
        var window = transform.Find("Window");
        if (window != null)
        {
            var windowRect = window as RectTransform;
            var panel = window.GetComponent<Image>();
            if (panel != null)
            {
                panel.sprite = task.Data.minigameWindowBackground;
                // Nine-slicing expands the empty desktop area without distorting its border or icons.
                panel.type = Image.Type.Sliced;
                panel.preserveAspect = false;
                panel.pixelsPerUnitMultiplier = 2f;
                float aspect = 1.65f;
                float availableWidth = Mathf.Max(1, rootRect.rect.width * .98f);
                float availableHeight = Mathf.Max(1, rootRect.rect.height * .96f);
                float windowHeight = Mathf.Min(660f, availableHeight * .9f, availableWidth * .9f / aspect);
                windowRect.anchorMin = windowRect.anchorMax = new Vector2(.5f, .5f);
                windowRect.pivot = new Vector2(.5f, .5f);
                windowRect.anchoredPosition = Vector2.zero;
                windowRect.sizeDelta = new Vector2(windowHeight * aspect, windowHeight);
                panel.color = panel.sprite == null
                    ? bossRole ? new Color(.22f, .25f, .43f, .96f) : new Color(.16f, .34f, .31f, .96f)
                    : bossRole ? new Color(.96f, .96f, 1f, 1f) : new Color(.94f, 1f, .94f, 1f);
            }
        }
        SetImageBackgroundAlpha("Header", task.Data.minigameWindowBackground != null
            ? Color.clear
            : bossRole ? new Color(.96f, .62f, .28f, 1f) : new Color(.32f, .65f, .95f, 1f));
        SetImageBackgroundAlpha("MinigameContainer", Color.clear);
        SetImageBackgroundAlpha("MinigameContainer/EmptyMinigameState", Color.clear);
        if (titleText != null)
        {
            titleText.text = task.Data.taskName;
            titleText.color = Color.black;
        }
        if (taskDescriptionText != null) taskDescriptionText.text = task.Data.description;
        if (roleText != null) roleText.text = (bossRole ? "JOSEFA" : "TOMÁS") + " · " + (kind == InteractionKind.Refusal ? "CONVERSACIÓN" : "TAREA");
        if (controlsText != null) controlsText.text = GetCompactControls(kind);
        if (closeText != null) closeText.text = "";
        onCompleteCallback = onComplete;

        bool hasMinigame = false;
        if (minigamePrefab != null && minigameContainer != null) {
            // Configure difficulty before OnEnable initializes the puzzle.
            minigameContainer.gameObject.SetActive(false);
            activeMinigameInstance = Instantiate(minigamePrefab, minigameContainer, false);

            if (activeMinigameInstance.TryGetComponent<TimedMinigame>(out var timedMinigame))
                timedMinigame.SetWaveDifficulty(task.Difficulty);

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

        if (controlsText != null && hasMinigame && activeMinigame != null)
            controlsText.text = string.IsNullOrWhiteSpace(task.Data.instruction)
                ? activeMinigame.ControlsHint
                : task.Data.instruction;

        ConfigureCompactLayout(hasMinigame);
        if (minigameContainer != null) minigameContainer.gameObject.SetActive(true);

        if (closeButton != null) {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => FinishMinigame(hasMinigame ? InteractionResult.Cancelled : InteractionResult.Success));
        }
        if (closeText != null)
        {
            closeText.text = hasMinigame ? "" : "RESOLVER";
            closeText.gameObject.SetActive(!hasMinigame);
        }
        if (!hasMinigame && placeholderContent != null)
        {
            var hint = placeholderContent.GetComponentInChildren<Text>(true);
            if (hint != null) hint.text = "Haz clic en RESOLVER para simular la tarea.";
        }
        RefreshTimer();
    }

    private void SetImageBackgroundAlpha(string childPath, Color color)
    {
        var child = transform.Find("Window/" + childPath);
        var image = child != null ? child.GetComponent<Image>() : null;
        if (image == null) return;
        image.color = color;
    }

    private void ConfigureCompactLayout(bool hasMinigame)
    {
        var window = transform.Find("Window") as RectTransform;
        var header = transform.Find("Window/Header") as RectTransform;
        if (header != null)
            SetRect(header, new Vector2(.02f, 1), new Vector2(.98f, 1), new Vector2(0, -4), new Vector2(0, 80));

        SetRect(titleText != null ? titleText.rectTransform : null, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, -35), new Vector2(-190, 50));
        if (titleText != null)
        {
            titleText.rectTransform.pivot = new Vector2(0, .5f);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 13;
            titleText.resizeTextMaxSize = 19;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleText.verticalOverflow = VerticalWrapMode.Truncate;
            titleText.fontSize = 19;
        }

        if (hasMinigame)
        {
            // Deja una línea de ayuda debajo del área jugable para mantener los controles visibles.
            if (taskDescriptionText != null) taskDescriptionText.gameObject.SetActive(false);
            if (roleText != null) roleText.gameObject.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (timerFill != null) timerFill.gameObject.SetActive(false);
            var timerBar = transform.Find("Window/Header/TimerBar") ?? transform.Find("Window/TimerBar");
            if (timerBar != null) timerBar.gameObject.SetActive(false);
            if (controlsText != null) controlsText.gameObject.SetActive(true);
            if (closeButton != null)
            {
                SetRect(closeButton.transform as RectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -35), new Vector2(45, 48));
                var closeImage = closeButton.GetComponent<Image>();
                if (closeImage != null) closeImage.color = Color.clear;
                closeButton.gameObject.SetActive(true);
            }
            if (closeText != null) closeText.gameObject.SetActive(false);

            if (window != null && minigameContainer != null)
            {
                float headerHeight = header != null ? header.sizeDelta.y : 42f;
                float width = Mathf.Max(1f, window.rect.width - 20f);
                bool detailedHint = controlsText != null && controlsText.text.Length > 90;
                float hintHeight = detailedHint ? 62f : 44f;
                const float hintMargin = 8f;
                const float gap = 5f;
                float top = window.rect.height * .5f - headerHeight - 10f;
                float bottom = -window.rect.height * .5f + hintMargin + hintHeight + gap;
                float height = Mathf.Max(1f, top - bottom);
                SetRect(minigameContainer, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, (top + bottom) * .5f), new Vector2(width, height));
                SetRect(controlsText != null ? controlsText.rectTransform : null, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, hintMargin + hintHeight * .5f), new Vector2(width, hintHeight));
            }

            if (controlsText != null)
            {
                bool detailedHint = controlsText != null && controlsText.text.Length > 90;
                controlsText.fontSize = detailedHint ? 16 : 17;
                controlsText.resizeTextForBestFit = true;
                controlsText.resizeTextMinSize = detailedHint ? 14 : 15;
                controlsText.resizeTextMaxSize = detailedHint ? 16 : 17;
                controlsText.color = Color.black;
                controlsText.alignment = TextAnchor.MiddleCenter;
                controlsText.horizontalOverflow = HorizontalWrapMode.Wrap;
                controlsText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            if (placeholderContent != null)
                SetRect(placeholderContent.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return;
        }

        SetRect(taskDescriptionText != null ? taskDescriptionText.rectTransform : null, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -108), new Vector2(310, 44));
        SetRect(roleText != null ? roleText.rectTransform : null, new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -65), new Vector2(160, 26));
        SetRect(timerText != null ? timerText.rectTransform : null, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -65), new Vector2(72, 26));
        if (taskDescriptionText != null)
        {
            taskDescriptionText.fontSize = 12;
            taskDescriptionText.resizeTextForBestFit = true;
            taskDescriptionText.resizeTextMinSize = 10;
            taskDescriptionText.resizeTextMaxSize = 12;
            taskDescriptionText.color = Color.black;
            taskDescriptionText.alignment = TextAnchor.MiddleCenter;
            taskDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            taskDescriptionText.verticalOverflow = VerticalWrapMode.Truncate;
        }
        if (roleText != null)
        {
            roleText.rectTransform.pivot = new Vector2(0, .5f);
            roleText.alignment = TextAnchor.MiddleLeft;
            roleText.fontSize = 12;
            roleText.resizeTextForBestFit = false;
            roleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            roleText.color = Color.black;
        }
        if (timerText != null)
        {
            timerText.rectTransform.pivot = new Vector2(1, .5f);
            timerText.alignment = TextAnchor.MiddleRight;
            timerText.fontSize = 12;
            timerText.resizeTextForBestFit = false;
            timerText.color = Color.black;
        }
        SetRect(minigameContainer, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -48), new Vector2(320, 168));
        if (placeholderContent != null)
            SetRect(placeholderContent.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetRect(controlsText != null ? controlsText.rectTransform : null, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-48, 24), new Vector2(210, 30));
        if (controlsText != null)
        {
            controlsText.fontSize = 10;
            controlsText.resizeTextForBestFit = false;
            controlsText.color = Color.black;
            controlsText.alignment = TextAnchor.MiddleCenter;
            controlsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            controlsText.verticalOverflow = VerticalWrapMode.Truncate;
        }
        if (closeButton != null)
            SetRect(closeButton.transform as RectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-45, 22), new Vector2(90, 30));
        if (closeText != null)
        {
            closeText.fontSize = 11;
            closeText.resizeTextForBestFit = false;
        }
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

    private static string GetCompactControls(InteractionKind kind) => "Haz clic en los elementos para completar la tarea.";

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
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
