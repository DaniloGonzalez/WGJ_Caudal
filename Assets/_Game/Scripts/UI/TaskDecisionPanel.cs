using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TaskDecisionPanel : MonoBehaviour
    {
        [SerializeField] Image windowBackground, acceptBackground, rejectBackground;
        [SerializeField] Text titleLabel, instructionLabel, acceptLabel, rejectLabel;
        [SerializeField] Button acceptButton, rejectButton;
        [SerializeField] Sprite bossWindow, workerWindow, bossAcceptButton, workerAcceptButton, rejectButtonSprite;
        [SerializeField, Range(0.1f, 0.4f)] float rolePanelOffset = 0.25f;

        GameManager game;
        int taskIndex = -1;
        bool buttonsBound;

        public void Show(GameManager owner, int index)
        {
            if (owner == null || owner.State != GameState.Playing || index < 0 || index >= owner.Tasks.Tasks.Count)
                return;

            var task = owner.Tasks.Tasks[index];
            if (task.Owner != owner.ActiveRole || task.Status != TaskStatus.Available)
                return;

            game = owner;
            taskIndex = index;
            BindButtons();

            bool isBoss = task.Owner == CharacterRole.Boss;
            var windowRect = windowBackground != null ? windowBackground.rectTransform : null;
            var rootRect = transform as RectTransform;
            if (windowRect != null && rootRect != null)
                windowRect.anchoredPosition = new Vector2(0, rootRect.rect.height * rolePanelOffset * (isBoss ? 1 : -1));

            Sprite hoverSprite = isBoss ? bossAcceptButton : workerAcceptButton;
            SetButtonHoverState(acceptButton, acceptBackground, hoverSprite);
            SetButtonHoverState(rejectButton, rejectBackground, hoverSprite);

            if (windowBackground != null)
            {
                windowBackground.sprite = isBoss ? bossWindow : workerWindow;
                windowBackground.preserveAspect = true;
                windowBackground.color = Color.white;
            }
            if (acceptBackground != null)
            {
                acceptBackground.sprite = rejectButtonSprite;
                acceptBackground.preserveAspect = true;
                acceptBackground.color = Color.white;
            }
            if (rejectBackground != null)
            {
                rejectBackground.sprite = rejectButtonSprite;
                rejectBackground.preserveAspect = true;
                rejectBackground.color = Color.white;
            }

            if (titleLabel != null) titleLabel.text = task.Data.taskName;
            if (instructionLabel != null) instructionLabel.text = task.Data.description;
            if (acceptLabel != null) acceptLabel.text = "ACEPTAR";
            if (rejectLabel != null) rejectLabel.text = "RECHAZAR";
            gameObject.SetActive(true);
        }

        static void SetButtonHoverState(Button button, Image background, Sprite hoverSprite)
        {
            if (button == null) return;
            if (background != null) button.targetGraphic = background;
            button.transition = Selectable.Transition.SpriteSwap;
            var sprites = button.spriteState;
            sprites.highlightedSprite = hoverSprite;
            sprites.pressedSprite = hoverSprite;
            button.spriteState = sprites;
        }

        void BindButtons()
        {
            if (buttonsBound) return;
            if (acceptButton != null) acceptButton.onClick.AddListener(AcceptTask);
            if (rejectButton != null) rejectButton.onClick.AddListener(RejectTask);
            buttonsBound = true;
        }

        void AcceptTask()
        {
            var owner = game;
            int index = taskIndex;
            Hide();
            if (owner != null) owner.Tasks.StartOwnTask(index);
        }

        void RejectTask()
        {
            var owner = game;
            int index = taskIndex;
            Hide();
            if (owner != null) owner.Tasks.RejectOwnTask(index);
        }

        public void Hide()
        {
            taskIndex = -1;
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
    }
}
