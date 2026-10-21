using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BreakScheduleMinigame : TimedMinigame, IMinigameDragReceiver
{
    public override string ControlsHint => "Arrastra a cada empleado a un horario con espacio disponible.";

    [SerializeField] private RectTransform[] employees;
    [SerializeField] private RectTransform[] slots;
    [SerializeField] private Image[] slotImages;
    [SerializeField] private TMP_Text[] scheduleLabels;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite fullSlotSprite;

    private int[] scheduleCounts;
    private bool[] usedSlots;
    private int assignedCount;
    private int employeeCount;
    private int[] scheduleCapacities;
    private bool dragging;

    protected override void Awake()
    {
        base.Awake();
        CacheHomePositions();
    }

    protected override void OnMinigameStarted()
    {
        CacheHomePositions();
        assignedCount = 0;
        dragging = false;
        employeeCount = Difficulty == 1 ? Mathf.Min(3, employees.Length) : employees.Length;
        scheduleCounts = new int[3];
        scheduleCapacities = Difficulty == 3 ? new[] { 2, 1, 1 } : new[] { 2, 2, 2 };
        usedSlots = new bool[slots.Length];

        for (int i = 0; i < employees.Length; i++)
        {
            employees[i].GetComponent<MinigameDragHandle>().RestoreHome();
            var group = employees[i].GetComponent<CanvasGroup>();
            if (group != null) group.blocksRaycasts = true;
            employees[i].gameObject.SetActive(i < employeeCount);
        }

        float slotScale = Difficulty == 1 ? 1f : Difficulty == 2 ? .84f : .68f;
        for (int i = 0; i < slots.Length; i++)
        {
            int schedule = i / 2;
            bool available = i < slots.Length && (i % 2) < scheduleCapacities[schedule];
            slots[i].gameObject.SetActive(available);
            slots[i].localScale = Vector3.one * slotScale;
            slotImages[i].gameObject.SetActive(true);
            slotImages[i].sprite = emptySlotSprite;
            slotImages[i].preserveAspect = true;
        }

        BindDragHandles();
        SetSelectionHighlight(employees.Length > 0 ? employees[0].GetComponent<Image>() : null);
        ShowProgress();
        UpdateScheduleLabels();
    }

    private void CacheHomePositions()
    {
        for (int i = 0; i < employees.Length; i++)
            employees[i].GetComponent<MinigameDragHandle>().RememberHome();
    }

    private void BindDragHandles()
    {
        foreach (var handle in GetComponentsInChildren<MinigameDragHandle>(true))
        {
            handle.Receiver = this;
            handle.Index = System.Array.IndexOf(employees, handle.Rect);
        }
    }

    public void DragStarted(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (handle.Index < 0 || handle.Index >= employees.Length || employees[handle.Index] != handle.Rect) return;
        if (IsAssigned(handle.Rect)) return;
        dragging = true;
        SetSelectionHighlight(handle.GetComponent<Image>());
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = false;
        handle.BeginMove((RectTransform)transform, eventData);
    }

    public void DragMoved(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging) return;
        handle.Move(eventData);
    }

    public void DragEnded(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = true;

        int target = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!usedSlots[i] && RectTransformUtility.RectangleContainsScreenPoint(
                slots[i], eventData.position, eventData.pressEventCamera))
            {
                target = i;
                break;
            }
        }

        if (target < 0 || scheduleCounts[target / 2] >= scheduleCapacities[target / 2])
        {
            AddError("Ese horario no tiene espacio disponible.");
            RestoreEmployee(handle.Index);
            return;
        }

        usedSlots[target] = true;
        scheduleCounts[target / 2]++;
        handle.PlaceIn(slots[target], Vector2.zero, Mathf.Min(slots[target].rect.width, slots[target].rect.height) - 6);
        slotImages[target].sprite = fullSlotSprite;
        slotImages[target].preserveAspect = true;
        assignedCount++;
        SetFeedback("Descanso asignado.");
        ShowProgress();
        UpdateScheduleLabels();
        int nextEmployee = -1;
        for (int i = 0; i < employees.Length; i++)
            if (employees[i].gameObject.activeSelf && !IsAssigned(employees[i])) { nextEmployee = i; break; }
        SetSelectionHighlight(nextEmployee >= 0 ? employees[nextEmployee].GetComponent<Image>() : null);
        if (assignedCount >= employeeCount)
            Finish(Caudal.InteractionResult.Success);
    }

    private void RestoreEmployee(int index)
    {
        if (index < 0 || index >= employees.Length) return;
        employees[index].GetComponent<MinigameDragHandle>().RestoreHome();
        SetSelectionHighlight(employees[index].GetComponent<Image>());
    }

    private bool IsAssigned(RectTransform employee)
    {
        for (int i = 0; i < slots.Length; i++)
            if (employee.parent == slots[i]) return true;
        return false;
    }

    private void ShowProgress()
    {
        progressText.text = "Empleados asignados: " + assignedCount + " / " + employeeCount;
    }

    private void UpdateScheduleLabels()
    {
        for (int i = 0; i < scheduleLabels.Length; i++)
            scheduleLabels[i].text = scheduleCounts[i] + " / " + scheduleCapacities[i] + " personas";
    }
}
