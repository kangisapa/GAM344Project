using UnityEngine;

//Petition to rename this class to something more descriptive? - JGN
public class IndexSquare : MonoBehaviour
{
    private TowerPlacementSpot towerPlacementSpot;
    private int index;
    private SpriteRenderer sr;

    private float targetScale = 1;

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
            bool canPurchase = MasterController.Instance.CheckCurrency(index);
            //visual touches, if we can't purchase grey out with no response. If we can purchase, make it normally colored and scale as if its a button
            sr.color = canPurchase ? Color.white : Color.grey;
            transform.localScale = canPurchase ? Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * 6.5f) : Vector3.one;
        }
    }

    private void OnMouseEnter() => targetScale = 1.25f;

    private void OnMouseExit() => targetScale = 1f;

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