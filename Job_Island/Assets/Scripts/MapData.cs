using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public enum NodeState : byte { ÇØº¯, µéÆÇ, ½£, ¶¥, ´« }

public struct MapData : IDisposable
{
	public NativeArray<float> Values;
	public NativeArray<NodeState> Nodes;
	public NativeArray<Color32> Colors;

	public MapData(int width, int height, float mult = 10, float pow = 2)
	{
		var length = width * height;
		Values = new NativeArray<float>(length, Allocator.Persistent);
		Nodes = new NativeArray<NodeState>(length, Allocator.Persistent);
		Colors = new NativeArray<Color32>(length, Allocator.Persistent);

		var rand = math.float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));

		using PerlinNoise noise = new PerlinNoise(width, height, rand);
		using Gradient gradient = new Gradient(width, height);
		using Power power = new Power(width, height);

		new UpdateDataJob()
		{
			Noise = noise.Values,
			Gradient = gradient.Values,
			Power = power.Values,
			Mult = mult,
			Pow = pow,
			Values = Values,
			OutputNodes = Nodes,
			OutputColors = Colors
		}
		.Schedule(length, 8)
		.Complete();
	}

	public void Dispose()
	{
		Values.Dispose();
		Nodes.Dispose();
		Colors.Dispose();
	}

	[BurstCompile]
	private struct UpdateDataJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float> Noise;
		[ReadOnly] public NativeArray<float> Gradient;
		[ReadOnly] public NativeArray<float> Power;
		[ReadOnly] public float Mult;
		[ReadOnly] public float Pow;
		[WriteOnly] public NativeArray<float> Values;
		[WriteOnly] public NativeArray<NodeState> OutputNodes;
		[WriteOnly] public NativeArray<Color32> OutputColors;

		public void Execute(int index)
		{
			var value = (Noise[index] * Gradient[index] * Power[index] * Power[index]) * Mult;
			value = math.pow(value, Pow);
			Values[index] = value;

			if (value < 5)
			{
				OutputNodes[index] = NodeState.ÇØº¯;
				OutputColors[index] = new Color32(236, 236, 195, 255);
			}
			else if (value < 7.5f)
			{
				OutputNodes[index] = NodeState.µéÆÇ;
				OutputColors[index] = new Color32(122, 212, 51, 255);
			}
			else if (value < 22.5f)
			{
				OutputNodes[index] = NodeState.½£;
				OutputColors[index] = new Color32(56, 166, 65, 255);
			}
			else if (value < 30)
			{
				OutputNodes[index] = NodeState.¶¥;
				OutputColors[index] = new Color32(132, 116, 102, 255);
			}
			else
			{
				OutputNodes[index] = NodeState.´«;
				OutputColors[index] = new Color32(111, 111, 111, 111);
			}
		}
	}
}
