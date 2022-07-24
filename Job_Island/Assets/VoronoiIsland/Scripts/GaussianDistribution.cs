using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//가우시안 알고리즘 : https://answers.unity.com/questions/421968/normal-distribution-random.html
public struct GaussianDistribution : IJobParallelFor
{
	[ReadOnly] public int Width;
	[ReadOnly] public int Height;
	[WriteOnly] public NativeArray<float2> Corners;

	static Unity.Mathematics.Random s_Random;

	public GaussianDistribution(int width, int height, NativeArray<float2> corners)
	{
		Width = width;
		Height = height;
		Corners = corners;

		s_Random = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);

		Corners[corners.Length - 4] = math.float2(0, 0);
		Corners[corners.Length - 3] = math.float2(width, 0);
		Corners[corners.Length - 2] = math.float2(0, height);
		Corners[corners.Length - 1] = math.float2(width, height);
	}

	public void Execute(int index)
	{
		Corners[index] = math.float2(Gaussian(0, Width), Gaussian(0, Height));
	}

	private float Gaussian(float minValue = 0.0f, float maxValue = 1.0f)
	{
		float u, v, S;
		do
		{
			u = 2.0f * s_Random.NextFloat() - 1.0f;
			v = 2.0f * s_Random.NextFloat() - 1.0f;
			S = u * u + v * v;
		}
		while (S > 1.0f);

		float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

		float mean = (minValue + maxValue) / 2.0f;
		float sigma = (maxValue - mean) / 3.0f;
		return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
	}
}
