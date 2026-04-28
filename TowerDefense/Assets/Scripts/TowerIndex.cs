using UnityEngine;

public class IndexSquare : MonoBehaviour
{
    private int index;

    public void Setup(int spotIndex) => index = spotIndex;

    private void OnMouseDown()
    {
        Debug.Log($"Tower index {index} selected");
    }
}