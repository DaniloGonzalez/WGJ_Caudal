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
            happinessValue.color = Color.white;
            productivityValue.color = Color.white;
            UpdateMeter(happinessMeter, happinessValue, game.Stats.Happiness, happinessColor, false);
            UpdateMeter(productivityMeter, productivityValue, game.Stats.Productivity, productivityColor, true);
        }

        void UpdateMeter(Image meter, Text valueText, float value, Color color, bool tintByValue)
        {
            float clampedValue = Mathf.Clamp(value, 0f, 100f);
            meter.color = tintByValue ? Color.Lerp(Color.white, color, clampedValue / 100f) : color;
            meter.fillAmount = 1f;
            valueText.text = Mathf.RoundToInt(clampedValue).ToString();
        }
    }
}
