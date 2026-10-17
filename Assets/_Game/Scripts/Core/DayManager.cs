using UnityEngine;
namespace Caudal
{
    public sealed class DayManager : MonoBehaviour
    {
        [SerializeField, Min(1)] int totalDays = 5;
        [SerializeField, Min(1)] float dayDuration = 90, quotaTarget = 60;
        public int CurrentDay { get; private set; }
        public int TotalDays => totalDays;
        public float TimeRemaining { get; private set; }
        public float QuotaProgress { get; private set; }
        public float QuotaTarget => quotaTarget;
        public int MissedQuotas { get; private set; }
        public void ResetDays()
        {
            CurrentDay = 0;
            TimeRemaining = 0;
            QuotaProgress = 0;
            MissedQuotas = 0;
        }
        public void StartDay()
        {
            CurrentDay++;
            TimeRemaining = Mathf.Max(1, dayDuration);
            QuotaProgress = 0;
        }
        public bool Tick(float delta)
        {
            TimeRemaining = Mathf.Max(0, TimeRemaining - delta);
            return TimeRemaining <= 0;
        }
        public void AddProduction(float value)
        {
            QuotaProgress += Mathf.Max(0, value);
        }
        public bool CloseDay()
        {
            bool met = QuotaProgress >= quotaTarget;
            if (!met)
                MissedQuotas++;
            return met;
        }
    }
}
