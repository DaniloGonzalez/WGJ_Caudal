using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Caudal
{
    public sealed class CharacterCard : MonoBehaviour
    {
        [SerializeField] RawImage portrait;
        [SerializeField] Text roleLabel, nameLabel, biographyLabel, bonusesLabel, controlsLabel;
        [SerializeField] Text previousArrow, nextArrow;
        [SerializeField] GameObject readyIndicator;

        public void Refresh(CharacterData character, RoleKeys keys, bool ready, bool boss, bool canCycle)
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
                ? $"{FormatKey(keys.cancel)}: VOLVER A ELEGIR"
                : $"{FormatKey(keys.left)} / {FormatKey(keys.right)} CAMBIAR · {FormatKey(keys.confirm)} ELEGIR";
            if (previousArrow != null) previousArrow.gameObject.SetActive(canCycle && !ready);
            if (nextArrow != null) nextArrow.gameObject.SetActive(canCycle && !ready);
        }

        static string FormatKey(Key key)
        {
            switch (key)
            {
                case Key.LeftArrow: return "←";
                case Key.RightArrow: return "→";
                case Key.UpArrow: return "↑";
                case Key.DownArrow: return "↓";
                case Key.Enter: return "ENTER";
                case Key.RightShift: return "SHIFT DER.";
                default: return key.ToString().ToUpperInvariant();
            }
        }
    }
}
