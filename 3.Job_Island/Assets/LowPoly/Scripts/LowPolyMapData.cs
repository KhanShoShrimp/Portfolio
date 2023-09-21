using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct LowPolyMapData : IDisposable/*, IComparer<float2>*/
{
	public NativeArray<float2> Vertices;
	public NativeArray<Triangle> Triangles;
	public NativeArray<float> Heights;

	private readonly int Width, Height;
	private int Length;

	private readonly float2 Center;

	public LowPolyMapData(int width, int height, int count)
	{
		Width = width;
		Height = height;
		Length = count;

		Center = math.float2(width, height) * 0.5f;

		Vertices = new NativeArray<float2>();
		Heights = new NativeArray<float>(Length, Allocator.Persistent);
		Triangles = new NativeArray<Triangle>();

		PickDot();
		CalcTriangle();
		CalcHeight();
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
		if (Heights.IsCreated)
		{
			Heights.Dispose();
		}
	}

	private void PickDot()
	{
		using NativeArray<float2> verts = new NativeArray<float2>(Length, Allocator.TempJob);
		new GaussianDistribution(Width, Height, verts).Schedule(Length - 4, 8).Complete();
		Vertices = new NativeArray<float2>(verts.Distinct().ToArray(), Allocator.Persistent);
		Length = Vertices.Length;
	}

	private void CalcTriangle()
	{
		using DeluanayTriangulation deluanay = new DeluanayTriangulation(Vertices);
		deluanay.Run(Length);
		Triangles = new NativeArray<Triangle>(deluanay.Triangles, Allocator.Persistent);
	}

	private void CalcHeight()
	{
		using NativeArray<float> areas = new NativeArray<float>(Length, Allocator.TempJob);

		new CalcArea()
		{
			Vertices = Vertices,
			Triangles = Triangles,
			Areas = areas
		}.Schedule(Triangles.Length, 8).Complete();

		new CalcHeight()
		{
			Areas = areas,
			Mult = (Width * Height),
			Heights = Heights
		}.Schedule(Length, 8).Complete();
	}
}
