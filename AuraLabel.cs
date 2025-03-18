using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraLabel
{
    public int uniqueIndex;
    public AuraData auraData = new();
    public AuraSetupSO auraSetup
    {
        get
        {
            return auraData.auraSetup;
        }
        set
        {
            auraData.auraSetup = value;
        }
    }
    public HexCoordinates coordinates;
    public GUIStyle currentStyle;

    public bool isSelected;
    public bool isRandom;

    public override string ToString()
    {
        if (auraSetup == null) return "Empty aura";

        return $"{auraSetup.auraSetup.name}";
    }
}