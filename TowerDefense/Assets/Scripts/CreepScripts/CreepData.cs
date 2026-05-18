using UnityEngine;

[CreateAssetMenu(fileName = "NewCreepData", menuName = "TowerDefense/CreepData")]
public class CreepData : ScriptableObject
{
    // --- Visuals ---
    public SpriteAnimationData animationData;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;

    [Header("Rewards")]
    public int currencyOnDeath = 10;
    public int damageToBase = 1;

    [Header("Slow")]
    public bool isSlow = false;
    public float slowRadius = 3f;
    [Range(0f, 1f)] public float slowMultiplier = 0.5f;

    [Header("Visuals")]
    public Sprite sprite;

    [Header("Summoning")]
    public bool isSummoner = false;
    public CreepData summonCreepData;
    public int summonCount = 1;
    public float summonInterval = 3f;
}