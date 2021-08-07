using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    /// <summary>
    /// The static reference to the instance
    /// </summary>
    public static T instance { get; protected set; }

    /// <summary>
    /// Returns true if an instance of this singleton exists
    /// </summary>
    public static bool instanceExists
    {
        get { return instance != null; }
    }

    /// <summary>
    /// This is to associate current singleton with instance
    /// </summary>
    protected virtual void Awake()
    {
        if (instanceExists)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
        }
    }

    /// <summary>
    /// Clears singleton association 
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
