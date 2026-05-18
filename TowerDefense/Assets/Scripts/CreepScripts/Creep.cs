using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    protected float maxHealth = 100f;
    protected float moveSpeed = 3f;

    [Header("Rewards")]
    protected int currencyOnDeath = 10;
    protected int damageToBase = 1;

    [Header("Slow")]
    public bool isSlow = false;
    public float slowRadius = 3f;
    [Range(0f, 1f)] public float slowMultiplier = 0.5f;

    private AudioManager audioManager;


    // --- Path Stats ---
    protected List<int> pathToFollow; //the overall path
    public int pathIndex; //index of the spline to follow
    public float splineCompletion; //progress 0->100% of the spline we are on
    protected Coroutine rewindCoroutine;

    // --- Slow ---
    private List<Tower> slowedTowers = new List<Tower>();

    // --- Runtime State ---
    protected float currentHealth;
    public int pathProgress;
    protected bool isDead = false;

    // --- Animation ---
    protected SpriteAnimationSystem animationSystem;

    private const int ANIM_WALK = 0;
    private const int ANIM_DAMAGE = 1;

    public float targetHealth { get; private set; } //Seperate health stat used by the towers to know if this creep will die or not

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


    public static GameObject CreateNewBossCreep(CreepData creationData, List<int> pathIndexes)
    {
        GameObject newBossObject = new GameObject(creationData.name);
        newBossObject.AddComponent<BossCreep>();
        newBossObject.AddComponent<SpriteRenderer>();
        newBossObject.AddComponent<CircleCollider2D>();
        newBossObject.AddComponent<SpriteAnimationSystem>();

        newBossObject.layer = LayerMask.NameToLayer("Creeps");

        BossCreep bossReference = newBossObject.GetComponent<BossCreep>();
        bossReference.SetValves(creationData, pathIndexes);

        SpriteRenderer renderer = newBossObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newBossObject.GetComponent<CircleCollider2D>();

        bossReference.animationSystem = newBossObject.GetComponent<SpriteAnimationSystem>();
        bossReference.animationSystem.InitializeAnimationSystem(creationData.animationData, renderer);

        renderer.sprite = creationData.animationData.animations[creationData.animationData.idleAnimation].animationSprites[0];
        collider.radius = renderer.bounds.extents.x / newBossObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        return newBossObject;
    }

    protected void CheckSlowRadius()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slowRadius);
        List<Tower> towersInRange = new List<Tower>();

        foreach (Collider2D hit in hits)
        {
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null && tower.slowable)
            {
                float distance = Vector2.Distance(tower.transform.position, transform.position);
                if (distance <= tower.towerRange)
                {
                    towersInRange.Add(tower);
                }
            }
        }

        foreach (Tower tower in towersInRange)
        {
            if (!slowedTowers.Contains(tower))
            {
                tower.ApplySlow(slowMultiplier);
                slowedTowers.Add(tower);
            }
        }

        for (int i = slowedTowers.Count - 1; i >= 0; i--)
        {
            if (!towersInRange.Contains(slowedTowers[i]))
            {
                slowedTowers[i].RemoveSlow(slowMultiplier);
                slowedTowers.RemoveAt(i);
            }
        }
    }

    private void UnslowAll()
    {
        foreach (Tower tower in slowedTowers)
        {
            if (tower != null)
                tower.RemoveSlow(slowMultiplier);
        }
        slowedTowers.Clear();
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
        isSlow = creepData.isSlow;
        slowRadius = creepData.slowRadius;
        slowMultiplier = creepData.slowMultiplier;
        audioManager = AudioManager.Instance;
        if(MasterController.Instance)
        MasterController.Instance.OnRewindInitiated += OnPanicButtonPressed;

        transform.position = PathController.Instance.StartPosition;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if(rewindCoroutine == null)
        {
            FollowPath();
        }
        MoveCreep();
        if (isSlow) CheckSlowRadius();
    }


    // Path Following
    protected void FollowPath()
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
    }

    protected void MoveCreep()
    {
        //move our creep to the world position of the spline we are on based on its completion
        transform.position = Vector3.Lerp(transform.position, PathController.Instance.GetPosition(pathIndex, splineCompletion), Time.deltaTime * 5);
        transform.right = Vector3.Lerp(transform.right, PathController.Instance.GetTangent(pathIndex, splineCompletion), Time.deltaTime * 5);
    }

    // Panic Button behavior
    private void OnPanicButtonPressed()
    {
        if(this)
        rewindCoroutine = StartCoroutine(PanicButtonActions(MasterController.Instance.rewindTime));
    }

    private IEnumerator PanicButtonActions(float rewindTime)
    {
        float unitsToRewind = rewindTime * moveSpeed;
        float rewindSpeed = unitsToRewind / MasterController.Instance.rewindActionTime; //rewind will take x seconds

        while (unitsToRewind > 0)
        {
            unitsToRewind -= rewindSpeed * Time.deltaTime;
            splineCompletion -= rewindSpeed / PathController.Instance.PathLengths[pathIndex] * Time.deltaTime;
            if(splineCompletion <= 0)
            {
                float overshootDistanceOnNextSpline = Mathf.Abs(splineCompletion) * PathController.Instance.PathLengths[pathIndex];
                if(pathProgress - 1 >= 0)
                {
                    pathProgress = Mathf.Max(0, pathProgress - 1);
                    pathIndex = pathToFollow[pathProgress];

                    splineCompletion = PathController.Instance.DistanceToT(pathIndex, PathController.Instance.PathLengths[pathIndex] - overshootDistanceOnNextSpline);
                }
                else
                {
                    pathProgress = 0;
                    splineCompletion = 0;
                }

            }
            splineCompletion = Mathf.Clamp01(splineCompletion);
            yield return null;

        }
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(PanicButtonEffects());
        rewindCoroutine = null;
    }

    private IEnumerator PanicButtonEffects()
    {
        switch (GameManager.Instance.panicButtonType)
        {
            case PanicButtonBehavior.Slowing:
                moveSpeed *= MasterController.Instance.slowPercent;
                yield return new WaitForSeconds(MasterController.Instance.slowTime);
                moveSpeed /= MasterController.Instance.slowPercent;
                break;
            case PanicButtonBehavior.Damaging:
                DecreaseTargetHealth(MasterController.Instance.damageDealt);
                DamageCreep(MasterController.Instance.damageDealt);
                break;
            default:
                yield return null;
                break;
        }
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
        animationSystem.PlayAnimation(ANIM_WALK);

        if (currentHealth <= 0f){
            Die();
        }
        else
        {
            audioManager.PlaySFX(audioManager.basicCreepHitSFX); // Will need to change dynamically in the future, likely just based on index
        }
    }

    
    // Outcome handlers — report directly to MasterController
    
    private void Die()
    {
        isDead = true;
        if (isSlow) UnslowAll();
        if (MasterController.Instance)
        {
            MasterController.Instance.OnCreepKilled(currencyOnDeath);
            MasterController.Instance.OnRewindInitiated -= OnPanicButtonPressed;
        }
        audioManager.PlaySFX(audioManager.basicCreepDeathSFX); // Will need to change dynamically in the future, likely just based on index
        Destroy(gameObject);
    }

    private void ReachedEnd()
    {
        isDead = true;
        if (isSlow) UnslowAll();
        MasterController.Instance.OnCreepReachedEnd(damageToBase);
        Destroy(gameObject);
    }
}

