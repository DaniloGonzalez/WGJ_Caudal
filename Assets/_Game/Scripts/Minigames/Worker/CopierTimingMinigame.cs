using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class CopierTimingMinigame : TimedMinigame
{
    [SerializeField] private RectTransform track;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private Text progressText;
    [SerializeField, Min(.1f)] private float speed = 1.7f;

    private float position;
    private int direction = 1;
    private int pages;

    protected override void OnMinigameStarted()
    {
        position = 0;
        direction = 1;
        pages = 0;
        targetZone.anchorMin = new Vector2(.42f, targetZone.anchorMin.y);
        targetZone.anchorMax = new Vector2(.62f, targetZone.anchorMax.y);
        UpdateIndicator();
        ShowProgress();
    }

    protected override void AdvanceGame(float deltaTime)
    {
        position += direction * speed * deltaTime;
        if (position >= 1) { position = 1; direction = -1; }
        if (position <= 0) { position = 0; direction = 1; }
        UpdateIndicator();
    }

    protected override void ReadInput()
    {
        if (!PressedConfirm) return;
        if (position >= .42f && position <= .62f)
        {
            pages++;
            SetFeedback("Página impresa.");
            ShowProgress();
            if (pages >= 3) Finish(InteractionResult.Success);
        }
        else
            AddError("Atasco: fuera de la zona.");
    }

    private void UpdateIndicator()
    {
        if (track == null || indicator == null) return;
        float halfWidth = track.rect.width * .5f;
        indicator.anchoredPosition = new Vector2(-halfWidth + position * track.rect.width, indicator.anchoredPosition.y);
    }

    private void ShowProgress()
    {
        if (progressText != null) progressText.text = "Páginas impresas: " + pages + " / 3";
    }
}
