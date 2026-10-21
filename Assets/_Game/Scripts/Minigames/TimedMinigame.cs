using System;
using Caudal;
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
    [SerializeField] private Image timerFill;
    [SerializeField] private Image selectionOutline;

    private float remaining;
    private int errors;
    private int startFrame;
    private bool finished;

    public event Action<InteractionResult> OnFinished;
    public virtual string ControlsHint => "Haz clic en los elementos para completar la tarea.";
    protected int ErrorCount => errors;
    protected int ErrorLimit => errorLimit;
    protected float Remaining => remaining;
    protected Text FeedbackLabel => feedbackText;

    public void SetWaveDifficulty(int level)
    {
        difficulty = Mathf.Clamp(level, 1, 3);
    }

    protected virtual void OnEnable()
    {
        if (selectionOutline == null)
        {
            var outlineTransform = transform.Find("SelectionOutline");
            if (outlineTransform != null)
                selectionOutline = outlineTransform.GetComponent<Image>();
        }
        if (selectionOutline != null)
            selectionOutline.gameObject.SetActive(false);
        remaining = duration;
        errors = 0;
        finished = false;
        startFrame = Time.frameCount;
        foreach (var label in GetComponentsInChildren<Text>(true))
            label.color = Color.black;
        SetFeedback("");
        OnMinigameStarted();
        UpdateHud();
    }

    protected virtual void Update()
    {
        if (finished || Time.frameCount <= startFrame)
            return;

        remaining -= Time.deltaTime * difficulty;
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
        if (errorsText != null)
            errorsText.text = failOnErrorLimit ? "Errores: " + errors + " / " + errorLimit : "Errores: " + errors;
        if (timerFill != null)
            timerFill.fillAmount = duration > 0 ? Mathf.Clamp01(remaining / duration) : 0;
    }
}
