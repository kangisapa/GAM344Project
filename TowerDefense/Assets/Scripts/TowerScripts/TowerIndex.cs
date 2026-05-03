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
        if (masterController.CheckCurrency(index) == false)
            return;


        if (towerPlacementSpot == null || masterController == null) return;
        if (towerPlacementSpot.HasTower) return; 

        towerPlacementSpot.CloseMenu();

        masterController.SpawnTower(index, towerPlacementSpot.transform.position);

        towerPlacementSpot.MarkAsOccupied();
    }
}