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
            float angle = 2 * Mathf.PI * alpha;
            float x = radius * Mathf.Sin(angle);
            float y = radius * Mathf.Cos(angle);

            //Create the image with the tower to visually represent it
            GameObject towerButton = new GameObject($"{MasterController.Instance.GetTowerName(i)} button", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(IndexSquare));
            towerButton.transform.SetParent(menuObject.transform, false);
            towerButton.transform.localPosition = new(x, y, 0);
            SpriteRenderer sr = towerButton.GetComponent<SpriteRenderer>();
            sr.sprite = MasterController.Instance.GetTowerSprite(i);
            towerButton.GetComponent<BoxCollider2D>().size = Vector2.one;

            //Setup click handler
            IndexSquare iSqr = towerButton.GetComponent<IndexSquare>();
            iSqr.Setup(i);
            iSqr.SetTowerPlacement(this);
        }

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