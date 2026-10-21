using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ServerSequenceMinigame : TimedMinigame
{
    public override string ControlsHint => "Memoriza los colores y haz clic en los botones en el mismo orden.";

    [SerializeField] private Button[] colorButtons;
    [SerializeField] private Image[] sequenceIndicators;
    [SerializeField] private Sprite[] indicatorSprites;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField, Min(.1f)] private float flashDuration = .36f;
    [SerializeField, Min(.05f)] private float pauseDuration = .16f;

    private int[] sequence;
    private int playbackIndex;
    private int inputIndex;
    private float playbackTimer;
    private bool showingColor;
    private bool acceptingInput;

    protected override void OnMinigameStarted()
    {
        int length = Mathf.Min(sequenceIndicators.Length, 2 + Difficulty);
        sequence = new int[length];
        for (int i = 0; i < length; i++)
        {
            int color = Random.Range(0, colorButtons.Length);
            if (i > 0 && color == sequence[i - 1])
                color = (color + Random.Range(1, colorButtons.Length)) % colorButtons.Length;
            sequence[i] = color;
        }

        playbackIndex = 0;
        inputIndex = 0;
        playbackTimer = .35f;
        showingColor = false;
        acceptingInput = false;
        statusText.text = "Memoriza la secuencia";
        foreach (var indicator in sequenceIndicators)
            indicator.gameObject.SetActive(false);

        for (int i = 0; i < colorButtons.Length; i++)
        {
            colorButtons[i].interactable = false;
            colorButtons[i].onClick.RemoveAllListeners();
            int color = i;
            colorButtons[i].onClick.AddListener(() => SelectColor(color));
        }
        ShowProgress();
    }

    protected override void AdvanceGame(float deltaTime)
    {
        if (acceptingInput) return;
        playbackTimer -= deltaTime;
        if (playbackTimer > 0) return;

        if (showingColor)
        {
            sequenceIndicators[playbackIndex].gameObject.SetActive(false);
            playbackIndex++;
            showingColor = false;
            if (playbackIndex >= sequence.Length)
            {
                acceptingInput = true;
                statusText.text = "Tu turno: repite la secuencia";
                foreach (var button in colorButtons)
                    button.interactable = true;
                return;
            }
            playbackTimer = pauseDuration;
            return;
        }

        var indicator = sequenceIndicators[playbackIndex];
        indicator.sprite = indicatorSprites[sequence[playbackIndex]];
        indicator.preserveAspect = true;
        indicator.gameObject.SetActive(true);
        showingColor = true;
        playbackTimer = flashDuration;
    }

    private void SelectColor(int color)
    {
        if (!acceptingInput) return;
        if (inputIndex >= sequence.Length) return;
        if (color != sequence[inputIndex])
        {
            AddError("Secuencia incorrecta. Vuelve a intentarlo desde el inicio.");
            if (ErrorCount < ErrorLimit)
            {
                inputIndex = 0;
                playbackIndex = 0;
                playbackTimer = .55f;
                showingColor = false;
                acceptingInput = false;
                statusText.text = "Observa otra vez la secuencia";
                foreach (var button in colorButtons) button.interactable = false;
                foreach (var indicator in sequenceIndicators) indicator.gameObject.SetActive(false);
                ShowProgress();
            }
            return;
        }

        inputIndex++;
        if (inputIndex >= sequence.Length)
        {
            acceptingInput = false;
            foreach (var button in colorButtons)
                button.interactable = false;
            SetFeedback("Servidor reiniciado.");
            Finish(Caudal.InteractionResult.Success);
        }
        else
            SetFeedback("Correcto.");
        ShowProgress();
    }

    private void ShowProgress()
    {
        progressText.text = "Secuencia: " + inputIndex + " / " + (sequence == null ? 0 : sequence.Length);
    }
}
