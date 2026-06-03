using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class TowerPlacementSpot : MonoBehaviour
{
    [SerializeField] private Sprite whiteSquare;
    private bool menuOpen = false;
    private bool hasTower = false;
    private GameObject menuObject;
    private GameObject placedTower;

    private List<IndexSquare> menuObjects = new();

    public bool HasTower => hasTower;
    public Vector3 PlacementPosition => transform.position;


    private void OnMouseDown()
    {
        if (hasTower || closing || GameManager.Instance.PauseActive) return;          // Already occupied — ignore clicks

        if (menuOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        foreach(Object obj in FindObjectsByType(typeof(TowerPlacementSpot), FindObjectsSortMode.None))
        {
            TowerPlacementSpot spot = obj as TowerPlacementSpot;
            if(spot && spot.menuOpen)
            {
                spot.CloseMenu();
            }
        }

        menuOpen = true;
        menuObject = new GameObject("Menu");
        menuObject.transform.SetParent(transform, worldPositionStays: false);
        menuObject.transform.localPosition = Vector3.zero;

        float radius = 1.5f;

        menuObjects.Clear();

        for(int i = 0; i < MasterController.Instance.NumberOfAvailableTowers; i++)
        {
            float alpha = (float)i / Mathf.Max(MasterController.Instance.NumberOfAvailableTowers, 1);
            float angle = 2 * Mathf.PI * alpha;
            float x = radius * Mathf.Sin(angle);
            float y = radius * Mathf.Cos(angle);

            //Create the image with the tower to visually represent it
            GameObject towerButton = new GameObject($"{MasterController.Instance.GetTowerName(i)} button", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(IndexSquare));
            towerButton.transform.SetParent(menuObject.transform, false);
            towerButton.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = towerButton.GetComponent<SpriteRenderer>();
            sr.sprite = MasterController.Instance.GetTowerSprite(i);
            sr.color = Color.clear;
            sr.sortingOrder = 100;
            towerButton.GetComponent<BoxCollider2D>().size = Vector2.one;
            towerButton.GetComponent<BoxCollider2D>().layerOverridePriority = 10;

            //Setup click handler
            IndexSquare iSqr = towerButton.GetComponent<IndexSquare>();
            iSqr.Setup(i, new(x, y, 0), true);
            iSqr.SetTowerPlacement(this);
            menuObjects.Add(iSqr);
        }
    }
    bool closing = false;

    public void CloseMenu()
    {
        if(!closing)
        {
            StartCoroutine(CloseMenuAnim());
        }
    }


    private IEnumerator CloseMenuAnim()
    {
        closing = true;
        for(int i = 0; i < menuObjects.Count; i++)
        {
            menuObjects[i].Setup(i, Vector3.zero, false);
        }
        yield return new WaitForSecondsRealtime(.1f);

        menuOpen = false;
        if (menuObject != null) Destroy(menuObject);
        closing = false;
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