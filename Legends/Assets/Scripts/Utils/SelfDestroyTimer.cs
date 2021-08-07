using UnityEngine;

/// <summary>
/// Helper component for self destruction
/// </summary>
public class SelfDestroyTimer : MonoBehaviour
{
    public float timeBeforeDestroy = 30;

    public Timer timer;

    protected virtual void OnEnable()
    {
        if (timer == null)
        {
            timer = new Timer(timeBeforeDestroy, OnTimeEnd);
        }
        else
        {
            timer.Reset();
        }
    }

    protected virtual void Update()
    {
        if (timer == null)
        {
            return;
        }
        timer.Tick(Time.deltaTime);
    }

    protected virtual void OnTimeEnd()
    {
        Poolable.TryPool(gameObject);
        timer.Reset();
    }
}
