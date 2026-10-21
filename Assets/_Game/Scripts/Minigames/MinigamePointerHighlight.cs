using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MinigamePointerHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage;

    private TimedMinigame minigame;

    private void Awake()
    {
        minigame = GetComponentInParent<TimedMinigame>();
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (minigame != null && targetImage != null)
            minigame.ShowPointerHighlight(targetImage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (minigame != null && targetImage != null)
            minigame.ClearPointerHighlight(targetImage);
    }
}
