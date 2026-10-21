using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Caudal
{
    public sealed class TaskCard : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] Text nameLabel, statusLabel;
        [SerializeField] GameObject selectionIndicator;
        [SerializeField] Image selectionImage;
        [SerializeField] Image artwork;
        [SerializeField] Image background;
        [SerializeField] Button acceptButton;
        [SerializeField] Button rejectButton;
        [SerializeField] TaskDecisionPanel decisionPanel;
        RectTransform rect;
        CanvasGroup canvasGroup;
        bool urgent;
        bool waveWarning;
        float waveOffset;
        float waveAlpha = 1f;
        Color normalName;
        Quaternion baseRotation;
        Vector2 basePosition;
        GameManager game;
        int taskIndex = -1;
        float floatSeed;
        void Awake()
        {
            rect = transform as RectTransform;
            canvasGroup = GetComponent<CanvasGroup>();
            if (rect != null)
            {
                baseRotation = rect.localRotation;
                basePosition = rect.anchoredPosition;
            }
            floatSeed = Random.value * 100f;
            if (background == null) background = GetComponent<Image>();
            if (background != null) background.raycastTarget = true;
            if (selectionImage == null && selectionIndicator != null) selectionImage = selectionIndicator.GetComponent<Image>();
            if (nameLabel != null) normalName = nameLabel.color;
            if (statusLabel != null) statusLabel.gameObject.SetActive(false);
            if (selectionIndicator != null) selectionIndicator.SetActive(false);
        }

        void LateUpdate()
        {
            if (rect != null)
            {
                float phase = Time.unscaledTime * 1.6f + floatSeed;
                float tilt = Mathf.Sin(phase * .7f) * .45f;
                if (urgent) tilt += Mathf.Sin(phase * 2f) * .2f;
                float shake = waveWarning ? Mathf.Sin(Time.unscaledTime * 45f + floatSeed) * 3f : 0f;
                rect.anchoredPosition = basePosition + Vector2.up * Mathf.Sin(phase) * 4f + Vector2.right * (shake + waveOffset);
                rect.localRotation = baseRotation * Quaternion.Euler(0, 0, tilt);
            }
            if (canvasGroup != null) canvasGroup.alpha = waveAlpha;
        }

        public void SetWaveWarning(bool active) => waveWarning = active;

        public void SetWaveTransition(float horizontalOffset, float alpha)
        {
            waveWarning = false;
            waveOffset = horizontalOffset;
            waveAlpha = Mathf.Clamp01(alpha);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = waveAlpha;
                canvasGroup.blocksRaycasts = waveAlpha > .8f;
                canvasGroup.interactable = waveAlpha > .8f;
            }
        }

        public void ResetWaveTransition()
        {
            waveOffset = 0;
            waveAlpha = 1;
            waveWarning = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }

        public void Bind(GameManager owner, int index)
        {
            Bind(owner, index, decisionPanel);
        }

        public void Bind(GameManager owner, int index, TaskDecisionPanel panel)
        {
            game = owner;
            taskIndex = index;
            decisionPanel = panel;
            DisableOldButtonHotspots();
        }

        void DisableOldButtonHotspots()
        {
            if (acceptButton != null)
            {
                acceptButton.enabled = false;
                if (acceptButton.targetGraphic != null) acceptButton.targetGraphic.raycastTarget = false;
            }
            if (rejectButton != null)
            {
                rejectButton.enabled = false;
                if (rejectButton.targetGraphic != null) rejectButton.targetGraphic.raycastTarget = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (game != null) game.SelectTask(taskIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (game == null || eventData.button != PointerEventData.InputButton.Left) return;
            game.SelectTask(taskIndex);
            if (decisionPanel != null) decisionPanel.Show(game, taskIndex);
        }

        public void Refresh(TaskInstance task, bool selected, bool urgent)
        {
            this.urgent = urgent;
            nameLabel.text = GetDisplayName(task.Data.taskName);
            if (statusLabel != null)
                statusLabel.gameObject.SetActive(false);
            if (selectionIndicator != null) selectionIndicator.SetActive(false);
            if (nameLabel != null)
            {
                nameLabel.fontSize = 14;
                nameLabel.resizeTextForBestFit = true;
                nameLabel.resizeTextMinSize = 11;
                nameLabel.resizeTextMaxSize = 13;
                nameLabel.fontStyle = FontStyle.Bold;
                nameLabel.alignment = TextAnchor.MiddleLeft;
                nameLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                nameLabel.verticalOverflow = VerticalWrapMode.Truncate;
            }
            if (artwork != null)
            {
                artwork.sprite = task.Data.taskArtwork;
                artwork.enabled = task.Data.taskArtwork != null;
                artwork.color = Color.white;
            }
            if (selectionImage != null) selectionImage.enabled = false;
            if (nameLabel != null) nameLabel.color = urgent && selected ? Color.white : normalName;
            transform.localScale = Vector3.one * (selected ? 1.035f : 1f);
        }

        static string GetDisplayName(string taskName)
        {
            return taskName switch
            {
                "Organizar agenda" => "Organizar\nagenda",
                "Preparar reunión y documentos" => "Preparar reunión\n+ documentos",
                "Aprobar solicitudes" => "Aprobar\nsolicitudes",
                "Resolver urgencia comercial" => "Resolver urgencia\ncomercial",
                "Reparar conexión de datos" => "Reparar conexión\nde datos",
                "Limpiar correos" => "Limpiar\ncorreos",
                "Fotocopiar documentos" => "Fotocopiar\ndocumentos",
                "Revisar inventario" => "Revisar\ninventario",
                _ => taskName
            };
        }
    }
}
