using System;
using UnityEngine;
using MoreMountains.Feedbacks;

public class Health : MonoBehaviour
{
    public float maxHealth = 100;

    public float startingHealth = 100;

    public float currentHealth;

    public MMFeedbacks DamageFeedback;

    public MMFeedbacks DeathFeedbacks;

    public GameObject DeathSmoke;

    public GameObject DeathSkull;

    private CharacterData character;

    private LootDrop lootDrop;

    public Action<Loot> Dead;

    public Action LowHealth;

    [HideInInspector]
    public Loot medKit;

    private bool dead = false;

    private void Start()
    {
        character = GetComponent<CharacterData>();
        lootDrop = GetComponent<LootDrop>();
        currentHealth = startingHealth;
    }

    public void TakeDamage(float damage)
    {
        ModifyHealth(-damage);
        // something is not working properly here
        // I don't know what is wrong with the feedbacks position
        DamageFeedback?.PlayFeedbacks(transform.position, (int)damage);

        if (currentHealth == 0)
        {
            if (!dead)
            {
                dead = true;
                CreateDeathFX();
                DropLoot();
                Dead?.Invoke(medKit);
                Destroy(gameObject);
            }
        }
        else if (currentHealth < 100)
        {
            LowHealth?.Invoke();
        }
    }

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (currentHealth == 100)
        {
            var legend = GetComponent<GoapLegend>();
            if (legend) legend.beliefs.RemoveState("hasLowHealth");
        }
    }

    private void DropLoot()
    {
        lootDrop.CreateDropEvent(character.CurrentWeapon, 60);
        lootDrop.DropLoot();
    }

    private void CreateDeathFX()
    {
        var deathSkull = Poolable.TryGetPoolable<Poolable>(DeathSkull);
        deathSkull.transform.position = new Vector3(transform.position.x, 1.71f, transform.position.z);

        var deathSmoke = Poolable.TryGetPoolable<Poolable>(DeathSmoke);
        deathSmoke.transform.position = transform.position;
    }
}
