using System.Collections.Generic;
using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class DocumentSortingMinigame : TimedMinigame
{
    [SerializeField] private Image[] documents;
    [SerializeField] private Image[] trays;
    [SerializeField] private Sprite[] documentSprites;
    [SerializeField] private Sprite[] traySprites;
    [SerializeField] private Text phaseText;

    private readonly List<int> remainingDocuments = new List<int>();
    private int[] documentColors;
    private int selected;
    private int carriedDocument = -1;

    protected override void OnMinigameStarted()
    {
        documentColors = new[] { 0, 1, 2 };
        Shuffle(documentColors);
        remainingDocuments.Clear();
        for (int i = 0; i < documents.Length; i++)
        {
            remainingDocuments.Add(i);
            if (documentSprites != null && documentColors[i] < documentSprites.Length)
                documents[i].sprite = documentSprites[documentColors[i]];
            documents[i].color = Color.white;
            documents[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < trays.Length; i++)
        {
            if (traySprites != null && i < traySprites.Length)
                trays[i].sprite = traySprites[i];
            trays[i].color = Color.white;
        }
        selected = 0;
        ShowSelection();
    }

    protected override void ReadInput()
    {
        if (PressedUp) selected = Mathf.Max(0, selected - 1);
        if (PressedDown) selected = Mathf.Min((carriedDocument < 0 ? remainingDocuments.Count : trays.Length) - 1, selected + 1);
        if (PressedConfirm)
        {
            if (carriedDocument < 0)
            {
                if (remainingDocuments.Count == 0) return;
                carriedDocument = remainingDocuments[selected];
                selected = 0;
                SetFeedback("Elige la bandeja del mismo color.");
            }
            else if (documentColors[carriedDocument] == selected)
            {
                documents[carriedDocument].gameObject.SetActive(false);
                remainingDocuments.Remove(carriedDocument);
                carriedDocument = -1;
                selected = 0;
                if (remainingDocuments.Count == 0) Finish(InteractionResult.Success);
                else SetFeedback("Documento clasificado.");
            }
            else
            {
                selected = 0;
                AddError("Bandeja equivocada.");
            }
        }
        ShowSelection();
    }

    private void ShowSelection()
    {
        if (phaseText != null)
            phaseText.text = carriedDocument < 0 ? "W/S: documento  ·  Enter: recoger" : "W/S: bandeja  ·  Enter: depositar";
        for (int i = 0; i < documents.Length; i++)
        {
            bool isSelected = carriedDocument < 0 && remainingDocuments.IndexOf(i) == selected;
            documents[i].transform.localScale = isSelected ? Vector3.one * 1.08f : Vector3.one;
        }
        for (int i = 0; i < trays.Length; i++)
            trays[i].transform.localScale = carriedDocument >= 0 && i == selected ? Vector3.one * 1.08f : Vector3.one;

        Highlight(carriedDocument < 0
            ? remainingDocuments.Count > 0 ? documents[remainingDocuments[Mathf.Clamp(selected, 0, remainingDocuments.Count - 1)]] : null
            : trays[Mathf.Clamp(selected, 0, trays.Length - 1)]);
    }

    private static void Shuffle(int[] values)
    {
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}
