using UnityEngine;
namespace Caudal
{
    public sealed class DayManager : MonoBehaviour
    {
        [SerializeField, Min(1)] int totalDays = 5;
        [SerializeField, Min(1)] float dayDuration = 45, quotaTarget = 60;
        float closingHappinessTotal, closingProductivityTotal;
        int recordedClosingDays, lastRecordedDay;
        public int CurrentDay { get; private set; }
        public int TotalDays => totalDays;
        public int RecordedClosingDays => recordedClosingDays;
        public float AverageHappiness => recordedClosingDays == 0 ? 0 : closingHappinessTotal / recordedClosingDays;
        public float AverageProductivity => recordedClosingDays == 0 ? 0 : closingProductivityTotal / recordedClosingDays;
        public float TimeRemaining { get; private set; }
        public float QuotaProgress { get; private set; }
        public float QuotaTarget => quotaTarget;
        public float ProductivityChange { get; private set; }
        public int MissedQuotas { get; private set; }
        public void ResetDays()
        {
            CurrentDay = 0;
            TimeRemaining = 0;
            QuotaProgress = 0;
            ProductivityChange = 0;
            MissedQuotas = 0;
            closingHappinessTotal = closingProductivityTotal = 0;
            recordedClosingDays = lastRecordedDay = 0;
        }
        public void StartDay()
        {
            CurrentDay++;
            TimeRemaining = Mathf.Max(1, dayDuration);
            QuotaProgress = 0;
            ProductivityChange = 0;
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
            float progress = quotaTarget > 0 ? QuotaProgress / quotaTarget : 0;
            ProductivityChange = progress >= 1 ? 6 : progress >= .75f ? 2 : progress >= .5f ? -4 : -8;
            if (!met)
                MissedQuotas++;
            return met;
        }

        public void RecordClosingStats(float happiness, float productivity)
        {
            if (CurrentDay <= 0 || lastRecordedDay == CurrentDay) return;
            closingHappinessTotal += happiness;
            closingProductivityTotal += productivity;
            recordedClosingDays++;
            lastRecordedDay = CurrentDay;
        }
    }
}
