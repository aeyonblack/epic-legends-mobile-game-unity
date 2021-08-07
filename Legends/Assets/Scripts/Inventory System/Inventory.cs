using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for storing the player's collected items
/// </summary>
public abstract class Inventory
{
    public class InventoryEntry
    {
        public Collectable Item;
        public int Count;
        public int Slot;
    }

    public InventoryEntry[] Entries = new InventoryEntry[MAX_ENTRIES];

    /// <summary>
    /// The inventory can only hold 4 items at any instant
    /// </summary>
    private const int MAX_ENTRIES = 16;

    /// <summary>
    /// Defines an inventory slot that is available for storing items
    /// </summary>
    private int emptySlot;

    private Collectable itemToAdd;

    private CharacterData character;

    public void Init(CharacterData character)
    {
        this.character = character;
    }

    public void Add(Collectable item) {
        itemToAdd = item;
        AddNewItem();
    }

    public virtual bool Use(InventoryEntry entry)
    { 
        if (entry.Item.UsedBy(character))
        {
            if (entry.Item.consumable)
            {
                entry.Count -= 1;
                if (entry.Count <= 0) Entries[entry.Slot] = null;
            }
            return true;
        }
        return false;
    }


    public virtual void RemoveItem(Collectable item) { }

    private bool FindEmptySlot()
    {
        emptySlot = -1;
        for (int i = 0; i < MAX_ENTRIES; i++)
        {
            if (Entries[i] == null)
            {
                if (emptySlot == -1)
                    emptySlot = i;
                return true;
            }
            if (Entries[i].Item == itemToAdd)
            {
                return StackItem(i);
            }
        }
        return false;
    }

    private bool StackItem(int itemPosition)
    {
        if (Entries[itemPosition].Item.isStackable)
        {
            Entries[itemPosition].Count++;
        }

        return false;
    }

    private void AddNewItem()
    {
        bool foundEmptySlot = FindEmptySlot();
        if (foundEmptySlot)
        {
            if (itemToAdd.consumable)
            {
                if (!itemToAdd.UsedBy(character)) return;
            }
            else
            {
                InventoryEntry entry = new InventoryEntry();
                entry.Item = itemToAdd;
                entry.Count = 1;
                Entries[emptySlot] = entry;
                entry.Slot = emptySlot;
                itemToAdd.UsedBy(character);
            }
        }
    }

}
