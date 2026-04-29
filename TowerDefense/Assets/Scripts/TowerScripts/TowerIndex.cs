using UnityEngine;

public class IndexSquare : MonoBehaviour
{
    private TowerPlacementSpot towerPlacementSpot;
    private MasterController masterController;
    private int index;

    public void Setup(int spotIndex) => index = spotIndex;

    public void SetTowerPlacement(TowerPlacementSpot parentTowerPlacementSpot)
    {
        towerPlacementSpot = parentTowerPlacementSpot;
    }

    public void SetMasterController(MasterController gameMasterController)
    {
        masterController = gameMasterController;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Tower index {index} selected");

        towerPlacementSpot.CloseMenu();

        masterController.SpawnTower(0, towerPlacementSpot.transform.position);

    }
}