using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // ---------- Tower Visuals ----------
    public Sprite projectileSprite;

    public SpriteAnimationData animationData;

    // ---------- Tower Settings ----------
    [Min(0)] public float targetRadius;
    [InspectorName("Angle Set")]
    public Vector2[] firingAngles = { new(0, 360) };


    [Min(0)] public float damagePerShot;
    [Min(0)] public float shotsPerSecond;
    [Tooltip("How long will it take the projectile to reach the target in seconds"), Min(0)] 
    public float projectileTargetTime;
    [Tooltip("just used to sync up the animation to firing"), Min(0)]
    public float firingDelay;
    [Min(1), Tooltip("Put an obscenly high value if you want the tower to target everything in range")]
    public int creepsToTarget = 1;
    public bool slowable = true;

    public int cost;

    [Tooltip("Not referenced anywhere in code atm, just use to make not or other things")]
    public string notes;
}
