using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class NoiseMesh : MonoBehaviour
{
	MeshFilter m_MeshFilter;
	MeshRenderer m_MeshRenderer;

	Texture2D m_Texture;
	Mesh m_Mesh;

	public int Size_Width;
	public int Size_Height;

	public float Total_Height;
	public float Mountain_Height;

	private void Awake()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_MeshRenderer = GetComponent<MeshRenderer>();

		m_Texture = new Texture2D(0, 0);
		m_Mesh = new Mesh();

		m_MeshFilter.mesh = m_Mesh;
		m_MeshRenderer.materials[0].mainTexture = m_Texture;

		CreateMap();
	}

	public void CreateMap()
	{
		using (MapData mapData = new MapData(Size_Width, Size_Height, Total_Height, Mountain_Height))
		{
			m_Texture.Resize(Size_Width, Size_Height);
			m_Texture.SetPixelData(mapData.Colors, 0);
			m_Texture.Apply();

			using NativeArray<float3> Vertices = new NativeArray<float3>(Size_Width * Size_Height, Allocator.TempJob);
			using NativeArray<float2> Uvs = new NativeArray<float2>(Size_Width * Size_Height, Allocator.TempJob);
			using NativeArray<int> Triangles = new NativeArray<int>((Size_Width - 1) * (Size_Height - 1) * 6, Allocator.TempJob);

			new MeshJob()
			{
				Noises = mapData.Values,
				Width = Size_Width,
				Height = Size_Height,
				Resolutions = math.float2(Size_Width, Size_Height),
				Vertices = Vertices,
				Uvs = Uvs,
			}.Schedule(Size_Width * Size_Height, 8).Complete();

			new TriangleJob()
			{
				Width = Size_Width,
				Height = Size_Height,
				Triangles = Triangles
			}.Schedule((Size_Width - 1) * (Size_Height - 1) * 6, 8).Complete();

			m_Mesh.Clear();
			m_Mesh.SetVertices(Vertices);
			m_Mesh.SetUVs(0, Uvs);
			m_Mesh.SetTriangles(Triangles.ToArray(), 0);
			m_Mesh.RecalculateNormals();
		}
	}

	[BurstCompile]
	private struct MeshJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float> Noises;
		[ReadOnly] public int Width;
		[ReadOnly] public int Height;
		[ReadOnly] public float2 Resolutions;
		[WriteOnly] public NativeArray<float3> Vertices;
		[WriteOnly] public NativeArray<float2> Uvs;

		public void Execute(int index)
		{
			int x = index % Width;
			int y = index / Width;
			Vertices[index] = math.float3(x, Noises[index], y);
			Uvs[index] = math.float2(x / Resolutions.x, y / Resolutions.y);
		}
	}

	[BurstCompile]
	private struct TriangleJob : IJobParallelFor
	{
		[ReadOnly] public int Width;
		[ReadOnly] public int Height;
		[WriteOnly] public NativeArray<int> Triangles;

		public void Execute(int index)
		{
			int i = index / 6;
			int x = i % (Width - 1);
			int y = i / (Width - 1);

			if (x < Width - 1 && y < Height - 1)
			{
				switch (index % 6)
				{
					case 0:
						Triangles[index] = i + Width;
						break;
					case 1:
						Triangles[index] = i + 1;
						break;
					case 2:
						Triangles[index] = i;
						break;
					case 3:
						Triangles[index] = i;
						break;
					case 4:
						Triangles[index] = i + Width - 1;
						break;
					case 5:
						Triangles[index] = i + Width;
						break;
				}
			}
		}
	}
}
