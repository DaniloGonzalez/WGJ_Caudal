using System.Collections;
using UnityEngine;

namespace Caudal
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] string teamName = "Caudal";
        [SerializeField, Min(0)] float introDuration = 2f;
        [SerializeField, Min(0.01f)] float fadeDuration = 0.5f;
        [Header("Objetos de Main")]
        [SerializeField] Camera sceneCamera;
        [SerializeField] Canvas sceneCanvas;
        [SerializeField] GameScreen screen;
        [SerializeField] IntroPanel intro;
        [Header("Sistemas")]
        [SerializeField] GameManager gameManager;
        [SerializeField] StatsManager statsManager;
        [SerializeField] DayManager dayManager;
        [SerializeField] TaskManager taskManager;
        [SerializeField] InputRouter inputRouter;
        [Header("Audio de fondo")]
        [SerializeField] AudioClip backgroundEffectsClip;
        [SerializeField] AudioClip backgroundMusicClip;
        [SerializeField, Range(0f, 1f)] float backgroundEffectsVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] float backgroundMusicVolume = 0.5f;

        AudioSource backgroundEffectsSource;
        AudioSource backgroundMusicSource;
        bool backgroundAudioStarted;

        IEnumerator Start()
        {
            if (sceneCamera == null || sceneCanvas == null || screen == null || intro == null)
            {
                Debug.LogError("GameBootstrap: faltan referencias de la escena.", this);
                yield break;
            }
            gameManager.Initialize(statsManager, dayManager, taskManager, inputRouter);
            statsManager.ResetStats();
            screen.Initialize(gameManager);
            intro.Show(teamName);
            yield return new WaitForSecondsRealtime(Mathf.Max(0, introDuration));
            float duration = Mathf.Max(0.01f, fadeDuration);
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                intro.SetAlpha(1 - elapsed / duration);
                yield return null;
            }
            intro.Hide();
            gameManager.Restart();
            screen.Refresh();
        }

        void Update()
        {
            if (backgroundAudioStarted || gameManager == null || gameManager.State != GameState.Playing)
                return;

            backgroundAudioStarted = true;
            PlayBackgroundLoop(backgroundEffectsClip, backgroundEffectsVolume, ref backgroundEffectsSource, "Background Effects");
            PlayBackgroundLoop(backgroundMusicClip, backgroundMusicVolume, ref backgroundMusicSource, "Background Music");
        }

        void PlayBackgroundLoop(AudioClip clip, float volume, ref AudioSource source, string sourceName)
        {
            if (clip == null)
                return;

            var audioObject = new GameObject(sourceName);
            audioObject.transform.SetParent(transform, false);
            source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.Play();
        }
    }
}
