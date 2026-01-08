using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Handles all audio playback for the gallery.
    /// Plays SFX in response to exhibit interaction events.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Library")]
        [SerializeField] private SFXLibrary sfxLibrary;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.3f;

        private AudioSource _sfxSource;
        private AudioSource _ambientSource;

        protected override void Awake()
        {
            base.Awake();

            // Get or create audio sources
            _sfxSource = GetComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f; // 2D for UI feedback sounds

            // Create separate source for ambient
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.loop = true;
            _ambientSource.spatialBlend = 0f;
        }

        private void Start()
        {
            // Start ambient audio if available
            if (sfxLibrary != null && sfxLibrary.ambientLoop != null)
            {
                PlayAmbient();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnExhibitHovered += HandleExhibitHovered;
            GameEvents.OnExhibitSelected += HandleExhibitSelected;
            GameEvents.OnExhibitDeselected += HandleExhibitDeselected;
        }

        private void OnDisable()
        {
            GameEvents.OnExhibitHovered -= HandleExhibitHovered;
            GameEvents.OnExhibitSelected -= HandleExhibitSelected;
            GameEvents.OnExhibitDeselected -= HandleExhibitDeselected;
        }

        private void HandleExhibitHovered(ExhibitData data)
        {
            PlayHoverSound();
        }

        private void HandleExhibitSelected(ExhibitData data)
        {
            // Use exhibit-specific sound if available, otherwise default
            if (data.selectSound != null)
            {
                PlaySFX(data.selectSound);
            }
            else
            {
                PlaySelectSound();
            }
        }

        private void HandleExhibitDeselected()
        {
            PlayDeselectSound();
        }

        /// <summary>
        /// Play an audio clip as a one-shot SFX.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Play an audio clip at a specific world position (3D spatial).
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }

        public void PlayHoverSound()
        {
            if (sfxLibrary != null)
            {
                PlaySFX(sfxLibrary.hoverSound);
            }
        }

        public void PlaySelectSound()
        {
            if (sfxLibrary != null)
            {
                PlaySFX(sfxLibrary.selectSound);
            }
        }

        public void PlayDeselectSound()
        {
            if (sfxLibrary != null)
            {
                PlaySFX(sfxLibrary.deselectSound);
            }
        }

        private void PlayAmbient()
        {
            if (sfxLibrary == null || sfxLibrary.ambientLoop == null) return;

            _ambientSource.clip = sfxLibrary.ambientLoop;
            _ambientSource.volume = ambientVolume;
            _ambientSource.Play();
        }
    }
}
