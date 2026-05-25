using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // ---------- Tower Visuals ----------
    public Sprite projectileSprite; //done

    public SpriteAnimationData animationData; //done

    // ---------- Tower Settings ----------
    [Min(0)] public float targetRadius;//done
    [InspectorName("Angle Set")]
    public Vector2[] firingAngles = { new(0, 360) }; //done


    [Min(0)] public float damagePerShot; //done
    [Min(0)] public float shotsPerSecond; //done
    [Tooltip("How long will it take the projectile to reach the target in seconds"), Min(0)] 
    public float projectileTargetTime; //done
    [Tooltip("just used to sync up the animation to firing"), Min(0)]
    public float firingDelay; //done
    [Min(1), Tooltip("Put an obscenly high value if you want the tower to target everything in range")]
    public int creepsToTarget = 1; //done
    public bool slowable = true;

    public int cost;
}
