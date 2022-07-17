using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class MeshExtension
{
	public static void MakePerlinMesh(this Mesh mesh, int width, int height, NativeArray<float> values)
	{
		using NativeArray<float3> Vertices = new NativeArray<float3>(width * height, Allocator.TempJob);
		using NativeArray<float2> Uvs = new NativeArray<float2>(width * height, Allocator.TempJob);
		using NativeArray<int> Triangles = new NativeArray<int>((width - 1) * (height - 1) * 6, Allocator.TempJob);

		new MeshData()
		{
			Width = width,
			Height = height,
			Resolutions = math.float2(width, height),

			Values = values,
			Vertices = Vertices,
			Uvs = Uvs,
		}.Schedule(width * height, 8).Complete();

		new MeshTriangle()
		{
			Width = width,
			Height = height,

			Triangles = Triangles
		}.Schedule((width - 1) * (height - 1) * 6, 8).Complete();

		mesh.Clear();
		mesh.SetVertices(Vertices);
		mesh.SetUVs(0, Uvs);
		mesh.SetTriangles(Triangles.ToArray(), 0);
		mesh.RecalculateNormals();
	}
}

[BurstCompile]
public struct MeshData : IJobParallelFor
{
	[ReadOnly] public int Width;
	[ReadOnly] public int Height;
	[ReadOnly] public float2 Resolutions;

	[ReadOnly] public NativeArray<float> Values;
	[WriteOnly] public NativeArray<float3> Vertices;
	[WriteOnly] public NativeArray<float2> Uvs;

	public void Execute(int index)
	{
		int x = index % Width;
		int y = index / Width;
		Vertices[index] = math.float3(x, Values[index], y);
		Uvs[index] = math.float2(x, y) / Resolutions;
	}
}

[BurstCompile]
public struct MeshTriangle : IJobParallelFor
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