using System.Collections.Generic;
using UnityEngine;

public struct ProjectileData
{
    public Transform transform;
    public Vector3 startPosition;
    public Transform target;
    public float uptime;
    public float timeToTarget;
    public System.Action onHit;
}

public class ProjectileManager : MonoBehaviour
{
    // ---------- Singleton ----------
    public static ProjectileManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // ---------- Object pooling ----------

    private Queue<GameObject> pool = new();
    private List<ProjectileData> activeProjectiles = new();

    // ================================================================

    //when we want to fire a projectile, we either get one from the pool, or create a new one in the pool
    public void FireProjectile(Vector3 start, Transform targetTransform, float travelTime, Sprite projectileSprite, System.Action onHitEvent)
    {
        GameObject obj = GetFromPool();
        obj.transform.position = start;
        obj.GetComponent<SpriteRenderer>().sprite = projectileSprite;
        obj.SetActive(true);

        //add to the list of active projectiles the new one
        activeProjectiles.Add(new ProjectileData
        {
            transform = obj.transform,
            startPosition = start,
            target = targetTransform,
            uptime = 0,
            timeToTarget = travelTime,
            onHit = onHitEvent
        });

    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        //go though each active projectile
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            //increase its uptime and progress
            ProjectileData projectile = activeProjectiles[i];
            projectile.uptime += deltaTime;
            float alpha = projectile.uptime / projectile.timeToTarget;

            //send the projectile back to the pool if its done
            if (alpha >= 1 || projectile.target == null)
            {
                projectile.onHit?.Invoke();
                ReturnToPool(projectile);
                activeProjectiles.RemoveAt(i);
            }
            //move the projectile along if its still active
            else
            {
                projectile.transform.position = Vector3.Lerp(projectile.startPosition, projectile.target.position, alpha);
                activeProjectiles[i] = projectile;
            }
        }
    }

    /// <summary>
    /// Instanciate a brand new projectile
    /// </summary>
    /// <returns>A reference to the newly created projectile</returns>
    public static GameObject CreateProjectile()
    {
        return new GameObject("Projectile", new System.Type[] {typeof(SpriteRenderer)} );
    }

    /// <summary>
    /// return a projectile back to the pool of available ones
    /// </summary>
    /// <param name="projectile">the data of the active projectile to return</param>
    private void ReturnToPool(ProjectileData projectile)
    {
        projectile.transform.gameObject.SetActive(false);
        pool.Enqueue(projectile.transform.gameObject);
    }

    /// <summary>
    /// Get a projectile to use
    /// </summary>
    /// <returns>Projectile GameObject</returns>
    private GameObject GetFromPool()
    {
        return pool.Count > 0 ? pool.Dequeue() : CreateProjectile();
    }
}
