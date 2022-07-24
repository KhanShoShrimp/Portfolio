using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct GetNode : IJobParallelFor, IDisposable
{
	public NativeArray<float2> Vertices;
	public NativeArray<Triangle> Triangles;
	public NativeList<float2>[] Nodes;

	public GetNode(NativeArray<float2> vertices, NativeArray<Triangle> triangles)
	{
		Vertices = vertices;
		Triangles = triangles;
		int length = vertices.Length;

		Nodes = new NativeList<float2>[length];
		for (int i = 0; i < length; i++)
		{
			Nodes[i] = new NativeList<float2>(Allocator.Temp);
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < Nodes.Length; i++)
		{
			if (Nodes[i].IsCreated)
			{
				Nodes[i].Dispose();
			}
		}
	}

	public void Execute(int index)
	{
		var triangle = Triangles[index];
		var center = triangle.Center;
		int index1 = Vertices.IndexOf(triangle.Point1);
		int index2 = Vertices.IndexOf(triangle.Point2);
		int index3 = Vertices.IndexOf(triangle.Point3);

		Nodes[index1].Add(center);
		Nodes[index2].Add(center);
		Nodes[index3].Add(center);
	}
}