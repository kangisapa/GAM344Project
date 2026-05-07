using UnityEditor;
using UnityEngine;


// JGN - AI slop but editor stuff isnt my expertise
[CustomPropertyDrawer(typeof(InspectorNameAttribute))]
public class InspectorNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. Grab the attribute data (the "Path" you typed in your list)
        InspectorNameAttribute nameAttr = (InspectorNameAttribute)attribute;
        string prefix = nameAttr.displayedName;

        // 2. Get the current index from the property path (e.g., "paths.Array.data[0]")
        int index = System.Convert.ToInt32(property.propertyPath.Substring(property.propertyPath.IndexOf("[")).Replace("[", "").Replace("]", ""));

        // 3. Create the final label string (e.g., "Path 0: MyValue")
        string finalLabel = $"{prefix} {index}";

        // 4. Draw it
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        property.stringValue = EditorGUI.TextField(position, finalLabel, property.stringValue);

        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}