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

	public NativeArray<float2> Centroids;
	public NativeArray<float4> Edges;

	private readonly int Width;
	private readonly int Height;
	private readonly int Length;

	public VoronoiMapData(int width, int height, int count)
	{
		Width = width;
		Height = height;
		Length = count;

		Vertices = new NativeArray<float2>(Length, Allocator.Persistent);
		Triangles = new NativeArray<Triangle>();

		Centroids = new NativeArray<float2>();
		Edges = new NativeArray<float4>();

		PickDot();
		CalcTriangle();
		CalcCentroid();
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
		if (Centroids.IsCreated)
		{
			Centroids.Dispose();
		}
		if (Edges.IsCreated)
		{
			Edges.Dispose();
		}
	}

	private void PickDot()
	{
		new GaussianDistribution(Width, Height, Vertices).Schedule(Length - 4, 8).Complete();
	}

	private void CalcTriangle()
	{
		using DeluanayTriangulation deluanay = new DeluanayTriangulation(Vertices);
		deluanay.Run(Length);
		Triangles = new NativeArray<Triangle>(deluanay.Triangles, Allocator.Persistent);

		using Voronoi voronoi = new Voronoi(Triangles);
		voronoi.Run(Triangles.Length);
		Edges = new NativeArray<float4>(voronoi.Edges, Allocator.Persistent);
	}

	private void CalcCentroid()
	{
		NativeList<float2>[] nodes = new NativeList<float2>[Length];
		for (int i = 0; i < Length; i++)
		{
			nodes[i] = new NativeList<float2>(Allocator.TempJob);
		}

		for (int i = 0; i < Triangles.Length; i++)
		{
			var center = Triangles[i].Center;

			int index1 = Vertices.IndexOf(Triangles[i].Point1);
			int index2 = Vertices.IndexOf(Triangles[i].Point2);
			int index3 = Vertices.IndexOf(Triangles[i].Point3);

			nodes[index1].Add(center);
			nodes[index2].Add(center);
			nodes[index3].Add(center);
		}

		Centroids = new NativeArray<float2>(Length, Allocator.Persistent);
		//for (int i = 0; i < Length; i++)
		//{
		//	var nodesLength = nodes[i].Length;
		//	for (int j = 0; j < nodesLength; j++)
		//	{
		//		Centroids[i] += nodes[i][j];
		//	}
		//	Centroids[i] /= nodesLength;
		//}
		
		for (int i = 0; i < Length; i++)
		{
			var nodesLength = nodes[i].Length;

			float Area = 0;
			Centroids[i] = float2.zero;

			for (int j = 0; j < nodesLength; j++)
			{
				float2 crnt = nodes[i][j];
				float2 next = nodes[i][(j + 1) % nodesLength];

				float k = crnt.x * next.y - next.x * crnt.y;

				Area += k;
				Centroids[i] += math.float2((crnt.x + next.x) * k, (crnt.y + next.y) * k);
			}
			Area *= 0.5f;
			Centroids[i] /= 6 * Area;
		}

		for (int i = 0; i < Length; i++)
		{
			nodes[i].Dispose();
		}
	}

	private void Lloyid()
	{

	}

	private void Fortune()
	{

	}
}
