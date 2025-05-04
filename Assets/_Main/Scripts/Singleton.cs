using UnityEngine;

/// <summary>
/// Generic Singleton base class.
/// T must inherit from Singleton<T>.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    // The single instance 
    private static T instance;

    /// <summary>
    /// Accessor for the Singleton instance.
    /// Initializes on first access if necessary.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance in scene
                var existing = FindObjectOfType<T>();
                if (existing != null)
                {
                    instance = existing;
                    DontDestroyOnLoad(instance.gameObject);
                }
                else
                {
                    // Optionally create new GameObject
                    var obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Awake ensures only one instance persists.
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}