using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class PathController : MonoBehaviour
{
    // ---------- Singleton ----------
    public static PathController Instance { get; private set; }

    // ---------- Spline ----------
    private SplineContainer spline;
    private float pathLength;

    // ---------- Public accessors ----------
    public SplineContainer Spline => spline;
    public float PathLength        => pathLength;
    public Vector3 StartPosition   => GetPosition(0f);
    public Vector3 EndPosition     => GetPosition(1f);

    // ================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        spline = GetComponent<SplineContainer>();
        pathLength = spline.CalculateLength();
    }

    public Vector3 GetPosition(float t)
    {
        return (Vector3)spline.EvaluatePosition(Mathf.Clamp01(t));
    }

    public Vector3 GetTangent(float t)
    {
        return ((Vector3)spline.EvaluateTangent(Mathf.Clamp01(t))).normalized;
    }

    public float DistanceToT(float distance)
    {
        return pathLength > 0f ? Mathf.Clamp01(distance / pathLength) : 0f;
    }
}