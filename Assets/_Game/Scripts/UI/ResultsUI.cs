using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class ResultsUI : MonoBehaviour
    {
        [Header("Resumen")]
        [SerializeField] Text dayText;
        [SerializeField] Text completedText;
        [SerializeField] Text completedCaption;
        [SerializeField] Text detailText;
        [SerializeField] Text promptText;
        [SerializeField] Text controls;

        [Header("Selección para mañana")]
        [SerializeField] Button bossButton;
        [SerializeField] Button workerButton;
        [SerializeField] Button continueButton;
        [SerializeField] Image bossImage;
        [SerializeField] Image workerImage;
        [SerializeField] Sprite bossSelectedSprite;
        [SerializeField] Sprite bossIdleSprite;
        [SerializeField] Sprite workerSelectedSprite;
        [SerializeField] Sprite workerIdleSprite;

        [Header("Finales")]
        [SerializeField] Image endingArtwork;
        [SerializeField] Sprite bestPlaceSprite;
        [SerializeField] Sprite miserableSuccessSprite;
        [SerializeField] Sprite happyCrisisSprite;
        [SerializeField] Sprite officeCollapseSprite;
        [SerializeField] Color finalTextColor = new Color32(250, 248, 255, 255);
        [SerializeField] Color finalPanelColor = new Color32(42, 38, 58, 224);
        [SerializeField] Image finalInfoBackground;
        [SerializeField] Image completedBackground;
        [SerializeField] Image selectionBackground;

        [Header("Indicadores")]
        [SerializeField] Image happinessMeter;
        [SerializeField] Image productivityMeter;
        [SerializeField] Text happinessValue;
        [SerializeField] Text productivityValue;
        [SerializeField] Color lowColor = new Color(231f / 255f, 106f / 255f, 106f / 255f, 1f);
        [SerializeField] Color middleColor = Color.white;
        [SerializeField] Color highColor = new Color(101f / 255f, 199f / 255f, 131f / 255f, 1f);

        GameManager game;
        Color dayTextColor, captionColor, completedColor, detailColor, promptColor;
        RectTransform detailRect, promptRect;
        Vector2 detailAnchorMin, detailAnchorMax, promptAnchorMin, promptAnchorMax;
        int detailFontSize;
        Color infoBackgroundColor;
        RectTransform infoBackgroundRect;
        Vector2 infoBackgroundAnchorMin, infoBackgroundAnchorMax;
        int infoBackgroundSiblingIndex;

        void Awake()
        {
            dayTextColor = dayText.color;
            captionColor = completedCaption.color;
            completedColor = completedText.color;
            detailColor = detailText.color;
            promptColor = promptText.color;
            detailRect = detailText.rectTransform;
            promptRect = promptText.rectTransform;
            detailAnchorMin = detailRect.anchorMin;
            detailAnchorMax = detailRect.anchorMax;
            promptAnchorMin = promptRect.anchorMin;
            promptAnchorMax = promptRect.anchorMax;
            detailFontSize = detailText.fontSize;
            infoBackgroundRect = finalInfoBackground.rectTransform;
            infoBackgroundAnchorMin = infoBackgroundRect.anchorMin;
            infoBackgroundAnchorMax = infoBackgroundRect.anchorMax;
            infoBackgroundSiblingIndex = infoBackgroundRect.GetSiblingIndex();
            infoBackgroundColor = finalInfoBackground.color;
        }

        public void Initialize(GameManager gameManager)
        {
            game = gameManager;
            bossButton.onClick.RemoveAllListeners();
            workerButton.onClick.RemoveAllListeners();
            continueButton.onClick.RemoveAllListeners();
            bossButton.onClick.AddListener(() => game.SelectNextDayCharacter(true));
            workerButton.onClick.AddListener(() => game.SelectNextDayCharacter(false));
            continueButton.onClick.AddListener(Continue);
        }

        public void Continue()
        {
            if (game == null) return;
            if (game.State == GameState.DaySummary) game.ContinueFromDaySummary();
            else if (game.State == GameState.GameOver) game.Restart();
        }

        public void Refresh(GameManager game)
        {
            this.game = game;
            bool hasNextDay = game.Days.CurrentDay < game.Days.TotalDays;
            bool isFinal = game.State == GameState.GameOver;
            string[] resultLines = game.ResultText.Split('\n');
            if (endingArtwork != null)
            {
                endingArtwork.gameObject.SetActive(isFinal);
                endingArtwork.sprite = GetEndingSprite(resultLines[0]);
                endingArtwork.preserveAspect = true;
            }
            dayText.fontSize = isFinal ? 46 : 76;
            dayText.text = isFinal ? FormatEndingTitle(resultLines[0]) : $"DÍA: {game.Days.CurrentDay:00}";
            completedCaption.text = isFinal ? "RESULTADO FINAL" : "TAREAS COMPLETADAS";
            completedText.text = isFinal ? "5 DÍAS" :
                game.Tasks.CompletedThisDay.ToString();
            detailText.text = isFinal
                ? game.ResultText.Replace(resultLines[0] + "\n\n", "").Replace("\n\nClic para reiniciar", "")
                : $"OLEADAS {game.Tasks.WaveNumber}  ·  RECHAZADAS {game.Tasks.RejectedThisDay}\n" +
                    $"FALLIDAS {game.Tasks.FailedThisDay}  ·  VENCIDAS {game.Tasks.ExpiredThisDay}";
            promptText.text = isFinal ? "HAZ CLIC EN LA FLECHA PARA REINICIAR" : hasNextDay
                ? "SELECCIONA PERSONAJE\nPARA EL SIGUIENTE DÍA" : "SEMANA COMPLETADA";
            controls.text = isFinal ? "" : hasNextDay
                ? $"{game.Input.boss.left} / {game.Input.boss.right}  ·  {game.Input.boss.confirm}"
                : $"{game.Input.boss.confirm}: ver final";
            continueButton.interactable = true;

            bossButton.gameObject.SetActive(hasNextDay && !isFinal);
            workerButton.gameObject.SetActive(hasNextDay && !isFinal);
            bossImage.sprite = hasNextDay && game.NextDayBossSelected ? bossSelectedSprite : bossIdleSprite;
            workerImage.sprite = hasNextDay && !game.NextDayBossSelected ? workerSelectedSprite : workerIdleSprite;

            SetFinalLayout(isFinal);
            if (isFinal)
            {
                dayText.color = completedCaption.color = completedText.color = detailText.color = promptText.color = finalTextColor;
                completedCaption.gameObject.SetActive(false);
                completedText.gameObject.SetActive(false);
                happinessMeter.transform.parent.gameObject.SetActive(false);
                productivityMeter.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                dayText.color = dayTextColor;
                completedCaption.color = captionColor;
                completedText.color = completedColor;
                detailText.color = detailColor;
                promptText.color = promptColor;
                completedCaption.gameObject.SetActive(true);
                completedText.gameObject.SetActive(true);
                happinessMeter.transform.parent.gameObject.SetActive(true);
                productivityMeter.transform.parent.gameObject.SetActive(true);
            }

            float happiness = isFinal && game.Days.RecordedClosingDays >= game.Days.TotalDays
                ? game.Days.AverageHappiness : game.Stats.Happiness;
            float productivity = isFinal && game.Days.RecordedClosingDays >= game.Days.TotalDays
                ? game.Days.AverageProductivity : game.Stats.Productivity;
            UpdateMeter(happinessMeter, happinessValue, happiness);
            UpdateMeter(productivityMeter, productivityValue, productivity);
        }

        Sprite GetEndingSprite(string title)
        {
            switch (title)
            {
                case "Best Place to Work": return bestPlaceSprite;
                case "Éxito miserable": return miserableSuccessSprite;
                case "Oficina feliz en crisis": return happyCrisisSprite;
                case "Colapso de oficina": return officeCollapseSprite;
                default: return null;
            }
        }

        string FormatEndingTitle(string title)
        {
            switch (title)
            {
                case "Best Place to Work": return "Best Place\nto Work";
                case "Oficina feliz en crisis": return "Oficina feliz\nen crisis";
                default: return title;
            }
        }

        void SetFinalLayout(bool isFinal)
        {
            if (detailRect == null || promptRect == null) return;
            detailRect.anchorMin = isFinal ? new Vector2(.07f, .37f) : detailAnchorMin;
            detailRect.anchorMax = isFinal ? new Vector2(.37f, .65f) : detailAnchorMax;
            promptRect.anchorMin = isFinal ? new Vector2(.07f, .27f) : promptAnchorMin;
            promptRect.anchorMax = isFinal ? new Vector2(.38f, .35f) : promptAnchorMax;
            dayText.rectTransform.anchorMin = isFinal ? new Vector2(.07f, .70f) : new Vector2(.07f, .60f);
            dayText.rectTransform.anchorMax = isFinal ? new Vector2(.38f, .88f) : new Vector2(.48f, .80f);
            dayText.horizontalOverflow = isFinal ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            detailText.horizontalOverflow = isFinal ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            promptText.horizontalOverflow = isFinal ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            detailText.fontSize = isFinal ? 20 : detailFontSize;
            dayText.fontSize = isFinal ? 34 : 76;
            completedBackground.gameObject.SetActive(!isFinal);
            finalInfoBackground.gameObject.SetActive(isFinal);
            selectionBackground.gameObject.SetActive(!isFinal);
            if (isFinal)
            {
                infoBackgroundRect.anchorMin = new Vector2(.04f, .24f);
                infoBackgroundRect.anchorMax = new Vector2(.40f, .91f);
                infoBackgroundRect.SetSiblingIndex(1);
                finalInfoBackground.color = finalPanelColor;
            }
            else
            {
                infoBackgroundRect.anchorMin = infoBackgroundAnchorMin;
                infoBackgroundRect.anchorMax = infoBackgroundAnchorMax;
                infoBackgroundRect.SetSiblingIndex(infoBackgroundSiblingIndex);
                finalInfoBackground.color = infoBackgroundColor;
            }
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
