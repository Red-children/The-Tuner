using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BuffAddedEvent
{
    public BuffInstance buff;
}
public struct BuffRemovedEvent
{
    public BuffInstance buff;
}
public struct BuffStackChangedEvent
{
    public BuffInstance buff;
}

