using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TaskCard : MonoBehaviour
    {
        [SerializeField] Text nameLabel, statusLabel;
        [SerializeField] GameObject selectionIndicator;
        [SerializeField] Image selectionImage;
        [SerializeField] Image artwork;
        [SerializeField] Image background;
        RectTransform rect;
        bool urgent;
        Color normalBackground, normalName, normalStatus, normalSelection;
        Quaternion baseRotation;

        void Awake()
        {
            rect = transform as RectTransform;
            if (rect != null) baseRotation = rect.localRotation;
            if (background == null) background = GetComponent<Image>();
            if (selectionImage == null && selectionIndicator != null) selectionImage = selectionIndicator.GetComponent<Image>();
            if (background != null) normalBackground = background.color;
            if (nameLabel != null) normalName = nameLabel.color;
            if (statusLabel != null) normalStatus = statusLabel.color;
            if (selectionImage != null) normalSelection = selectionImage.color;
            if (artwork == null) CreateArtworkPlaceholder();
        }

        void LateUpdate()
        {
            if (rect != null)
                rect.localRotation = baseRotation * Quaternion.Euler(0, 0, urgent ? Mathf.Sin(Time.unscaledTime * 36f) * 0.65f : 0);
        }

        void CreateArtworkPlaceholder()
        {
            var slot = new GameObject("TaskArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            slot.transform.SetParent(transform, false);
            var slotRect = (RectTransform)slot.transform;
            slotRect.anchorMin = new Vector2(0, .5f);
            slotRect.anchorMax = new Vector2(0, .5f);
            slotRect.pivot = new Vector2(0, .5f);
            slotRect.anchoredPosition = new Vector2(7, 0);
            slotRect.sizeDelta = new Vector2(22, 22);
            artwork = slot.GetComponent<Image>();
            artwork.color = new Color(.35f, .39f, .4f, 1);
            artwork.raycastTarget = false;
            artwork.preserveAspect = true;
            if (nameLabel != null)
            {
                var labelRect = nameLabel.rectTransform;
                labelRect.anchorMin = new Vector2(.12f, labelRect.anchorMin.y);
                labelRect.offsetMin = Vector2.zero;
            }
        }

        public void Refresh(TaskInstance task, bool selected, bool urgent)
        {
            this.urgent = urgent;
            nameLabel.text = task.Data.taskName;
            statusLabel.text = task.Status.ToString();
            selectionIndicator.SetActive(selected);
            if (selectionImage != null)
                selectionImage.color = urgent && selected ? new Color(.95f, .28f, .28f, .9f) : normalSelection;
            if (artwork != null)
            {
                artwork.sprite = task.Data.taskArtwork;
                artwork.enabled = true;
                artwork.color = task.Data.taskArtwork != null ? Color.white : new Color(.35f, .39f, .4f, 1);
            }
            if (background != null)
                background.color = urgent ? new Color(0.78f, 0.19f, 0.2f, 1f) : normalBackground;
            if (nameLabel != null) nameLabel.color = urgent ? Color.white : normalName;
            if (statusLabel != null) statusLabel.color = urgent ? new Color(1f, .88f, .88f) : normalStatus;
        }
    }
}
