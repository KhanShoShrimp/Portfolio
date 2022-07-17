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
	public NativeArray<Triangle> Triangles;

	private NativeList<Triangle> TriangleList;
	private readonly int Length;

	public VoronoiMapData(int width, int height, int count)
	{
		Vertices = new NativeArray<float2>(count, Allocator.Persistent);
		Triangles = new NativeArray<Triangle>();

		TriangleList = new NativeList<Triangle>((count - 3) * 2, Allocator.Persistent);
		Length = count;

		PickDot(width, height);
		DrawTriangle();
	}

	public void Dispose()
	{
		if (Vertices.IsCreated)
		{
			Vertices.Dispose();
		}
		if (Triangles.IsCreated)
		{
			Triangles.Dispose();
		}
		if (TriangleList.IsCreated)
		{
			TriangleList.Dispose();
		}
	}

	public void PickDot(int width, int height)
	{
		Vertices[Length - 4] = math.float2(0, 0);
		Vertices[Length - 3] = math.float2(width, 0);
		Vertices[Length - 2] = math.float2(0, height);
		Vertices[Length - 1] = math.float2(width, height);

		new GaussianDistribution(width, height, Vertices).Schedule(Length - 4, 8).Complete();
	}

	//들로네 삼각분법 : https://www.secmem.org/blog/2019/01/11/Deluanay_Triangulation/
	public void DrawTriangle()
	{
		using NativeList<float2> checks = new NativeList<float2>(Length, Allocator.TempJob)
		{
			Vertices[Length - 4],
			Vertices[Length - 3],
			Vertices[Length - 2],
			Vertices[Length - 1]
		};

		TriangleList.Add(new Triangle(Vertices[Length - 4], Vertices[Length - 2], Vertices[Length - 1]));
		TriangleList.Add(new Triangle(Vertices[Length - 4], Vertices[Length - 3], Vertices[Length - 1]));

		new DeluanayTriangulation()
		{
			Vertices = Vertices,
			Checks = checks,
			Triangles = TriangleList
		}.Run(Length);

		Triangles = TriangleList.AsArray();
	}
}