public class BossCreep : Creep
{
    private CreepData summonCreepData;
    private int summonCount = 1;
    private float summonInterval = 3f;
    private bool isSummoner;
    private Vector3 lastPosition;
    private float summonTimer = 0f;

    public new void SetValves(CreepData creepData, List<int> pathIndexes)
    {
        base.SetValves(creepData, pathIndexes);
        isSummoner = creepData.isSummoner;
        summonCreepData = creepData.summonCreepData;
        summonCount = creepData.summonCount;
        summonInterval = creepData.summonInterval;
    }

    protected override void Update()
    {
        if (isDead) return;

        lastPosition = transform.position;

        if (rewindCoroutine == null)
            FollowPath();

        MoveCreep();
        if (isSlow) CheckSlowRadius();

        if (!isSummoner || summonCreepData == null) return;
        summonTimer += Time.deltaTime;
        if (summonTimer > summonInterval)
        if (summonTimer >= summonInterval)
        {
            summonTimer = 0f;
            StartCoroutine(SpawnSummons());
        }
    }

    private IEnumerator SpawnSummons()
    {
        for (int i = 0; i < summonCount; i++)
        {
            GameObject summon = Creep.CreateNewCreep(summonCreepData, pathToFollow);
            summon.transform.parent = MasterController.Instance.CreepParent;

            Creep summonCreep = summon.GetComponent<Creep>();
            summonCreep.splineCompletion = splineCompletion;
            summonCreep.pathProgress = pathProgress;
            summonCreep.pathIndex = pathIndex;
            summon.transform.position = lastPosition;

            MasterController.Instance.IncrementEnemies();
            yield return new WaitForSeconds(0.2f);
        }
    }
}