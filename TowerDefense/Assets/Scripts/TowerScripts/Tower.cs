using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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
    #region Slow Functions
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
    #endregion
    // ---------- Tower Damaging ----------
    private CircleCollider2D rangeCollider;
    private Vector2[] firingAngles;
    public float towerRange => rangeCollider.radius;
    private float damagePerShot;
    private float shotsPerSecond;
    private float projectileTargetTime;
    private float firingDelay;
    private int creepsToTarget;
    public bool slowable { get; private set; }

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
        GameObject newTowerObject = new GameObject(creationData.name, typeof(Tower), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpriteAnimationSystem));
        GameObject newTowerRangeObject = new GameObject("TowerRange",  typeof(CircleCollider2D));

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
        firingAngles = creationData.firingAngles;
        damagePerShot = creationData.damagePerShot;
        shotsPerSecond = creationData.shotsPerSecond;
        projectileTargetTime = creationData.projectileTargetTime;
        firingDelay = creationData.firingDelay;
        creepsToTarget = creationData.creepsToTarget;
        slowable = creationData.slowable;
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

        while (towerEnabled)
        {
            overlaps.Clear();
            rangeCollider.Overlap(contactFilter, overlaps);

            //if nothing overlapping, dont continue, wait a frame then start again
            if( overlaps.Count  == 0)
            {
                yield return null;
                continue;
            }

            List<Creep> targetableCreeps = ValidateTargetableCreeps(overlaps);

            //If nothing is actually targetable, don't continue, wait a frame, then start again
            if(targetableCreeps.Count == 0)
            {
                yield return null;
                continue;
            }

            /*
             In both instances below, we AssignDamage before we actually wait and fire so other towers know that this creep is bout to die anyways and not bother targeting and creating issues
             */

            if (targetableCreeps.Count <= creepsToTarget)
            {
                //If the number of creeps we can target is <= to the max the tower can target, don't bother checking the furthest along and just smite them all
                animationSystem.PlayAnimation(1);
                AssignDamage(targetableCreeps);
                yield return firingDelayWait;
                DealDamage(targetableCreeps);
                audioManager.PlaySFX(audioManager.basicAttackSFX);
                yield return new WaitForSeconds((1 / shotsPerSecond) - firingDelay);
            }
            else
            {
                List<Creep> furthestCreeps = new();
                
                /*
                 If we have more creeps in range than the tower can target at once, then we want to search through all the targetable ones and pick out the furthest
                Once we do that, we can target the rest.
                 */

                for (int i = 0; i < creepsToTarget; i++)
                {
                    float furthestProgress = -1;
                    int furthestIndex = 0;
                    for(int j = 0;  j < targetableCreeps.Count; j++)
                    {
                        float progress = targetableCreeps[j].GetComponent<Creep>().GetProgress();
                        if(progress > furthestProgress)
                        {
                            furthestIndex = j;
                            furthestProgress = progress;
                        }
                    }
                    furthestCreeps.Add(targetableCreeps[furthestIndex]);
                    targetableCreeps.RemoveAt(furthestIndex);
                }
                if(furthestCreeps.Count > 0)
                {
                    animationSystem.PlayAnimation(1);
                    AssignDamage(furthestCreeps);
                    yield return firingDelayWait;
                    DealDamage(furthestCreeps);
                    audioManager.PlaySFX(audioManager.basicAttackSFX);
                    yield return new WaitForSeconds((1 / shotsPerSecond) - firingDelay);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }

    private List<Creep> ValidateTargetableCreeps(List<Collider2D> overlaps)
    {
        List<Creep> targetable = new();
        foreach (Collider2D creep in overlaps)
        {
            Creep creepComponent = creep.GetComponent<Creep>();
            float distance = Vector2.Distance(transform.position, creep.transform.position);
            if (creepComponent != null && 
                creepComponent.targetHealth > 0 && 
                distance <= rangeCollider.radius &&
                CustomMathLibrary.AngleWithinRanges(firingAngles, CustomMathLibrary.AngleBetweenVector2Positions(transform.position, creep.transform.position)))
            {
                targetable.Add(creepComponent);
            }
        }
        return targetable;
    }

    /// <summary>
    /// specifically runs through the creeps to damage and sets their furture health to what it would be
    /// after the shot so other towers know whether its worht to fire or not
    /// </summary>
    /// <param name="toDamage">target creeps list</param>
    private void AssignDamage(List<Creep> toDamage)
    {
        foreach (Creep creep in toDamage)
        {
            creep.DecreaseTargetHealth(damagePerShot);
        }
    }

    /// <summary>
    /// Specifically runs through the creeps to damage and actually runs the logic to create the "projectiles" that will damage/smite the creeps
    /// once they reach the target
    /// </summary>
    /// <param name="toDamage">target creeps list</param>
    private void DealDamage(List<Creep> toDamage)
    {
        foreach(Creep creep in toDamage)
        {
            Creep c = creep;
            ProjectileManager.Instance.FireProjectile(transform.position, c.transform, projectileTargetTime, projectileSprite, () => DamageCreep(c));
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
