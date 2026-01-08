using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Gallery.Debugging
{
    /// <summary>
    /// Debug script to diagnose XR interaction issues.
    /// Attach to XR Origin or any active GameObject.
    /// </summary>
    public class InteractionDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableRaycastDebug = true;
        [SerializeField] private bool enableInteractableDebug = true;
        [SerializeField] private LayerMask raycastLayers = ~0; // All layers
        [SerializeField] private float raycastDistance = 10f;

        [Header("References (Auto-find if null)")]
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private XRInteractionManager interactionManager;

        private void Start()
        {
            // Auto-find XRInteractionManager
            if (interactionManager == null)
            {
                interactionManager = FindAnyObjectByType<XRInteractionManager>();
                if (interactionManager != null)
                {
                    UnityEngine.Debug.Log($"[InteractionDebugger] Found XRInteractionManager: {interactionManager.name}");
                }
                else
                {
                    UnityEngine.Debug.LogError("[InteractionDebugger] No XRInteractionManager found in scene!");
                }
            }

            // Log all interactables in the scene
            if (enableInteractableDebug)
            {
                LogAllInteractables();
            }
        }

        private void Update()
        {
            if (enableRaycastDebug && rayOrigin != null)
            {
                PerformDebugRaycast();
            }

            // Press F5 to manually log interactables
            if (Input.GetKeyDown(KeyCode.F5))
            {
                LogAllInteractables();
            }

            // Press F6 to check registration status
            if (Input.GetKeyDown(KeyCode.F6))
            {
                CheckRegistrationStatus();
            }
        }

        private void PerformDebugRaycast()
        {
            Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, raycastLayers, QueryTriggerInteraction.Collide))
            {
                UnityEngine.Debug.Log($"[InteractionDebugger] Raycast hit: {hit.collider.name} at {hit.point}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, IsTrigger: {hit.collider.isTrigger}");

                // Check if it has an interactable
                var interactable = hit.collider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                if (interactable != null)
                {
                    UnityEngine.Debug.Log($"[InteractionDebugger] Found interactable: {interactable.name}, Registered: {interactable.interactionManager != null}");
                }
            }
        }

        private void LogAllInteractables()
        {
            var interactables = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"[InteractionDebugger] === Found {interactables.Length} Interactables ===");

            foreach (var interactable in interactables)
            {
                bool hasManager = interactable.interactionManager != null;
                int colliderCount = interactable.colliders.Count;

                UnityEngine.Debug.Log($"[InteractionDebugger] {interactable.name}: Manager={hasManager}, Colliders={colliderCount}, Layers={interactable.interactionLayers.value}");

                foreach (var col in interactable.colliders)
                {
                    if (col != null)
                    {
                        UnityEngine.Debug.Log($"  - Collider: {col.name}, IsTrigger: {col.isTrigger}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
                    }
                }
            }
        }

        private void CheckRegistrationStatus()
        {
            UnityEngine.Debug.Log("[InteractionDebugger] === Registration Status ===");

            // Check interactors
            var interactors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"[InteractionDebugger] Found {interactors.Length} Interactors:");
            foreach (var interactor in interactors)
            {
                bool hasManager = interactor.interactionManager != null;
                UnityEngine.Debug.Log($"  - {interactor.name}: Manager={hasManager}, HoverActive={interactor.isHoverActive}, Layers={interactor.interactionLayers.value}");
            }

            // Check interaction manager
            if (interactionManager != null)
            {
                UnityEngine.Debug.Log($"[InteractionDebugger] XRInteractionManager '{interactionManager.name}' is active");
            }
        }
    }
}
