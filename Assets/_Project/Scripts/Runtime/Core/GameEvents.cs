using System;

namespace Gallery
{
    /// <summary>
    /// Central event bus for decoupled communication between systems.
    /// Components fire events here, managers subscribe and react.
    /// </summary>
    public static class GameEvents
    {
        // Exhibit interaction events
        public static event Action<ExhibitData> OnExhibitHovered;
        public static event Action OnExhibitUnhovered;
        public static event Action<ExhibitData> OnExhibitSelected;
        public static event Action OnExhibitDeselected;

        /// <summary>
        /// Fire when user hovers over an exhibit pedestal.
        /// </summary>
        public static void ExhibitHovered(ExhibitData data)
        {
            OnExhibitHovered?.Invoke(data);
        }

        /// <summary>
        /// Fire when user stops hovering over an exhibit.
        /// </summary>
        public static void ExhibitUnhovered()
        {
            OnExhibitUnhovered?.Invoke();
        }

        /// <summary>
        /// Fire when user selects (grabs/triggers) an exhibit.
        /// </summary>
        public static void ExhibitSelected(ExhibitData data)
        {
            OnExhibitSelected?.Invoke(data);
        }

        /// <summary>
        /// Fire when user releases/deselects an exhibit.
        /// </summary>
        public static void ExhibitDeselected()
        {
            OnExhibitDeselected?.Invoke();
        }
    }
}
