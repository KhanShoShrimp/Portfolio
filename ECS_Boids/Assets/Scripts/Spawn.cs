using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System.Diagnostics;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SpawnComponent : IComponentData
{
	public Entity Prefab;
	public uint Count;
}

public partial class SpawnSystem : SystemBase
{
	protected override void OnStartRunning()
	{
		var spawnComponent = GetSingleton<SpawnComponent>();
		var commandBuffer = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
		var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

		Job.WithCode(() =>
		{
			for (int i = 0; i < spawnComponent.Count; i++)
			{
				var entity = commandBuffer.Instantiate(spawnComponent.Prefab);
				commandBuffer.SetComponent(entity, new ObjectTag() { Diretion = rand.NextFloat3(new float3(-1, -1, -1), new float3(1, 1, 1)) });
				commandBuffer.SetComponent(entity, new Translation() { Value = rand.NextFloat3(-2, 2) });
				commandBuffer.SetComponent(entity, new Rotation()
				{
					Value = new Quaternion()
					{
						eulerAngles = rand.NextFloat3(0, 360)
					}
				});
			}
		}).Schedule();
	}
	protected override void OnUpdate() { }
}