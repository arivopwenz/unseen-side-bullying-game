using UnityEngine;

namespace BullyingGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        private float targetVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
        }

        public void PlayBGM(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            targetVolume = volume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }
        public void SetBGMGain(float volume) => targetVolume = Mathf.Clamp01(volume);
        private void Update()
        {
            if (bgmSource != null) bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, targetVolume, Time.unscaledDeltaTime * .5f);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
