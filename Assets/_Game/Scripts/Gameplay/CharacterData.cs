using UnityEngine;

namespace Caudal
{
    public enum CharacterRole { Worker, Boss }

    [CreateAssetMenu(menuName = "Caudal/Personaje", fileName = "NuevoPersonaje")]
    public class CharacterData : ScriptableObject
    {
        public CharacterRole role;
        public string characterName;
        public Texture portrait;
        [TextArea(2, 5)] public string biography;
        [Min(0)] public float quotaBonusPercent;
        [Min(0)] public float productivityBonusPercent;
        [Range(0, 100)] public float happinessCostReductionPercent;
    }
}
