using UnityEngine;
using UnityEditor;

public class PositionOffsetTool : EditorWindow
{
    private float offsetX = 0f;
    private float offsetY = 0f;

    [MenuItem("Tools/Position Offset Tool")]
    public static void ShowWindow()
    {
        PositionOffsetTool window = GetWindow<PositionOffsetTool>("Offset & Snap Tool");
        window.minSize = new Vector2(300, 250);
    }

    private void OnGUI()
    {
        int selectionCount = Selection.transforms.Length;

        // --- SECTION 1: OFFSET ---
        GUILayout.Label("Offset Selected Objects", EditorStyles.boldLabel);
        offsetX = EditorGUILayout.FloatField("X Offset", offsetX);
        offsetY = EditorGUILayout.FloatField("Y Offset", offsetY);

        EditorGUILayout.Space(5);

        EditorGUI.BeginDisabledGroup(selectionCount == 0);
        if (GUILayout.Button("Apply Offset", GUILayout.Height(25)))
        {
            ApplyOffsetToSelection();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);
        Handles.DrawLine(new Vector2(10, GUILayoutUtility.GetLastRect().yMax + 5), new Vector2(position.width - 10, GUILayoutUtility.GetLastRect().yMax + 5));
        EditorGUILayout.Space(10);

        // --- SECTION 2: GRID SNAPPING ---
        GUILayout.Label("Grid Snapping", EditorStyles.boldLabel);

        // Fetch Unity's current active editor grid size
        float currentGridSize = EditorSnapSettings.gridSize.x;
        GUILayout.Label($"Current Editor Grid Size: {currentGridSize}", EditorStyles.miniLabel);

        EditorGUILayout.Space(5);

        EditorGUI.BeginDisabledGroup(selectionCount == 0);
        if (GUILayout.Button($"Snap X & Y to Grid ({currentGridSize})", GUILayout.Height(30)))
        {
            SnapSelectionToGrid(currentGridSize);
        }
        EditorGUI.EndDisabledGroup();

        // --- FOOTER ---
        EditorGUILayout.Space(15);
        GUILayout.Label($"Selected Objects: {selectionCount}", EditorStyles.centeredGreyMiniLabel);
    }

    private void ApplyOffsetToSelection()
    {
        Undo.RecordObjects(Selection.transforms, "Apply Position Offset");

        Vector3 offset = new Vector3(offsetX, offsetY, 0f);
        foreach (Transform t in Selection.transforms)
        {
            t.position += offset;
        }
    }

    private void SnapSelectionToGrid(float gridSize)
    {
        if (gridSize <= 0f) return;

        Undo.RecordObjects(Selection.transforms, "Snap Objects to Grid");

        foreach (Transform t in Selection.transforms)
        {
            Vector3 pos = t.position;

            // Math conversion to round the position to the nearest grid step
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;

            t.position = pos;
        }
    }
}