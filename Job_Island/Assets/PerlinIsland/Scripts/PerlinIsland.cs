using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PerlinIsland : MonoBehaviour
{
	const int WIDTH = 250;
	const int HEIGHT = 250;
	const int TOTAL = 10;
	const float MOUNTAIN = 1.5f;

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
		using PerlinMapData mapData = new PerlinMapData(WIDTH, HEIGHT, TOTAL, MOUNTAIN);

		m_Texture.Resize(WIDTH, HEIGHT);
		m_Texture.SetPixelData(mapData.Colors, 0);
		m_Texture.Apply();

		m_Mesh.MakePerlinMesh(WIDTH, HEIGHT, mapData.Values);
	}

}
