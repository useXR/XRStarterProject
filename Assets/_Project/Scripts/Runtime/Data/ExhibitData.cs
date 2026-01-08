using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Data container for a single exhibit.
    /// Create assets via: Create > Gallery > Exhibit Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewExhibit", menuName = "Gallery/Exhibit Data")]
    public class ExhibitData : ScriptableObject
    {
        [Header("Display Info")]
        [Tooltip("Name shown in the info panel")]
        public string title;

        [TextArea(3, 6)]
        [Tooltip("Description shown in the info panel")]
        public string description;

        [Header("Visuals")]
        [Tooltip("The 3D object displayed on the pedestal")]
        public GameObject displayPrefab;

        [Tooltip("Optional icon for UI elements")]
        public Sprite icon;

        [Header("Audio")]
        [Tooltip("Optional sound override when this exhibit is selected")]
        public AudioClip selectSound;
    }
}
