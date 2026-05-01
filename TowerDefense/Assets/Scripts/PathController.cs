using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class PathController : MonoBehaviour
{
    // ---------- Singleton ----------
    public static PathController Instance { get; private set; }

    // ---------- Spline ----------
    private SplineContainer spline;
    private List<float> pathLengths;

    // ---------- Public accessors ----------
    public SplineContainer Spline  => spline;
    public List<float> PathLengths => pathLengths;
    /// <summary>
    /// World position at the beginning first spline
    /// </summary>
    public Vector3 StartPosition   => GetPosition(0f); 
    /// <summary>
    /// World position at the end of the last spline
    /// </summary>
    public Vector3 EndPosition     => GetPosition(pathLengths.Count - 1, 1f);

    // ================================================================


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        spline = GetComponent<SplineContainer>();
        FixZPostions();

        pathLengths = new List<float>();
        //Get all the path lengths for each spline
        for(int i = 0; i < spline.Splines.Count; i++)
        {
            pathLengths.Add(spline.CalculateLength(i));
        }

    }

    /// <summary>
    ///  Get the position at the current progress along the path we are on
    /// </summary>
    /// <param name="t">progress from 0-1 along that spline</param>
    /// <returns>Position in world space</returns>
    public Vector3 GetPosition(float t)
    {
        return GetPosition(0, t);
    }

    /// <summary>
    /// Get the position at the current progress along the path we are on
    /// </summary>
    /// <param name="splineIndex">index of the spline in the spline container</param>
    /// <param name="t">progress from 0-1 along that spline</param>
    /// <returns>Position in world space</returns>
    public Vector3 GetPosition(int splineIndex, float t)
    {
        return (Vector3)spline.EvaluatePosition(splineIndex, Mathf.Clamp01(t));
    }

    /// <summary>
    /// Get the tangent at the current progress along the path we are on
    /// </summary>
    /// <param name="t">progress from 0-1 along that spline</param>
    /// <returns>tangent in world space</returns>
    public Vector3 GetTangent(float t)
    {
        return GetTangent(0, t);
    }

    /// <summary>
    /// Get the tangent at the current progress along the path we are on
    /// </summary>
    /// <param name="splineIndex">index of the spline in our spline container</param>
    /// <param name="t">progress from 0-1 along that spline</param>
    /// <returns>tangent in world space</returns>
    public Vector3 GetTangent(int splineIndex, float t)
    {
        return ((Vector3)spline.EvaluateTangent(splineIndex, Mathf.Clamp01(t))).normalized;
    }

    public float DistanceToT(float distance)
    {
        return DistanceToT(0, distance);
    }
    
    public float DistanceToT(int splineIndex, float distance)
    {
        return pathLengths[splineIndex] > 0f ? Mathf.Clamp01(distance / pathLengths[splineIndex]) : 0f;
    }

    /// <summary>
    /// Goes through each spline in our spline container, and sets the Z positions of all the knots to 0;
    /// </summary>
    private void FixZPostions()
    {
        if (spline == null)
        {
            spline = GetComponent<SplineContainer>();
        }
        for (int i = 0; i < spline.Splines.Count; i++)
        {
            Spline s = spline.Splines[i];
            for (int j = 0; j < s.Count; j++)
            {
                BezierKnot knot = s[j];
                Vector3 pos = knot.Position;
                pos.z = 0;
                knot.Position = pos;
                s[j] = knot;
            }
        }
    }

}