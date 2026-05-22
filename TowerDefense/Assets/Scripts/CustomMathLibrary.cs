using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CustomMathLibrary
{
    /// <summary>
    /// Calculates the angle between two points (vector 2's)
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <returns>Angle in rad</returns>
    public static float AngleBetweenVector2Positions(Vector2 origin, Vector2 target)
    {
        float radian = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
        float degrees180 = radian * Mathf.Rad2Deg;
        return Mathf.Repeat(degrees180, 360);
    }

    /// <summary>
    /// Checks if the specified angle is within a range
    /// </summary>
    /// <param name="Range"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static bool AngleInRange(Vector2 Range, float angle)
    {
        if(Range.x < Range.y)
        {
            return angle >= Range.x && angle <= Range.y;
        }
        else
        {
            return angle >= Range.x || angle <= Range.y;
        }
    }

    public static bool AngleWithinRanges(Vector2[] ranges, float angle)
    {
        return AngleWithinRanges(ranges.ToList(), angle);
    }
    public static bool AngleWithinRanges(List<Vector2> ranges, float angle)
    {
        foreach(Vector2 range in ranges)
        {
            if(AngleInRange(range, angle))
            {
                return true;
            }
        }
        return false;
    }

}
