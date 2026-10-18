using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class StatsGraphUI : MonoBehaviour
    {
        [SerializeField] Image happinessMeter, productivityMeter;

        public void Refresh(GameManager game)
        {
            happinessMeter.color = GetMeterColor(game.Stats.Happiness);
            productivityMeter.color = GetMeterColor(game.Stats.Productivity);
        }

        static Color GetMeterColor(float value)
        {
            float distance = Mathf.Clamp01(Mathf.Abs(value - 50f) / 50f);
            Color target = value >= 50f
                ? new Color(0.2f, 0.95f, 0.25f, 1f)
                : new Color(1f, 0.2f, 0.2f, 1f);
            return Color.Lerp(Color.white, target, distance);
        }
    }
}
