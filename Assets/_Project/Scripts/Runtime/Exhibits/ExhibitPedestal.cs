using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Gallery
{
    /// <summary>
    /// Attached to each pedestal prefab. Detects XR interactions
    /// and fires GameEvents for the rest of the system to respond to.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ExhibitPedestal : MonoBehaviour
    {
        [Header("Exhibit Configuration")]
        [SerializeField] private ExhibitData exhibitData;

        [Header("Display")]
        [SerializeField] private Transform displayPoint;

        [Header("Visual Feedback")]
        [SerializeField] private Outline outline;

        private XRSimpleInteractable _interactable;
        private GameObject _spawnedDisplay;

        /// <summary>
        /// The ExhibitData assigned to this pedestal.
        /// </summary>
        public ExhibitData Data => exhibitData;

        private void Awake()
        {
            _interactable = GetComponent<XRSimpleInteractable>();

            // Auto-find outline if not assigned
            if (outline == null)
            {
                outline = GetComponentInChildren<Outline>();
            }

            // Disable outline by default
            if (outline != null)
            {
                outline.enabled = false;
            }

            if (_interactable == null)
            {
                Debug.LogError($"[ExhibitPedestal] {name} - XRSimpleInteractable NOT FOUND!");
            }
        }

        private void Start()
        {
            // Spawn the display object if we have data and a display point
            SpawnDisplayObject();
        }

        private void OnEnable()
        {
            // Ensure interactable is cached (OnEnable can run before Awake in some cases)
            if (_interactable == null)
            {
                _interactable = GetComponent<XRSimpleInteractable>();
            }

            if (_interactable == null) return;

            // Subscribe to XRI events
            _interactable.hoverEntered.AddListener(OnHoverEntered);
            _interactable.hoverExited.AddListener(OnHoverExited);
            _interactable.selectEntered.AddListener(OnSelectEntered);
            _interactable.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            if (_interactable == null) return;

            // Unsubscribe from XRI events
            _interactable.hoverEntered.RemoveListener(OnHoverEntered);
            _interactable.hoverExited.RemoveListener(OnHoverExited);
            _interactable.selectEntered.RemoveListener(OnSelectEntered);
            _interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void SpawnDisplayObject()
        {
            if (exhibitData == null || exhibitData.displayPrefab == null) return;
            if (displayPoint == null)
            {
                Debug.LogWarning($"[ExhibitPedestal] No display point set on {name}");
                return;
            }

            _spawnedDisplay = Instantiate(
                exhibitData.displayPrefab,
                displayPoint.position,
                displayPoint.rotation,
                displayPoint
            );
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {

            // Enable outline
            if (outline != null)
            {
                outline.enabled = true;
            }

            // Fire global event
            if (exhibitData != null)
            {
                GameEvents.ExhibitHovered(exhibitData);
            }
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            // Disable outline
            if (outline != null)
            {
                outline.enabled = false;
            }

            // Fire global event
            GameEvents.ExhibitUnhovered();
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log($"[ExhibitPedestal] {name} - OnSelectEntered! Interactor: {args.interactorObject?.transform.name}");
            if (exhibitData != null)
            {
                GameEvents.ExhibitSelected(exhibitData);
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            GameEvents.ExhibitDeselected();
        }

        /// <summary>
        /// Set the exhibit data at runtime (useful for dynamic pedestals).
        /// </summary>
        public void SetExhibitData(ExhibitData data)
        {
            exhibitData = data;

            // Destroy old display and spawn new one
            if (_spawnedDisplay != null)
            {
                Destroy(_spawnedDisplay);
            }

            SpawnDisplayObject();
        }
    }
}
