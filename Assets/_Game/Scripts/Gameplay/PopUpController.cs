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
        bool portraitCanvas = Screen.width < Screen.height;
        rootRect.anchorMin = portraitCanvas
            ? new Vector2(.06f, bossRole ? .52f : .03f)
            : new Vector2(.665f, bossRole ? .39f : .12f);
        rootRect.anchorMax = portraitCanvas
            ? new Vector2(.94f, bossRole ? .97f : .48f)
            : new Vector2(.985f, bossRole ? .765f : .47f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
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
                float aspect = panel.sprite != null
                    ? panel.sprite.rect.width / panel.sprite.rect.height
                    : 410f / 250f;
                float availableWidth = Mathf.Max(1, rootRect.rect.width * .98f);
                float availableHeight = Mathf.Max(1, rootRect.rect.height * .96f);
                float windowHeight = Mathf.Min(400f, availableHeight, availableWidth / aspect);
                windowRect.sizeDelta = new Vector2(windowHeight * aspect, windowHeight);
                panel.color = panel.sprite == null
                    ? bossRole ? new Color(.22f, .25f, .43f, .96f) : new Color(.16f, .34f, .31f, .96f)
                    : bossRole ? new Color(.96f, .96f, 1f, 1f) : new Color(.94f, 1f, .94f, 1f);
            }
        }
        SetImageBackgroundAlpha("Header", bossRole
            ? new Color(.96f, .62f, .28f, 1f)
            : new Color(.32f, .65f, .95f, 1f));
        SetImageBackgroundAlpha("MinigameContainer", Color.clear);
        SetImageBackgroundAlpha("MinigameContainer/EmptyMinigameState", Color.clear);
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

        if (controlsText != null && hasMinigame && activeMinigame != null)
            controlsText.text = activeMinigame.ControlsHint;

        ConfigureCompactLayout(hasMinigame);

        if (closeButton != null) {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => FinishMinigame(InteractionResult.Cancelled));
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
            SetRect(header, new Vector2(.02f, 1), new Vector2(.98f, 1), new Vector2(0, -4), new Vector2(0, 42));

        SetRect(titleText != null ? titleText.rectTransform : null, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, -22), new Vector2(-36, 38));
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
            if (controlsText != null) controlsText.gameObject.SetActive(true);
            if (closeButton != null) closeButton.gameObject.SetActive(false);

            if (window != null && minigameContainer != null)
            {
                float headerHeight = header != null ? header.sizeDelta.y : 42f;
                float width = Mathf.Max(1f, window.rect.width - 20f);
                bool detailedHint = activeMinigame.ControlsHint.Contains("\n");
                float hintHeight = detailedHint ? 30f : 22f;
                const float hintMargin = 10f;
                const float gap = 5f;
                float top = window.rect.height * .5f - headerHeight - 10f;
                float bottom = -window.rect.height * .5f + hintMargin + hintHeight + gap;
                float height = Mathf.Max(1f, top - bottom);
                SetRect(minigameContainer, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, (top + bottom) * .5f), new Vector2(width, height));
                SetRect(controlsText != null ? controlsText.rectTransform : null, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, hintMargin + hintHeight * .5f), new Vector2(width, hintHeight));
            }

            if (controlsText != null)
            {
                bool detailedHint = activeMinigame.ControlsHint.Contains("\n");
                controlsText.fontSize = detailedHint ? 10 : 12;
                controlsText.resizeTextForBestFit = true;
                controlsText.resizeTextMinSize = detailedHint ? 9 : 10;
                controlsText.resizeTextMaxSize = detailedHint ? 10 : 12;
                controlsText.color = new Color(.91f, .94f, .98f, 1f);
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
            taskDescriptionText.color = new Color(.13f, .16f, .24f, 1f);
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
            roleText.color = new Color(.13f, .16f, .24f, 1f);
        }
        if (timerText != null)
        {
            timerText.rectTransform.pivot = new Vector2(1, .5f);
            timerText.alignment = TextAnchor.MiddleRight;
            timerText.fontSize = 12;
            timerText.resizeTextForBestFit = false;
            timerText.color = new Color(.13f, .16f, .24f, 1f);
        }
        SetRect(minigameContainer, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -48), new Vector2(320, 168));
        if (placeholderContent != null)
            SetRect(placeholderContent.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetRect(controlsText != null ? controlsText.rectTransform : null, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-48, 24), new Vector2(210, 30));
        if (controlsText != null)
        {
            controlsText.fontSize = 10;
            controlsText.resizeTextForBestFit = false;
            controlsText.color = new Color(.13f, .16f, .24f, 1f);
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

    private static string GetCompactControls(InteractionKind kind) => kind == InteractionKind.Assignment
        ? "Usa las flechas para elegir · Enter: confirmar"
        : "Usa W / A / S / D para elegir · F: confirmar · G: salir";

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
