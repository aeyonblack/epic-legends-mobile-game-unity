using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class World
{
    private static readonly World instance = new World();

    private static WorldStates world;

    public static World Instance
    {
        get { return instance; }
    }

    private World() { }

    static World()
    {
        world = new WorldStates();
    }

    public WorldStates GetWorld()
    {
        return world;
    }
}
