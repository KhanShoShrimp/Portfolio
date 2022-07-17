using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct D_CalcTriangle : IJobFor
{
	[ReadOnly] public NativeArray<float2> Corners;
	public NativeList<float2> CheckList;
	public NativeList<Triangle> Triangles;

	public void Execute(int index)
	{
		if (!CheckList.Contains(Corners[index]))
		{
			CheckList.Add(Corners[index]);

			for (int i = Triangles.Length - 1; i >= 0; --i)
			{
				if (Triangles[i].InsidePoint(Corners[index]))
				{
					var triangle = Triangles[i];

					Triangles.RemoveAt(i);

					try
					{
						var triangle1 = new Triangle(triangle.Point1, triangle.Point2, Corners[index]);
						if (!triangle1.InsidePoints(CheckList))
						{
							Triangles.Add(triangle1);
						}
					}
					catch { }

					try
					{
						var triangle2 = new Triangle(triangle.Point2, triangle.Point3, Corners[index]);
						if (!triangle2.InsidePoints(CheckList))
						{
							Triangles.Add(triangle2);
						}
					}
					catch { }

					try
					{
						var triangle3 = new Triangle(triangle.Point3, triangle.Point1, Corners[index]);
						if (!triangle3.InsidePoints(CheckList))
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
