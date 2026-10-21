using UnityEngine;
using UnityEngine.EventSystems;

public interface IMinigameDragReceiver
{
    void DragStarted(MinigameDragHandle handle, PointerEventData eventData);
    void DragMoved(MinigameDragHandle handle, PointerEventData eventData);
    void DragEnded(MinigameDragHandle handle, PointerEventData eventData);
}

public sealed class MinigameDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Index;
    public IMinigameDragReceiver Receiver { private get; set; }
    public RectTransform Rect => (RectTransform)transform;

    private Transform homeParent;
    private Vector2 homeMin, homeMax, homePivot, homeSize, homePosition;
    private Vector3 homeScale;
    private int homeSibling;
    private Vector3 pointerOffset;
    private bool hasHome;

    public void RememberHome()
    {
        if (hasHome) return;
        hasHome = true;
        homeParent = Rect.parent;
        homeMin = Rect.anchorMin;
        homeMax = Rect.anchorMax;
        homePivot = Rect.pivot;
        homeSize = Rect.sizeDelta;
        homePosition = Rect.anchoredPosition;
        homeScale = Rect.localScale;
        homeSibling = Rect.GetSiblingIndex();
    }

    public void RestoreHome()
    {
        if (!hasHome) return;
        Rect.SetParent(homeParent, false);
        Rect.anchorMin = homeMin;
        Rect.anchorMax = homeMax;
        Rect.pivot = homePivot;
        Rect.sizeDelta = homeSize;
        Rect.anchoredPosition = homePosition;
        Rect.localScale = homeScale;
        Rect.SetSiblingIndex(homeSibling);
        SetRaycasts(true);
    }

    public void BeginMove(RectTransform surface, PointerEventData data)
    {
        RememberHome();
        // Keep the grabbed point under the cursor, even with stretched anchors.
        Vector2 size = Rect.rect.size;
        Vector3 position = Rect.position;
        Rect.SetParent(surface, true);
        Rect.anchorMin = Rect.anchorMax = new Vector2(.5f, .5f);
        Rect.sizeDelta = size;
        Rect.position = position;
        Rect.SetAsLastSibling();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(surface, data.pressPosition,
            data.pressEventCamera, out Vector3 pointer);
        pointerOffset = Rect.position - pointer;
        SetRaycasts(false);
        Move(data);
    }

    public void Move(PointerEventData data)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)Rect.parent,
            data.position, data.pressEventCamera, out Vector3 pointer))
            Rect.position = pointer + pointerOffset;
    }

    public void PlaceIn(RectTransform target, Vector2 position, float size)
    {
        Rect.SetParent(target, false);
        Rect.anchorMin = Rect.anchorMax = new Vector2(.5f, .5f);
        Rect.pivot = new Vector2(.5f, .5f);
        Rect.sizeDelta = Vector2.one * size;
        Rect.anchoredPosition = position;
        Rect.localScale = Vector3.one;
        SetRaycasts(false);
    }

    private void SetRaycasts(bool value)
    {
        var group = GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = value;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Receiver?.DragStarted(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Receiver?.DragMoved(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Receiver?.DragEnded(this, eventData);
    }
}
