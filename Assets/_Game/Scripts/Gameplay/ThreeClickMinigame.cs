using System;
using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class ThreeClickMinigame : MonoBehaviour, IMinigame
{
    [SerializeField] private Button clickButton;
    [SerializeField] private Text instructionText;
    [SerializeField] private Text progressText;

    public event Action<InteractionResult> OnFinished;

    private int clicks;
    private bool finished;

    private void Awake()
    {
        clickButton.onClick.AddListener(CountClick);
        UpdateProgress();
    }

    private void OnDestroy()
    {
        if (clickButton != null)
            clickButton.onClick.RemoveListener(CountClick);
    }

    private void CountClick()
    {
        if (finished)
            return;

        clicks++;
        UpdateProgress();

        if (clicks >= 3)
        {
            finished = true;
            clickButton.interactable = false;
            instructionText.text = "¡Listo!";
            OnFinished?.Invoke(InteractionResult.Success);
        }
    }

    private void UpdateProgress()
    {
        instructionText.text = "Haz clic tres veces";
        progressText.text = clicks + " / 3";
    }
}
