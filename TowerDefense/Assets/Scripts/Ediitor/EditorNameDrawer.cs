using UnityEditor;
using UnityEngine;


// JGN - AI slop but editor stuff isnt my expertise
[CustomPropertyDrawer(typeof(InspectorNameAttribute))]
public class InspectorNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InspectorNameAttribute nameAttr = (InspectorNameAttribute)attribute;

        // 1. SAFE INDEX EXTRACTION: Grab the very last bracket pair for the current nested level
        int index = 0;
        string path = property.propertyPath;

        int lastOpenBracket = path.LastIndexOf('[');
        int lastCloseBracket = path.LastIndexOf(']');

        if (lastOpenBracket >= 0 && lastCloseBracket > lastOpenBracket)
        {
            string indexString = path.Substring(lastOpenBracket + 1, lastCloseBracket - lastOpenBracket - 1);
            int.TryParse(indexString, out index);
        }

        // 2. Build the foldout label safely
        string finalLabel = $"{nameAttr.displayedName} {index}";

        // 3. Draw it exactly like before
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        Rect adjustedPosition = new Rect(position.x, position.y + 1, position.width, position.height - 1);
        EditorGUI.PropertyField(adjustedPosition, property, new GUIContent(finalLabel), true);

        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // This calculates the exact height Unity needs to draw the struct 
        // and all of its internal fields, including spacing.
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

