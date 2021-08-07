using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.UI;

public class LootBox : InteractableObject
{
    /// <summary>
    /// Feedbacks played when the box spawns loot
    /// </summary>
    public MMFeedbacks DropFeedbacks;

    public GameObject LootBoxNormal;
    public GameObject LootBoxFractured;
    public GameObject LootBoxBase;

    private  CharacterData Player; 
    private Animator animator;
    private LootDrop loot;

    private Slider lootProgress;
    private bool playerOpeningBox;
    private bool legendOpeningBox;

    private void Start()
    {
        animator = GetComponent<Animator>();
        lootProgress = GetComponentInChildren<Slider>();
        loot = GetComponent<LootDrop>();
        lootProgress.maxValue = 10;
        GetPlayer();
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

    public override void InteractWith(CharacterData character)
    {

    }

    public override IEnumerator InteractWithDelay(CharacterData character)
    {
        legendOpeningBox = true;
        yield return OpenLootBox(character);
    }

    /// <summary>
    /// Called via animation as animation event
    /// </summary>
    public void LootReward()
    {
        DropFeedbacks?.PlayFeedbacks(transform.position);
        CrackBox();
        loot.DropLoot();
    }

    private void CrackBox()
    {
        LootBoxNormal.SetActive(false);
        LootBoxBase.SetActive(false);
        lootProgress.gameObject.SetActive(false);
        if (LootBoxFractured != null)
        {
            Instantiate(LootBoxFractured, transform.position, transform.rotation);
        }
        Destroy(gameObject, 1);
    }

    private IEnumerator OpenLootBox(CharacterData character)
    {
        if (character)
        {
            for (float l = 0; l < 10; l += 0.05f)
            {
                lootProgress.value = l;
                yield return null;
            }
            animator.SetBool("Open", true);
        }
    }

    private void TryOpenBox(CharacterData character)
    {
        if (!(playerOpeningBox || legendOpeningBox))
        {
            StartCoroutine(OpenLootBox(character));
            playerOpeningBox = true;
        }
        else
        {
            // TODO - play loot denied feedback
        }
    }

    private void ListenForPlayer()
    {
        if (Player != null)
        {
            bool inRange = Vector3.Distance(transform.position, Player.transform.position) < aggroRange;
            if (inRange)
            {
                Hover(true);
                TryOpenBox(Player);
            }
            else
            {
                Hover(false);
                if (playerOpeningBox)
                {
                    StopAllCoroutines();
                    playerOpeningBox = false;
                    lootProgress.value = 0;
                }
            }
        }
    }

    private void Hover(bool hover)
    {
        animator.SetBool("Idle", !hover);
        animator.SetBool("Hover", hover);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

}
