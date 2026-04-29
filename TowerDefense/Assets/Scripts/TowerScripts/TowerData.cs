using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // ---------- Tower Visuals ----------
    public Sprite projectileSprite;

    public SpriteAnimationData animationData;

    // ---------- Tower Settings ----------
    [Min(0)] public float targetRadius;

    [Min(0)] public float damagePerShot;
    [Min(0)] public float shotsPerSecond;
    [Tooltip("How long will it take the projectile to reach the target in seconds"), Min(0)] 
    public float projectileTargetTime;
    [Tooltip("just used to sync up the animation to firing"), Min(0)]
    public float firingDelay;

    public int cost;
}
