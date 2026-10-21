using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DeskSortingMinigame : TimedMinigame, IMinigameDragReceiver
{
    public override string ControlsHint => "Documento/carpeta → Documentos; taza, grapadora o lápiz → Oficina; planta → Personal.";

    [SerializeField] private RectTransform[] items;
    [SerializeField] private RectTransform[] categories;
    [SerializeField] private Image[] itemImages;
    [SerializeField] private Image[] correctMarks;
    [SerializeField] private TMP_Text progressText;

    private readonly int[] correctCategories = { 0, 1, 2, 1, 0, 1 };
    private int[] categoryCounts;
    private bool[] sorted;
    private int itemCount;
    private int sortedCount;
    private bool dragging;

    protected override void Awake()
    {
        base.Awake();
        CacheHomePositions();
    }

    protected override void OnMinigameStarted()
    {
        CacheHomePositions();
        ResetItems();
        itemCount = Mathf.Min(items.Length, Difficulty == 1 ? 3 : Difficulty == 2 ? 5 : 6);
        sorted = new bool[itemCount];
        sortedCount = 0;
        categoryCounts = new int[categories.Length];
        dragging = false;
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(i < itemCount);
            correctMarks[i].gameObject.SetActive(false);
        }
        if (categories.Length > 3)
            categories[3].gameObject.SetActive(false); // Varios no se usa sin objetos con regla definida.
        BindDragHandles();
        SetSelectionHighlight(itemCount > 0 ? itemImages[0] : null);
        ShowProgress();
    }

    private void CacheHomePositions()
    {
        for (int i = 0; i < items.Length; i++)
            items[i].GetComponent<MinigameDragHandle>().RememberHome();
    }

    private void ResetItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].GetComponent<MinigameDragHandle>().RestoreHome();
            var group = items[i].GetComponent<CanvasGroup>();
            if (group != null) group.blocksRaycasts = true;
            itemImages[i].raycastTarget = true;
        }
    }

    private void BindDragHandles()
    {
        foreach (var handle in GetComponentsInChildren<MinigameDragHandle>(true))
        {
            handle.Receiver = this;
            handle.Index = System.Array.IndexOf(items, handle.Rect);
        }
    }

    public void DragStarted(MinigameDragHandle handle, PointerEventData eventData)
    {
        int index = handle.Index;
        if (index < 0 || index >= itemCount || sorted[index]) return;
        dragging = true;
        SetSelectionHighlight(itemImages[index]);
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = false;
        handle.BeginMove((RectTransform)transform, eventData);
    }

    public void DragMoved(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging) return;
        handle.Move(eventData);
    }

    public void DragEnded(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        int index = handle.Index;
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = true;

        int target = -1;
        for (int i = 0; i < categories.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(categories[i], eventData.position,
                eventData.pressEventCamera))
            {
                target = i;
                break;
            }
        }

        if (target == correctCategories[index])
        {
            sorted[index] = true;
            sortedCount++;
            int slot = categoryCounts[target]++;
            float width = categories[target].rect.width;
            handle.PlaceIn(categories[target], new Vector2((slot == 0 ? -.18f : .18f) * width, -.15f * width), width * .33f);
            correctMarks[index].gameObject.SetActive(true);
            itemImages[index].raycastTarget = false;
            SetFeedback("Objeto bien ordenado.");
            ShowProgress();
            int nextItem = System.Array.FindIndex(sorted, value => !value);
            SetSelectionHighlight(nextItem >= 0 ? itemImages[nextItem] : null);
            if (sortedCount >= itemCount)
                Finish(Caudal.InteractionResult.Success);
        }
        else
        {
            AddError(target < 0 ? "Suelta el objeto dentro de una categoría." : "Ese objeto va en otra categoría.");
            RestoreItem(index);
            SetSelectionHighlight(itemImages[index]);
        }
    }

    private void RestoreItem(int index)
    {
        items[index].GetComponent<MinigameDragHandle>().RestoreHome();
    }

    private void ShowProgress()
    {
        progressText.text = "Objetos ordenados: " + sortedCount + " / " + itemCount;
    }
}
