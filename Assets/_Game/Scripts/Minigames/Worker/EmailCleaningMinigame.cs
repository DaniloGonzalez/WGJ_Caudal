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
    private int selected;
    private int removedSpam;

    protected override void OnMinigameStarted()
    {
        isSpam = new bool[cards.Length];
        removedSpam = 0;
        for (int i = 0; i < cards.Length / 2; i++) isSpam[i] = true;
        for (int i = isSpam.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (isSpam[i], isSpam[j]) = (isSpam[j], isSpam[i]);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            int column = i % 3;
            bool topRow = i < 3;
            float xMin = .08f + column * .31f;
            float xMax = xMin + .22f;
            cards[i].rectTransform.anchorMin = new Vector2(xMin, topRow ? .64f : .30f);
            cards[i].rectTransform.anchorMax = new Vector2(xMax, topRow ? .82f : .48f);
            cards[i].gameObject.SetActive(true);
            labels[i].text = (isSpam[i] ? "SPAM" : "IMPORTANTE") + "\nCorreo " + (i + 1);
            labels[i].gameObject.SetActive(true);
            labels[i].raycastTarget = false;
            labels[i].rectTransform.anchorMin = new Vector2(0, -.55f);
            labels[i].rectTransform.anchorMax = new Vector2(1, -.05f);
            labels[i].rectTransform.offsetMin = Vector2.zero;
            labels[i].rectTransform.offsetMax = Vector2.zero;
            labels[i].alignment = TextAnchor.MiddleCenter;
            labels[i].fontSize = 12;
            labels[i].resizeTextForBestFit = true;
            labels[i].resizeTextMinSize = 10;
            labels[i].resizeTextMaxSize = 12;
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
                if (removedSpam == cards.Length / 2) Finish(InteractionResult.Success);
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
        if (progressText != null) progressText.text = "Spam eliminado: " + removedSpam + " / " + (cards.Length / 2);
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
