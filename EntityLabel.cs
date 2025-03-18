using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLabel
{
    public int uniqueIndex;

    public EntityData entityData;

    public string name;
    public HexCoordinates coordinates;
    public GUIStyle currentStyle;

    public bool isSelected;
    public bool isRandom;

    public override string ToString()
    {
        if (isRandom)
        {
            return $"{name}" +
            $"\nRandom";
        }

        return $"{name}" +
               $"\n{coordinates.ToVector2Int()}";
    }
}

public class EntityDataLabel
{
    public EntityData entityData;
    public GUIStyle currentStyle;
    public bool isSelected;

    public override string ToString()
    {
        return $"{entityData.selectedEntity}";
    }
}