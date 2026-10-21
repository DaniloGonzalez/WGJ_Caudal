using Caudal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScheduleMinigame : TimedMinigame, IPointerClickHandler
{
    public override string ControlsHint => "Haz clic en una tarjeta y luego en el espacio de su prioridad.";
    [SerializeField] private Image[] cards;
    [SerializeField] private Text[] cardLabels;
    [SerializeField] private Text[] slots;
    [SerializeField] private Sprite[] prioritySprites;
    [SerializeField] private Text phaseText;

    private int[] priorities;
    private int selected;
    private int carriedCard = -1;

    protected override void OnMinigameStarted()
    {
        priorities = new[] { 0, 1, 2 };
        for (int i = priorities.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (priorities[i], priorities[j]) = (priorities[j], priorities[i]);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(true);
            cardLabels[i].text = PriorityName(priorities[i]);
            if (prioritySprites != null && priorities[i] < prioritySprites.Length)
                cards[i].sprite = prioritySprites[priorities[i]];
            cards[i].color = Color.white;
            cards[i].raycastTarget = true;
        }
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].text = "ESPACIO " + (i + 1) + "\n—";
            var image = slots[i].transform.parent.GetComponent<Image>();
            if (image != null)
            {
                // Un fondo claro mantiene legible el texto negro de los espacios.
                image.sprite = null;
                image.color = new Color(.91f, .94f, .97f, 1f);
                image.raycastTarget = true;
            }
        }
        selected = 0;
        ShowSelection();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var target = eventData.pointerPressRaycast.gameObject != null ? eventData.pointerPressRaycast.gameObject.transform : eventData.pointerCurrentRaycast.gameObject?.transform;
        if (target == null) return;

        if (carriedCard < 0)
        {
            for (int i = 0; i < cards.Length; i++)
                if (target == cards[i].transform || target.IsChildOf(cards[i].transform))
                {
                    if (!IsCardAvailable(i)) return;
                    carriedCard = i;
                    selected = 0;
                    SetFeedback("Haz clic en el espacio de esa prioridad.");
                    ShowSelection();
                    return;
                }
        }
        else
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slotTransform = slots[i].transform.parent;
                if (slotTransform == null || (target != slotTransform && !target.IsChildOf(slotTransform))) continue;
                if (priorities[carriedCard] != i)
                {
                    AddError("Ese espacio no corresponde.");
                    return;
                }
                slots[i].text = "ESPACIO " + (i + 1) + "\n" + PriorityName(priorities[carriedCard]);
                cards[carriedCard].gameObject.SetActive(false);
                bool allPlaced = true;
                foreach (var card in cards) allPlaced &= !card.gameObject.activeSelf;
                carriedCard = -1;
                if (allPlaced) Finish(InteractionResult.Success);
                else
                {
                    selected = FindFirstAvailableCard();
                    SetFeedback("Prioridad correcta.");
                }
                ShowSelection();
                return;
            }
        }
    }

    private void ShowSelection()
    {
        if (phaseText != null)
            phaseText.text = carriedCard < 0 ? "SELECCIONA UNA TARJETA" : "ELIGE EL ESPACIO DE PRIORIDAD";
        for (int i = 0; i < cards.Length; i++)
            if (cards[i] != null)
                cards[i].transform.localScale = carriedCard < 0 && cards[i].gameObject.activeSelf && i == selected ? Vector3.one * 1.08f : Vector3.one;
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] != null && slots[i].transform.parent != null)
                slots[i].transform.parent.localScale = carriedCard >= 0 && i == selected ? Vector3.one * 1.06f : Vector3.one;

        Image target = null;
        if (carriedCard < 0)
        {
            if (IsCardAvailable(selected)) target = cards[selected];
        }
        else if (slots.Length > 0)
        {
            int slot = Mathf.Clamp(selected, 0, slots.Length - 1);
            if (slots[slot] != null && slots[slot].transform.parent != null)
                target = slots[slot].transform.parent.GetComponent<Image>();
        }
        Highlight(target);
    }

    private bool IsCardAvailable(int index)
    {
        return index >= 0 && index < cards.Length && cards[index] != null && cards[index].gameObject.activeSelf;
    }

    private int FindFirstAvailableCard()
    {
        for (int i = 0; i < cards.Length; i++)
            if (IsCardAvailable(i)) return i;
        return -1;
    }

    private static string PriorityName(int priority) => priority == 0 ? "ALTA" : priority == 1 ? "MEDIA" : "BAJA";
}
