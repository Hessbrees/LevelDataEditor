using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDataGrid
{
    public List<HexagonData> hexagonDataList = new List<HexagonData>();
    int hexagonSize = 60;
    public EditorInputEvents editorEvents;
    public LevelDataStyleSetup styleData;
    public LevelDataEditor editor;
    public LevelDataAura levelDataAura;
    public LevelDataGrid(EditorInputEvents editorEvents, LevelDataStyleSetup styleData, LevelDataEditor editor)
    {
        this.editorEvents = editorEvents;
        this.styleData = styleData;
        this.editor = editor;
    }
    public void Setup(LevelDataAura levelDataAura)
    {
        this.levelDataAura = levelDataAura;
    }
    // Draw hex grid
    public void DrawHexagonGrid()
    {
        Queue<HexagonData> selectedHexQueue = new();

        foreach (HexagonData data in hexagonDataList)
        {
            if (data.currentColor != styleData.defaultColor)
            {
                selectedHexQueue.Enqueue(data);
                continue;
            }
            // reset occuping entity on hex
            data.occupingEntity = "";

            GenerateHexagon(data);
        }

        while (selectedHexQueue.Count > 0)
        {
            GenerateHexagon(selectedHexQueue.Dequeue());
        }
    }
    // Draw single hex with lines
    private void GenerateHexagon(HexagonData hexagonData)
    {
        // center hex pos with drag position offset
        Vector2 centerPos = hexagonData.centerPos + editorEvents.dragPosition;

        float connectingLineWigth = 3;
        float height = hexagonSize * Mathf.Sqrt(3) / 2; // height of equilateral triangle in hexagon

        Vector2 upPoint = new Vector2(centerPos.x, centerPos.y + hexagonSize);
        Vector2 leftUpPoint = new Vector2(centerPos.x - height, centerPos.y + hexagonSize / 2);
        Vector2 rightUpPoint = new Vector2(centerPos.x + height, centerPos.y + hexagonSize / 2);
        Vector2 downPoint = new Vector2(centerPos.x, centerPos.y - hexagonSize);
        Vector2 leftDownPoint = new Vector2(centerPos.x - height, centerPos.y - hexagonSize / 2);
        Vector2 rightDownPoint = new Vector2(centerPos.x + height, centerPos.y - hexagonSize / 2);

        Handles.DrawBezier(upPoint, leftUpPoint, upPoint, leftUpPoint, hexagonData.currentColor, null, connectingLineWigth);
        Handles.DrawBezier(leftUpPoint, leftDownPoint, leftUpPoint, leftDownPoint, hexagonData.currentColor, null, connectingLineWigth);
        Handles.DrawBezier(leftDownPoint, downPoint, leftDownPoint, downPoint, hexagonData.currentColor, null, connectingLineWigth);
        Handles.DrawBezier(downPoint, rightDownPoint, downPoint, rightDownPoint, hexagonData.currentColor, null, connectingLineWigth);
        Handles.DrawBezier(rightDownPoint, rightUpPoint, rightDownPoint, rightUpPoint, hexagonData.currentColor, null, connectingLineWigth);
        Handles.DrawBezier(rightUpPoint, upPoint, rightUpPoint, upPoint, hexagonData.currentColor, null, connectingLineWigth);

        string coordinatesLabel = $"({hexagonData.coordinates.X},{hexagonData.coordinates.Z})";

        // show entity, coordinates and aura on hex
        string showLabel = coordinatesLabel + "\n" + hexagonData.occupingEntity;

        levelDataAura.AddAuraNamesOnGrid(hexagonData,ref showLabel);

        Vector2 labelSize = new Vector2(0, 0);
        Vector2 labelOffset = new Vector2(0, -8);

        // show coordinates and entity on hex
        EditorGUI.LabelField(new Rect(centerPos + labelOffset, labelSize), showLabel, hexagonData.currentStyle);

        GUI.changed = true;
    }
    // clear hex data list and generate new
    public void CreateNewGridData(Vector2Int size)
    {
        hexagonDataList.Clear();

        int[] rowsLengths = CalculateLenghtSize(size.x, size.y);
        int[] rowsOffset = CalculateOffsetSize(size.y);

        Vector2Int centerOffset = CalculateCenterOffset(size, rowsOffset);

        // move grid to drawining starting point
        Vector2 startingPointOffset = CalculateGridDistanceToCenter(size);

        for (int y = 0; y < size.y; y++)
        {
            int rowLength = rowsLengths[y];
            int rowOffset = rowsOffset[y];

            for (int x = 0; x < rowLength; x++)
            {
                AddHexagonToList(x + 1 + rowOffset, y, centerOffset, startingPointOffset);
            }
        }
    }
    // calculate hex positions and coordinates
    private int[] CalculateOffsetSize(int y)
    {
        int[] offsetArray = new int[y];

        int biggestNumber = -1;

        int maxIndex;

        if (y % 2 == 0) maxIndex = y - 1;
        else maxIndex = y;

        int middleIndex = (maxIndex - 1) / 2;
        int k = 2;

        if (y % 4 == 0 || (y + 1) % 4 == 0)
        {
            for (int i = 0; i < offsetArray.Length; i++)
            {
                int j = Mathf.FloorToInt((middleIndex - i + 1) / 2);

                if (i <= middleIndex)
                {
                    offsetArray[i] = biggestNumber + j;
                }
                else if (i > middleIndex)
                {
                    offsetArray[i] = biggestNumber + Mathf.FloorToInt(k / 2);
                    k++;
                }
            }
        }
        else
        {
            for (int i = 0; i < offsetArray.Length; i++)
            {
                int j = Mathf.FloorToInt((middleIndex - i) / 2);

                if (i <= middleIndex)
                {
                    offsetArray[i] = biggestNumber + j;
                }
                else if (i == middleIndex + 1)
                {
                    offsetArray[i] = -1;
                }
                else if (i > middleIndex + 1)
                {
                    offsetArray[i] = biggestNumber + Mathf.FloorToInt(k / 2);
                    k++;
                }
            }
        }

        return offsetArray;
    }
    private int[] CalculateLenghtSize(int x, int y)
    {
        int[] leghtArray = new int[y];

        int biggestNumber = x;
        int maxIndex;

        if (x % 2 == 0)
        {
            if (y % 2 == 0)
            {
                maxIndex = y - 1;
            }
            else
            {
                maxIndex = y + 1;
            }
        }
        else maxIndex = y;

        int middleIndex = (maxIndex - 1) / 2;
        int smallestNumber = x - middleIndex;

        int k = 0;

        for (int i = 0; i < leghtArray.Length; i++)
        {
            if (i <= middleIndex)
            {
                leghtArray[i] = smallestNumber + i;
            }
            else if (i > middleIndex)
            {
                k++;
                leghtArray[i] = biggestNumber - k;
            }
            if (leghtArray[i] < 0) leghtArray[i] = 0;
        }
        return leghtArray;
    }
    public Vector2 CalculateGridDistanceToCenter(Vector2 size)
    {
        float distanceX = 0;

        if ((size.y + 1) % 4 == 0 || (size.y) % 4 == 0) distanceX = -(hexagonSize * 0.86251f);

        if (size.y < 2) return new Vector2(distanceX, 0);

        if (size.y % 2 == 0)
        {
            return new Vector2(distanceX, (size.y - 2) * hexagonSize * 0.75f);
        }
        else
        {
            return new Vector2(distanceX, (size.y - 1) * hexagonSize * 0.75f);
        }

    }
    // check every hex in list after click left mouse button
    public bool CheckClickInHexagonData(HexagonData hexagonData, Vector2 currentMousePos)
    {
        Vector2 centerPos = hexagonData.centerPos + editorEvents.dragPosition;

        if (Mathf.Abs(centerPos.x - currentMousePos.x) > hexagonSize) return false;

        float distance = hexagonSize - Mathf.Abs(centerPos.x - currentMousePos.x);

        float height = hexagonSize / 2 + distance / 2;

        if (Mathf.Abs(centerPos.y - currentMousePos.y) > height) return false;

        if (!editorEvents.isLeftCtrl)
        {
            HandleSelectedHex(hexagonData);
        }
        else if (editorEvents.isLeftCtrl)
        {
            HandleClone(hexagonData);
        }

        return true;
    }
    private void HandleClone(HexagonData hexagonData)
    {
        if (editorEvents.isAuraMode)
        {
            levelDataAura.HandleCloneAura(hexagonData);
        }
        else
        {
            HandleCloneEntity(hexagonData);
        }
    }
    // deep clone selected entity to target position
    private void HandleCloneEntity(HexagonData hexagonData)
    {
        for (int i = 0; i < editor.entityLabelList.Count; i++)
        {
            if (editor.entityLabelList[i].isSelected)
            {
                EntityData entityData = new EntityData(
                    editor.entityLabelList[i].entityData.entitySetData,
                    editor.entityLabelList[i].entityData.entityType,
                    editor.entityLabelList[i].entityData.selectedEntity,
                    hexagonData.coordinates.ToVector3Int());

                editor.entitySet.entityDataList.Insert(i, entityData);
                editor.InsertNewEntityData(entityData, i);
                
                EditorUtility.SetDirty(editor.entitySet); // save changes in so
                break;
            }
        }
        editor.UpdateIndexLabelList();
    }
    // deep clone selected entity on list with new entities to target position
    private void HandleCloneEntity(HexagonData hexagonData, EntityData entityData, int index)
    {
        EntityData cloneEntityData = new EntityData(
                entityData.entitySetData,
                entityData.entityType,
                entityData.selectedEntity,
                hexagonData.coordinates.ToVector3Int());

        editor.entitySet.entityDataList.Insert(index, cloneEntityData);
        editor.InsertNewEntityData(cloneEntityData, index);

        editor.UpdateIndexLabelList();

        EditorUtility.SetDirty(editor.entitySet); // save changes in so
    }
    private void SelectEntityLabel(HexagonData hexagonData)
    {
        foreach (var entityLabel in editor.entityLabelList)
        {
            if (!entityLabel.isRandom)
            {
                if (entityLabel.isSelected && !hexagonData.isGreen)
                {
                    ChangeSelectedCoordinates(entityLabel, hexagonData.coordinates);

                    hexagonData.currentColor = styleData.selectedColor;
                    hexagonData.currentStyle = styleData.selectedHexLabelStyle;
                    hexagonData.isGreen = false;
                    return;
                }
                else if (entityLabel.coordinates == hexagonData.coordinates)
                {
                    // change selected target on hexgrid
                    editor.ResetEntityLabelStyle();
                    editor.ResetEntityDataLabelStyle();

                    entityLabel.isSelected = true;
                    entityLabel.currentStyle = styleData.selectedEntityLabelStyle;
                    return;
                }
            }
        }
    }
    // Find selected hex in labels list
    private void HandleSelectedHex(HexagonData hexagonData)
    {
        if(editorEvents.isAuraMode)
        {
            levelDataAura.SelectAuraLabel(hexagonData);
            
        }
        else
        {
            SelectEntityLabel(hexagonData);
            CloneSelectedEntity(hexagonData); // clone selected entity data 
        }

    }
    private void CloneSelectedEntity(HexagonData hexagonData)
    {
        foreach (var entityDataLabel in editor.entityDataLabelList)
        {
            if (entityDataLabel.isSelected && !hexagonData.isGreen)
            {
                HandleCloneEntity(hexagonData, entityDataLabel.entityData, 0);

                hexagonData.currentColor = styleData.selectedColor;
                hexagonData.currentStyle = styleData.selectedHexLabelStyle;
                hexagonData.isGreen = true;
                return;
            }
        }
    }
    public void ResetGridLabelStyle()
    {
        foreach (var data in hexagonDataList)
        {
            data.currentColor = styleData.defaultColor;
            data.currentStyle = styleData.defaultHexLabelStyle;
        }
    }
    // change coordinates in entity label for selected hex
    private void ChangeSelectedCoordinates(EntityLabel entityLabel, HexCoordinates hexCoordinates)
    {
        entityLabel.entityData.position = hexCoordinates.ToVector3Int();
        entityLabel.coordinates = hexCoordinates;
    }
    // create hex data and setup hex coordinates
    private void AddHexagonToList(int x, int y, Vector2Int centerOffset, Vector2 distanceToCenter)
    {
        HexagonData hexagonData = new HexagonData();

        hexagonData.currentColor = styleData.defaultColor;
        hexagonData.currentStyle = styleData.defaultHexLabelStyle;
        hexagonData.isGreen = false;

        Vector2 coordinates = new();
        coordinates.x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f * 2 * hexagonSize);
        coordinates.y = -y * (HexMetrics.outerRadius * 1.5f * 2 * hexagonSize);

        hexagonData.centerPos = new Vector2(
            editor.startingGridPositionDraw.x + coordinates.x,
            editor.startingGridPositionDraw.y + coordinates.y);

        hexagonData.centerPos += distanceToCenter;

        SetupHexCoordinates(hexagonData, x, y, centerOffset);

        hexagonDataList.Add(hexagonData);
    }
    // Setup hex coordinates with center offset
    private void SetupHexCoordinates(HexagonData hexagonData, int x, int y, Vector2Int centerOffset)
    {
        var coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
        hexagonData.coordinates = new HexCoordinates(coordinates.X - centerOffset.x, coordinates.Z - centerOffset.y);
    }
    private Vector2Int CalculateCenterOffset(Vector2Int size, int[] rowsOffset)
    {
        int maxIndexInX;
        int maxIndexInY;

        if (size.y % 2 == 0) maxIndexInY = size.y - 1;
        else maxIndexInY = size.y;

        if (size.x % 2 == 0) maxIndexInX = size.x - 1;
        else maxIndexInX = size.x;

        int middleIndexInX = (maxIndexInX - 1) / 2;
        int middleIndexInY = (maxIndexInY - 1) / 2;

        Vector2Int startPos = new Vector2Int(1 + rowsOffset[0], 0);

        int centerX = startPos.x + middleIndexInY * -1 + middleIndexInX;
        int centerY = startPos.y + middleIndexInY * 1;

        return new Vector2Int(centerX - 3, centerY - 2);
    }
}
