using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CalcArea : IJobParallelFor
{
	[ReadOnly] public NativeArray<float2> Vertices;
	[ReadOnly] public NativeArray<Triangle> Triangles;
	[NativeDisableParallelForRestriction] public NativeArray<float> Areas;

	public void Execute(int index)
	{
		float area = Triangles[index].Area;
		Areas[Vertices.IndexOf(Triangles[index].Point1)] += area;
		Areas[Vertices.IndexOf(Triangles[index].Point2)] += area;
		Areas[Vertices.IndexOf(Triangles[index].Point3)] += area;
	}
}
