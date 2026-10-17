using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        GameManager game;
        GameObject panel;
        RawImage workerPreview, bossPreview, workerPortrait, bossPortrait;
        Text workerInfo, bossInfo, workerTitle, bossTitle;

        public void Initialize(GameManager game, Transform parent, RawImage workerPortrait,
            RawImage bossPortrait, Text workerTitle, Text bossTitle)
        {
            this.game = game;
            this.workerPortrait = workerPortrait;
            this.bossPortrait = bossPortrait;
            this.workerTitle = workerTitle;
            this.bossTitle = bossTitle;
            var root = GameScreen.Panel(parent, "Seleccion de personajes", new Color(.03f,.06f,.09f), Vector2.zero, Vector2.one);
            panel = root.gameObject;
            GameScreen.Label(root, "ELIJAN SU PERSONAJE", 36, Color.white, new Vector2(0,.9f), Vector2.one);
            GameScreen.Label(root, "La jornada comienza cuando ambos confirman", 20, Color.white, new Vector2(0,0), new Vector2(1,.07f));
            CreateCard(root, false, out workerPreview, out workerInfo);
            CreateCard(root, true, out bossPreview, out bossInfo);
            panel.SetActive(false);
        }

        void CreateCard(Transform parent, bool boss, out RawImage portrait, out Text info)
        {
            float left = boss ? .51f : .02f;
            var card = GameScreen.Panel(parent, boss ? "Jefe" : "Trabajador", new Color(.08f,.14f,.18f),
                new Vector2(left,.08f), new Vector2(left+.47f,.88f));
            GameScreen.Label(card, boss ? "JEFE" : "TRABAJADOR", 28, Color.white, new Vector2(0,.9f), Vector2.one);
            portrait = GameScreen.Picture(card, null, new Vector2(.25f,.48f), new Vector2(.75f,.89f));
            info = GameScreen.Label(card, "", 23, Color.white, new Vector2(.04f,.03f), new Vector2(.96f,.47f));
        }

        void LateUpdate()
        {
            panel.SetActive(game.State == GameState.CharacterSelection);
            Refresh(false, workerPreview, workerInfo, workerPortrait, workerTitle);
            Refresh(true, bossPreview, bossInfo, bossPortrait, bossTitle);
        }

        void Refresh(bool boss, RawImage preview, Text info, RawImage portrait, Text title)
        {
            var character = boss ? game.BossCharacter : game.WorkerCharacter;
            var keys = boss ? game.Input.boss : game.Input.worker;
            bool ready = boss ? game.BossReady : game.WorkerReady;
            if (character == null)
            {
                SetPortrait(preview, null);
                info.text = $"Perfil sin configurar\n{keys.up}/{keys.down}: elegir otro";
                return;
            }

            SetPortrait(preview, character.portrait);
            SetPortrait(portrait, character.portrait);
            title.text = (boss ? "JEFE · " : "TRABAJADOR · ") + character.characterName;
            info.text = $"{character.characterName}\n{character.biography}\n\n" +
                $"Cuota +{character.quotaBonusPercent:0}% · Productividad +{character.productivityBonusPercent:0}%\n" +
                $"Coste de felicidad -{character.happinessCostReductionPercent:0}%\n\n";
            if (ready) info.text += $"LISTO · {keys.cancel}: cambiar elección";
            else info.text += $"{keys.up}/{keys.down}: elegir · {keys.confirm}: confirmar";
        }

        void SetPortrait(RawImage image, Texture texture)
        {
            if (image.texture == texture) return;
            image.texture = texture;
            image.enabled = texture != null;
            if (texture != null)
                image.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }
    }
}
