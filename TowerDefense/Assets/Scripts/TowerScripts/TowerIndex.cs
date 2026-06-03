using TMPro;
using UnityEngine;

//Petition to rename this class to something more descriptive? - JGN
public class IndexSquare : MonoBehaviour
{
    private TowerPlacementSpot towerPlacementSpot;
    private int index;
    private SpriteRenderer sr;
    private BoxCollider2D hitBox;

    private float targetScale = 1;
    private Vector3 targetLocalPosition = Vector3.zero;

    float targetAlpha;

    public void Setup(int spotIndex, Vector3 targetPos, bool visisble)
    {
        index = spotIndex;
        sr = GetComponent<SpriteRenderer>();
        targetAlpha = visisble ? 1 : 0;
        targetLocalPosition = targetPos;
    }

    public void SetTowerPlacement(TowerPlacementSpot parentTowerPlacementSpot)
    {
        towerPlacementSpot = parentTowerPlacementSpot;
    }

    public void LateUpdate()
    {
        if(sr != null)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.unscaledDeltaTime * 10);
            Color c = sr.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * 10);
            bool canPurchase = MasterController.Instance.CheckCurrency(index);
            //visual touches, if we can't purchase grey out with no response. If we can purchase, make it normally colored and scale as if its a button
            c = canPurchase ? new(1, 1, 1, c.a) : new(0.5f, 0.5f, 0.5f, c.a);
            sr.color = c;
            transform.localScale = canPurchase ? Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 6.5f) : Vector3.one;
            if(Time.timeScale < 1)
            {
                Physics2D.SyncTransforms();
            }
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