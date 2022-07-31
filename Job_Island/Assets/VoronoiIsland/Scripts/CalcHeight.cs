using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CalcHeight : IJobParallelFor
{
	[ReadOnly] public NativeArray<float> Areas;
	[ReadOnly] public float Mult;
	[WriteOnly] public NativeArray<float> Heights;

	public void Execute(int index)
	{
		if (Areas[index] > 0)
		{
			Heights[index] = Areas[index] / Mult * -500 + 12;
		}
		else
		{
			Heights[index] = 0;
		}
	}
}
