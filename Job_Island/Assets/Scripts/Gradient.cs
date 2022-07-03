using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct Gradient : IDisposable
{
	public NativeArray<float> Values;

	public Gradient(int width, int height)
	{
		Values = new NativeArray<float>(width * height, Allocator.Persistent);

		new GradientJob()
		{
			Width = width,
			Resolution = math.float2(width, height),
			Output = Values
		}
		.Schedule(width * height, 8)
		.Complete();
	}

	public void Dispose()
	{
		Values.Dispose();
	}

	[BurstCompile]
	private struct GradientJob : IJobParallelFor
	{
		[ReadOnly] public int Width;
		[ReadOnly] public float2 Resolution;
		[WriteOnly] public NativeArray<float> Output;

		public void Execute(int index)
		{
			float2 uv = math.float2(index % Width, index / Width) / Resolution;
			uv = uv - 0.5f;
			uv = math.pow(uv, 2);
			Output[index] = math.remap(0.25f, -0.25f, 0, 1, uv.x + uv.y);
		}
	}
}
