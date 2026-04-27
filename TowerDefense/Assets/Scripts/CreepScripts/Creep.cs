using UnityEngine;
using System;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [Header("Stats")]
    private float maxHealth = 100f;
    private float moveSpeed = 3f;

    [Header("Rewards")]
    private int currencyOnDeath = 10;
    private int damageToBase = 1;

    // --- Runtime State ---
    private float currentHealth;
    private float pathProgress;
    private bool isDead = false;


    // Called by MasterController.SpawnCreep()

    public static GameObject CreateNewCreep(CreepData creationData)
    {
        //Create our new tower and its associated range object
        GameObject newCreepObject = new GameObject(creationData.name, new System.Type[] { typeof(Creep), typeof(SpriteRenderer), typeof(CircleCollider2D) });

        //Tower Setup (script values => tower visuals => range setup 
        Creep creepScriptReference = newCreepObject.GetComponent<Creep>();
        creepScriptReference.SetValves(creationData);

        SpriteRenderer renderer = newCreepObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = newCreepObject.GetComponent<CircleCollider2D>();

        renderer.sprite = creationData.sprite;
        collider.radius = renderer.bounds.extents.x / newCreepObject.transform.lossyScale.x;
        collider.offset = Vector2.zero;

        return newCreepObject;
    }


    public void SetValves(CreepData creepData)
    {
        maxHealth = creepData.maxHealth;
        moveSpeed = creepData.moveSpeed;
        currencyOnDeath = creepData.currencyOnDeath;
        damageToBase = creepData.damageToBase;

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