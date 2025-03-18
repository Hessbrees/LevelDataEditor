using SoE.Levels;
using SoE.LevelDataEditor;
using UnityEditor;
using UnityEditorInternal.VR;
using UnityEngine;

public class EditorInputEvents
{
    public Vector2 dragPosition = new();
    public bool isLeftCtrl;
    public bool isAuraMode;
    private LevelDataEditor editor;
    private LevelDataGrid levelDataGrid;
    private LevelDataStyleSetup styleData;
    private LevelDataAura levelDataAura;
    
    //drag hexgrid
    private float dragPower = 2;
    private bool isLeftArrow;
    private bool isRightArrow;
    private bool isUpArrow;
    private bool isDownArrow;

    public EditorInputEvents(LevelDataEditor editor)
    {
        this.editor = editor;
    }
    public void Setup(LevelDataGrid levelDataGrid, LevelDataStyleSetup styleData, LevelDataAura levelDataAura)
    {
        this.levelDataGrid = levelDataGrid;
        this.styleData = styleData;
        this.levelDataAura = levelDataAura;
    }
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.KeyDown:
                ProcessKeyDownEvent(currentEvent);
                TurnOnDragKeys(currentEvent);
                break;
            case EventType.KeyUp:
                ProcessKeyUpEvent(currentEvent);
                TurnOffDragKeys(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }
    private void ProcessKeyUpEvent(Event currentEvent)
    {
        if (currentEvent.keyCode == KeyCode.LeftControl)
        {
            isLeftCtrl = false;
        }
    }
    private void ProcessKeyDownEvent(Event currentEvent)
    {
        if (currentEvent.keyCode == KeyCode.LeftControl)
        {
            isLeftCtrl = true;
        }
        else if (currentEvent.keyCode == KeyCode.LeftAlt)
        {
            DeleteSelected(null);
        }
        else if (currentEvent.keyCode == KeyCode.K)
        {
            SwitchMode();
        }
    }
    private void SwitchMode()
    {
        isAuraMode = !isAuraMode;

        ClearSelected();
    }
    private void ClearSelected()
    {
        levelDataGrid.ResetGridLabelStyle();
        editor.ResetEntityLabelStyle();
        editor.ResetEntityDataLabelStyle();
        levelDataAura.ResetAuraLabelStyle();
    }
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1) // right mouse button
        {
            ClearSelected();
        }
        else if (currentEvent.button == 0) // left mouse button
        {
            SelectHex(currentEvent);
        }
    }

    public void UpdateHexGridPosition()
    {
        var dragVector = new Vector2();

        if (isLeftArrow)
        {
            dragVector += new Vector2(-dragPower, 0);
        }
        if (isRightArrow)
        {
            dragVector += new Vector2(dragPower, 0);
        }
        if (isUpArrow)
        {
            dragVector += new Vector2(0, -dragPower);
        }
        if (isDownArrow)
        {
            dragVector += new Vector2(0, dragPower);
        }

        dragPosition += dragVector;
    }
    private void TurnOnDragKeys(Event currentEvent)
    {
        if (currentEvent.keyCode == KeyCode.LeftArrow)
        {
            isLeftArrow = true;
        }

        if (currentEvent.keyCode == KeyCode.RightArrow)
        {
            isRightArrow = true;
        }

        if (currentEvent.keyCode == KeyCode.UpArrow)
        {
            isUpArrow = true;
        }

        if (currentEvent.keyCode == KeyCode.DownArrow)
        {
            isDownArrow = true;
        }
    }
    private void TurnOffDragKeys(Event currentEvent)
    {
        if (currentEvent.keyCode == KeyCode.LeftArrow)
        {
            isLeftArrow = false;
        }

        if (currentEvent.keyCode == KeyCode.RightArrow)
        {
            isRightArrow = false;
        }

        if (currentEvent.keyCode == KeyCode.UpArrow)
        {
            isUpArrow = false;
        }

        if (currentEvent.keyCode == KeyCode.DownArrow)
        {
            isDownArrow = false;
        }
    }
    /*        private void ChangeDragPosition(Event currentEvent)
            {
                if (currentEvent.keyCode == KeyCode.LeftArrow)
                {
                    dragPosition += new Vector2(-dragPower, 0);
                }

                if (currentEvent.keyCode == KeyCode.RightArrow)
                {
                    dragPosition += new Vector2(dragPower, 0);
                }

                if (currentEvent.keyCode == KeyCode.UpArrow)
                {
                    dragPosition += new Vector2(0, -dragPower);
                }

                if (currentEvent.keyCode == KeyCode.DownArrow)
                {
                    dragPosition += new Vector2(0, dragPower);
                }
            }*/
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 2) // middle mouse button
        {
            dragPosition += currentEvent.delta;
        }
    }
    private void DeleteSelectedEntity()
    {
        for (int i = 0; i < editor.entityLabelList.Count; i++)
        {
            if (editor.entityLabelList[i].isSelected)
            {
                editor.entitySet.entityDataList.RemoveAt(i);
                editor.RemoveEntityData(i);

                // select next free entity label
                if (i < editor.entityLabelList.Count)
                {
                    editor.SelectEntityLabel(editor.entityLabelList[i]);
                }
                else if (editor.entityLabelList.Count != 0)
                {
                    editor.SelectEntityLabel(editor.entityLabelList[i - 1]);
                }

                EditorUtility.SetDirty(editor.entitySet); // save changes in so

                break;
            }
        }
        editor.UpdateIndexLabelList();
        levelDataGrid.ResetGridLabelStyle();
    }
    private void DeleteSelected(object mousePositionObject)
    {
        if (isAuraMode)
        {
            levelDataAura.DeleteSelectedAura();
        }
        else
        {
            DeleteSelectedEntity();
        }
    }
    private void SelectHex(Event currentEvent)
    {
        levelDataGrid.ResetGridLabelStyle();

        foreach (var data in levelDataGrid.hexagonDataList)
        {
            if (levelDataGrid.CheckClickInHexagonData(data, currentEvent.mousePosition))
            {
                break;
            }
        }
    }
}