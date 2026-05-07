using UnityEngine;

public class InspectorNameAttribute : PropertyAttribute
{
    public string displayedName;
    public InspectorNameAttribute(string name)
    {
        displayedName = name;
    }
}