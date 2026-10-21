using Caudal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EmailCleaningMinigame : TimedMinigame, IPointerClickHandler
{
    public override string ControlsHint => "Haz clic en los correos marcados como SPAM para eliminarlos.";
    [SerializeField] private Image[] cards;
    [SerializeField] private Text[] labels;
    [SerializeField] private Sprite spamSprite;
    [SerializeField] private Sprite importantSprite;
    [SerializeField] private Text progressText;

    private bool[] isSpam;
    private int cardCount;
    private int spamCount;
    private int selected;
    private int removedSpam;

    protected override void OnMinigameStarted()
    {
        cardCount = Difficulty == 1 ? Mathf.Min(4, cards.Length) : cards.Length;
        spamCount = cardCount / 2;
        if (Difficulty == 3) SetErrorLimit(1);
        isSpam = new bool[cards.Length];
        removedSpam = 0;
        for (int i = 0; i < spamCount; i++) isSpam[i] = true;
        for (int i = cardCount - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (isSpam[i], isSpam[j]) = (isSpam[j], isSpam[i]);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            bool visible = i < cardCount;
            cards[i].gameObject.SetActive(visible);
            labels[i].gameObject.SetActive(visible);
            if (!visible) continue;
            int columns = cardCount <= 4 ? 2 : 3;
            int rows = Mathf.CeilToInt((float)cardCount / columns);
            int row = i / columns;
            int column = i % columns;
            float cellWidth = .82f / columns;
            float centerX = .09f + cellWidth * (column + .5f);
            float centerY = rows == 1 ? .55f : row == 0 ? .70f : .36f;
            cards[i].rectTransform.anchorMin = new Vector2(centerX - cellWidth * .38f, centerY - .10f);
            cards[i].rectTransform.anchorMax = new Vector2(centerX + cellWidth * .38f, centerY + .10f);
            cards[i].gameObject.SetActive(true);
            labels[i].text = isSpam[i] ? "SPAM" : "IMPORTANTE";
            labels[i].gameObject.SetActive(true);
            labels[i].raycastTarget = false;
            labels[i].rectTransform.anchorMin = new Vector2(0, -.55f);
            labels[i].rectTransform.anchorMax = new Vector2(1, -.05f);
            labels[i].rectTransform.offsetMin = Vector2.zero;
            labels[i].rectTransform.offsetMax = Vector2.zero;
            labels[i].alignment = TextAnchor.MiddleCenter;
            labels[i].fontSize = 20;
            labels[i].resizeTextForBestFit = true;
            labels[i].resizeTextMinSize = 18;
            labels[i].resizeTextMaxSize = 20;
            labels[i].horizontalOverflow = HorizontalWrapMode.Wrap;
            labels[i].verticalOverflow = VerticalWrapMode.Truncate;
            cards[i].sprite = isSpam[i] ? spamSprite : importantSprite;
            cards[i].color = Color.white;
            cards[i].raycastTarget = true;
        }
        selected = 0;
        ShowSelection();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var target = eventData.pointerPressRaycast.gameObject != null ? eventData.pointerPressRaycast.gameObject.transform : eventData.pointerCurrentRaycast.gameObject?.transform;
        if (target == null) return;
        for (int i = 0; i < cards.Length; i++)
        {
            if (target != cards[i].transform && !target.IsChildOf(cards[i].transform)) continue;
            if (!cards[i].gameObject.activeSelf) return;
            selected = i;
            if (!isSpam[i]) AddError("Ese correo era importante.");
            else
            {
                cards[i].gameObject.SetActive(false);
                removedSpam++;
                SetFeedback("Spam eliminado.");
                MoveSelectionToAvailableCard();
                if (removedSpam == spamCount) Finish(InteractionResult.Success);
            }
            ShowSelection();
            return;
        }
    }

    private void ShowSelection()
    {
        for (int i = 0; i < cards.Length; i++)
            cards[i].transform.localScale = cards[i].gameObject.activeSelf && i == selected ? Vector3.one * 1.08f : Vector3.one;
        if (selected >= 0 && selected < cards.Length && cards[selected].gameObject.activeSelf)
            Highlight(cards[selected]);
        else
            Highlight(null);
        if (progressText != null)
        {
            int importantTotal = cardCount - spamCount;
            int importantPreserved = 0;
            for (int i = 0; i < cardCount; i++)
                if (!isSpam[i] && cards[i].gameObject.activeSelf) importantPreserved++;
            progressText.text = "Spam eliminado: " + removedSpam + " / " + spamCount
                + " · Importantes preservados: " + importantPreserved + " / " + importantTotal;
        }
    }

    private void MoveSelectionToAvailableCard()
    {
        for (int offset = 1; offset <= cards.Length; offset++)
        {
            int candidate = (selected + offset) % cards.Length;
            if (cards[candidate].gameObject.activeSelf)
            {
                selected = candidate;
                return;
            }
        }
    }
}
