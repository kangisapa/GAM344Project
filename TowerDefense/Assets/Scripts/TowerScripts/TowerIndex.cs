using UnityEngine;

public class IndexSquare : MonoBehaviour
{
    private TowerPlacementSpot towerPlacementSpot;
    private int index;

    public void Setup(int spotIndex) => index = spotIndex;

    public void SetTowerPlacement(TowerPlacementSpot parentTowerPlacementSpot)
    {
        towerPlacementSpot = parentTowerPlacementSpot;
    }

    private void OnMouseDown()
    {
        if (MasterController.Instance.CheckCurrency(index) == false)
            return;


        if (towerPlacementSpot == null || MasterController.Instance == null) return;
        if (towerPlacementSpot.HasTower) return; 

        towerPlacementSpot.CloseMenu();

        MasterController.Instance.SpawnTower(index, towerPlacementSpot.transform.position);

        towerPlacementSpot.MarkAsOccupied();
    }
}