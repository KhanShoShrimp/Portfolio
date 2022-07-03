using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

public struct PerlinNoise : IDisposable
{
	public NativeArray<float> Values;

	public PerlinNoise(int width, int height, int range = 10)
	{
		Values = new NativeArray<float>(width * height, Allocator.Persistent);

		new UpdateValueJob
		{
			Result = Values,
			Offset = math.float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)),
			Width = width,
			Resolution = math.float2(width, height),
			Range = range,
		}
		.Schedule(width * height, 8)
		.Complete();
	}

	public PerlinNoise(int width, int height, float2 offset, int range = 10)
	{
		Values = new NativeArray<float>(width * height, Allocator.Persistent);

		new UpdateValueJob
		{
			Result = Values,
			Offset = offset,
			Width = width,
			Resolution = math.float2(width, height),
			Range = range,
		}
		.Schedule(width * height, 8)
		.Complete();
	}

	public void Dispose()
	{
		Values.Dispose();
	}

	[BurstCompile]
	private struct UpdateValueJob : IJobParallelFor
	{
		[ReadOnly] public float2 Offset;
		[ReadOnly] public int Width;
		[ReadOnly] public float2 Resolution;
		[ReadOnly] public float Range;
		[WriteOnly] public NativeArray<float> Result;

		public void Execute(int index)
		{
			float2 uv = math.float2(index % Width, index / Width) / Resolution;
			float value = noise.pnoise((uv + Offset) * Range, Resolution);
			Result[index] = math.remap(-1, 1, 0, 1, value);
		}
	}
}