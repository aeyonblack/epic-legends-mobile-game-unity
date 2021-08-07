using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : WeaponController
{
    public Weapon weaponData;

    public Clip clip;
    private GameObject target;
    private Agent owner;

    private void Start()
    {
        owner = GetComponentInParent<Agent>();
    }

    public void StopAttack()
    {
        if (owner.animator) owner.animator.SetBool("attacking", false);
        StopCoroutine("DelayAttack");
    }

    public void StartAttack(GameObject target)
    {
        this.target = target;
        StartCoroutine("DelayAttack", target);
    }

    private IEnumerator DelayAttack(GameObject target)
    {
        owner.animator.SetBool("attacking", true);
        yield return new WaitForSeconds(0.5f);
        while(true)
        {
            ShootProjectile(target);
            AlertTarget();
            yield return new WaitForSeconds(weaponData.fireRate);
        }
    }

    public void ShootProjectile(GameObject target)
    {
        RemoveDeathListener();
        AddDeathListener(target);
        var projectile = Poolable.TryGetPoolable<Poolable>(Projectile);
        var bullet = projectile.GetComponent<Bullet>();

        if (bullet) bullet.Launch(FirePoint, weaponData.damage);
    }

    public void ThrowGrenade(GameObject target)
    {
        this.target = target;

        float? angle = RotateLaunchPoint();
        if (angle != null)
        {
            var projectile = Poolable.TryGetPoolable<Poolable>(Projectile);
            projectile.transform.position = FirePoint.position;
            projectile.transform.rotation = FirePoint.rotation;
            var grenade = projectile.GetComponent<Grenade>();

            if (grenade)
            {
                grenade.Launch(this);
            }
        }
        AlertTarget();
    }

    private float? RotateLaunchPoint()
    {
        float? angle = CalculateAngle(false);
        if (angle != null)
        {
            FirePoint.localEulerAngles = new Vector3(360f-(float)angle, 0f, 0f);
        }
        return angle; 
    }

    private float? CalculateAngle(bool low)
    {
        Vector3 targetDirection = target.transform.position - FirePoint.transform.position;
        float y = targetDirection.y;
        targetDirection.y = 0;
        float x = targetDirection.magnitude;
        float gravity = Physics.gravity.magnitude;
        float squareSpeed = GrenadeLaunchForce * GrenadeLaunchForce;
        float underSquareRoot = (squareSpeed * squareSpeed) - gravity * (gravity * x * x + 2 * y * squareSpeed);

        if (underSquareRoot >= 0)
        {
            float root = Mathf.Sqrt(underSquareRoot);
            float highAngle = squareSpeed + root;
            float lowAngle = squareSpeed - root;

            if (low)
            {
                return (Mathf.Atan2(lowAngle, gravity * x) * Mathf.Rad2Deg);
            }
            else
            {
                return (Mathf.Atan2(highAngle, gravity * x) * Mathf.Rad2Deg);
            }
        }
        return null;
    }

    /// <summary>
    /// What on earth is going on here mannn!!!!!!!!!!
    /// </summary>
    /// <param name="medKit"></param>
    private void OnTargetDied(Loot medKit)
    {
        StopAttack();
        if (!MaxHealth())
        {
            owner.OnEnemyKilled(medKit);
        }
    }

    private bool MaxHealth()
    {
        if (owner == null) return false;
        var health = owner.gameObject.GetComponent<Health>();
        return health.currentHealth == health.maxHealth;
    }

    /// <summary>
    /// listen to the current target's death event
    /// </summary>
    private void AddDeathListener(GameObject target)
    {
        if (target)
        {
            this.target = target;
            var rootHealth = target.GetComponent<Health>();
            var health = rootHealth ? rootHealth : target.GetComponentInParent<Health>();
            health.Dead += OnTargetDied;
        }
    }

    /// <summary>
    /// remove listner for the current target, we've either lost it or it's already dead
    /// </summary>
    private void RemoveDeathListener()
    {
        if (target)
        {
            var rootHealth = target.GetComponent<Health>();
            var health = rootHealth ? rootHealth : target.GetComponentInParent<Health>();
            health.Dead -= OnTargetDied;
        }
    }

    private void AlertTarget()
    {
        if (target)
        {
            if (target.GetComponent<Agent>())
            {
                target.GetComponent<Agent>().OnAttacked();
            }
        }
    }
}
