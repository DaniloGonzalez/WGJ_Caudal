using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Caudal.Editor
{
    public static class UIPrefabValidation
    {
        public static string[] PrefabNames => new[] {
            "IntroPanel", "CharacterCard", "CharacterSelection", "TrabajadorPanel",
            "JefePanel", "TaskCard", "SharedHUD", "StatsGraph", "ResultsPanel"
        };

        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("UI: " + message);
        }

        [MenuItem("Caudal/Validar prefabs de UI")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/_Game/Scenes/Main.unity");
            try { CheckScene(scene); }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
            Debug.Log("CAUDAL_UI_PREFABS_PASSED: prefabs, nested instances, references, aspect ratios and scene layout.");
        }

        public static void CheckScene(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var canvases = roots.SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).ToArray();
            Check(canvases.Length == 1 && canvases[0].name == "Game UI", "One Canvas named Game UI");
            Check(roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true)).Count() == 1, "One camera");
            var canvas = canvases[0];
            var scaler = canvas.GetComponent<CanvasScaler>();
            Check(scaler != null && scaler.referenceResolution == new Vector2(1280, 720) &&
                scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize, "1280 x 720 scaler");
            foreach (var root in roots) CheckReferences(root);
            foreach (string name in PrefabNames)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/UI/" + name + ".prefab");
                Check(prefab != null && prefab.transform.childCount > 0, "Populated prefab: " + name);
                CheckReferences(prefab);
            }
            foreach (Transform panel in canvas.transform)
                Check(PrefabUtility.IsPartOfPrefabInstance(panel.gameObject), "Panel is a prefab instance: " + panel.name);
            var cards = canvas.GetComponentsInChildren<CharacterCard>(true);
            Check(cards.Length == 2, "Two character cards");
            foreach (var card in cards)
                Check(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(card.gameObject).EndsWith("/CharacterCard.prefab"),
                    "Nested CharacterCard");
            var tasks = canvas.GetComponentsInChildren<TaskCard>(true);
            Check(tasks.Length == 3, "Three reusable task rows");
            foreach (var task in tasks)
                Check(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(task.gameObject).EndsWith("/TaskCard.prefab"),
                    "Nested TaskCard");
            Check(canvas.GetComponentInChildren<TrabajadorHUD>(true).gameObject.activeSelf, "Worker visible in Edit Mode");
            Check(canvas.GetComponentInChildren<JefeHUD>(true).gameObject.activeSelf, "Boss visible in Edit Mode");
        }

        static void CheckReferences(GameObject root)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Check(behaviour != null, "Missing script in " + root.name);
                // Only our UI and bootstrap require every serialized reference.
                if (!(behaviour is GameBootstrap) && behaviour.GetType().Namespace != "Caudal") continue;
                if (!(behaviour is GameBootstrap) && !(behaviour is GameScreen) &&
                    !(behaviour is IntroPanel) && !(behaviour is CharacterCard) &&
                    !(behaviour is CharacterSelectionUI) && !(behaviour is TrabajadorHUD) &&
                    !(behaviour is JefeHUD) && !(behaviour is TaskCard) &&
                    !(behaviour is SharedHUD) && !(behaviour is StatsGraphUI) && !(behaviour is ResultsUI)) continue;
                var serialized = new SerializedObject(behaviour);
                var property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                        Check(property.objectReferenceValue != null, behaviour.name + ": " + property.propertyPath);
            }
            foreach (var picture in root.GetComponentsInChildren<RawImage>(true))
            {
                var aspect = picture.GetComponent<AspectRatioFitter>();
                Check(aspect != null && aspect.aspectMode == AspectRatioFitter.AspectMode.FitInParent, "Image fits its frame");
                Check(picture.texture != null, "Preview texture: " + picture.name);
                Check(Mathf.Abs(aspect.aspectRatio - (float)picture.texture.width / picture.texture.height) < .001f,
                    "Image aspect ratio: " + picture.name);
                var path = AssetDatabase.GetAssetPath(picture.texture);
                Check(path.StartsWith("Assets/_Game/Art/Sprites/placeholders_oficina_gamejam/"), "Existing artwork only");
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Check(importer != null && importer.npotScale == TextureImporterNPOTScale.None, "NPOT unchanged");
            }
        }
    }
}
