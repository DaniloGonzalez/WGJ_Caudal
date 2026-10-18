using UnityEngine;

namespace Caudal
{
    public enum CharacterRole { Worker, Boss }

    [System.Serializable]
    public sealed class CharacterDialogueLine
    {
        [Tooltip("Acción o resultado que muestra esta línea para este personaje.")]
        public DialogueTrigger trigger;
        [TextArea(2, 4)] public string text;
        [Min(1)] public float duration = 3;
    }

    [CreateAssetMenu(menuName = "Caudal/Personaje", fileName = "NuevoPersonaje")]
    public class CharacterData : ScriptableObject
    {
        public CharacterRole role;
        public string characterName;
        public Texture portrait;
        [Tooltip("Clip opcional para acompañar diálogos de este rol.")]
        public AudioClip speechClip;
        [Tooltip("Líneas exclusivas de este personaje, elegidas según la acción o resultado.")]
        public CharacterDialogueLine[] dialogueLines;
        [TextArea(2, 5)] public string biography;
        [Min(0)] public float quotaBonusPercent;
        [Min(0)] public float productivityBonusPercent;
        [Range(0, 100)] public float happinessCostReductionPercent;
    }
}
