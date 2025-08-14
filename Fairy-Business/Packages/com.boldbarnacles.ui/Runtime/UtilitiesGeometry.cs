using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public sealed class UtilitiesGeometry
{
    internal const float kPickDistance = 5f;

    internal static float s_CustomPickDistance = 5f;
    internal static Transform[] ignoreRaySnapObjects;

    internal static float GetParametrization(Vector2 x0, Vector2 x1, Vector2 x2)
    {
        return 0f - Vector2.Dot(x1 - x0, x2 - x1) / (x2 - x1).sqrMagnitude;
    }

    //
    // Summary:
    //     Returns the parameter for the projection of the point on the given line.
    //
    // Parameters:
    //   point:
    //
    //   linePoint:
    //
    //   lineDirection:
    public static float PointOnLineParameter(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
    {
        return Vector3.Dot(lineDirection, point - linePoint) / lineDirection.sqrMagnitude;
    }

    //
    // Summary:
    //     Project point onto a line.
    //
    // Parameters:
    //   point:
    //
    //   lineStart:
    //
    //   lineEnd:
    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector = lineEnd - lineStart;
        float magnitude = vector.magnitude;
        Vector3 vector2 = vector;
        if (magnitude > 1E-06f)
        {
            vector2 /= magnitude;
        }

        float value = Vector3.Dot(vector2, rhs);
        value = Mathf.Clamp(value, 0f, magnitude);
        return lineStart + vector2 * value;
    }
    [BurstCompile]
    public static float SquaredDistanceToRayInfinite(Vector3 point, Ray ray)
    {
        var c = Vector3.Cross(ray.direction, point - ray.origin);
        return c.x * c.x + c.y * c.y + c.z * c.z;
        
        // this would be the code for checking distance to a finite Ray. Basically to the line OR to one of its endings, if its closer
        //return Vector3.Magnitude(ProjectPointLine(point, ray.origin, ray.origin * ray.direction) - point);
    }
    [BurstCompile]
    public static float DistanceToRayInfinite(Vector3 point, Ray ray)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        
        // this would be the code for checking distance to a finite Ray. Basically to the line OR to one of its endings, if its closer
        //return Vector3.Magnitude(ProjectPointLine(point, ray.origin, ray.origin * ray.direction) - point);
    }
    //
    // Summary:
    //     Calculate distance between a point and a line.
    //
    // Parameters:
    //   point:
    //
    //   lineStart:
    //
    //   lineEnd:
    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    internal static float DistanceToLineInternal(Vector3 point, Vector3 p1, Vector3 p2)
    {
        float num = DistancePointLine(point, p1, p2);
        if (num < 0f)
        {
            num = 0f;
        }

        return num;
    }

    //
    // Summary:
    //     Distance from a point p in 2d to a line defined by two points a and b.
    //
    // Parameters:
    //   p:
    //
    //   a:
    //
    //   b:
    public static float DistancePointLine2D(Vector2 p, Vector2 a, Vector2 b)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y)) / (b - a).magnitude;
    }

    //
    // Summary:
    //     Distance from a point p in 2d to a line segment defined by two points a and b.
    //
    //
    // Parameters:
    //   p:
    //
    //   a:
    //
    //   b:
    public static float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        float sqrMagnitude = (b - a).sqrMagnitude;
        if ((double)sqrMagnitude == 0.0)
        {
            return (p - a).magnitude;
        }

        float num = Vector2.Dot(p - a, b - a) / sqrMagnitude;
        if ((double)num < 0.0)
        {
            return (p - a).magnitude;
        }

        if ((double)num > 1.0)
        {
            return (p - b).magnitude;
        }

        Vector2 vector = a + num * (b - a);
        return (p - vector).magnitude;
    }


    public static float CalcPointSide(Vector2 l0, Vector2 l1, Vector2 point)
    {
        return (l1.y - l0.y) * (point.x - l0.x) - (l1.x - l0.x) * (point.y - l0.y);
    }

    public static float DistancePointToConvexHull(Vector2 p, List<Vector2> hull)
    {
        float num = float.PositiveInfinity;
        if (hull == null || hull.Count == 0)
        {
            return num;
        }

        bool flag = hull.Count > 1;
        int num2 = 0;
        for (int i = 0; i < hull.Count; i++)
        {
            int index = ((i == 0) ? (hull.Count - 1) : (i - 1));
            Vector2 vector = hull[i];
            Vector2 vector2 = hull[index];
            float num3 = CalcPointSide(vector, vector2, p);
            int num4 = ((num3 >= 0f) ? 1 : (-1));
            if (num2 == 0)
            {
                num2 = num4;
            }
            else if (num4 != num2)
            {
                flag = false;
            }

            float b = DistancePointToLineSegment(p, vector, vector2);
            num = Mathf.Min(num, b);
        }

        if (flag)
        {
            num = 0f;
        }

        return num;
    }

    public static void RemoveInsidePoints(int countLimit, Vector2 pt, List<Vector2> hull)
    {
        while (hull.Count >= countLimit && CalcPointSide(hull[hull.Count - 2], hull[hull.Count - 1], pt) <= 0f)
        {
            hull.RemoveAt(hull.Count - 1);
        }
    }

    public static void CalcConvexHull2D(Vector3[] points, List<Vector2> outHull)
    {
        outHull.Clear();
        if (points == null || points.Length == 0)
        {
            return;
        }

        int num = points.Length + 1;
        if (outHull.Capacity < num)
        {
            outHull.Capacity = num;
        }

        if (points.Length == 1)
        {
            outHull.Add(points[0]);
            return;
        }

        Array.Sort(points, delegate (Vector3 a, Vector3 b)
        {
            int num3 = a.x.CompareTo(b.x);
            return (num3 != 0) ? num3 : a.y.CompareTo(b.y);
        });
        foreach (Vector2 vector in points)
        {
            RemoveInsidePoints(2, vector, outHull);
            outHull.Add(vector);
        }

        int num2 = points.Length - 2;
        int countLimit = outHull.Count + 1;
        while (num2 >= 0)
        {
            Vector2 vector2 = points[num2];
            RemoveInsidePoints(countLimit, vector2, outHull);
            outHull.Add(vector2);
            num2--;
        }

        outHull.RemoveAt(outHull.Count - 1);
    }

}
