using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleMinigame : TimedMinigame
{
    public override string ControlsHint => "↑/↓: mover · Enter: tomar y colocar";
    [SerializeField] private Image[] cards;
    [SerializeField] private Text[] cardLabels;
    [SerializeField] private Text[] slots;
    [SerializeField] private Sprite[] prioritySprites;
    [SerializeField] private Sprite slotSprite;
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
        }
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].text = "ESPACIO " + (i + 1) + "\n—";
            var image = slots[i].transform.parent.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = slotSprite;
                image.color = Color.white;
            }
        }
        selected = 0;
        ShowSelection();
    }

    protected override void ReadInput()
    {
        int direction = PressedUp ? -1 : PressedDown ? 1 : 0;
        if (direction != 0)
        {
            if (carriedCard < 0)
                MoveToNextAvailableCard(direction);
            else
                selected = WrapIndex(selected + direction, slots.Length);
        }

        if (PressedConfirm)
        {
            if (carriedCard < 0)
            {
                if (!IsCardAvailable(selected)) selected = FindFirstAvailableCard();
                if (selected < 0) return;
                carriedCard = selected;
                selected = 0;
                SetFeedback("Elige el lugar según prioridad.");
            }
            else if (priorities[carriedCard] == selected)
            {
                slots[selected].text = "ESPACIO " + (selected + 1) + "\n" + PriorityName(priorities[carriedCard]);
                cards[carriedCard].gameObject.SetActive(false);
                bool allPlaced = true;
                foreach (var card in cards) allPlaced &= !card.gameObject.activeSelf;
                if (allPlaced)
                {
                    carriedCard = -1;
                    selected = -1;
                    Highlight(null);
                    Finish(InteractionResult.Success);
                    return;
                }

                carriedCard = -1;
                selected = FindFirstAvailableCard();
                SetFeedback("Prioridad correcta.");
            }
            else
                AddError("Ese espacio no corresponde.");
        }
        ShowSelection();
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

    private void MoveToNextAvailableCard(int direction)
    {
        if (cards.Length == 0) return;
        int index = selected;
        for (int i = 0; i < cards.Length; i++)
        {
            index = WrapIndex(index + direction, cards.Length);
            if (IsCardAvailable(index))
            {
                selected = index;
                return;
            }
        }
    }

    private static int WrapIndex(int index, int count)
    {
        return count > 0 ? (index % count + count) % count : 0;
    }

    private static string PriorityName(int priority) => priority == 0 ? "ALTA" : priority == 1 ? "MEDIA" : "BAJA";
}
