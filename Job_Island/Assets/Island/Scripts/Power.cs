using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct Power : IDisposable
{
	public NativeArray<float> Values;

	public Power(int width, int height)
	{
		Values = new NativeArray<float>(width * height, Allocator.Persistent);

		new PowerJob()
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
	private struct PowerJob : IJobParallelFor
	{
		[ReadOnly] public int Width;
		[ReadOnly] public float2 Resolution;
		[WriteOnly] public NativeArray<float> Output;

		public void Execute(int index)
		{
			float2 uv = math.float2(index % Width, index / Width) / Resolution;
			uv = math.abs(uv - 0.5f);
			uv = (uv - 0.5f) * -2;
			Output[index] = uv.x + uv.y;
		}
	}
}
