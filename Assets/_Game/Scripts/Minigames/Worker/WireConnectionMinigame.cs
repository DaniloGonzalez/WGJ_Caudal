using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class WireConnectionMinigame : TimedMinigame
{
    [SerializeField] private Image[] cables;
    [SerializeField] private Image[] connectors;
    [SerializeField] private Image[] connectionLines;
    [SerializeField] private Sprite[] leftCableSprites;
    [SerializeField] private Sprite[] rightConnectorSprites;
    [SerializeField] private Text phaseText;

    private static readonly Color[] colors = { new Color(.91f, .32f, .29f), new Color(.27f, .69f, .48f), new Color(.30f, .55f, .88f) };
    private int[] connectorColors;
    private bool[] connected;
    private int selected;
    private int selectedCable = -1;

    protected override void OnMinigameStarted()
    {
        connectorColors = new[] { 0, 1, 2 };
        for (int i = connectorColors.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (connectorColors[i], connectorColors[j]) = (connectorColors[j], connectorColors[i]);
        }
        connected = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            if (leftCableSprites != null && i < leftCableSprites.Length)
                cables[i].sprite = leftCableSprites[i];
            if (rightConnectorSprites != null && connectorColors[i] < rightConnectorSprites.Length)
                connectors[i].sprite = rightConnectorSprites[connectorColors[i]];
            cables[i].color = Color.white;
            connectors[i].color = Color.white;
            connectionLines[i].gameObject.SetActive(false);
            cables[i].gameObject.SetActive(true);
            connectors[i].gameObject.SetActive(true);
        }
        selected = 0;
        ShowSelection();
    }

    protected override void ReadInput()
    {
        int count = selectedCable < 0 ? cables.Length : connectors.Length;
        if (PressedUp) selected = Mathf.Max(0, selected - 1);
        if (PressedDown) selected = Mathf.Min(count - 1, selected + 1);
        if (PressedConfirm)
        {
            if (selectedCable < 0)
            {
                selectedCable = selected;
                selected = 0;
                SetFeedback("Elige el conector del mismo color.");
            }
            else if (!connected[selectedCable] && connectorColors[selected] == selectedCable)
            {
                connected[selectedCable] = true;
                DrawConnection(selectedCable, selected);
                cables[selectedCable].transform.localScale = Vector3.one * .9f;
                connectors[selected].transform.localScale = Vector3.one * .9f;
                selectedCable = -1;
                selected = 0;
                bool allConnected = true;
                foreach (bool value in connected) allConnected &= value;
                if (allConnected) Finish(InteractionResult.Success);
                else SetFeedback("Cable conectado.");
            }
            else
            {
                AddError("Conector equivocado.");
            }
        }
        ShowSelection();
    }

    private void DrawConnection(int cable, int connector)
    {
        RectTransform line = connectionLines[cable].rectTransform;
        Vector3 start = cables[cable].rectTransform.position;
        Vector3 end = connectors[connector].rectTransform.position;
        line.position = (start + end) * .5f;
        Vector3 direction = end - start;
        line.sizeDelta = new Vector2(direction.magnitude, 8);
        line.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        connectionLines[cable].color = colors[cable];
        connectionLines[cable].gameObject.SetActive(true);
    }

    private void ShowSelection()
    {
        if (phaseText != null)
            phaseText.text = selectedCable < 0 ? "W/S: cable  ·  F: seleccionar" : "W/S: conector  ·  F: unir";
        for (int i = 0; i < cables.Length; i++)
            cables[i].transform.localScale = selectedCable < 0 && i == selected && !connected[i] ? Vector3.one * 1.12f : Vector3.one;
        for (int i = 0; i < connectors.Length; i++)
            connectors[i].transform.localScale = selectedCable >= 0 && i == selected ? Vector3.one * 1.12f : Vector3.one;
        Highlight(selectedCable < 0
            ? cables[Mathf.Clamp(selected, 0, cables.Length - 1)]
            : connectors[Mathf.Clamp(selected, 0, connectors.Length - 1)]);
    }
}
