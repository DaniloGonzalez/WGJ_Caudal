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

        [Header("Colores de indicadores")]
        [SerializeField] Color happinessColor = new Color32(139, 232, 77, 255);
        [SerializeField] Color productivityColor = new Color32(159, 151, 254, 255);

        public void Refresh(GameManager game)
        {
            UpdateMeter(happinessMeter, happinessValue, game.Stats.Happiness, happinessColor);
            UpdateMeter(productivityMeter, productivityValue, game.Stats.Productivity, productivityColor);
        }

        void UpdateMeter(Image meter, Text valueText, float value, Color color)
        {
            float clampedValue = Mathf.Clamp(value, 0f, 100f);
            meter.color = Color.Lerp(Color.white, color, clampedValue / 100f);
            meter.fillAmount = 1f;
            valueText.text = Mathf.RoundToInt(clampedValue).ToString();
        }
    }
}
