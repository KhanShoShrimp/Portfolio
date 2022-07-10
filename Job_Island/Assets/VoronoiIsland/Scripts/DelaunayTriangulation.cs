using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//들로네 삼각분법 : https://www.secmem.org/blog/2019/01/11/Deluanay_Triangulation/
public struct DelaunayTriangulation
{
	public IReadOnlyList<Triangle> Triangles;
	private List<float2> points;
	private List<Triangle> triangles;

	public DelaunayTriangulation(NativeArray<float2> vertices)
	{
		int Length = vertices.Length;

		points = new List<float2>()
		{
			vertices[Length - 4],
			vertices[Length - 3],
			vertices[Length - 2],
			vertices[Length - 1]
		};
		triangles = new List<Triangle>()
		{
			new Triangle(vertices[Length - 4], vertices[Length - 3], vertices[Length - 1]),
			new Triangle(vertices[Length - 4], vertices[Length - 2], vertices[Length - 1])
		};
		Triangles = triangles;

		foreach (var point in vertices)
		{
			if (points.Contains(point))
			{
				continue;
			}
			else
			{
				points.Add(point);
			}

			foreach (var triangle in BadTriangle(point))
			{
				triangles.Remove(triangle);

				try
				{
					var triangle1 = new Triangle(triangle.Point1, triangle.Point2, point);
					if (!triangle1.InsidePoints(points))
					{
						triangles.Add(triangle1);
					}
				}
				catch { }

				try
				{
					var triangle2 = new Triangle(triangle.Point2, triangle.Point3, point);
					if (!triangle2.InsidePoints(points))
					{
						triangles.Add(triangle2);
					}
				}
				catch { }

				try
				{
					var triangle3 = new Triangle(triangle.Point3, triangle.Point1, point);
					if (!triangle3.InsidePoints(points))
					{
						triangles.Add(triangle3);
					}
				}
				catch { }
			}
		}
	}

	private IEnumerable<Triangle> BadTriangle(float2 point)
	{
		for (int i = triangles.Count - 1; i >= 0; --i)
		{
			if (triangles[i].InsidePoint(point))
			{
				yield return triangles[i];
			}
		}
	}
}
