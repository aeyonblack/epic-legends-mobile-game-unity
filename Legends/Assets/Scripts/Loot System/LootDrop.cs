using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This behaviour can be attached to anything that can drop/spawn 
/// loot items e.g. crates, barrels and characters
/// </summary>
public class LootDrop : MonoBehaviour
{

    [System.Serializable]
    public class DropEvent
    {
        public LootEntry[] Entries;
    }

    [System.Serializable]
    public class LootEntry
    {
        public int Weight;
        public Collectable Item;
    }

    public class InternalPercentageEntry
    {
        public LootEntry Entry;
        public float Percentage;
    }

    public DropEvent[] Events;

    private CharacterData player;



    private void Start()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null)
        {
            if (obj.GetComponent<CharacterData>())
            {
                player = obj.GetComponent<CharacterData>();
            }
        }
    }

    /// <summary>
    /// Dont even bother modifying this function
    /// </summary>
    public void DropLoot()
    {
        for (int i = 0; i < Events.Length; ++i)
        {
            DropEvent Event = Events[i];

            //first iterate over all object to make a total weight value.
            int totalWeight = 0;
            foreach (var entry in Event.Entries)
            {
                totalWeight += entry.Weight;
            }

            //if we don't have any weight just exit
            if (totalWeight == 0)
                continue;

            //then go back again on all the object to build a lookup table based on percentage.
            List<InternalPercentageEntry> lookupTable = new List<InternalPercentageEntry>();
            float previousPercent = 0.0f;
            foreach (var entry in Event.Entries)
            {
                float percent = entry.Weight / (float)totalWeight;
                InternalPercentageEntry percentageEntry = new InternalPercentageEntry();
                percentageEntry.Entry = entry;
                percentageEntry.Percentage = previousPercent + percent;
                previousPercent = percentageEntry.Percentage;
                lookupTable.Add(percentageEntry);
            }

            float rng = Random.value;
            for (int k = 0; k < lookupTable.Count; ++k)
            {
                if (rng <= lookupTable[k].Percentage)
                {
                    if (lookupTable[k].Entry.Item)
                    {
                        GameObject obj = new GameObject(lookupTable[k].Entry.Item.itemName);
                        var l = obj.AddComponent<Loot>();
                        if (lookupTable[k].Entry.Item.itemName == "Medkit")
                            GetComponent<Health>().medKit = l;
                        l.item = lookupTable[k].Entry.Item;
                        l.Player = player;
                        l.aggroRange = 2.64f;
                        l.Spawn(transform.position, transform.rotation);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a drop event for any items
    /// that a dead character might be carrying
    /// </summary>
    /// <param name="item"></param>
    /// <param name="weight"></param>
    public void CreateDropEvent(Collectable item, int weight)
    {
        for (int i = 0; i < Events.Length; i++)
        {
            for (int j = 0; j < Events[i].Entries.Length; j++)
            {
                if (Events[i].Entries[j].Item == null)
                {
                    Events[i].Entries[j].Item = item;
                    Events[i].Entries[j].Weight = weight;
                    break;
                }
            }
        }
    }

}
