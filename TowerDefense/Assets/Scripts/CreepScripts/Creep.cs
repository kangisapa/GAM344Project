using UnityEngine;
using System;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    protected float maxHealth = 100f;
    protected float moveSpeed = 3f;

    [Header("Rewards")]
    protected int currencyOnDeath = 10;
    protected int damageToBase = 1;

    // --- Runtime State ---
    protected float currentHealth;
    protected float pathProgress;
    protected bool isDead = false;

    // --- Animation ---
    protected SpriteAnimationSystem animationSystem;

    private const int ANIM_WALK = 0;
    private const int ANIM_DAMAGE = 1;

    // Called by MasterController.SpawnCreep()

    public static GameObject CreateNewCreep(CreepData creationData)
    {
        //Create our new creep
        GameObject newCreepObject = new GameObject(creationData.name, new System.Type[] { typeof(Creep), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpriteAnimationSystem)});

        newCreepObject.layer = LayerMask.NameToLayer("Creeps");

        //Creep Setup
        Creep creepScriptReference = newCreepObject.GetComponent<Creep>();
        creepScriptReference.SetValves(creationData);

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
    public void SetValves(CreepData creepData)
    {
        maxHealth = creepData.maxHealth;
        moveSpeed = creepData.moveSpeed;
        currencyOnDeath = creepData.currencyOnDeath;
        damageToBase = creepData.damageToBase;
        currentHealth = maxHealth;
        pathProgress = 0f;

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
        pathProgress += (moveSpeed / PathController.Instance.PathLength) * Time.deltaTime;

        if (pathProgress >= 1f)
        {
            pathProgress = 1f;
            ReachedEnd();
            return;
        }

        transform.position = PathController.Instance.GetPosition(pathProgress);
        transform.right = PathController.Instance.GetTangent(pathProgress);
    }

    
    // Called by Towers
    
    public float GetProgress() => pathProgress;

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