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
        }

        void LateUpdate()
        {
            if (rect != null)
                rect.localRotation = baseRotation * Quaternion.Euler(0, 0, urgent ? Mathf.Sin(Time.unscaledTime * 36f) * 0.65f : 0);
        }

        public void Refresh(TaskInstance task, bool selected, bool urgent)
        {
            this.urgent = urgent;
            nameLabel.text = task.Data.taskName;
            statusLabel.gameObject.SetActive(false);
            selectionIndicator.SetActive(false);
            nameLabel.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
            if (selectionImage != null)
                selectionImage.color = urgent && selected ? new Color(.95f, .28f, .28f, .9f) : normalSelection;
            if (artwork != null)
            {
                artwork.sprite = task.Data.taskArtwork;
                artwork.enabled = task.Data.taskArtwork != null;
                artwork.color = Color.white;
            }
            Color taskColor = task.Status == TaskStatus.Completed
                ? new Color(.55f, .9f, .52f, 1f)
                : task.Status == TaskStatus.Failed
                    ? new Color(.95f, .42f, .42f, 1f)
                    : urgent ? new Color(.78f, .19f, .2f, 1f) : normalBackground;
            if (background != null)
                background.color = taskColor;
            if (nameLabel != null) nameLabel.color = urgent ? Color.white : normalName;
            if (statusLabel != null) statusLabel.color = urgent ? new Color(1f, .88f, .88f) : normalStatus;
        }
    }
}
