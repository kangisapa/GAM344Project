using System.Collections.Generic;
using UnityEngine;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    private float maxHealth = 100f;
    private float moveSpeed = 3f;

    [Header("Rewards")]
    private int currencyOnDeath = 10;
    private int damageToBase = 1;

    // --- Path Stats ---
    private List<int> pathToFollow; //the overall path
    private int pathProgress; //which index of the list we are on
    private int pathIndex; //index of the spline to follow
    private float splineCompletion; //progress 0->100% of the spline we are on


    // --- Runtime State ---
    private float currentHealth;
    public float targetHealth { get; private set; } //Seperate health stat used by the towers to know if this creep will die or not
    private bool isDead = false;

    // Called by MasterController.SpawnCreep(). It creates the objects needed for a creep object at runtime
    public static GameObject CreateNewCreep(CreepData creationData, List<int> pathIndexes)
    {
        //Create our new tower and its associated range object
        GameObject newCreepObject = new GameObject(creationData.name, new System.Type[] { typeof(Creep), typeof(SpriteRenderer), typeof(CircleCollider2D) });

        //Tower Setup (script values => tower visuals => range setup 
        Creep creepScriptReference = newCreepObject.GetComponent<Creep>();
        creepScriptReference.SetValves(creationData, pathIndexes);

        SpriteRenderer renderer = newCreepObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newCreepObject.GetComponent<CircleCollider2D>();

        renderer.sprite = creationData.sprite;
        collider.radius = renderer.bounds.extents.x / newCreepObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        return newCreepObject;
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
}