using UnityEngine;

namespace Caudal
{
    public sealed class GameBootstrap : MonoBehaviour
    {
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

        void Start()
        {
            if (sceneCamera == null || sceneCanvas == null || screen == null || intro == null)
            {
                Debug.LogError("GameBootstrap: faltan referencias de la escena.", this);
                return;
            }
            gameManager.Initialize(statsManager, dayManager, taskManager, inputRouter);
            statsManager.ResetStats();
            gameManager.ShowStartScreen();
            intro.Initialize(gameManager);
            intro.Hide();
            screen.Initialize(gameManager);
        }

        void Update()
        {
            if (gameManager == null || intro == null || gameManager.Input == null)
                return;

            if (gameManager.State == GameState.IntroScreen)
            {
                if (!intro.gameObject.activeSelf)
                {
                    intro.Show();
                    return;
                }

                if (gameManager.Input.Pressed(gameManager.Input.boss.confirm))
                {
                    intro.Hide();
                    gameManager.ContinueFromIntro();
                }
            }
            else if (intro.gameObject.activeSelf)
                intro.Hide();

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
