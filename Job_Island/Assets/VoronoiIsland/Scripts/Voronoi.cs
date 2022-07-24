using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Voronoi : IJobFor, IDisposable
{
	[ReadOnly] public NativeArray<Triangle> Triangles;
	[WriteOnly] public NativeList<float4> Edges;

	public Voronoi(NativeArray<Triangle> triangles)
	{
		Triangles = triangles;
		Edges = new NativeList<float4>(triangles.Length * 10, Allocator.TempJob);
	}

	public void Dispose()
	{
		if (Edges.IsCreated)
		{
			Edges.Dispose();
		}
	}

	public void Execute(int index)
	{
		var triangle = Triangles[index];

		foreach (var nearTriangle in Triangles)
		{
			if (triangle.Center.x == nearTriangle.Center.x && 
				triangle.Center.y == nearTriangle.Center.y)
			{
				continue;
			}

			if (triangle.IsNearby(nearTriangle))
			{
				Edges.Add(math.float4(triangle.Center, nearTriangle.Center));
			}
		}
	}
}
