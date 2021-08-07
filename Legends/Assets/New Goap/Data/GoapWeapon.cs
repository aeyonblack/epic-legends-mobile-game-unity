using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapWeapon : MonoBehaviour
{
    public Transform FirePoint;
    public GameObject Projectile;
    public Weapon weaponData;

    private GameObject target;
    private GoapLegend owner;

    private void Start()
    {
        owner = GetComponentInParent<GoapLegend>();
    }

    public void Fire(GameObject target)
    {
        ListenForTargetDeathEvent(target);
        StartCoroutine(DelayShoot());
    }

    public void HoldFire()
    {
        owner.character.GetAnimator().SetBool("attacking", false);
        owner.character.GetAnimator().SetLayerWeight(1, 1);
        StopAllCoroutines();
    }

    private IEnumerator DelayShoot()
    {
        owner.character.GetAnimator().SetBool("attacking", true);
        owner.character.GetAnimator().SetLayerWeight(1, 0);
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            ShootProjectile();
            yield return new WaitForSeconds(weaponData.fireRate);
        }
    }

    private void ShootProjectile()
    {
        OnAttackTarget();

        var projectile = Poolable.TryGetPoolable<Poolable>(Projectile);
        var bullet = projectile.GetComponent<Bullet>();

        if (bullet) bullet.Launch(FirePoint, weaponData.damage);
    }

    private void OnAttackTarget()
    {
        if (target)
        {
            var enemy = target.GetComponent<GoapLegend>();
            if (enemy) enemy.OnAttacked();
        }
    }

    private void OnTargetDead(Loot medkit)
    {
        owner.OnTargetDead();
    }

    private void ListenForTargetDeathEvent(GameObject newTarget)
    {
        if (target)
        {
            RemoveListeners();
        }
        target = newTarget;
        AddListeners();
    }

    private void RemoveListeners()
    {
        var rootHealth = target.GetComponent<Health>();
        var health = rootHealth ? rootHealth : target.GetComponentInParent<Health>();
        health.Dead -= OnTargetDead;
    }

    private void AddListeners()
    {
        var rootHealth = target.GetComponent<Health>();
        var health = rootHealth ? rootHealth : target.GetComponentInParent<Health>();
        health.Dead += OnTargetDead;
    }

    private void OnDestroy()
    {
        if (target)
        {
            RemoveListeners();
        }
    }

}
