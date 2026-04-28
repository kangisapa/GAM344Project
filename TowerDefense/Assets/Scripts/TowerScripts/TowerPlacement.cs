using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class TowerPlacementSpot : MonoBehaviour
{
    private MasterController masterController;
    [SerializeField] private int spotIndex = 0;

    private bool menuOpen = false;
    private GameObject menuObject;


    private void Awake()
    {
        masterController = FindAnyObjectByType<MasterController>();
        if (masterController == null)
        {
            Debug.LogWarning("Could not find a MasterController in the scene.");
        }
    }

    private void OnMouseDown()
    {
        if (menuOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        menuOpen = true;
        menuObject = new GameObject("Menu");
        menuObject.transform.SetParent(transform, worldPositionStays: false);
        menuObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        // Square
        GameObject square = new GameObject("Square", typeof(SpriteRenderer), typeof(BoxCollider2D));
        square.transform.SetParent(menuObject.transform, worldPositionStays: false);
        square.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        square.GetComponent<BoxCollider2D>().size = Vector2.one;

        // Label
        GameObject label = new GameObject("Label", typeof(TextMesh));
        label.transform.SetParent(square.transform, worldPositionStays: false);
        label.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        TextMesh tm = label.GetComponent<TextMesh>();
        tm.text = $"#{spotIndex}";
        tm.fontSize = 12;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = Color.white;

        // Click handler
        square.AddComponent<IndexSquare>().Setup(spotIndex);
        square.GetComponent<IndexSquare>().SetTowerPlacement(this);
        square.GetComponent<IndexSquare>().SetMasterController(masterController);
    }

    public void CloseMenu()
    {
        menuOpen = false;
        Destroy(menuObject);
    }
}