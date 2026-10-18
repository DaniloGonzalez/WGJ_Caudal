using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class EmailCleaningMinigame : TimedMinigame
{
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
            cards[i].gameObject.SetActive(true);
            labels[i].text = (isSpam[i] ? "SPAM" : "IMPORTANTE") + "\nCorreo " + (i + 1);
            cards[i].sprite = isSpam[i] ? spamSprite : importantSprite;
            cards[i].color = Color.white;
        }
        selected = 0;
        ShowSelection();
    }

    protected override void ReadInput()
    {
        int columns = 3;
        if (PressedLeft) selected = selected % columns == 0 ? selected + columns - 1 : selected - 1;
        if (PressedRight) selected = selected % columns == columns - 1 ? selected - columns + 1 : selected + 1;
        if (PressedUp) selected = (selected + cards.Length - columns) % cards.Length;
        if (PressedDown) selected = (selected + columns) % cards.Length;
        selected = Mathf.Clamp(selected, 0, cards.Length - 1);

        if (PressedConfirm && cards[selected].gameObject.activeSelf)
        {
            if (!isSpam[selected])
                AddError("Ese correo era importante.");
            else
            {
                cards[selected].gameObject.SetActive(false);
                removedSpam++;
                SetFeedback("Spam eliminado.");
                MoveSelectionToAvailableCard();
                if (removedSpam == cards.Length / 2)
                    Finish(InteractionResult.Success);
            }
        }
        ShowSelection();
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
