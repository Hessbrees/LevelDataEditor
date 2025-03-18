using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDataEditor : EditorWindow
{
    EditorInputEvents editorEvents;
    LevelDataInfobox infobox;
    LevelDataStyleSetup styleData;
    LevelDataGrid levelDataGrid;
    LevelDataAura levelDataAura;

    public EntitySetDataSO entitySetData;
    public EntitySetSO entitySet;
    public EntitySetSO currentEntitySet; // for compare with changed entity set

    public List<EntityLabel> entityLabelList = new List<EntityLabel>();
    public List<EntityDataLabel> entityDataLabelList = new List<EntityDataLabel>();

    public Vector2 startingGridPositionDraw = new Vector2(700, 450);

    Vector2Int gridSize = new Vector2Int(9, 5);

    Vector2 scrollPos;
    Vector2 scrollDataPos;

    private bool isEntityLabelListUpdated;

    // create new entity 
    private EntitySetType entitySetType;
    private EntitySetType currentSetType;

    [MenuItem("Level Data Editor", menuItem = "Window/Level Data/Level Data Editor")]

    private static void OpenWindow()
    {
        GetWindow<LevelDataEditor>("Level Data Editor");
    }
    private void OnEnable()
    {
        editorEvents = new(this);
        infobox = new();
        styleData = new();
        levelDataGrid = new(editorEvents, styleData, this);
        levelDataAura = new(this, styleData,levelDataGrid,editorEvents);

        editorEvents.Setup(levelDataGrid, styleData,levelDataAura);
        levelDataGrid.Setup(levelDataAura);

        styleData.SetupStyles();
        infobox.CreateInfobox();
    }
    public void OnDisable()
    {
        // destroy created textures
        DestroyImmediate(styleData.defaultBackground);
        DestroyImmediate(styleData.selectedBackground);
        DestroyImmediate(styleData.wrongBackground);
        DestroyImmediate(styleData.randomBackground);
    }
    private void OnGUI()
    {
        CreateWindow();

        infobox.ShowInfoBox();

        editorEvents.ProcessEvents(Event.current);

        editorEvents.UpdateHexGridPosition();
        
        levelDataGrid.DrawHexagonGrid();

        levelDataAura.GenerateAuraSet();

        GenerateEntitySet();

        GenerateEntitySetData();

        // Create aura setup window
        levelDataAura.CreateWindow();

        ShowAllLabelsOnGrid();

        ShowSelectedLabelOnGrid();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        if (EditorGUI.EndChangeCheck() && entitySet != null)
            EditorUtility.SetDirty(entitySet);

        if (GUI.changed)
            Repaint();
    }
    // Create visible elements in editor
    private void CreateWindow()
    {
        GUILayout.BeginArea(new Rect(20, 20, 800, 400));

        if (GUI.Button(new Rect(00, 50, 120, 25), "Generate grid"))
        {
            levelDataGrid.CreateNewGridData(gridSize);
        }

        if (GUI.Button(new Rect(00, 140, 150, 25), "Set random position"))
        {
            SetRandomPosition();
        }

        gridSize = EditorGUI.Vector2IntField(new Rect(0, 0, 120, 25), "Grid Size", gridSize);

        entitySet = EditorGUI.ObjectField(new Rect(0, 100, 200, 25), entitySet, typeof(EntitySetSO), true) as EntitySetSO;

        if (currentEntitySet != entitySet || isEntityLabelListUpdated)
        {
            UpdateLabelList();
            levelDataAura.UpdateLabelList();

            isEntityLabelListUpdated = false;
            currentEntitySet = entitySet;
        }

        // Create new entity panel
        entitySetData = EditorGUI.ObjectField(
            new Rect(210, 100, 200, 25), entitySetData, typeof(EntitySetDataSO), true) as EntitySetDataSO;

        entitySetType = (EntitySetType)EditorGUI.EnumPopup(new Rect(210, 140, 200, 25), entitySetType);

        if (currentSetType != entitySetType) // reset list after change enum
        {
            currentSetType = entitySetType;
            UpdateEntitySetDataList();
        }

        GUILayout.EndArea();
    }
    // Change entity to random/notRandom position
    private void SetRandomPosition()
    {
        for (int i = 0; i < entityLabelList.Count; i++)
        {
            if (entityLabelList[i].isSelected)
            {
                bool isRandom = !entityLabelList[i].isRandom;
                entityLabelList[i].isRandom = isRandom;
                this.entitySet.entityDataList[i].isRandomPosition = isRandom;

                if (isRandom)
                {
                    foreach (var hexagonData in levelDataGrid.hexagonDataList)
                    {
                        if (entityLabelList[i].coordinates == hexagonData.coordinates)
                        {
                            hexagonData.currentColor = styleData.defaultColor;
                            hexagonData.currentStyle = styleData.defaultHexLabelStyle;
                            hexagonData.isGreen = false;

                            break;
                        }
                    }
                }

                break;
            }
        }
    }
    // Create list with entity labels with current type from selected enum
    public void UpdateEntitySetDataList()
    {
        if (entitySetData == null) return;

        entityDataLabelList.Clear();

        if (entitySetType == EntitySetType.TERRAIN)
        {
            foreach (var gameObject in entitySetData.terrainList)
            {
                EntityData entityData = new EntityData(entitySetData, entitySetType, gameObject.name.ToString(), new Vector3Int(0, 0, 0));

                EntityDataLabel entityDataLabel = GetNewEntityDataLabel(entityData);

                entityDataLabelList.Add(entityDataLabel);
            }
        }
        else if (entitySetType == EntitySetType.ALLY)
        {
            foreach (var gameObject in entitySetData.allyList)
            {
                EntityData entityData = new EntityData(entitySetData, entitySetType, gameObject.name.ToString(), new Vector3Int(0, 0, 0));

                EntityDataLabel entityDataLabel = GetNewEntityDataLabel(entityData);

                entityDataLabelList.Add(entityDataLabel);
            }
        }
        else if (entitySetType == EntitySetType.ENEMY)
        {
            foreach (var gameObject in entitySetData.monsterList)
            {
                EntityData entityData = new EntityData(entitySetData, entitySetType, gameObject.name.ToString(), new Vector3Int(0, 0, 0));

                EntityDataLabel entityDataLabel = GetNewEntityDataLabel(entityData);

                entityDataLabelList.Add(entityDataLabel);
            }
        }
        else if (entitySetType == EntitySetType.POWER_UP)
        {
            foreach (var gameObject in entitySetData.powerUpList)
            {
                EntityData entityData = new EntityData(entitySetData, entitySetType, gameObject.name.ToString(), new Vector3Int(0, 0, 0));

                EntityDataLabel entityDataLabel = GetNewEntityDataLabel(entityData);

                entityDataLabelList.Add(entityDataLabel);
            }
        }
    }
    // reset and create new entity label list
    public void UpdateLabelList()
    {
        if (entitySet == null) return;

        entityLabelList.Clear();

        int index = 0;

        foreach (var entityData in entitySet.entityDataList)
        {
            AddEntityLabelToList(entityData, index);
            index++;
        }
    }
    // reset unique index in label list
    public void UpdateIndexLabelList()
    {
        for (int i = 0; i < entityLabelList.Count; i++)
        {
            entityLabelList[i].uniqueIndex = i;
        }
    }
    public void InsertNewEntityData(EntityData entityData, int index)
    {
        if (entityData == null) return;

        EntityLabel entityLabel = GetNewEntityLabel(entityData, 0);

        entityLabelList.Insert(index, entityLabel);
    }
    public void RemoveEntityData(int index)
    {
        entityLabelList.RemoveAt(index);
    }
    private void AddEntityLabelToList(EntityData entityData, int index)
    {
        if (entityData == null) return;

        EntityLabel entityLabel = GetNewEntityLabel(entityData, index);

        entityLabelList.Add(entityLabel);
    }
    // Create new Entity label with reference to entityData
    private EntityLabel GetNewEntityLabel(EntityData entityData, int index)
    {
        EntityLabel entityLabel = new EntityLabel();

        entityLabel.entityData = entityData;
        entityLabel.uniqueIndex = index;
        entityLabel.name = entityData.selectedEntity;
        entityLabel.coordinates = ConvertVectorToCoordinates(entityData.position);
        
        if(entityData.isRandomPosition)
        {
            entityLabel.currentStyle = styleData.randomEntityLabelStyle;
            entityLabel.isRandom = true;
        }
        else
        {
            entityLabel.currentStyle = styleData.defaultEntityLabelStyle;
            entityLabel.isRandom = false;
        }

        return entityLabel;
    }
    // Create new Entity label with reference to entityData
    private EntityDataLabel GetNewEntityDataLabel(EntityData entityData)
    {
        EntityDataLabel entityDataLabel = new EntityDataLabel();
        entityDataLabel.entityData = entityData;
        entityDataLabel.currentStyle = styleData.defaultEntityLabelStyle;

        return entityDataLabel;
    }
    public HexCoordinates ConvertVectorToCoordinates(Vector3Int position) => new HexCoordinates(position.x, position.z);
    // Create scroll area with entity labels
    private void GenerateEntitySet()
    {
        if (entitySet == null) return;
        GUILayout.BeginArea(new Rect(20, 200, styleData.backgroundLabelWidth + 2 * styleData.backgroundLabelBorder, 2000));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
            GUILayout.Width(styleData.backgroundLabelWidth), GUILayout.Height(400));

        foreach (var entityLabel in entityLabelList)
        {
            CreateEntityLabel(entityLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    // Create scroll area with entity labels for new entities
    private void GenerateEntitySetData()
    {
        if (entitySetData == null) return;
        GUILayout.BeginArea(new Rect(230, 200, styleData.backgroundLabelWidth + 2 * styleData.backgroundLabelBorder, 2000));
        scrollDataPos = EditorGUILayout.BeginScrollView(scrollDataPos, false, false,
            GUILayout.Width(styleData.backgroundLabelWidth), GUILayout.Height(400));

        foreach (var entityLabelData in entityDataLabelList)
        {
            CreateEntityDataLabel(entityLabelData);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    // Create label as interactive button with style
    private void CreateEntityLabel(EntityLabel entityLabel)
    {
        if (GUILayout.Button(entityLabel.ToString(), entityLabel.currentStyle))
        {
            if(!editorEvents.isAuraMode) // Check aura mode 
            SelectEntityLabel(entityLabel);
        }
    }
    // reset all selected entity on lists and find new on list
    public void SelectEntityLabel(EntityLabel entityLabel)
    {
        ResetEntityLabelStyle();
        ResetEntityDataLabelStyle();

        if (entityLabel.currentStyle == styleData.defaultEntityLabelStyle)
        {
            entityLabel.currentStyle = styleData.selectedEntityLabelStyle;
            entityLabel.isSelected = true;
        }
        else if (entityLabel.currentStyle == styleData.selectedEntityLabelStyle)
        {
            entityLabel.currentStyle = styleData.defaultEntityLabelStyle;
            entityLabel.isSelected = false;
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
    }
    // Create label as interactive button with style
    private void CreateEntityDataLabel(EntityDataLabel entityDataLabel)
    {
        if (GUILayout.Button(entityDataLabel.ToString(), entityDataLabel.currentStyle))
        {
            if (!editorEvents.isAuraMode) // Check aura mode 
                SelectEntityDataLabel(entityDataLabel);
        }
    }
    // reset all selected entity on lists and find new on list with new entites
    private void SelectEntityDataLabel(EntityDataLabel entityDataLabel)
    {
        ResetEntityLabelStyle();
        ResetEntityDataLabelStyle();

        if (entityDataLabel.currentStyle == styleData.defaultEntityLabelStyle)
        {
            entityDataLabel.currentStyle = styleData.selectedEntityLabelStyle;
            entityDataLabel.isSelected = true;
        }
        else if (entityDataLabel.currentStyle == styleData.selectedEntityLabelStyle)
        {
            entityDataLabel.currentStyle = styleData.defaultEntityLabelStyle;
            entityDataLabel.isSelected = false;
        }
    }
    // Check bool isSelected in all labels and change it style
    private void ShowSelectedLabelOnGrid()
    {
        foreach (var entityLabel in entityLabelList)
        {
            if (entityLabel.isSelected)
            {
                foreach (var hex in levelDataGrid.hexagonDataList)
                {
                    if (!entityLabel.isRandom)
                    {
                        if (entityLabel.coordinates == hex.coordinates)
                        {
                            hex.currentColor = styleData.selectedColor;
                            hex.currentStyle = styleData.selectedHexLabelStyle;
                            hex.isGreen = false;
                        }
                    }

                }
                break;
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
    }
    // Refresh all label styles in list 
    private void ShowAllLabelsOnGrid()
    {
        foreach (var entityLabel in entityLabelList)
        {
            //reset label style
            if (entityLabel.currentStyle == styleData.wrongEntityLabelStyle)
            {
                if (!entityLabel.isSelected)
                    entityLabel.currentStyle = styleData.defaultEntityLabelStyle;
            }

            bool isHexFind = false;

            foreach (var hex in levelDataGrid.hexagonDataList)
            {
                if(!entityLabel.isRandom)
                {
                    if (entityLabel.coordinates == hex.coordinates)
                    {
                        hex.currentColor = styleData.goodColor;
                        hex.currentStyle = styleData.goodHexLabelStyle;
                        hex.isGreen = true;
                        hex.occupingEntity = entityLabel.name;

                        isHexFind = true;
                        break;
                    }
                }
                else
                {
                    isHexFind = true;
                    if (!entityLabel.isSelected)
                        entityLabel.currentStyle = styleData.randomEntityLabelStyle;
                    break;
                }
            }
            if (!entityLabel.isSelected && !entityLabel.isRandom)
                CheckDuplicateCoordinates(entityLabel);

            if (!isHexFind)
            {
                if (!entityLabel.isSelected)
                    entityLabel.currentStyle = styleData.wrongEntityLabelStyle;
            }
        }

        levelDataAura.ShowAllLabelsOnGrid();
    }
    // compare label coordinates with all in list and change style to wrong if find other 
    private void CheckDuplicateCoordinates(EntityLabel currentLabel)
    {
        foreach (var entityLabel in entityLabelList)
        {
            if (entityLabel.coordinates == currentLabel.coordinates)
            {
                if (entityLabel.uniqueIndex != currentLabel.uniqueIndex && !entityLabel.isRandom)
                {
                    currentLabel.currentStyle = styleData.wrongEntityLabelStyle;
                }
            }
        }
    }
    // reset all entity style in list to default
    public void ResetEntityLabelStyle()
    {
        foreach (var entityLabel in entityLabelList)
        {
            entityLabel.currentStyle = styleData.defaultEntityLabelStyle;
            entityLabel.isSelected = false;
        }
    }
    // reset all entity (list with new entity) style in list to default
    public void ResetEntityDataLabelStyle()
    {
        foreach (var entityDataLabel in entityDataLabelList)
        {
            entityDataLabel.currentStyle = styleData.defaultEntityLabelStyle;
            entityDataLabel.isSelected = false;
        }
    }
}
