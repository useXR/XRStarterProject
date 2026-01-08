using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Central audio clip references for the gallery.
    /// Create via: Create > Gallery > SFX Library
    /// </summary>
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "Gallery/SFX Library")]
    public class SFXLibrary : ScriptableObject
    {
        [Header("Interaction Sounds")]
        [Tooltip("Played when hovering over an exhibit")]
        public AudioClip hoverSound;

        [Tooltip("Played when selecting an exhibit")]
        public AudioClip selectSound;

        [Tooltip("Played when deselecting an exhibit")]
        public AudioClip deselectSound;

        [Header("Ambient")]
        [Tooltip("Background ambient loop for the gallery")]
        public AudioClip ambientLoop;
    }
}
