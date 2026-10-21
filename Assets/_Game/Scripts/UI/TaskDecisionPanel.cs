using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TaskDecisionPanel : MonoBehaviour
    {
        [SerializeField] Image windowBackground, acceptBackground, rejectBackground;
        [SerializeField] Text titleLabel, descriptionLabel, instructionLabel, acceptLabel, rejectLabel;
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
            if (descriptionLabel != null) descriptionLabel.text = GetDescription(task, owner);
            string instruction = string.IsNullOrWhiteSpace(task.Data.instruction)
                ? task.Data.description
                : task.Data.instruction;
            if (instructionLabel != null) instructionLabel.text = "<b>INSTRUCCIÓN</b>\n" + instruction;
            if (acceptLabel != null) acceptLabel.text = "ACEPTAR";
            if (rejectLabel != null) rejectLabel.text = "RECHAZAR";
            ConfigureText(titleLabel, 16, 12, 16, FontStyle.Bold, TextAnchor.MiddleLeft);
            ConfigureText(descriptionLabel, 13, 11, 13, FontStyle.Normal, TextAnchor.MiddleCenter);
            ConfigureText(instructionLabel, 13, 11, 13, FontStyle.Normal, TextAnchor.MiddleCenter);
            gameObject.SetActive(true);
        }

        static string GetDescription(TaskInstance task, GameManager owner)
        {
            if (task.Data.taskId != "informe") return task.Data.description;
            switch (owner.Days.CurrentDay)
            {
                case 1: return "Un cliente pide adelantar un proyecto para el viernes. El equipo ya tiene un ánimo raro por los nuevos sensores. ¿Qué haces?";
                case 2: return "Un compañero pide tres días libres para cuidar a su gatito recién adoptado, que tiene problemas de salud. ¿Qué haces?";
                case 3: return "Hay presupuesto para una de dos cosas: una pausa activa con sala de descanso o nuevos equipos para mejorar la asignación de tareas.";
                case 4: return "Tomás te solicita un aumento de sueldo. Al parecer, los Churus han subido de precio en esta economía. ¿Qué haces?";
                case 5: return "Quedan entregas pendientes para cerrar la semana. ¿Qué haces?";
                default: return task.Data.description;
            }
        }

        static void ConfigureText(Text label, int size, int minSize, int maxSize, FontStyle style, TextAnchor alignment)
        {
            if (label == null) return;
            label.fontSize = size;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = minSize;
            label.resizeTextMaxSize = maxSize;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = new Color(.1f, .08f, .18f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
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
