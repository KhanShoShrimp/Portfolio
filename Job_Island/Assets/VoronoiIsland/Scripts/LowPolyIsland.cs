using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class LowPolyIsland : MonoBehaviour
{
	const int WIDTH = 100;
	const int HEIGHT = 100;
	const int COUNT = 100;

	Texture2D m_Texture;
	Mesh m_Mesh;

	private void Awake()
	{
		m_Texture = new Texture2D(0, 0);
		m_Mesh = new Mesh();

		GetComponent<MeshFilter>().mesh = m_Mesh;
		GetComponent<MeshRenderer>().materials[0].mainTexture = m_Texture;

		CreateMap();
	}

	public void CreateMap()
	{
		using LowPolyMapData data = new LowPolyMapData(WIDTH, HEIGHT, COUNT);

		//m_Texture.Resize(WIDTH, HEIGHT);
		//m_Texture.SetPixelData(mapData.Colors, 0);
		//m_Texture.Apply();

		m_Mesh.MakePerlinMesh(WIDTH, HEIGHT, mapData.
	}

	//private void OnDestroy()
	//{
	//	data.Dispose();
	//}

	//private void OnDrawGizmos()
	//{
	//	Gizmos.color = Color.white;
	//	for (int i = 0; i < data.Vertices.Length; ++i)
	//	{
	//		Gizmos.DrawSphere(new Vector3(data.Vertices[i].x, data.Heights[i], data.Vertices[i].y), 1);
	//	}

	//	for (int i = 0; i < data.Triangles.Length; ++i)
	//	{
	//		int index1 = data.Vertices.IndexOf(data.Triangles[i].Point1);
	//		int index2 = data.Vertices.IndexOf(data.Triangles[i].Point2);
	//		int index3 = data.Vertices.IndexOf(data.Triangles[i].Point3);

	//		Gizmos.DrawLine(
	//			new Vector3(data.Triangles[i].Point1.x, data.Heights[index1], data.Triangles[i].Point1.y),
	//			new Vector3(data.Triangles[i].Point2.x, data.Heights[index2], data.Triangles[i].Point2.y));
	//		Gizmos.DrawLine(
	//			new Vector3(data.Triangles[i].Point2.x, data.Heights[index2], data.Triangles[i].Point2.y),
	//			new Vector3(data.Triangles[i].Point3.x, data.Heights[index3], data.Triangles[i].Point3.y));
	//		Gizmos.DrawLine(
	//			new Vector3(data.Triangles[i].Point3.x, data.Heights[index3], data.Triangles[i].Point3.y),
	//			new Vector3(data.Triangles[i].Point1.x, data.Heights[index1], data.Triangles[i].Point1.y));
	//	}
	//	Gizmos.color = Color.red;
	//	Gizmos.DrawSphere(new Vector3(WIDTH, 0, 0), 1);
	//}
}
