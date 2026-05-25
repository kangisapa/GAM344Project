using UnityEngine;

[CreateAssetMenu(fileName = "NewCreepData", menuName = "TowerDefense/CreepData")]
public class CreepData : ScriptableObject
{
    // --- Visuals ---
    public SpriteAnimationData animationData; //done

    [Header("Stats")]
    public float maxHealth = 100f;//done
    public float moveSpeed = 3f;//done

    [Header("Rewards")]
    public int currencyOnDeath = 10;//done
    public int damageToBase = 1;//done

    [Header("Slow")]
    public bool isSlow = false; //done
    public float slowRadius = 3f; //done
    [Range(0f, 1f)] public float slowMultiplier = 0.5f; //done

    [Header("Visuals")]
    public Sprite sprite; //wtf is this even needed for

    [Header("Summoning")]
    public bool isSummoner = false; //done
    public CreepData summonCreepData; //done
    public int summonCount = 1; //done
    public float summonInterval = 3f; //done
}