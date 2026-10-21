using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class StartScreenUI : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] GameManager game;
        [SerializeField] Button playButton;
        [SerializeField] Button exitButton;
        [SerializeField] Image playImage;
        [SerializeField] Image exitImage;
        [SerializeField] Sprite selectedSprite;
        [SerializeField] Sprite idleSprite;
        [SerializeField] RectTransform playVisual;
        [SerializeField] RectTransform exitVisual;

        int selectedButton;

        public void Initialize(GameManager gameManager)
        {
            game = gameManager;
            playButton.onClick.RemoveListener(Play);
            exitButton.onClick.RemoveListener(Exit);
            playButton.onClick.AddListener(Play);
            exitButton.onClick.AddListener(Exit);
            Refresh();
        }

        void Update()
        {
            if (game == null || game.State != GameState.StartScreen)
                return;

            if (game.Input.Pressed(game.Input.boss.up) || game.Input.Pressed(game.Input.boss.down))
            {
                selectedButton = 1 - selectedButton;
                Refresh();
            }

            if (game.Input.Pressed(game.Input.boss.confirm))
                (selectedButton == 0 ? playButton : exitButton).onClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter == playButton.gameObject)
                selectedButton = 0;
            else if (eventData.pointerEnter == exitButton.gameObject)
                selectedButton = 1;

            Refresh();
        }

        public void Refresh()
        {
            if (playImage != null) playImage.sprite = selectedButton == 0 ? selectedSprite : idleSprite;
            if (exitImage != null) exitImage.sprite = selectedButton == 1 ? selectedSprite : idleSprite;
            if (playVisual != null) playVisual.localScale = selectedButton == 0 ? Vector3.one * 1.04f : Vector3.one;
            if (exitVisual != null) exitVisual.localScale = selectedButton == 1 ? Vector3.one * 1.04f : Vector3.one;
        }

        void Play()
        {
            selectedButton = 0;
            Refresh();
            if (game != null) game.StartFromTitle();
        }

        void Exit()
        {
            selectedButton = 1;
            Refresh();
            if (game != null) game.QuitGame();
            else Application.Quit();
        }
    }
}
