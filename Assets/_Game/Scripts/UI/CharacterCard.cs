using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class CharacterCard : MonoBehaviour
    {
        [SerializeField] RawImage portrait;
        [SerializeField] Text roleLabel, nameLabel, biographyLabel, bonusesLabel, controlsLabel;
        [SerializeField] GameObject readyIndicator;

        public void Refresh(CharacterData character, RoleKeys keys, bool ready, bool boss)
        {
            roleLabel.text = boss ? "JEFE" : "TRABAJADOR";
            GameScreen.SetPortrait(portrait, character != null ? character.portrait : null);
            nameLabel.text = character != null ? character.characterName : "Perfil sin configurar";
            biographyLabel.text = character != null ? character.biography : "";
            bonusesLabel.text = character != null
                ? $"Cuota +{character.quotaBonusPercent:0}% · Productividad +{character.productivityBonusPercent:0}%\nCoste de felicidad -{character.happinessCostReductionPercent:0}%"
                : "";
            readyIndicator.SetActive(ready);
            controlsLabel.text = ready
                ? $"{keys.cancel}: cambiar elección"
                : $"{keys.up}/{keys.down}: elegir · {keys.confirm}: confirmar";
        }
    }
}
