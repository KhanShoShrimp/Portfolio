using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Triangle
{
	public float2 Point1;
	public float2 Point2;
	public float2 Point3;
	public float2 Center;
	public float Radius;

	public Triangle(float2 p1, float2 p2, float2 p3)
	{
		var value = (p2.y - p1.y) * (p3.x - p2.x) - (p3.y - p2.y) * (p2.x - p1.x);
		if (value == 0)
		{
			throw new Exception("3 points lies on a same line");
		}
		else if (value > 0)
		{
			Point1 = p1;
			Point2 = p2;
			Point3 = p3;
		}
		else
		{
			Point1 = p3;
			Point2 = p2;
			Point3 = p1;
		}

		var dA = math.distancesq(Point1, float2.zero);
		var dB = math.distancesq(Point2, float2.zero);
		var dC = math.distancesq(Point3, float2.zero);

		var aux1 = dA * (Point3.y - Point2.y) + dB * (Point1.y - Point3.y) + dC * (Point2.y - Point1.y);
		var aux2 = -(dA * (Point3.x - Point2.x) + dB * (Point1.x - Point3.x) + dC * (Point2.x - Point1.x));
		var div = 2 * (Point1.x * (Point3.y - Point2.y) + Point2.x * (Point1.y - Point3.y) + Point3.x * (Point2.y - Point1.y));

		if (div == 0)
		{
			throw new DivideByZeroException();
		}

		Center = math.float2(aux1 / div, aux2 / div);
		Radius = math.distancesq(p1, Center);
	}

	public bool InsidePoint(float2 point)
	{
		return math.distancesq(point, Center) < Radius;
	}

	public bool InsidePoints(IEnumerable<float2> points)
	{
		foreach (var point in points)
		{
			if (Point1.x == point.x && Point1.y == point.y)
			{
				continue;
			}
			if (Point2.x == point.x && Point2.y == point.y)
			{
				continue;
			}
			if (Point3.x == point.x && Point3.y == point.y)
			{
				continue;
			}

			if (InsidePoint(point))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return $"{Point1}, {Point2}, {Point3}";
	}
}