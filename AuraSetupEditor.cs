using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AuraSetupEditor
{
    LevelDataAura levelDataAura;
    public AuraSetupEditor(LevelDataAura levelDataAura)
    {
        this.levelDataAura = levelDataAura;
    }

    public void Draw(AuraLabel auraLabel)
    {
        GUILayout.BeginArea(new Rect(440, 20, 800, 400));

        auraLabel.auraSetup = EditorGUI.ObjectField(new Rect(0, 100, 200, 25), auraLabel.auraSetup, typeof(AuraSetupSO), true) as AuraSetupSO;

        GUILayout.EndArea();
    }
    public void DrawMenu()
    {
        GUILayout.BeginArea(new Rect(440, 20, 800, 400));

        // add new aura button
        if (GUI.Button(new Rect(00, 140, 120, 25), "Add new aura"))
        {
            levelDataAura.AddNewAuraToList();
        }

        GUILayout.EndArea();
    }
}
