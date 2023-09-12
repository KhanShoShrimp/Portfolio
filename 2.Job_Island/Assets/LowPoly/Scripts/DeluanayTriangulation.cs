using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//들로네 삼각분법 : https://www.secmem.org/blog/2019/01/11/Deluanay_Triangulation/
public struct DeluanayTriangulation : IJobFor, IDisposable
{
	[ReadOnly] public NativeArray<float2> Vertices;
	public NativeList<float2> Checks;
	public NativeList<Triangle> Triangles;

	public DeluanayTriangulation(NativeArray<float2> vertices)
	{
		Vertices = vertices;
		Triangles = new NativeList<Triangle>((vertices.Length - 3) * 2, Allocator.TempJob);

		int length = Vertices.Length;

		Checks = new NativeList<float2>(length, Allocator.TempJob)
		{
			Vertices[length - 4],
			Vertices[length - 3],
			Vertices[length - 2],
			Vertices[length - 1]
		};

		Triangles.Add(new Triangle(Vertices[length - 4], Vertices[length - 2], Vertices[length - 1]));
		Triangles.Add(new Triangle(Vertices[length - 4], Vertices[length - 3], Vertices[length - 1]));
	}

	public void Dispose()
	{
		if (Triangles.IsCreated)
		{
			Triangles.Dispose();
		}
		if (Checks.IsCreated)
		{
			Checks.Dispose();
		}
	}

	public void Execute(int index)
	{
		if (!Checks.Contains(Vertices[index]))
		{
			Checks.Add(Vertices[index]);

			for (int i = Triangles.Length - 1; i >= 0; --i)
			{
				if (Triangles[i].InsidePoint(Vertices[index]))
				{
					var triangle = Triangles[i];

					Triangles.RemoveAt(i);

					try
					{
						var triangle1 = new Triangle(triangle.Point1, triangle.Point2, Vertices[index]);
						if (!triangle1.InsidePoints(Checks))
						{
							Triangles.Add(triangle1);
						}
					}
					catch { }

					try
					{
						var triangle2 = new Triangle(triangle.Point2, triangle.Point3, Vertices[index]);
						if (!triangle2.InsidePoints(Checks))
						{
							Triangles.Add(triangle2);
						}
					}
					catch { }

					try
					{
						var triangle3 = new Triangle(triangle.Point3, triangle.Point1, Vertices[index]);
						if (!triangle3.InsidePoints(Checks))
						{
							Triangles.Add(triangle3);
						}
					}
					catch { }
				}
			}
		}
	}
}
