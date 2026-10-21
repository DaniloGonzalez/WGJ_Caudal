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
            if (simulatorLabel != null)
            {
                simulatorLabel.text = $"OLEADA {game.Tasks.WaveNumber:00}  ·  {Mathf.CeilToInt(game.Tasks.WaveTimeRemaining):00}s";
                simulatorLabel.fontSize = 12;
                simulatorLabel.resizeTextForBestFit = true;
                simulatorLabel.resizeTextMinSize = 9;
                simulatorLabel.resizeTextMaxSize = 12;
                simulatorLabel.alignment = TextAnchor.MiddleCenter;
                simulatorLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                simulatorLabel.verticalOverflow = VerticalWrapMode.Truncate;
                simulatorLabel.color = new Color(.62f, .95f, .83f, 1f);
                simulatorLabel.gameObject.SetActive(true);
            }
        }
    }
}
