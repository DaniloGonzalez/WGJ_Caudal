using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class SharedHUD : MonoBehaviour
    {
        [SerializeField] Text dayLabel, clockLabel, quotaLabel, simulatorLabel;

        public void Refresh(GameManager game)
        {
            int time = Mathf.CeilToInt(game.Days.TimeRemaining);
            dayLabel.text = $"DÍA {game.Days.CurrentDay:00}";
            clockLabel.text = $"{time / 60:00}:{time % 60:00}";
            if (quotaLabel != null) quotaLabel.gameObject.SetActive(false);
            if (simulatorLabel != null) simulatorLabel.gameObject.SetActive(false);
        }
    }
}
