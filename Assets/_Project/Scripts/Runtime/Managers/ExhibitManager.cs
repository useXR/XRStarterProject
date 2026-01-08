using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Coordinates exhibit state across the application.
    /// Tracks currently hovered/selected exhibit and prevents simultaneous selections.
    /// </summary>
    public class ExhibitManager : Singleton<ExhibitManager>
    {
        private ExhibitData _currentExhibit;
        private bool _isExhibitSelected;

        /// <summary>
        /// The currently active exhibit data (hovered or selected).
        /// </summary>
        public ExhibitData CurrentExhibit => _currentExhibit;

        /// <summary>
        /// Whether an exhibit is currently selected (not just hovered).
        /// </summary>
        public bool IsExhibitSelected => _isExhibitSelected;

        private void OnEnable()
        {
            GameEvents.OnExhibitHovered += HandleExhibitHovered;
            GameEvents.OnExhibitUnhovered += HandleExhibitUnhovered;
            GameEvents.OnExhibitSelected += HandleExhibitSelected;
            GameEvents.OnExhibitDeselected += HandleExhibitDeselected;
        }

        private void OnDisable()
        {
            GameEvents.OnExhibitHovered -= HandleExhibitHovered;
            GameEvents.OnExhibitUnhovered -= HandleExhibitUnhovered;
            GameEvents.OnExhibitSelected -= HandleExhibitSelected;
            GameEvents.OnExhibitDeselected -= HandleExhibitDeselected;
        }

        private void HandleExhibitHovered(ExhibitData data)
        {
            // Don't change hover state while something is selected
            if (_isExhibitSelected) return;

            _currentExhibit = data;
        }

        private void HandleExhibitUnhovered()
        {
            // Don't clear if we have a selection
            if (_isExhibitSelected) return;

            _currentExhibit = null;
        }

        private void HandleExhibitSelected(ExhibitData data)
        {
            _currentExhibit = data;
            _isExhibitSelected = true;
        }

        private void HandleExhibitDeselected()
        {
            _isExhibitSelected = false;
            _currentExhibit = null;
        }
    }
}
