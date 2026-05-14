using UnityEngine;

[Tooltip("Add this to the inspector to any object to make a little area you can note things down")]
public class ObjectNote : MonoBehaviour
{
    [TextArea(1, 15)]
    public string note = "Write down what you want here, up to 15 lines can be written here";
}
