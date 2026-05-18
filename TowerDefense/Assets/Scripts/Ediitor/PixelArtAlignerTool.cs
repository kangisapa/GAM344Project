using UnityEngine;
using UnityEditor;

public class PixelArtAlignerTool : EditorWindow
{
    [MenuItem("Tools/Pixel Art Aligner")]
    public static void ShowWindow()
    {
        PixelArtAlignerTool window = GetWindow<PixelArtAlignerTool>("Pixel Aligner");
        window.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        int selectionCount = Selection.transforms.Length;

        GUILayout.Label("Pixel Art Grid Aligner", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Snaps objects based on Sprite Pixel Dimensions.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);

        // Fetch Unity's active scene grid size
        float currentGridSize = EditorSnapSettings.gridSize.x;
        GUILayout.Label($"Current Editor Grid Size: {currentGridSize}", EditorStyles.miniLabel);

        EditorGUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(selectionCount == 0);
        if (GUILayout.Button("Align Selected Pixel Sprites", GUILayout.Height(40)))
        {
            AlignSprites(currentGridSize);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(15);
        GUILayout.Label($"Selected Objects: {selectionCount}", EditorStyles.centeredGreyMiniLabel);
    }

    private void AlignSprites(float gridSize)
    {
        if (gridSize <= 0f) return;

        Undo.RecordObjects(Selection.transforms, "Align Pixel Art to Grid");

        int alignedCount = 0;

        foreach (Transform t in Selection.transforms)
        {
            SpriteRenderer spriteRenderer = t.GetComponent<SpriteRenderer>();

            // Skip objects that don't have a sprite renderer or a valid sprite texture
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            // Get the actual pixel dimensions from the sprite texture rect
            Rect spriteRect = spriteRenderer.sprite.rect;

            int pixelWidth = Mathf.RoundToInt(spriteRect.width);
            int pixelHeight = Mathf.RoundToInt(spriteRect.height);
            Vector3 pos = t.position;

            // --- X Axis Alignment ---
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize; // Standard snap
            if (pixelWidth % 2 != 0)
            {
                // Width is ODD: Shift by half a grid size
                pos.x += gridSize * 0.5f;
            }

            // --- Y Axis Alignment ---
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize; // Standard snap
            if (pixelHeight % 2 != 0)
            {
                // Height is ODD: Shift by half a grid size
                pos.y += gridSize * 0.5f;
            }

            t.position = pos;
            alignedCount++;
        }

        Debug.Log($"Pixel Art Aligner: Successfully aligned {alignedCount} sprite(s).");
    }
}