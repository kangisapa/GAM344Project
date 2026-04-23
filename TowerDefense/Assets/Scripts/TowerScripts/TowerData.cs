using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // ---------- Tower Visuals ----------
    public Sprite towerSprite;
    public Sprite projectileSprite;

    // ---------- Tower Settings ----------
    [Min(0)] public float targetRadius;

    [Min(0)] public float damagePerShot;
    [Min(0)] public float shotsPerSecond;
    [Tooltip("How long will it take the projectile to reach the target in seconds"), Min(0)] 
    public float projectileTargetTime;

    public int cost;
}
