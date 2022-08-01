using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class MeshExtension
{
	public static void MakeMesh(this Mesh mesh, int width, int height, NativeArray<float> heights)
	{
		int length = width * height;
		using NativeArray<float2> vertices2 = new NativeArray<float2>(length, Allocator.TempJob);
		new GetVertices()
		{
			Width = width,
			Vertices = vertices2
		}.Schedule(length, 8).Complete();

		using NativeArray<float2> uvs = new NativeArray<float2>(length, Allocator.TempJob);
		new GetUvs()
		{
			Resolutions = math.float2(width, height),
			Vertices = vertices2,
			Uvs = uvs
		}.Schedule(length, 8).Complete();

		using NativeArray<float3> vertices3 = new NativeArray<float3>(length, Allocator.TempJob);
		new AddHeights()
		{
			Heights = heights,
			InputVertices = vertices2,
			ResultVertices = vertices3
		}.Schedule(length, 8).Complete();

		using NativeArray<int> triangles = new NativeArray<int>((width - 1) * (height - 1) * 6, Allocator.TempJob);
		new GetTriangle()
		{
			Width = width,
			Height = height,
			Triangles = triangles
		}.Schedule((width - 1) * (height - 1) * 6, 8).Complete();

		mesh.Clear();
		mesh.SetVertices(vertices3);
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(triangles.ToArray(), 0);
		mesh.RecalculateNormals();
	}

	public static void MakeMesh(this Mesh mesh, int width, int height, NativeArray<float2> vertices2, NativeArray<float> heights, NativeArray<Triangle> triangles)
	{
		int length = vertices2.Length;


		using NativeArray<float3> vertices3 = new NativeArray<float3>(length, Allocator.TempJob);
		new AddHeights()
		{
			Heights = heights,
			InputVertices = vertices2,
			ResultVertices = vertices3
		}.Schedule(length, 8).Complete();


		using NativeArray<float2> uvs = new NativeArray<float2>(length, Allocator.TempJob);
		new GetUvs()
		{
			Resolutions = math.float2(width, height),
			Vertices = vertices2,
			Uvs = uvs
		}.Schedule(length, 8).Complete();

		using NativeArray<int> triangleIndex = new NativeArray<int>(triangles.Length * 3, Allocator.TempJob);
		new GetTriangleIndex()
		{
			Input = triangles,
			Vertices = vertices2,
			Result = triangleIndex
		}.Schedule(triangles.Length, 8).Complete();

		mesh.Clear();
		mesh.SetVertices(vertices3);
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(triangleIndex.ToArray(), 0);
		mesh.RecalculateNormals();
	}
}

[BurstCompile]
public struct GetVertices : IJobParallelFor
{
	[ReadOnly] public int Width;
	[WriteOnly] public NativeArray<float2> Vertices;

	public void Execute(int index)
	{
		int x = index % Width;
		int y = index / Width;
		Vertices[index] = math.float2(x, y);
	}
}

[BurstCompile]
public struct AddHeights : IJobParallelFor
{
	[ReadOnly] public NativeArray<float2> InputVertices;
	[ReadOnly] public NativeArray<float> Heights;
	[WriteOnly] public NativeArray<float3> ResultVertices;

	public void Execute(int index)
	{
		ResultVertices[index] = math.float3(InputVertices[index].x, Heights[index], InputVertices[index].y);
	}
}

[BurstCompile]
public struct GetUvs : IJobParallelFor
{
	[ReadOnly] public float2 Resolutions;
	[ReadOnly] public NativeArray<float2> Vertices;
	[WriteOnly] public NativeArray<float2> Uvs;

	public void Execute(int index)
	{
		Uvs[index] = Vertices[index] / Resolutions;
	}
}

[BurstCompile]
public struct GetTriangle : IJobParallelFor
{
	[ReadOnly] public int Width;
	[ReadOnly] public int Height;
	[WriteOnly] public NativeArray<int> Triangles;

	public void Execute(int index)
	{
		int i = index / 6;
		int x = i % (Width - 1);
		int y = i / (Width - 1);

		if (x < Width - 1 && y < Height - 1)
		{
			switch (index % 6)
			{
				case 0:
					Triangles[index] = i + Width;
					break;
				case 1:
					Triangles[index] = i + 1;
					break;
				case 2:
					Triangles[index] = i;
					break;
				case 3:
					Triangles[index] = i;
					break;
				case 4:
					Triangles[index] = i + Width - 1;
					break;
				case 5:
					Triangles[index] = i + Width;
					break;
			}
		}
	}
}

[BurstCompile]
public struct GetTriangleIndex : IJobParallelFor
{
	[ReadOnly] public NativeArray<Triangle> Input;
	[ReadOnly] public NativeArray<float2> Vertices;
	[NativeDisableParallelForRestriction]
	[WriteOnly] public NativeArray<int> Result;

	public void Execute(int index)
	{
		Result[index * 3] = Vertices.IndexOf(Input[index].Point1);
		Result[index * 3 + 1] = Vertices.IndexOf(Input[index].Point2);
		Result[index * 3 + 2] = Vertices.IndexOf(Input[index].Point3);
	}
}

[BurstCompile]
public struct GetColor : IJobParallelFor
{
	[ReadOnly] public NativeArray<float> Heights;
	[WriteOnly] public NativeArray<Color32> Colors;

	public void Execute(int index)
	{
		var value = Heights[index] * 3;

		if (value < 20)
		{
			Colors[index] = new Color32(236, 236, 195, 255);
		}
		else
		{
			Colors[index] = new Color32(122, 212, 51, 255);
		}
	}
}