using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexagonData
{
    public HexCoordinates coordinates;

    public Vector2 centerPos;

    public string occupingEntity;

    public List<string> occupingAuras = new();

    public Color currentColor;

    public GUIStyle currentStyle;

    public bool isGreen;
}