using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class Tower : MonoBehaviour
{
    public static LayerMask creepLayer;

    // ---------- Tower Visuals ----------
    private Sprite projectileSprite;
    private SpriteAnimationSystem animationSystem;

    // ---------- Tower Slow ----------

    private float baseShotsPerSecond;
    private int slowCount = 0;
    private float strongestSlow = 1f;
    public void ApplySlow(float multiplier)
    {
        slowCount++;
        if (multiplier < strongestSlow)
        {
            strongestSlow = multiplier;
            shotsPerSecond = baseShotsPerSecond * strongestSlow;
        }
    }

    public void RemoveSlow(float multiplier)
    {
        slowCount = Mathf.Max(0, slowCount - 1);
        if (slowCount == 0)
        {
            strongestSlow = 1f;
            shotsPerSecond = baseShotsPerSecond;
        }
    }

    // ---------- Tower Damaging ----------
    private CircleCollider2D rangeCollider;
    private float targetRadius;
    private float damagePerShot;
    private float shotsPerSecond;
    private float projectileTargetTime;
    private float firingDelay;

    private int cost;
    // ---------- Tower Enabling ----------
    private bool towerEnabled = false;

    public void SetTowerEnabled(bool enabled) => towerEnabled = enabled;

    // Audio
    private AudioManager audioManager;

    // ================================================================

    public static GameObject CreateNewTower(TowerData creationData)
    {
        //Create our new tower and its associated range object
        GameObject newTowerObject = new GameObject(creationData.name, new System.Type[] {typeof(Tower), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpriteAnimationSystem) });
        GameObject newTowerRangeObject = new GameObject("TowerRange", new System.Type[] {typeof(CircleCollider2D)} );

        SpriteRenderer renderer = newTowerObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newTowerObject.GetComponent<CircleCollider2D>();

        //Tower Setup (script values => tower visuals => range setup 
        Tower towerScriptReference = newTowerObject.GetComponent<Tower>();
        towerScriptReference.SetValues(creationData, newTowerRangeObject.GetComponent<CircleCollider2D>());
        towerScriptReference.animationSystem = newTowerObject.GetComponent<SpriteAnimationSystem>();
        towerScriptReference.animationSystem.InitializeAnimationSystem(creationData.animationData, renderer);

        renderer.sprite = creationData.animationData.animations[creationData.animationData.idleAnimation].animationSprites[0];
        collider.radius = renderer.bounds.extents.x / newTowerObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;
        collider.isTrigger = true;

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
        firingDelay = creationData.firingDelay;
        cost = creationData.cost;
        this.rangeCollider = rangeCollider;
        baseShotsPerSecond = shotsPerSecond;

        audioManager = AudioManager.Instance;

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
        //update is (delay between shots - firing delay) since the 2 add up so if we want 1/second and we delay firing by 0.4 seconds
        //added together we would get a 1.4 second delay instead of 1, so this gives us 1 - 0.4 so it would be the 0.4 for animation + 0.6 between shots adding up to 1
        WaitForSeconds shotTargetDelay = new WaitForSeconds(projectileTargetTime);
        WaitForSeconds firingDelayWait = new WaitForSeconds(firingDelay);
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = LayerMask.GetMask("Creeps");
        List<Collider2D> overlaps = new();

        while(towerEnabled)
        {
            overlaps.Clear();
            rangeCollider.Overlap(contactFilter, overlaps);
            Collider2D furthestCreep = null;
            float furthestProgress = -1;

            foreach(Collider2D creep in overlaps)
            {
                Creep creepComponent = creep.GetComponent<Creep>();
                float distance = Vector2.Distance(transform.position, creep.transform.position);
                if (creepComponent == null || creepComponent.targetHealth <= 0 || distance > rangeCollider.radius)
                {
                    continue;
                }
                float progress = creep.GetComponent<Creep>().GetProgress();
                if(progress > furthestProgress)
                {
                    furthestProgress = progress;
                    furthestCreep = creep;
                }
            }


            //minor changes below, if the tower shoots, we want to wait the delay before it can shoot again, but once it can check again, check as fast as possible to reduce weird delays
            if(furthestCreep != null)
            {
                Creep targetCreep = furthestCreep.GetComponent<Creep>();
                animationSystem.PlayAnimation(1);
                targetCreep.DecreaseTargetHealth(damagePerShot);
                yield return firingDelayWait;
                ProjectileManager.Instance.FireProjectile(transform.position, targetCreep.transform, projectileTargetTime, projectileSprite, () => DamageCreep(targetCreep));
                audioManager.PlaySFX(audioManager.basicAttackSFX); // Will need to change dynamically in the future, likely just based on index
                yield return new WaitForSeconds((1 / shotsPerSecond) - firingDelay);
            }
            else
            {
                yield return null;
            }
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
