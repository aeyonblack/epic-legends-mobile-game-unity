using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;


public class PlayerWeapon : WeaponController
{
    [Header("Settings")]
    public Joystick FireButton;
    public AimingHelper WeaponAim;
    public Transform PlayerForward;
    public LayerMask TargetMask;
    public Weapon weaponData;
    public Clip clip;
   
    [Header("CameraShaking")]
    public float ShakeStrength;
    public float ShakeDuration;

    public MMFeedbacks AttackFeedback;

    private Animator weaponAnimator;

    private WeaponState state = WeaponState.IDLING;


    private void Awake()
    {
        if (GetComponent<Animator>())
        {
            weaponAnimator = GetComponent<Animator>();
        }

        FireButton.OnAttack += StartDelayAttack;
        FireButton.OnStopAttack += StopAttack;

        FireButton.OnHandleAtCenter += ToggleAim;
        FireButton.OnHandleNotAtCenter += ToggleAim;

        FireButton.Attack = false;
    }

    private void Update()
    {
        UpdateControllerState();
        TryDrawPath();
    }

    private void StartDelayAttack()
    {
        StartCoroutine("DelayAttack");
    }

    private void StopAttack()
    {
        PlayerController.instance.Animator.SetBool("attacking", false);
        StopCoroutine("DelayAttack");
    }

    private IEnumerator DelayAttack()
    {
        PlayerController.instance.Animator.SetBool("attacking", true);
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            ShootProjectile();
            yield return new WaitForSeconds(weaponData.fireRate);
        }
    }

    public void ShootProjectile()
    {
        if (state != WeaponState.IDLING || clip.rounds == 0)
            return;

        state = WeaponState.SHOOTING;

        //CameraShaker.instance.Shake(ShakeStrength, ShakeDuration);
        AttackFeedback?.PlayFeedbacks();

        if (weaponAnimator) weaponAnimator.SetTrigger("fire");

        var projectile = Poolable.TryGetPoolable<Poolable>(Projectile);
        var bullet = projectile.GetComponent<Bullet>();
        if (bullet) bullet.Launch(FirePoint, weaponData.damage);

        AlertTarget();

        clip.rounds--;
    }

    /// <summary>
    /// Maybe this has to go at some point
    /// </summary>
    public void ThrowGrenade()
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

    public void Reload()
    {
        if (state != WeaponState.IDLING) return;
        if (clip.ammo == 0) return;
        state = WeaponState.RELOADING;
        int roundsToLoad = (int)Mathf.Min(clip.ammo, clip.size - clip.rounds);
        clip.rounds += roundsToLoad;
        clip.ModifyAmmo(-roundsToLoad);
        PlayerController.instance.Animator.SetTrigger("reload");
    }

    private void UpdateControllerState()
    {
        var info = PlayerController.instance.Animator.GetCurrentAnimatorStateInfo(0);
        WeaponState newState;

        if (info.shortNameHash == fireNameHash)
        {
            newState = WeaponState.SHOOTING;
        }
        else if (info.shortNameHash == reloadNameHash)
        {
            newState = WeaponState.RELOADING;
        }
        else
        {
            newState = WeaponState.IDLING;
        }

        if (newState != state)
        {
            var oldState = state;
            state = newState;
            if (oldState == WeaponState.SHOOTING)
            {
                if (clip.rounds == 0)
                {
                    StopAttack();
                    Reload();
                }
            }
        }
    }

    private void AlertTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(PlayerForward.position, PlayerForward
            .TransformDirection(Vector3.forward), out hit, TargetMask))
        {
            if (hit.collider.gameObject.GetComponent<GoapLegend>())
            {
                hit.collider.gameObject.GetComponent<GoapLegend>().OnAttacked();
            }
        }
    }

    private void TryDrawPath()
    {
        if (WeaponAim.isProjection && WeaponAim.isEnabled)
        {
            DrawProjection projection = WeaponAim as DrawProjection;
            projection.DrawPath(this);
        }
    }

    private void ToggleAim()
    {
        if (FireButton.HandleAtCenter)
        {
            WeaponAim.Disable();
        }
        else
        {
            WeaponAim.Enable();
        }
    }

    private void OnDisable()
    {
        FireButton.OnAttack -= StartDelayAttack;
        FireButton.OnStopAttack -= StopAttack;
    }
}
