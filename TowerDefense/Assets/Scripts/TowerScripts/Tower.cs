using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class Tower : MonoBehaviour
{
    public static LayerMask creepLayer;

    // ---------- Tower Visuals ----------
    private Sprite projectileSprite;

    // ---------- Tower Damaging ----------
    private CircleCollider2D rangeCollider;
    private float targetRadius;
    private float damagePerShot;
    private float shotsPerSecond;
    private float projectileTargetTime;

    private int cost;
    // ---------- Tower Enabling ----------
    private bool towerEnabled = false;

    public void SetTowerEnabled(bool enabled) => towerEnabled = enabled;

    // ================================================================

    public static GameObject CreateNewTower(TowerData creationData)
    {
        //Create our new tower and its associated range object
        GameObject newTowerObject = new GameObject(creationData.name, new System.Type[] {typeof(Tower), typeof(SpriteRenderer), typeof(CircleCollider2D) });
        GameObject newTowerRangeObject = new GameObject("TowerRange", new System.Type[] {typeof(CircleCollider2D)} );

        //Tower Setup (script values => tower visuals => range setup 
        Tower towerScriptReference = newTowerObject.GetComponent<Tower>();
        towerScriptReference.SetValues(creationData, newTowerRangeObject.GetComponent<CircleCollider2D>());

        SpriteRenderer renderer = newTowerObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newTowerObject.GetComponent<CircleCollider2D>();

        renderer.sprite = creationData.towerSprite;
        collider.radius = renderer.bounds.extents.x / newTowerObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        //Tower Range Setup
        newTowerRangeObject.transform.parent = newTowerObject.transform;
        newTowerRangeObject.transform.localPosition = Vector3.zero;
        towerScriptReference.rangeCollider.radius = creationData.targetRadius;
        towerScriptReference.rangeCollider.offset = Vector2.zero;

        return newTowerObject;
    }

    private void SetValues(TowerData creationData, CircleCollider2D rangeCollider)
    {
        projectileSprite = creationData.projectileSprite;
        targetRadius = creationData.targetRadius;
        damagePerShot = creationData.damagePerShot;
        shotsPerSecond = creationData.shotsPerSecond;
        projectileTargetTime = creationData.projectileTargetTime;
        cost = creationData.cost;
        this.rangeCollider = rangeCollider;
    }

    public void PlaceTower(Vector3 position)
    {
        gameObject.transform.position = position;
        towerEnabled = true;
        StartCoroutine(UpdateLoop());
    }

    /*
     * Heres the basic rundown of how the update loops works:
     *  Every loop the tower will check, along the creep layer mask if any are overlapping its range. It will find the creep that is furthest along
     *  Through a yet to be determined .GetProgress() function or something similar the creep will have. 
     *  Once there is a furthest creep along the spline that is found, the tower will tell the projectile manager to send out a projectile
     *  It will also create an anonymous function to listen from then projectile once its finished traveling, at which point the creep should take damage
     */
    private IEnumerator UpdateLoop()
    {
        WaitForSeconds updateWait = new WaitForSeconds(1 / shotsPerSecond);
        WaitForSeconds shotTargetDelay = new WaitForSeconds(projectileTargetTime);
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = LayerMask.GetMask("Creeps");
        List<Collider2D> overlaps = new();

        while(towerEnabled)
        {
            rangeCollider.Overlap(contactFilter, overlaps);
            Collider2D furthestCreep = null;
            float furthestProgress = -1;

            foreach(Collider2D creep in overlaps)
            {
                Creep creepComponent = creep.GetComponent<Creep>();
                if(creepComponent == null)
                {
                    continue;
                }
                //UNCOMMENT ONCE CREEPS ARE IMPLEMENTED
                float progress = creep.GetComponent<Creep>().GetProgress();
                if(progress > furthestProgress)
                {
                    furthestProgress = progress;
                    furthestCreep = creep;
                }
            }

            if(furthestCreep != null)
            {
                //UNCOMMENT ONCE CREEPS ARE IMPLEMENTED
                Creep targetCreep = furthestCreep.GetComponent<Creep>();
                ProjectileManager.Instance.FireProjectile(transform.position, targetCreep.transform, projectileTargetTime, projectileSprite, () => DamageCreep(targetCreep));
            }
            yield return updateWait;
        }
    }

      private void DamageCreep(Creep targetCreep)
      {
         if (targetCreep != null)
         { 
             targetCreep.DamageCreep(damagePerShot); 
         }
      }

}
