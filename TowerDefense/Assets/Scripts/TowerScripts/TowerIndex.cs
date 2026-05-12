using UnityEngine;

//Petition to rename this class to something more descriptive? - JGN
public class IndexSquare : MonoBehaviour
{
    private TowerPlacementSpot towerPlacementSpot;
    private int index;
    private SpriteRenderer sr;

    public void Setup(int spotIndex)
    {
        index = spotIndex;
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetTowerPlacement(TowerPlacementSpot parentTowerPlacementSpot)
    {
        towerPlacementSpot = parentTowerPlacementSpot;
    }

    public void LateUpdate()
    {
        if(sr != null)
        {
            sr.color = MasterController.Instance.CheckCurrency(index) ? Color.white : Color.grey;
        }
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