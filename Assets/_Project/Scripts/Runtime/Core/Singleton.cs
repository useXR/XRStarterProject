using UnityEngine;

namespace Gallery
{
    /// <summary>
    /// Generic singleton base class for manager MonoBehaviours.
    /// Usage: public class AudioManager : Singleton&lt;AudioManager&gt;
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    // Check for destroyed instance (handles domain reload disabled)
                    if (_instance == null || !_instance)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            Debug.LogWarning($"[Singleton] No instance of {typeof(T)} found in scene.");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            // Reset quit flag on new play session
            _applicationIsQuitting = false;

            if (_instance == null || !_instance)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} found. Destroying this one.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
