using UnityEngine;
namespace Caudal
{
    public sealed class StatsManager : MonoBehaviour
    {
        [SerializeField] float initialHappiness = 50, initialProductivity = 50;
        public float Happiness { get; private set; }
        public float Productivity { get; private set; }
        public void ResetStats()
        {
            Happiness = Mathf.Clamp(initialHappiness, 0, 100);
            Productivity = Mathf.Clamp(initialProductivity, 0, 100);
        }
        public void Apply(float happiness, float productivity)
        {
            Happiness = Mathf.Clamp(Happiness + happiness, 0, 100);
            Productivity = Mathf.Clamp(Productivity + productivity, 0, 100);
        }
    }
}
