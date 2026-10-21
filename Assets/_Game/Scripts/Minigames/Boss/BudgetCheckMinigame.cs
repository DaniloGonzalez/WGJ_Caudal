using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BudgetCheckMinigame : TimedMinigame
{
    public override string ControlsHint => "Haz clic en la operación con el resultado incorrecto.";

    [SerializeField] private TMP_Text[] operationLabels;
    [SerializeField] private TMP_Text[] answerLabels;
    [SerializeField] private Button[] operationButtons;
    [SerializeField] private Image[] resultImages;
    [SerializeField] private Sprite correctResultSprite;
    [SerializeField] private Sprite incorrectResultSprite;

    private int incorrectIndex;

    protected override void OnMinigameStarted()
    {
        int count = Mathf.Min(operationLabels.Length, 2 + Difficulty);
        incorrectIndex = Random.Range(0, count);

        for (int i = 0; i < operationLabels.Length; i++)
        {
            bool visible = i < count;
            operationLabels[i].transform.parent.gameObject.SetActive(visible);
            if (!visible) continue;

            int left = Random.Range(180, 950);
            int right = Random.Range(90, 620);
            bool addition = Random.value > .35f;
            int answer = addition ? left + right : left - right;
            bool isIncorrect = i == incorrectIndex;
            int shownAnswer = isIncorrect ? answer + Random.Range(15, 90) : answer;
            operationLabels[i].text = left + (addition ? " + " : " − ") + right + " =";
            answerLabels[i].text = shownAnswer.ToString();
            // Keep every answer chip neutral until the player has made a choice.
            resultImages[i].sprite = null;
            resultImages[i].color = Color.white;
            operationButtons[i].onClick.RemoveAllListeners();
            int index = i;
            operationButtons[i].onClick.AddListener(() => SelectOperation(index));
        }
    }

    private void SelectOperation(int index)
    {
        if (index == incorrectIndex)
        {
            resultImages[index].sprite = incorrectResultSprite;
            resultImages[index].preserveAspect = true;
            SetFeedback("Operación incorrecta encontrada.");
            Finish(Caudal.InteractionResult.Success);
        }
        else
        {
            resultImages[index].sprite = correctResultSprite;
            resultImages[index].preserveAspect = true;
            AddError("Ese resultado es correcto.");
        }
    }
}
