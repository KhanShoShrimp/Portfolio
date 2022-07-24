using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//포츈 알고리즘 : https://jacquesheunis.com/post/fortunes-algorithm/
public struct Fortune
{
	public Arc BeachArc;
	[WriteOnly] public NativeArray<float2> Center;
	[WriteOnly] public NativeArray<float2> Vertices;
}

public class Arc
{
	public Arc Next = null;
	public Arc Prev = null;

	public float2 Point;

	public Arc GetArc(float sweepX)
	{
		if (Prev != null && Point.x < sweepX)
		{
			return Prev;
		}
		if (Next != null && Point.x > sweepX)
		{
			return Next;
		}
		return this;
	}

	public void Push(Arc arc)
	{
		if (arc.Point.x < Point.x)
		{
			if (Prev != null)
			{
				Prev.Push(arc);
			}
			else
			{
				Prev = arc;
			}
		}
	}

	public bool Contanis(float sweepY, float2 point)
	{
		var in_dist = math.distancesq(point, Point);
		var out_dist = math.distancesq(point, math.float2(point.x, sweepY));

		return in_dist < out_dist;
	}

	public bool Contact(float sweepY, float2 point)
	{
		var in_dist = math.distancesq(point, Point);
		var out_dist = math.distancesq(point, math.float2(point.x, sweepY));

		return in_dist == out_dist;
	}
}