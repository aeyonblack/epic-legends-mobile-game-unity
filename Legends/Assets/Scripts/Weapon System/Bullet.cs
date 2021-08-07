using UnityEngine;

/// <summary>
/// Todo - clean up this mess 
/// </summary>
public class Bullet : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject flashEffect;
    public float speed = 25f;
    public float hitOffset = 0f;
    public bool useFirePointRotation;
    public Vector3 rotationOffset = Vector3.zero;
    public GameObject[] detachedPrefabs;

    private Rigidbody rigidBody;
    private float defaultSpeed;

    private ContactPoint contanctPoint;

    /// <summary>
    ///  amount of damage done by the weapon 
    ///  the bullet belongs to
    /// </summary>
    private float damage;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        defaultSpeed = speed; 
    }

    private void FixedUpdate()
    {
        if (speed != 0)
        {
            rigidBody.velocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        speed = 0;
        contanctPoint = collision.contacts[0];

        if (hitEffect) InstantiateHitEffect();

        if (detachedPrefabs.Length != 0) DetachPrefabs();

        DamageTarget(collision);

        ReturnToPool();
    }

    public void Launch(Transform firePoint, float damage)
    {
        this.damage = damage;
        transform.position = firePoint.position;
        transform.rotation = firePoint.rotation;
        if (flashEffect)
        {
            var flash = Poolable.TryGetPoolable<Poolable>(flashEffect);
            flash.transform.position = transform.position;
            flash.transform.rotation = Quaternion.identity;
            flash.transform.forward = transform.forward;
        }
        if (rigidBody) rigidBody.constraints = RigidbodyConstraints.None;
        if (speed == 0) speed = defaultSpeed;
    }

    private void InstantiateHitEffect()
    {
        var hit = Poolable.TryGetPoolable<Poolable>(hitEffect);
        hit.transform.position = contanctPoint.point + contanctPoint.normal * hitOffset;
        hit.transform.rotation = Quaternion.FromToRotation(Vector3.up, contanctPoint.normal);
        if (useFirePointRotation)
        {
            hit.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180, 0);
        }
        else if (rotationOffset != Vector3.zero)
        {
            hit.transform.rotation = Quaternion.Euler(rotationOffset);
        }
        else
        {
            hit.transform.LookAt(contanctPoint.normal + contanctPoint.point);
        }
    }

    private void DetachPrefabs()
    {
        foreach (var detachedPrefab in detachedPrefabs)
        {
            if (detachedPrefab) detachedPrefab.transform.parent = null;
        }
    }

    private void DamageTarget(Collision collision)
    {
        var child = collision.gameObject.GetComponent<Health>();
        //var target = collision.gameObject.GetComponent<Health>(); incase the
        //referenced collider is not directly attached to the parent game object
        var target = child ? child : collision.gameObject.GetComponentInParent<Health>();
        if (target != null)
        {
            ComputeDamage();
            target.TakeDamage(damage);
        }
    }

    private void ComputeDamage()
    {
        bool criticalHit = Random.Range(0, 100) < 25;
        float criticalDamage = Random.Range(1.5f, 2f);
        if (criticalHit) damage *= criticalDamage;
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;
        Poolable.TryPool(gameObject);
    }
}
