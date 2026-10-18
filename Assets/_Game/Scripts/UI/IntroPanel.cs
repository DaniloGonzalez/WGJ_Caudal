using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class IntroPanel : MonoBehaviour
    {
        [SerializeField] Text teamLabel;
        [SerializeField] CanvasGroup group;

        public void Show(string teamName)
        {
            teamLabel.text = teamName;
            group.alpha = 1;
            gameObject.SetActive(true);
        }
        public void SetAlpha(float alpha) => group.alpha = alpha;
        public void Hide() => gameObject.SetActive(false);
    }
}
