using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class TowerPlacementSpot : MonoBehaviour
{
    [SerializeField] private Sprite whiteSquare;
    private bool menuOpen = false;
    private bool hasTower = false;
    private GameObject menuObject;
    private GameObject placedTower;

    public bool HasTower => hasTower;
    public Vector3 PlacementPosition => transform.position;


    private void OnMouseDown()
    {
        if (hasTower) return;          // Already occupied — ignore clicks

        if (menuOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        if (!MasterController.Instance.AllTowersCached()) return;

        menuOpen = true;
        menuObject = new GameObject("Menu");
        menuObject.transform.SetParent(transform, worldPositionStays: false);
        menuObject.transform.localPosition = Vector3.zero;

        float radius = 1.5f;

        for(int i = 0; i < MasterController.Instance.NumberOfAvailableTowers; i++)
        {
            float alpha = (float)i / Mathf.Max(MasterController.Instance.NumberOfAvailableTowers, 1);
            Debug.Log(alpha);
            float angle = 2 * Mathf.PI * alpha;
            float x = radius * Mathf.Sin(angle);
            float y = radius * Mathf.Cos(angle);

            // Square
            GameObject square = new GameObject($"Square {i}", typeof(SpriteRenderer), typeof(BoxCollider2D));
            square.transform.SetParent(menuObject.transform, worldPositionStays: false);
            square.transform.localPosition = new(x, y, 0);
            SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
            sr.sprite = MasterController.Instance.GetTowerSprite(i);
            square.GetComponent<BoxCollider2D>().size = Vector2.one;

            // Click handler
            square.AddComponent<IndexSquare>().Setup(i);
            square.GetComponent<IndexSquare>().SetTowerPlacement(this);
        }


        //top would be (0, 1.5, 0)


    }

    public void CloseMenu()
    {
        menuOpen = false;
        if (menuObject != null) Destroy(menuObject);
    }

    public void MarkAsOccupied(GameObject tower = null)
    {
        hasTower = true;
        placedTower = tower;
        if (menuOpen) CloseMenu();
    }

    // Optional: call this if the tower is sold/destroyed so the spot becomes reusable
    public void ClearTower()
    {
        hasTower = false;
        placedTower = null;
    }
}