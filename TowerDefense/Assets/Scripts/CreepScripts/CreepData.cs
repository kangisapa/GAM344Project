using UnityEngine;

[CreateAssetMenu(fileName = "NewCreepData", menuName = "TowerDefense/CreepData")]
public class CreepData : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;

    [Header("Rewards")]
    public int currencyOnDeath = 10;
    public int damageToBase = 1;

    [Header("Visuals")]
    public Sprite sprite;
}