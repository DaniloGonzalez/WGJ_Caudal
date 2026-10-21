using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MuteNotificationsMinigame : TimedMinigame
{
    public override string ControlsHint => "Cierra las notificaciones irrelevantes; conserva las importantes.";

    [SerializeField] private GameObject[] cards;
    [SerializeField] private Button[] closeButtons;
    [SerializeField] private Image[] cardImages;
    [SerializeField] private TMP_Text[] titleLabels;
    [SerializeField] private TMP_Text[] messageLabels;
    [SerializeField] private Sprite irrelevantSprite;
    [SerializeField] private Sprite importantSprite;
    [SerializeField] private TMP_Text progressText;

    private readonly string[] titles =
    {
        "Nuevo correo", "Sistema", "Mensaje", "Recordatorio",
        "Oferta especial", "Notificación", "Seguridad"
    };
    private readonly string[] messages =
    {
        "Promoción comercial", "Error del servidor", "Almuerzo el viernes",
        "Reunión en 10 min", "Oferta: 50% menos", "Actualización lista",
        "Revisar los accesos"
    };
    private readonly bool[] important = { false, true, false, true, false, false, true };

    private bool[] closed;
    private bool[] shownImportant;
    private int visibleCount;
    private int irrelevantCount;
    private int removedCount;
    private int importantMistakes;

    protected override void OnMinigameStarted()
    {
        SetErrorLimit(2);
        visibleCount = Mathf.Min(cards.Length, Difficulty == 1 ? 3 : Difficulty == 2 ? 4 : 6);
        irrelevantCount = 0;
        removedCount = 0;
        importantMistakes = 0;
        closed = new bool[visibleCount];
        shownImportant = new bool[visibleCount];
        ArrangeCards();
        int[] order = new int[visibleCount];
        for (int i = 0; i < order.Length; i++) order[i] = i;
        for (int i = order.Length - 1; i > 0; i--)
        {
            int swap = Random.Range(0, i + 1);
            (order[i], order[swap]) = (order[swap], order[i]);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            bool visible = i < visibleCount;
            cards[i].SetActive(visible);
            closeButtons[i].onClick.RemoveAllListeners();
            if (!visible) continue;

            int messageIndex = order[i];
            shownImportant[i] = important[messageIndex];
            titleLabels[i].text = titles[messageIndex];
            messageLabels[i].text = messages[messageIndex];
            cardImages[i].sprite = shownImportant[i] ? importantSprite : irrelevantSprite;
            cardImages[i].preserveAspect = true;
            cardImages[i].raycastTarget = true;
            Transform importantIcon = cards[i].transform.Find("Important icon");
            if (importantIcon != null) importantIcon.gameObject.SetActive(shownImportant[i]);
            var textBacking = cards[i].transform.Find("Text backing");
            if (textBacking != null)
                textBacking.GetComponent<Image>().color = shownImportant[i]
                    ? new Color32(244, 187, 197, 255) : new Color32(219, 235, 247, 255);
            if (!shownImportant[i]) irrelevantCount++;
            int index = i;
            closeButtons[i].onClick.AddListener(() => CloseNotification(index));
        }
        ShowProgress();
    }

    private void ArrangeCards()
    {
        // Match the sprite ratio; keep the entire grid above the feedback footer.
        var area = (RectTransform)transform;
        int columns = visibleCount > 6 ? 3 : 2;
        int rows = Mathf.CeilToInt((float)visibleCount / columns);
        float height = Mathf.Min(132, (area.rect.height * .72f - 24) / rows);
        float width = height * (512f / 208f);
        for (int i = 0; i < cards.Length; i++)
        {
            var rect = cards[i].transform as RectTransform;
            if (rect == null) continue;

            int row = i / columns;
            int firstInRow = row * columns;
            int countInRow = Mathf.Min(columns, visibleCount - firstInRow);
            int column = i - firstInRow;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .84f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2((column - (countInRow - 1) * .5f) * (width + 24), -height * .5f - row * (height + 8));
            rect.sizeDelta = new Vector2(width, height);
        }
    }

    private void CloseNotification(int index)
    {
        if (index < 0 || index >= visibleCount || closed[index]) return;
        if (shownImportant[index])
        {
            importantMistakes++;
            AddError("Conserva las notificaciones importantes.");
            if (importantMistakes >= ErrorLimit)
                Finish(Caudal.InteractionResult.Failure);
            return;
        }

        closed[index] = true;
        cards[index].SetActive(false);
        removedCount++;
        SetFeedback("Notificación irrelevante cerrada.");
        ShowProgress();
        if (removedCount >= irrelevantCount)
            Finish(Caudal.InteractionResult.Success);
    }

    private void ShowProgress()
    {
        int importantPreserved = 0;
        int importantTotal = 0;
        for (int i = 0; i < visibleCount; i++)
        {
            if (!shownImportant[i]) continue;
            importantTotal++;
            if (!closed[i]) importantPreserved++;
        }
        progressText.text = "Irrelevantes cerradas: " + removedCount + " / " + irrelevantCount
            + "\nImportantes preservadas: " + importantPreserved + " / " + importantTotal;
    }
}
