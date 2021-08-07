using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Loot : InteractableObject
{
    public Collectable item;

    [HideInInspector]
    public CharacterData Player;

    public bool playerLooting;

    public bool legendLooting;

    public bool doneLooting;

    private Slider lootProgress;

    private void Start()
    {
        if (Player == null)
        {
            GetPlayer();
        }
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        CreateWorldObject();
        lootProgress = GetComponentInChildren<Slider>();
        lootProgress.maxValue = 5;
    }

    private void GetPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            Player = player.GetComponent<CharacterData>();
        }
    }

    private void Update()
    {
        ListenForPlayer();
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        Vector3 randomPoint = position + Random.insideUnitSphere * 1f;
        randomPoint.y = 0;
        transform.position = randomPoint;
        transform.rotation = rotation * Quaternion.AngleAxis(rotation.y, Vector3.up);
    }

    public override void InteractWith(CharacterData character)
    {
        legendLooting = true;
        StartCoroutine(Pickup(character));
    }

    public override IEnumerator InteractWithDelay(CharacterData character)
    {
        legendLooting = true;
        yield return Pickup(character);
    }

    private void ListenForPlayer()
    {
        if (Player != null)
        {
            bool inRange = Vector3.Distance(transform.position, Player.transform.position) < aggroRange;
            if (inRange)
            {
                TryPickup();
            }
            else
            {
                if (playerLooting)
                {
                    StopAllCoroutines();
                    playerLooting = false;
                    lootProgress.value = 0;
                }
            }
        }
    }

    private void TryPickup()
    {
        if (!(playerLooting || legendLooting))
        {
            StartCoroutine(Pickup(Player));
            playerLooting = true;
        }
        else if (legendLooting)
        {
            // TODO - play loot denied feedback
        }
    }

    private IEnumerator Pickup(CharacterData character)
    {
        if (character)
        {
            for (float l = 0; l < 5; l += character.lootSpeed)
            {
                lootProgress.value = l;
                yield return null;
            }
            character.Backpack.Add(item);
            Destroy(gameObject);
        }
    }

    private void CreateWorldObject()
    {
        if (item != null)
        {
            Vector3 localPosition = new Vector3(0f, 1f, 0f);
            var loot = Instantiate(item.worldObjectPrefab, transform, false);
            loot.transform.localPosition = localPosition;
            Helpers.RecursiveLayerChange(loot.transform, LayerMask.NameToLayer("Collectable"));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

}
