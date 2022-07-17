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
	public NativeArray<Triangle> Triangles;
	public NativeArray<float2> Corners;
	public NativeArray<float2> Centers;
	public NativeArray<float4> Edges;
	private readonly int Length;

	public VoronoiMapData(int width, int height, int count)
	{
		Corners = new NativeArray<float2>(count, Allocator.Persistent);
		Centers = new NativeArray<float2>();
		Triangles = new NativeArray<Triangle>();
		Edges = new NativeArray<float4>();
		Length = count;

		PickDot(width, height);
		DeluanayTriangulation();
		Voronoi();
	}

	public void Dispose()
	{
		if (Corners.IsCreated)
		{
			Corners.Dispose();
		}
		if (Triangles.IsCreated)
		{
			Triangles.Dispose();
		}
		if (Edges.IsCreated)
		{
			Edges.Dispose();
		}
	}

	public void PickDot(int width, int height)
	{
		Corners[Length - 4] = math.float2(0, 0);
		Corners[Length - 3] = math.float2(width, 0);
		Corners[Length - 2] = math.float2(0, height);
		Corners[Length - 1] = math.float2(width, height);

		new GaussianDistribution(width, height, Corners).Schedule(Length - 4, 8).Complete();
	}

	//들로네 삼각분법 : https://www.secmem.org/blog/2019/01/11/Deluanay_Triangulation/
	public void DeluanayTriangulation()
	{
		using NativeList<float2> corners = new NativeList<float2>(Length, Allocator.TempJob)
		{
			Corners[Length - 4],
			Corners[Length - 3],
			Corners[Length - 2],
			Corners[Length - 1]
		};

		using NativeList<Triangle> triangleList = new NativeList<Triangle>((Length - 3) * 2, Allocator.TempJob)
		{
			new Triangle(Corners[Length - 4], Corners[Length - 2], Corners[Length - 1]),
			new Triangle(Corners[Length - 4], Corners[Length - 3], Corners[Length - 1])
		};

		using NativeList<float4> edges = new NativeList<float4>(Length * 10, Allocator.TempJob);

		new D_CalcTriangle()
		{
			Corners = Corners,
			CheckList = corners,
			Triangles = triangleList
		}.Run(Length);

		new D_ConnectCenter()
		{
			Triangles = triangleList,
			Edges = edges
		}.Run(triangleList.Length);

		Triangles = new NativeArray<Triangle>(triangleList.Length, Allocator.Persistent);
		NativeArray<Triangle>.Copy(triangleList, Triangles);

		Edges = new NativeArray<float4>(edges.Length, Allocator.Persistent);
		NativeArray<float4>.Copy(edges, Edges);
	}

	public void Voronoi()
	{

	}
}
