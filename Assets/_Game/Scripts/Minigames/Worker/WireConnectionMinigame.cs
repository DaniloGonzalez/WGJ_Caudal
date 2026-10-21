using Caudal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WireConnectionMinigame : TimedMinigame, IPointerClickHandler
{
    public override string ControlsHint => "Haz clic en un cable y luego en el conector del mismo color.";
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
            cables[i].raycastTarget = true;
            connectors[i].raycastTarget = true;
            connectionLines[i].gameObject.SetActive(false);
            cables[i].gameObject.SetActive(true);
            connectors[i].gameObject.SetActive(true);
        }
        selected = cables.Length - 1;
        ShowSelection();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var target = eventData.pointerPressRaycast.gameObject != null ? eventData.pointerPressRaycast.gameObject.transform : eventData.pointerCurrentRaycast.gameObject?.transform;
        if (target == null) return;
        if (selectedCable < 0)
        {
            for (int i = 0; i < cables.Length; i++)
                if (target == cables[i].transform || target.IsChildOf(cables[i].transform))
                {
                    if (connected[i]) return;
                    selectedCable = i;
                    selected = connectors.Length - 1;
                    SetFeedback("Haz clic en su conector del mismo color.");
                    ShowSelection();
                    return;
                }
        }
        else
        {
            for (int i = 0; i < connectors.Length; i++)
                if (target == connectors[i].transform || target.IsChildOf(connectors[i].transform))
                {
                    if (connectorColors[i] != selectedCable)
                    {
                        AddError("Conector equivocado.");
                        return;
                    }
                    connected[selectedCable] = true;
                    DrawConnection(selectedCable, i);
                    selectedCable = -1;
                    selected = cables.Length - 1;
                    bool allConnected = true;
                    foreach (bool value in connected) allConnected &= value;
                    if (allConnected) Finish(InteractionResult.Success);
                    else SetFeedback("Cable conectado.");
                    ShowSelection();
                    return;
                }
        }
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
            phaseText.text = selectedCable < 0 ? "SELECCIONA UN CABLE" : "ELIGE SU CONECTOR";
        for (int i = 0; i < cables.Length; i++)
            cables[i].transform.localScale = selectedCable < 0 && i == selected && !connected[i] ? Vector3.one * 1.12f : Vector3.one;
        for (int i = 0; i < connectors.Length; i++)
            connectors[i].transform.localScale = selectedCable >= 0 && i == selected ? Vector3.one * 1.12f : Vector3.one;
        Highlight(selectedCable < 0
            ? cables[Mathf.Clamp(selected, 0, cables.Length - 1)]
            : connectors[Mathf.Clamp(selected, 0, connectors.Length - 1)]);
    }
}
