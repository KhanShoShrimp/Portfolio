using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct D_ConnectCenter : IJobFor
{
	[ReadOnly] public NativeList<Triangle> Triangles;
	[WriteOnly] public NativeList<float4> Edges;

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
