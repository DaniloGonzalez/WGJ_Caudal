using Caudal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CopierTimingMinigame : TimedMinigame, IPointerClickHandler
{
    public override string ControlsHint => "Haz clic en la barra cuando el cursor pase por la marca verde.";
    [SerializeField] private RectTransform track;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private Text progressText;
    [SerializeField] private Image[] pageImages;
    [SerializeField] private Sprite blankPageSprite;
    [SerializeField] private Sprite donePageSprite;
    [SerializeField] private Sprite timingTrackSprite;
    [SerializeField] private Sprite timingTargetSprite;
    [SerializeField] private Sprite timingCursorSprite;
    [SerializeField, Min(.1f)] private float speed = 1.7f;

    private float position;
    private int direction = 1;
    private int printedPages;
    private int pagesRequired;

    protected override void OnMinigameStarted()
    {
        pagesRequired = Difficulty == 1 ? 2 : Difficulty == 2 ? 3 : 4;
        position = 0;
        direction = 1;
        printedPages = 0;
        speed = Difficulty == 1 ? 1.35f : Difficulty == 2 ? 1.7f : 2.05f;
        var trackImage = track != null ? track.GetComponent<Image>() : null;
        var targetImage = targetZone != null ? targetZone.GetComponent<Image>() : null;
        var cursorImage = indicator != null ? indicator.GetComponent<Image>() : null;
        // La barra y la marca reciben clics y el evento sube hasta este componente.
        if (trackImage != null) trackImage.raycastTarget = true;
        if (targetImage != null) targetImage.raycastTarget = true;
        if (cursorImage != null) cursorImage.raycastTarget = false;
        if (trackImage != null && timingTrackSprite != null) trackImage.sprite = timingTrackSprite;
        if (targetImage != null && timingTargetSprite != null) targetImage.sprite = timingTargetSprite;
        if (cursorImage != null && timingCursorSprite != null) cursorImage.sprite = timingCursorSprite;
        // The visible bar and the movement area must have exactly the same proportions.
        track.anchorMin = track.anchorMax = new Vector2(.5f, .53f);
        track.anchoredPosition = Vector2.zero;
        track.sizeDelta = new Vector2(800, 136);
        targetZone.anchorMin = targetZone.anchorMax = new Vector2(.52f, .5f);
        targetZone.anchoredPosition = Vector2.zero;
        float targetWidth = Difficulty == 1 ? 210 : Difficulty == 2 ? 160 : 120;
        targetZone.sizeDelta = new Vector2(targetWidth, targetWidth * 58 / 140);
        indicator.sizeDelta = new Vector2(24, 95);
        if (pageImages != null)
            foreach (var page in pageImages)
                if (page != null) page.sprite = blankPageSprite;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (track == null || !RectTransformUtility.RectangleContainsScreenPoint(track, eventData.position, eventData.pressEventCamera)) return;
        TryPrintPage();
    }

    private void TryPrintPage()
    {
        UpdateIndicator();
        float cursorX = indicator.position.x;
        var corners = new Vector3[4];
        targetZone.GetWorldCorners(corners);
        if (cursorX >= corners[0].x && cursorX <= corners[2].x)
        {
            printedPages++;
            SetFeedback("Página impresa.");
            if (pageImages != null && printedPages - 1 < pageImages.Length && pageImages[printedPages - 1] != null)
                pageImages[printedPages - 1].sprite = donePageSprite;
            ShowProgress();
            if (printedPages >= pagesRequired) Finish(InteractionResult.Success);
        }
        else
            AddError("Atasco: fuera de la zona.");
    }

    private void UpdateIndicator()
    {
        if (track == null || indicator == null) return;

        // Ancla el cursor al centro de la barra y limita su recorrido a su ancho útil.
        indicator.anchorMin = new Vector2(.5f, indicator.anchorMin.y);
        indicator.anchorMax = new Vector2(.5f, indicator.anchorMax.y);
        float travelHalfWidth = Mathf.Max(0f, (track.rect.width - indicator.rect.width) * .5f);
        float x = Mathf.Lerp(-travelHalfWidth, travelHalfWidth, position);
        indicator.anchoredPosition = new Vector2(x, indicator.anchoredPosition.y);
    }

    private void ShowProgress()
    {
        if (progressText != null) progressText.text = "Páginas impresas: " + printedPages + " / " + pagesRequired;
    }
}
