using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TrabajadorHUD : MonoBehaviour
    {
        [SerializeField] Text title, label, controls;
        [SerializeField] RawImage portrait;
        [SerializeField] TaskCard assignedTaskCard;
        void Awake()
        {
            if (assignedTaskCard == null)
                assignedTaskCard = GetComponentInChildren<TaskCard>(true);
        }

        public void ShowDialogue(string text, float duration) { }

        public void Refresh(GameManager game)
        {
            bool showPortrait = game.State == GameState.Playing;
            title.gameObject.SetActive(showPortrait);
            if (portrait != null && portrait.transform.parent != null)
                portrait.transform.parent.gameObject.SetActive(showPortrait);
            if (portrait != null) portrait.gameObject.SetActive(false);
            if (label != null) label.gameObject.SetActive(false);
            if (controls != null) controls.gameObject.SetActive(false);
            if (assignedTaskCard != null) assignedTaskCard.gameObject.SetActive(false);
        }
    }
}
