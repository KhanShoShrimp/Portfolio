using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct VoronoiMapData : IDisposable
{
	public NativeArray<float2> Vertices;
	public IReadOnlyList<Triangle> Triangles;

	public VoronoiMapData(int width, int height, int count)
	{
		Vertices = new NativeArray<float2>(count, Allocator.Persistent);
		Triangles = null;

			InitVertices(width, height, count);
			CalcTriangle();
	}

	public void Dispose()
	{
		if (Vertices.IsCreated)
		{
			Vertices.Dispose();
		}
	}

	public void InitVertices(int width, int height, int count)
	{
		Vertices[count - 4] = math.float2(0, 0);
		Vertices[count - 3] = math.float2(width, 0);
		Vertices[count - 2] = math.float2(0, height);
		Vertices[count - 1] = math.float2(width, height);

		new GaussianDistribution(width, height, Vertices).Schedule(count - 4, 8).Complete();
	}

	public void CalcTriangle()
	{
		Triangles = new DelaunayTriangulation(Vertices).Triangles;
	}
}
