using System.Collections.Generic;
using UnityEngine;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    protected float maxHealth = 100f;
    protected float moveSpeed = 3f;

    [Header("Rewards")]
    protected int currencyOnDeath = 10;
    protected int damageToBase = 1;

    // --- Path Stats ---
    private List<int> pathToFollow; //the overall path
    private int pathProgress; //which index of the list we are on
    private int pathIndex; //index of the spline to follow
    private float splineCompletion; //progress 0->100% of the spline we are on


    // --- Runtime State ---
    protected float currentHealth;
    protected float pathProgress;
    protected bool isDead = false;

    // --- Animation ---
    protected SpriteAnimationSystem animationSystem;

    private const int ANIM_WALK = 0;
    private const int ANIM_DAMAGE = 1;
 
    protected float currentHealth;
    public float targetHealth { get; private set; } //Seperate health stat used by the towers to know if this creep will die or not
    protected bool isDead = false;

    // Called by MasterController.SpawnCreep(). It creates the objects needed for a creep object at runtime
    public static GameObject CreateNewCreep(CreepData creationData, List<int> pathIndexes)
    {
        //Create our new creep
        GameObject newCreepObject = new GameObject(creationData.name, new System.Type[] { typeof(Creep), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpriteAnimationSystem)});

        newCreepObject.layer = LayerMask.NameToLayer("Creeps");

        //Creep Setup
        Creep creepScriptReference = newCreepObject.GetComponent<Creep>();
        creepScriptReference.SetValves(creationData, pathIndexes);

        SpriteRenderer renderer = newCreepObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newCreepObject.GetComponent<CircleCollider2D>();

        // Initialize animation system exactly like Tower does
        creepScriptReference.animationSystem = newCreepObject.GetComponent<SpriteAnimationSystem>();
        creepScriptReference.animationSystem.InitializeAnimationSystem(creationData.animationData, renderer);

        renderer.sprite = creationData.animationData.animations[creationData.animationData.idleAnimation].animationSprites[0];
        collider.radius = renderer.bounds.extents.x / newCreepObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        return newCreepObject;
    }


    public static GameObject CreateNewBossCreep(CreepData creationData)
    {
        GameObject newBossObject = new GameObject(creationData.name, new System.Type[]
        {
            typeof(BossCreep),
            typeof(SpriteRenderer),
            typeof(CircleCollider2D),
            typeof(SpriteAnimationSystem)
        });

        newBossObject.layer = LayerMask.NameToLayer("Creeps");

        BossCreep bossReference = newBossObject.GetComponent<BossCreep>();
        bossReference.SetValves(creationData);

        SpriteRenderer renderer = newBossObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newBossObject.GetComponent<CircleCollider2D>();

        bossReference.animationSystem = newBossObject.GetComponent<SpriteAnimationSystem>();
        bossReference.animationSystem.InitializeAnimationSystem(creationData.animationData, renderer);

        renderer.sprite = creationData.animationData.animations[creationData.animationData.idleAnimation].animationSprites[0];
        collider.radius = renderer.bounds.extents.x / newBossObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        return newBossObject;
    }
    //Assigns all the values we need for the creep to fully function
    public void SetValves(CreepData creepData, List<int> pathIndexes)
    {
        maxHealth = creepData.maxHealth;
        currentHealth = maxHealth;
        targetHealth = maxHealth;
        moveSpeed = creepData.moveSpeed;
        currencyOnDeath = creepData.currencyOnDeath;
        damageToBase = creepData.damageToBase;
        pathToFollow = pathIndexes;
        splineCompletion = 0f;
        pathProgress = 0;
        pathIndex = pathToFollow[pathProgress];

        transform.position = PathController.Instance.StartPosition;
    }

    private void Update()
    {
        if (isDead) return;
        FollowPath();
    }

    
    // Path Following
    private void FollowPath()
    {
        //progress along the current spline
        splineCompletion += (moveSpeed / PathController.Instance.PathLengths[pathIndex]) * Time.deltaTime;

        if (splineCompletion >= 1f) //if we have completed the splibne
        {
            pathProgress++; //increment to the next index of our path, and if its above the length of our list, we finished the path
            if (pathProgress > pathToFollow.Count - 1)
            {
                splineCompletion = 1f;
                ReachedEnd();
                return;
            }
            //if not find the next path index we want to follow, and set our spline progress to 0
            pathIndex = pathToFollow[pathProgress];
            splineCompletion = 0;
        }

        //move our creep to the world position of the spline we are on based on its completion
        transform.position = PathController.Instance.GetPosition(pathIndex, splineCompletion);
        transform.right = PathController.Instance.GetTangent(pathIndex, splineCompletion);
    }

    
    // Called by Towers 
    //Check how far along the creep is, the pathprogress is added so creeps further along are targeted first
    public float GetProgress() => ((pathProgress/ pathToFollow.Count - 1) + splineCompletion);

    //Decrease the target health value the towers use to know if the creep should be dead or not
    public void DecreaseTargetHealth(float amount)
    {
        targetHealth -= amount;
    }

    //damage the creep if its alive
    public void DamageCreep(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        animationSystem.PlayAnimation(ANIM_DAMAGE);

        if (currentHealth <= 0f)
            Die();
    }

    
    // Outcome handlers — report directly to MasterController
    
    private void Die()
    {
        isDead = true;
        MasterController.Instance.OnCreepKilled(currencyOnDeath);
        Destroy(gameObject);
    }

    private void ReachedEnd()
    {
        isDead = true;
        MasterController.Instance.OnCreepReachedEnd(damageToBase);
        Destroy(gameObject);
    }


    public class BossCreep : Creep
    {
        public void SetHealth(float health) { maxHealth = health; currentHealth = health; }
        public void SetSpeed(float speed) => moveSpeed = speed;
        public void SetCurrencyReward(int amount) => currencyOnDeath = amount;
        public void SetDamageToBase(int damage) => damageToBase = damage;
    }
}