using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDataAura
{
    AuraSetupEditor auraSetupEditor;
    public List<AuraLabel> auraLabelList = new();
    private LevelDataEditor editor;
    private LevelDataStyleSetup styleData;
    private LevelDataGrid levelDataGrid;
    private EditorInputEvents editorEvents;
    Vector2 scrollPos;
    public LevelDataAura(LevelDataEditor editor, LevelDataStyleSetup styleData, LevelDataGrid levelDataGrid, EditorInputEvents editorEvents)
    {
        this.editor = editor;
        this.styleData = styleData;
        this.levelDataGrid = levelDataGrid;
        this.editorEvents = editorEvents;
        auraSetupEditor = new(this);
    }
    public void UpdateLabelList()
    {
        if (editor.entitySet == null) return;

        auraLabelList.Clear();

        int index = 0;

        foreach (var auraData in editor.entitySet.auraDataList)
        {
            AddAuraLabelToList(auraData, index);
            index++;
        }
    }
    private void AddAuraLabelToList(AuraData auraData, int index)
    {
        if (auraData == null) return;

        AuraLabel auraLabel = GetNewAuraLabel(auraData, index);

        auraLabelList.Add(auraLabel);
    }
    private AuraLabel GetNewAuraLabel(AuraData auraData, int index)
    {
        AuraLabel auraLabel = new AuraLabel();

        auraLabel.auraData = auraData;
        auraLabel.uniqueIndex = index;
        auraLabel.coordinates = editor.ConvertVectorToCoordinates(auraData.position);

        if (auraData.isRandomPosition)
        {
            auraLabel.currentStyle = styleData.randomEntityLabelStyle;
            auraLabel.isRandom = true;
        }
        else
        {
            auraLabel.currentStyle = styleData.defaultEntityLabelStyle;
            auraLabel.isRandom = false;
        }

        return auraLabel;
    }
    public void CreateWindow()
    {
        auraSetupEditor.DrawMenu();

        foreach (var auraLabel in auraLabelList)
        {
            if (auraLabel.isSelected)
            {
                auraSetupEditor.Draw(auraLabel);
                break;
            }
        }
    }
    public void GenerateAuraSet()
    {
        if (editor.entitySet == null) return;
        GUILayout.BeginArea(new Rect(440, 200, styleData.backgroundLabelWidth + 2 * styleData.backgroundLabelBorder, 2000));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
            GUILayout.Width(styleData.backgroundLabelWidth), GUILayout.Height(400));

        foreach (var auraLabel in auraLabelList)
        {
            CreateAuraLabel(auraLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    // Create label as interactive button with style
    private void CreateAuraLabel(AuraLabel auraLabel)
    {
        if (GUILayout.Button(auraLabel.ToString(), auraLabel.currentStyle))
        {
            if (editorEvents.isAuraMode) // Check mode
                SelectAuraLabel(auraLabel);
        }
    }
    // reset all selected entity on lists and find new on list
    public void SelectAuraLabel(AuraLabel auraLabel)
    {
        ResetAuraLabelStyle();

        if (auraLabel.currentStyle == styleData.defaultEntityLabelStyle)
        {
            auraLabel.currentStyle = styleData.selectedEntityLabelStyle;
            auraLabel.isSelected = true;
        }
        else if (auraLabel.currentStyle == styleData.selectedEntityLabelStyle)
        {
            auraLabel.currentStyle = styleData.defaultEntityLabelStyle;
            auraLabel.isSelected = false;
        }
    }
    // reset all aura style in list to default
    public void ResetAuraLabelStyle()
    {
        foreach (var auraLabel in auraLabelList)
        {
            auraLabel.currentStyle = styleData.defaultEntityLabelStyle;
            auraLabel.isSelected = false;
        }
    }

    public void ShowAllLabelsOnGrid()
    {
        foreach (var hex in levelDataGrid.hexagonDataList)
        {
            hex.occupingAuras.Clear();

            foreach (var auraLabel in auraLabelList)
            {
                if (!auraLabel.isRandom)
                {
                    if (auraLabel.coordinates == hex.coordinates)
                    {
                        hex.occupingAuras.Add(auraLabel.ToString());
                    }
                }
                else
                {
                    if (!auraLabel.isSelected)
                        auraLabel.currentStyle = styleData.randomEntityLabelStyle;
                    break;
                }
            }

        }
    }
    
    public void SelectAuraLabel(HexagonData hexagonData)
    {
        foreach (var auraLabel in auraLabelList)
        {
            if (!auraLabel.isRandom)
                if (auraLabel.isSelected)
                {
                    ChangeSelectedCoordinates(auraLabel, hexagonData.coordinates);

                    hexagonData.currentColor = styleData.selectedColor;
                    hexagonData.currentStyle = styleData.selectedHexLabelStyle;
                    hexagonData.isGreen = false;
                }
        }
        ResetAuraLabelStyle();

        // check all aura label
        foreach (var auraLabel in auraLabelList)
        {
            if (!auraLabel.isRandom)
                if (auraLabel.coordinates == hexagonData.coordinates)
                {
                    auraLabel.isSelected = true;
                    auraLabel.currentStyle = styleData.selectedEntityLabelStyle;
                }
        }

    }
    // change coordinates in aura label for selected hex
    private void ChangeSelectedCoordinates(AuraLabel auraLabel, HexCoordinates hexCoordinates)
    {
        auraLabel.auraData.position = hexCoordinates.ToVector3Int();
        auraLabel.coordinates = hexCoordinates;
    }
    // create new aura in list
    public void AddNewAuraToList()
    {
        AuraData auraData = new AuraData();
        editor.entitySet.auraDataList.Add(auraData);
        AuraLabel auraLabel = GetNewAuraLabel(auraData, 0);
        auraLabelList.Add(auraLabel); 
        UpdateIndexLabelList();
    }
    // deep clone selected aura to target position
    public void HandleCloneAura(HexagonData hexagonData)
    {
        for (int i = 0; i < auraLabelList.Count; i++)
        {
            if (auraLabelList[i].isSelected)
            {
                AuraData auraData = new AuraData(
                    auraLabelList[i].auraData.auraSetup,
                    hexagonData.coordinates.ToVector3Int());

                editor.entitySet.auraDataList.Insert(i, auraData);
                InsertNewAuraData(auraData, i);

                break;
            }
        }

        UpdateIndexLabelList();  
        EditorUtility.SetDirty(editor.entitySet); // save changes in so
    }
    // reset unique index in label list
    public void UpdateIndexLabelList()
    {
        for (int i = 0; i < auraLabelList.Count; i++)
        {
            auraLabelList[i].uniqueIndex = i;
        }
    }
    public void InsertNewAuraData(AuraData auraData, int index)
    {
        if (auraData == null) return;

        AuraLabel auraLabel = GetNewAuraLabel(auraData, 0);

        auraLabelList.Insert(index, auraLabel);
    }
    public void AddAuraNamesOnGrid(HexagonData hexagonData, ref string showLabel)
    {
        foreach (var occupingAura in hexagonData.occupingAuras)
        {
            showLabel += "\n" + occupingAura;
        }
    }
    public void RemoveAuraData(int index)
    {
        auraLabelList.RemoveAt(index);
    }
    public void DeleteSelectedAura()
    {
        for (int i = 0; i < auraLabelList.Count; i++)
        {
            if (auraLabelList[i].isSelected)
            {
                editor.entitySet.auraDataList.RemoveAt(i);
                RemoveAuraData(i);

                // select next free entity label
                if (i < auraLabelList.Count)
                {
                    SelectAuraLabel(auraLabelList[i]);
                }
                else if (auraLabelList.Count != 0)
                {
                    SelectAuraLabel(auraLabelList[i - 1]);
                }

                EditorUtility.SetDirty(editor.entitySet); // save changes in so
            }
        }
        UpdateIndexLabelList();
        levelDataGrid.ResetGridLabelStyle();
    }
}