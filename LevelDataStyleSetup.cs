using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataStyleSetup
{
    public int entityLabelHeight = 40;
    public int backgroundLabelWidth = 200;
    public int backgroundLabelBorder = 2;

    public Color defaultColor = Color.white;
    public Color selectedColor = Color.blue;
    public Color selectedTextColor = new Color(0.5f, 0.7f, 1f);
    public Color goodColor = Color.green;
    public Color randomColor = new Color(0.75f, 0f, 0.54f);

    public GUIStyle defaultHexLabelStyle; // empty hex
    public GUIStyle selectedHexLabelStyle; // selected entity
    public GUIStyle goodHexLabelStyle; // hex with occuping entity
    public GUIStyle defaultEntityLabelStyle; // not selected entity on hexgrid
    public GUIStyle randomEntityLabelStyle; // entity with random position

    public GUIStyle selectedEntityLabelStyle; // selected entity
    public GUIStyle wrongEntityLabelStyle; // wrong position

    public Texture2D defaultBackground;
    public Texture2D selectedBackground;
    public Texture2D wrongBackground;
    public Texture2D randomBackground;

    // Create all color styles for texts/lines/buttons 
    public void SetupStyles()
    {
        defaultBackground = MakeTexWithBorder(backgroundLabelWidth, entityLabelHeight, Color.grey,          backgroundLabelBorder, Color.black);
        selectedBackground = MakeTexWithBorder(backgroundLabelWidth, entityLabelHeight, Color.blue, backgroundLabelBorder, Color.black);
        wrongBackground = MakeTexWithBorder(backgroundLabelWidth, entityLabelHeight, Color.red, backgroundLabelBorder, Color.black);
        randomBackground = MakeTexWithBorder(backgroundLabelWidth, entityLabelHeight, randomColor, backgroundLabelBorder, Color.black);

        defaultBackground.hideFlags = HideFlags.HideAndDontSave;
        selectedBackground.hideFlags = HideFlags.HideAndDontSave;
        wrongBackground.hideFlags = HideFlags.HideAndDontSave;
        randomBackground.hideFlags = HideFlags.HideAndDontSave;

        defaultHexLabelStyle = CreateHexLabelStyle(defaultColor);
        selectedHexLabelStyle = CreateHexLabelStyle(selectedTextColor);
        goodHexLabelStyle = CreateHexLabelStyle(goodColor);

        defaultEntityLabelStyle = CreateEntityLabelStyle(defaultBackground);
        selectedEntityLabelStyle = CreateEntityLabelStyle(selectedBackground);
        wrongEntityLabelStyle = CreateEntityLabelStyle(wrongBackground);
        randomEntityLabelStyle = CreateEntityLabelStyle(randomBackground);
    }
    private GUIStyle CreateHexLabelStyle(Color color)
    {
        GUIStyle hexLabelStyle = new();

        hexLabelStyle.fontSize = 13;
        hexLabelStyle.alignment = TextAnchor.MiddleCenter;
        hexLabelStyle.normal.textColor = color;

        return hexLabelStyle;
    }

    private GUIStyle CreateEntityLabelStyle(Texture2D background)
    {
        GUIStyle entityLabelStyle = new();

        entityLabelStyle.stretchHeight = false;
        entityLabelStyle.padding = new RectOffset(backgroundLabelBorder + 2, 0, backgroundLabelBorder, 0);
        entityLabelStyle.fixedHeight = entityLabelHeight;
        entityLabelStyle.alignment = TextAnchor.MiddleLeft;
        entityLabelStyle.fontSize = 12;
        entityLabelStyle.fontStyle = FontStyle.Bold;
        entityLabelStyle.normal.textColor = Color.white;
        entityLabelStyle.normal.background = background;

        return entityLabelStyle;
    }
    private Texture2D MakeTexWithBorder(int width, int height, Color col, int borderWidth, Color borderColor)
    {
        Color[] pix = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth)
                {
                    pix[y * width + x] = borderColor;
                }
                else
                {
                    pix[y * width + x] = col;
                }
            }
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}