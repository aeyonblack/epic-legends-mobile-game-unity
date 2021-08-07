using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float TimeToDestroyed = 4.0f;
    public float ReachRadius = 5.0f;
    public GameObject Explosion;

    private Rigidbody rigidBody;
    private float timeSinceLaunch;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        timeSinceLaunch += Time.deltaTime;
        if (timeSinceLaunch >= TimeToDestroyed) SelfDestruct();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Do nothing for now
    }

    /// <summary>
    /// Used by player
    /// </summary>
    /// <param name="launcher"></param>
    public void Launch(PlayerWeapon launcher)
    {
        transform.position = launcher.FirePoint.position;
        transform.forward = launcher.FirePoint.forward;
        timeSinceLaunch = 0;
        Vector3 velocity = launcher.FirePoint.forward * launcher.GrenadeLaunchForce;
        rigidBody.velocity = launcher.FireButton.cachedInput.magnitude == 0 ? 
            velocity : velocity * launcher.FireButton.cachedInput.magnitude;
      
    }

    /// <summary>
    /// Used by agent
    /// </summary>
    public void Launch(AgentWeapon launcher)
    {
        timeSinceLaunch = 0;
        rigidBody.velocity = launcher.GrenadeLaunchForce * launcher.FirePoint.transform.forward;
    }

    private void SelfDestruct()
    {
        var explosion = Poolable.TryGetPoolable<Poolable>(Explosion);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        Poolable.TryPool(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, ReachRadius);
    }
}
