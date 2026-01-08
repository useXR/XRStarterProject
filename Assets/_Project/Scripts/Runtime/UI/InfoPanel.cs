using UnityEngine;
using TMPro;
using System.Collections;

namespace Gallery
{
    /// <summary>
    /// World-space UI panel that displays exhibit information.
    /// Handles show/hide animations and optional billboarding.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class InfoPanel : MonoBehaviour
    {
        [Header("Text References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Billboarding")]
        [SerializeField] private bool billboardToCamera = true;
        [SerializeField] private bool lockYAxis = true;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;
        private Transform _cameraTransform;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            // Start hidden
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            // Cache camera reference
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (billboardToCamera && _cameraTransform != null && _canvasGroup.alpha > 0f)
            {
                BillboardToCamera();
            }
        }

        private void BillboardToCamera()
        {
            Vector3 directionToCamera = _cameraTransform.position - transform.position;

            if (lockYAxis)
            {
                directionToCamera.y = 0f;
            }

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }

        /// <summary>
        /// Show the panel with exhibit data.
        /// </summary>
        public void Show(ExhibitData data)
        {
            if (data == null) return;

            SetContent(data.title, data.description);
            FadeIn();
        }

        /// <summary>
        /// Hide the panel.
        /// </summary>
        public void Hide()
        {
            FadeOut();
        }

        /// <summary>
        /// Set the panel content directly.
        /// </summary>
        public void SetContent(string title, string description)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
        }

        private void FadeIn()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _fadeCoroutine = StartCoroutine(FadeToAlpha(1f));
        }

        private void FadeOut()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _fadeCoroutine = StartCoroutine(FadeToAlpha(0f));
        }

        private IEnumerator FadeToAlpha(float targetAlpha)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(elapsed / fadeDuration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _fadeCoroutine = null;
        }
    }
}
