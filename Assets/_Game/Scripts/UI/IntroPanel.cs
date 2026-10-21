using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class IntroPanel : MonoBehaviour
    {
        [SerializeField] Text messageLabel;
        [SerializeField, TextArea(3, 8)] string introMessage = "BIENVENIDO A DON'T MAKE ME MISERABLE.\nEN DONDE TUS DECISIONES Y HABILIDAD COMO JEFATURA\nY TRABAJADOR DETERMINARÁN EL FUTURO DE TU EMPRESA.\nQUE TUS DECISIONES NO NOS HAGAN A TODOS MISERABLES\nO QUIEBREN LA EMPRESA\n¡AMEN!";
        [SerializeField] Button continueButton;

        GameManager game;

        public void Initialize(GameManager gameManager)
        {
            game = gameManager;
            if (continueButton == null)
                return;

            continueButton.onClick.RemoveListener(Continue);
            continueButton.onClick.AddListener(Continue);
        }

        public void Show()
        {
            if (messageLabel != null)
                messageLabel.text = introMessage;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (game == null || !gameObject.activeInHierarchy)
                return;

            if (game.Input != null && game.Input.Pressed(game.Input.boss.confirm))
                Continue();
        }

        void Continue()
        {
            if (game == null)
                return;

            game.ContinueFromIntro();
            Hide();
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
