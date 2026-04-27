using UnityEngine;
using System;

public class Creep : MonoBehaviour
{
    // --- Configuration ---
    [SerializeField] private CreepData data;

    // --- Runtime State ---
    private float currentHealth;
    private float pathProgress;
    private bool isDead = false;

   
    // Called by MasterController.SpawnCreep()
    
    public void Initialize(CreepData creepData)
    {
        data = creepData;
        currentHealth = data.maxHealth;
        pathProgress = 0f;

        transform.position = PathController.Instance.StartPosition;
        GetComponent<SpriteRenderer>().sprite = data.sprite;
    }

    private void Update()
    {
        if (isDead) return;
        FollowPath();
    }

    
    // Path Following
    
    private void FollowPath()
    {
        pathProgress += (data.moveSpeed / PathController.Instance.PathLength) * Time.deltaTime;

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
        MasterController.Instance.OnCreepKilled(data.currencyOnDeath);
        Destroy(gameObject);
    }

    private void ReachedEnd()
    {
        isDead = true;
        MasterController.Instance.OnCreepReachedEnd(data.damageToBase);
        Destroy(gameObject);
    }
}