using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleMinigame : TimedMinigame
{
    [SerializeField] private Image[] cards;
    [SerializeField] private Text[] cardLabels;
    [SerializeField] private Text[] slots;
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
        }
        for (int i = 0; i < slots.Length; i++)
            slots[i].text = "ESPACIO " + (i + 1) + "\n—";
        selected = 0;
        ShowSelection();
    }

    protected override void ReadInput()
    {
        int count = carriedCard < 0 ? cards.Length : slots.Length;
        if (PressedUp) selected = (selected + count - 1) % count;
        if (PressedDown) selected = (selected + 1) % count;
        if (PressedConfirm)
        {
            if (carriedCard < 0)
            {
                carriedCard = selected;
                selected = 0;
                SetFeedback("Elige el lugar según prioridad.");
            }
            else if (priorities[carriedCard] == selected)
            {
                slots[selected].text = "ESPACIO " + (selected + 1) + "\n" + PriorityName(priorities[carriedCard]);
                cards[carriedCard].gameObject.SetActive(false);
                carriedCard = -1;
                selected = 0;
                bool allPlaced = true;
                foreach (var card in cards) allPlaced &= !card.gameObject.activeSelf;
                if (allPlaced) Finish(InteractionResult.Success);
                else SetFeedback("Prioridad correcta.");
            }
            else
                AddError("Ese espacio no corresponde.");
        }
        ShowSelection();
    }

    private void ShowSelection()
    {
        if (phaseText != null)
            phaseText.text = carriedCard < 0 ? "W/S: tarjeta  ·  Enter: tomar" : "W/S: espacio  ·  Enter: colocar";
        for (int i = 0; i < cards.Length; i++)
            cards[i].transform.localScale = carriedCard < 0 && i == selected ? Vector3.one * 1.08f : Vector3.one;
        for (int i = 0; i < slots.Length; i++)
            slots[i].transform.parent.localScale = carriedCard >= 0 && i == selected ? Vector3.one * 1.06f : Vector3.one;
    }

    private static string PriorityName(int priority) => priority == 0 ? "ALTA" : priority == 1 ? "MEDIA" : "BAJA";
}
