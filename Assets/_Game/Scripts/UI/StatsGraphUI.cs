using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class StatsGraphUI : MonoBehaviour
    {
        [SerializeField] RectTransform marker;
        [SerializeField] Text valuesLabel, stateLabel, messageLabel;

        public void Refresh(GameManager game)
        {
            var point = new Vector2(game.Stats.Happiness / 100, game.Stats.Productivity / 100);
            marker.anchorMin = marker.anchorMax = point;
            marker.anchoredPosition = Vector2.zero;
            bool happy = game.Stats.Happiness >= 50;
            bool productive = game.Stats.Productivity >= 50;
            string quadrant;
            if (happy && productive) quadrant = "Equilibrio positivo";
            else if (happy) quadrant = "Feliz / improductivo";
            else if (productive) quadrant = "Productivo / infeliz";
            else quadrant = "Crisis";
            valuesLabel.text = $"FELICIDAD {game.Stats.Happiness:0} / 100     PRODUCTIVIDAD {game.Stats.Productivity:0} / 100";
            stateLabel.text = quadrant;
            messageLabel.text = game.Tasks.Message;
        }
    }
}
