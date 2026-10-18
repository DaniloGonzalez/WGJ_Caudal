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
            dayLabel.text = $"DÍA {game.Days.CurrentDay}/{game.Days.TotalDays}";
            clockLabel.text = $"TIEMPO {time / 60:00}:{time % 60:00}";
            quotaLabel.text = $"CUOTA {game.Days.QuotaProgress:0}/{game.Days.QuotaTarget:0}";
            simulatorLabel.gameObject.SetActive(game.Simulator);
        }
    }
}
