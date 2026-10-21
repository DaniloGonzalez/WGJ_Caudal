using System;
using Caudal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TimedMinigame : MonoBehaviour, IMinigame
{
    [SerializeField, Min(.1f)] private float duration = 10;
    [SerializeField, Min(1)] private int difficulty = 1;
    [SerializeField, Min(1)] private int errorLimit = 3;
    [SerializeField] private bool failOnErrorLimit = true;
    [SerializeField] private Text timerText;
    [SerializeField] private Text errorsText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private TMP_Text timerTextTMP;
    [SerializeField] private TMP_Text errorsTextTMP;
    [SerializeField] private TMP_Text feedbackTextTMP;
    [SerializeField] private Image timerFill;
    [SerializeField] private Image selectionOutline;

    private float remaining;
    private float roundDuration;
    private int errors;
    private int startFrame;
    private bool finished;
    private int configuredErrorLimit;
    private Image selectedTarget;
    private Image pointerTarget;

    public event Action<InteractionResult> OnFinished;
    public virtual string ControlsHint => "Haz clic en los elementos para completar la tarea.";
    protected int ErrorCount => errors;
    protected int ErrorLimit => errorLimit;
    protected int Difficulty => difficulty;
    protected float Remaining => remaining;
    protected Text FeedbackLabel => feedbackText;
    protected TMP_Text TMPFeedbackLabel => feedbackTextTMP;

    protected virtual void Awake()
    {
        configuredErrorLimit = errorLimit;
    }

    protected void SetErrorLimit(int value)
    {
        errorLimit = Mathf.Max(1, value);
        UpdateHud();
    }

    public void SetWaveDifficulty(int level)
    {
        difficulty = Mathf.Clamp(level, 1, 3);
    }

    protected virtual void OnEnable()
    {
        if (configuredErrorLimit <= 0)
            configuredErrorLimit = errorLimit;
        else
            errorLimit = configuredErrorLimit;
        if (selectionOutline == null)
        {
            var outlineTransform = transform.Find("SelectionOutline");
            if (outlineTransform != null)
                selectionOutline = outlineTransform.GetComponent<Image>();
        }
        if (selectionOutline != null)
            selectionOutline.gameObject.SetActive(false);
        selectedTarget = null;
        pointerTarget = null;
        roundDuration = Mathf.Max(.1f, duration - 2f * (difficulty - 1));
        remaining = roundDuration;
        errors = 0;
        finished = false;
        startFrame = Time.frameCount;
        foreach (var label in GetComponentsInChildren<Text>(true))
            label.color = Color.black;
        foreach (var label in GetComponentsInChildren<TMP_Text>(true))
            label.color = Color.black;
        SetFeedback("");
        OnMinigameStarted();
        UpdateHud();
    }

    protected virtual void Update()
    {
        if (finished || Time.frameCount <= startFrame)
            return;

        remaining -= Time.deltaTime;
        AdvanceGame(Time.deltaTime);
        UpdateHud();

        if (remaining <= 0)
        {
            Finish(InteractionResult.Failure);
            return;
        }

        // Las mecánicas reciben sus acciones desde eventos de clic de la UI.
    }

    protected virtual void OnMinigameStarted() { }
    protected virtual void AdvanceGame(float deltaTime) { }

    protected void AddError(string message)
    {
        errors++;
        SetFeedback(message);
        UpdateHud();
        if (failOnErrorLimit && errors >= errorLimit)
            Finish(InteractionResult.Failure);
    }

    protected void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
        if (feedbackTextTMP != null)
            feedbackTextTMP.text = message;
    }

    protected void Highlight(Image target)
    {
        if (selectionOutline == null)
            return;

        if (target == null)
        {
            selectionOutline.gameObject.SetActive(false);
            return;
        }

        var outlineTransform = selectionOutline.rectTransform;
        outlineTransform.SetParent(target.rectTransform, false);
        outlineTransform.anchorMin = Vector2.zero;
        outlineTransform.anchorMax = Vector2.one;
        outlineTransform.offsetMin = new Vector2(-4, -4);
        outlineTransform.offsetMax = new Vector2(4, 4);
        outlineTransform.localRotation = Quaternion.identity;
        outlineTransform.localScale = Vector3.one;
        selectionOutline.gameObject.SetActive(true);
    }

    public void SetSelectionHighlight(Image target)
    {
        selectedTarget = target;
        RefreshSelectionHighlight();
    }

    public void ShowPointerHighlight(Image target)
    {
        pointerTarget = target;
        RefreshSelectionHighlight();
    }

    public void ClearPointerHighlight(Image target)
    {
        if (pointerTarget != target)
            return;

        pointerTarget = null;
        RefreshSelectionHighlight();
    }

    private void RefreshSelectionHighlight()
    {
        Highlight(pointerTarget != null ? pointerTarget : selectedTarget);
    }

    protected void Finish(InteractionResult result)
    {
        if (finished)
            return;
        finished = true;
        OnFinished?.Invoke(result);
    }

    private void UpdateHud()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(0, remaining)) + " s";
        if (timerTextTMP != null)
            timerTextTMP.text = Mathf.CeilToInt(Mathf.Max(0, remaining)) + " s";
        if (errorsText != null)
            errorsText.text = failOnErrorLimit ? "Errores: " + errors + " / " + errorLimit : "Errores: " + errors;
        if (errorsTextTMP != null)
            errorsTextTMP.text = failOnErrorLimit ? "Errores: " + errors + " / " + errorLimit : "Errores: " + errors;
        if (timerFill != null)
            timerFill.fillAmount = roundDuration > 0 ? Mathf.Clamp01(remaining / roundDuration) : 0;
    }
}
