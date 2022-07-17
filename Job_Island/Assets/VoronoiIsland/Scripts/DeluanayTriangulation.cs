using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct DeluanayTriangulation : IJobFor
{
	[ReadOnly] public NativeArray<float2> Vertices;
	public NativeList<float2> Checks;
	public NativeList<Triangle> Triangles;

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
