using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TaskCard : MonoBehaviour
    {
        [SerializeField] Text nameLabel, statusLabel;
        [SerializeField] GameObject selectionIndicator;

        public void Refresh(TaskInstance task, bool selected)
        {
            nameLabel.text = task.Data.taskName;
            statusLabel.text = task.Status.ToString();
            selectionIndicator.SetActive(selected);
        }
    }
}
