using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class VoronoiIsland : MonoBehaviour
{
	const int width = 100;
	const int height = 100;
	public int count = 6;

	VoronoiMapData data;

	private void Awake()
	{
		data = new VoronoiMapData(width, height, count);
	}

	private void OnDestroy()
	{
		data.Dispose();
	}

	[ContextMenu("New Map")]
	private void NewMap()
	{
		data.Dispose();
		data = new VoronoiMapData(width, height, count);
	}

	private void OnDrawGizmos()
	{
		if (data.Vertices == null | data.Triangles == null | data.Centroids == null | data.Edges == null)
		{
			return;
		}

		Gizmos.color = Color.white;
		for (int i = 0; i < data.Vertices.Length; ++i)
		{
			Gizmos.DrawSphere(new Vector3(data.Vertices[i].x, 0, data.Vertices[i].y), 1);
		}

		for (int i = 0; i < data.Triangles.Length; ++i)
		{
			Gizmos.DrawLine(
				new Vector3(data.Triangles[i].Point1.x, 0, data.Triangles[i].Point1.y),
				new Vector3(data.Triangles[i].Point2.x, 0, data.Triangles[i].Point2.y));
			Gizmos.DrawLine(
				new Vector3(data.Triangles[i].Point2.x, 0, data.Triangles[i].Point2.y),
				new Vector3(data.Triangles[i].Point3.x, 0, data.Triangles[i].Point3.y));
			Gizmos.DrawLine(
				new Vector3(data.Triangles[i].Point3.x, 0, data.Triangles[i].Point3.y),
				new Vector3(data.Triangles[i].Point1.x, 0, data.Triangles[i].Point1.y));
		}

		Gizmos.color = Color.red;
		for (int i = 0; i < data.Centroids.Length; ++i)
		{
			Gizmos.DrawSphere(new Vector3(data.Centroids[i].x, 0, data.Centroids[i].y), 1);
		}
		for (int i = 0; i < data.Edges.Length; ++i)
		{
			Gizmos.DrawLine(
				new Vector3(data.Edges[i].x, 0, data.Edges[i].y),
				new Vector3(data.Edges[i].z, 0, data.Edges[i].w));
		}
	}
}
