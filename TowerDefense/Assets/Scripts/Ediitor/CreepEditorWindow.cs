using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Creep), true)]
public class CreepEditorWindow : Editor
{
    //value stuff
    private SerializedProperty isSlowPropertyName;
    private SerializedProperty slowRadiusPropertyName;
    private SerializedProperty slowMultiplierPropertyName;

    //boss creep stuff
    private SerializedProperty isSummonerPropertyName;
    private SerializedProperty summonCreepPrefabPropertyName;
    private SerializedProperty summonCountPropertyName;
    private SerializedProperty summonIntervalPropertyName;

    private void OnEnable()
    {
        isSlowPropertyName = serializedObject.FindProperty("isSlow");
        slowRadiusPropertyName = serializedObject.FindProperty("slowRadius");
        slowMultiplierPropertyName = serializedObject.FindProperty("slowMultiplier");

        if(IsBossCreep())
        {
            isSummonerPropertyName = serializedObject.FindProperty("isSummoner");
            summonCreepPrefabPropertyName = serializedObject.FindProperty("summonCreepPrefab");
            summonCountPropertyName = serializedObject.FindProperty("summonCount");
            summonIntervalPropertyName = serializedObject.FindProperty("summonInterval");
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while(iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (EqualContentsForMultiple(iterator, new[] { slowRadiusPropertyName, slowMultiplierPropertyName }) && !isSlowPropertyName.boolValue) continue;
            else if (IsBossCreep() && EqualContentsForMultiple(iterator, new[] { summonCreepPrefabPropertyName, summonCountPropertyName, summonIntervalPropertyName }) && !isSummonerPropertyName.boolValue) continue;
            EditorGUILayout.PropertyField(iterator, true); // Draw the property
        }
        serializedObject.ApplyModifiedProperties(); // Apply changes

    }

    private bool EqualContentsForMultiple(SerializedProperty iterator, SerializedProperty[] toCompareTo)
    {
        foreach(SerializedProperty property in toCompareTo)
        {
            if(SerializedProperty.EqualContents(iterator, property))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBossCreep()
    {
        return target is BossCreep;
    }
}
