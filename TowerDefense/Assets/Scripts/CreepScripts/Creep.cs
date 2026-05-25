using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 1;
    [SerializeField] protected float moveSpeed = 1;

    [Header("Rewards")]
    [SerializeField] protected int currencyOnDeath = 0;
    [SerializeField] protected int damageToBase = 1;

    [Header("Slow")]
    [SerializeField] protected bool isSlow = false;
    [SerializeField] protected float slowRadius = 3f;
    [SerializeField, Range(0f, 1f)] protected float slowMultiplier = 0.5f;

    private AudioManager audioManager;


    // --- Path Stats ---
    protected List<int> pathToFollow; //the overall path
    [HideInInspector] public int pathIndex; //index of the spline to follow
    protected float splineCompletion; //progress 0->100% of the spline we are on
    protected Coroutine rewindCoroutine;

    // --- Slow ---
    private List<Tower> slowedTowers = new List<Tower>();

    // --- Runtime State ---
    protected float currentHealth;
    [HideInInspector] public int pathProgress;
    protected bool isDead = false;

    [Header("Animation")]
    [SerializeField] private SpriteAnimationData animationData;
    protected SpriteAnimationSystem animationSystem;

    private const int ANIM_WALK = 0;
    private const int ANIM_DAMAGE = 1;
    private const int ANIM_DEATH = 2;

    public float targetHealth { get; private set; } //Seperate health stat used by the towers to know if this creep will die or not

    private void OnValidate()
    {
        if(animationData != null)
        {
            GetComponent<SpriteRenderer>().sprite = animationData.animations[animationData.idleAnimation].animationSprites[0];
        }
    }

    //Assigns all the values we need for the creep to fully function
    public virtual void SetValves(List<int> pathIndexes, float startProgress = 0)
    {
        gameObject.layer = LayerMask.NameToLayer("Creeps");

        animationSystem = GetComponent<SpriteAnimationSystem>();
        animationSystem.InitializeAnimationSystem(animationData, GetComponent<SpriteRenderer>());

        currentHealth = maxHealth;
        targetHealth = maxHealth;
        pathToFollow = pathIndexes;
        splineCompletion = startProgress;
        pathProgress = 0;
        pathIndex = pathToFollow[pathProgress];
        audioManager = AudioManager.Instance;
        if (MasterController.Instance)
            MasterController.Instance.OnRewindInitiated += OnPanicButtonPressed;

        transform.position = PathController.Instance.StartPosition;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if (rewindCoroutine == null)
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
        if (this)
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
            if (splineCompletion <= 0)
            {
                float overshootDistanceOnNextSpline = Mathf.Abs(splineCompletion) * PathController.Instance.PathLengths[pathIndex];
                if (pathProgress - 1 >= 0)
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
    public float GetProgress() => ((pathProgress / pathToFollow.Count - 1) + splineCompletion);

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
        {
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
        AudioClip[] sounds =
        {
            audioManager.basicCreepDeathSFX,
            audioManager.basicCreepDeathSFX2,
            audioManager.basicCreepDeathSFX3,
            audioManager.basicCreepDeathSFX4,
            audioManager.basicCreepDeathSFX5,
        };
        audioManager.PlaySFX(sounds[Random.Range(0, sounds.Length)]);

        animationSystem.PlayAnimation(ANIM_DEATH);
        animationSystem.enabled = false;
        Destroy(gameObject, 0.2f);
    }

    private void ReachedEnd()
    {
        isDead = true;
        if (isSlow) UnslowAll();
        if(MasterController.Instance)
        MasterController.Instance.OnCreepReachedEnd(damageToBase);
        Destroy(gameObject);
    }

    //Slow 
    protected void CheckSlowRadius()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slowRadius);
        List<Tower> towersInRange = new List<Tower>();

        foreach (Collider2D hit in hits)
        {
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null && tower.Slowable)
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
}