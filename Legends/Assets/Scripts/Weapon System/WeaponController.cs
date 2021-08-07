using UnityEngine;

[System.Serializable]
public class Clip
{
    public float ammo;

    public int size;

    public int rounds;

    public void ModifyAmmo(int amount)
    {
        ammo += amount;
    }
}

public enum WeaponState
{
    IDLING,
    SHOOTING, 
    RELOADING
}

public enum WeaponType
{
    GRENADE,
    GUN
}

/// <summary>
/// TODO - Setup auto-aiming for AI
/// TODO - Add function for refreshing weapon 
/// TODO - Add weapon sfx
/// </summary>
public abstract class WeaponController : MonoBehaviour
{
    public Transform FirePoint;
    public GameObject Projectile;
    public float GrenadeLaunchForce;
    protected int fireNameHash = Animator.StringToHash("attacking");
    protected int reloadNameHash = Animator.StringToHash("reload");
}
