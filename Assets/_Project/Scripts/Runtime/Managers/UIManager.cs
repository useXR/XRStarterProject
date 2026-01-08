using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Controls the world-space info panel.
    /// Shows exhibit information when user selects a pedestal.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("References")]
        [SerializeField] private InfoPanel infoPanel;

        [Header("Positioning")]
        [SerializeField] private Vector3 panelOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private bool followExhibit = true;

        private Transform _currentTarget;

        private void OnEnable()
        {
            GameEvents.OnExhibitSelected += HandleExhibitSelected;
            GameEvents.OnExhibitDeselected += HandleExhibitDeselected;
        }

        private void OnDisable()
        {
            GameEvents.OnExhibitSelected -= HandleExhibitSelected;
            GameEvents.OnExhibitDeselected -= HandleExhibitDeselected;
        }

        private void LateUpdate()
        {
            if (followExhibit && _currentTarget != null && infoPanel != null)
            {
                infoPanel.transform.position = _currentTarget.position + panelOffset;
            }
        }

        private void HandleExhibitSelected(ExhibitData data)
        {
            Debug.Log($"[UIManager] HandleExhibitSelected called with: {data?.title ?? "null"}");

            if (infoPanel == null)
            {
                Debug.LogWarning("[UIManager] InfoPanel reference not set!");
                return;
            }

            // Find the pedestal that fired this event to position the panel
            var pedestal = FindPedestalWithData(data);
            if (pedestal != null)
            {
                _currentTarget = pedestal.transform;
                infoPanel.transform.position = _currentTarget.position + panelOffset;
            }

            infoPanel.Show(data);
        }

        private void HandleExhibitDeselected()
        {
            if (infoPanel != null)
            {
                infoPanel.Hide();
            }

            _currentTarget = null;
        }

        private ExhibitPedestal FindPedestalWithData(ExhibitData data)
        {
            // Find all pedestals and return the one with matching data
            var pedestals = FindObjectsByType<ExhibitPedestal>(FindObjectsSortMode.None);
            foreach (var pedestal in pedestals)
            {
                if (pedestal.Data == data)
                {
                    return pedestal;
                }
            }
            return null;
        }

        /// <summary>
        /// Manually show the info panel with specific data.
        /// </summary>
        public void ShowInfo(ExhibitData data)
        {
            if (infoPanel != null)
            {
                infoPanel.Show(data);
            }
        }

        /// <summary>
        /// Manually hide the info panel.
        /// </summary>
        public void HideInfo()
        {
            if (infoPanel != null)
            {
                infoPanel.Hide();
            }
        }
    }
}
