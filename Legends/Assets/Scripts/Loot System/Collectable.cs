using UnityEngine;
using MoreMountains.Feedbacks;

public abstract class Collectable : ScriptableObject
{

    public string itemName;

    public bool consumable;

    /// <summary>
    /// Determines whether the item can be stacked or not
    /// </summary>
    public bool isStackable;

    /// <summary>
    /// for instantiating into the scene/world
    /// </summary>
    public GameObject worldObjectPrefab;

    /// <summary>
    /// Defines how a specific item is used and by who
    /// </summary>
    /// <param name="user">The character that used the item, could be the player
    /// or a bot</param>
    /// <returns>true if the item is used</returns>
    public abstract bool UsedBy(CharacterData user);


}
