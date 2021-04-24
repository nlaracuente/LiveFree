using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    [SerializeField, Tooltip("Enable this to prevent the object from being automatically destroyed on scene load")]
    protected bool isPersistent = false;

    public static T Instance
    {
        get
        {
            // Find existing one
            if (_instance == null)
                _instance = FindObjectOfType<T>();

            // Create new one
            if (_instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name, typeof(T));
                _instance = go.GetComponent<T>();
            }

            return _instance;
        }
    }

    public virtual void Awake()
    {
        // New instance but we already have one
        if (_instance != null && _instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        // Instance not yet set
        if (_instance == null)
        {
            _instance = this as T;

            if (isPersistent)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}