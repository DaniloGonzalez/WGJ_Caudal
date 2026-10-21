using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class StatsGraphUI : MonoBehaviour
    {
        [Header("Medidores")]
        [SerializeField] Image happinessMeter;
        [SerializeField] Image productivityMeter;
        [SerializeField] Text happinessValue;
        [SerializeField] Text productivityValue;

        [Header("Tintado progresivo (0 / 50 / 100)")]
        [SerializeField] Color lowColor = new Color(231f / 255f, 106f / 255f, 106f / 255f, 1f);
        [SerializeField] Color middleColor = Color.white;
        [SerializeField] Color highColor = new Color(101f / 255f, 199f / 255f, 131f / 255f, 1f);

        public void Refresh(GameManager game)
        {
            UpdateMeter(happinessMeter, happinessValue, game.Stats.Happiness);
            UpdateMeter(productivityMeter, productivityValue, game.Stats.Productivity);
        }

        void UpdateMeter(Image meter, Text valueText, float value)
        {
            float clampedValue = Mathf.Clamp(value, 0f, 100f);
            meter.color = GetMeterColor(clampedValue);
            meter.fillAmount = 1f;
            valueText.text = Mathf.RoundToInt(clampedValue).ToString();
        }

        Color GetMeterColor(float value)
        {
            if (value <= 50f) return Color.Lerp(lowColor, middleColor, value / 50f);
            return Color.Lerp(middleColor, highColor, (value - 50f) / 50f);
        }
    }
}
