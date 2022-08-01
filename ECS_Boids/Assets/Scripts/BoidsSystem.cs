using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(SpawnSystem))]
public partial class BoidsSystem : SystemBase
{
	//BoidsSettingComponent m_BoidsSetting;
	EntityQuery m_ObjectQuery;

	bool m_IsInit = false;
	NativeArray<Translation> m_Positions;
	NativeArray<Rotation> m_Rotations;

	protected override void OnStartRunning()
	{
		//m_BoidsSetting = GetSingleton<BoidsSettingComponent>();
		m_ObjectQuery = GetEntityQuery(typeof(ObjectTag), typeof(Translation), typeof(Rotation));
	}

	protected override void OnStopRunning()
	{
		if (m_Positions.IsCreated)
		{
			m_Positions.Dispose();
		}
		if (m_Rotations.IsCreated)
		{
			m_Rotations.Dispose();
		}
	}

	protected override void OnUpdate()
	{
		RequireSingletonForUpdate<BoidsSettingComponent>();
		BoidsSettingComponent setting = GetSingleton<BoidsSettingComponent>();
		float alignmentValue = setting.Alignment;
		float cohesionValue = setting.Coheshion;
		float separationValue = setting.Separation;
		float radius = setting.Range;
		float limitRotate = setting.LimitRotate;
		float moveSpeed = setting.MoveSpeed;
		float rotateSpeed = setting.RotateSpeed;
		float deltaTime = Time.DeltaTime;


		int count = m_ObjectQuery.CalculateEntityCount();
		if (!m_IsInit)
		{
			if (count <= 0)
			{
				return;
			}
			m_IsInit = true;
			m_Positions = m_ObjectQuery.ToComponentDataArray<Translation>(Allocator.Persistent);
			m_Rotations = m_ObjectQuery.ToComponentDataArray<Rotation>(Allocator.Persistent);
		}

		var positions = m_Positions;
		var rotations = m_Rotations;

		Entities
			.WithBurst()
			.WithNativeDisableParallelForRestriction(positions)
			.WithNativeDisableParallelForRestriction(rotations)
			.ForEach((ref Translation position, ref Rotation rotation, in ObjectTag objectTag) =>
			{
				float3 alignment = new float3(0, 0, 0);
				float3 coheshion = new float3(0, 0, 0);
				float3 separation = new float3(0, 0, 0);
				float3 force = float3.zero;

				int nearCount = 0;

				for (int i = 0; i < count; i++)
				{
					float distance = math.distancesq(positions[i].Value, position.Value);
					if (distance > 0 && distance < radius)
					{
						++nearCount;
						alignment += math.forward(rotations[i].Value);
						coheshion += positions[i].Value;
						separation += (position.Value - positions[i].Value) / distance;
					}
				}

				//var posEnumerator = positions.GetEnumerator();
				//var rotEnumerator = rotations.GetEnumerator();
				//
				//while (posEnumerator.MoveNext() & rotEnumerator.MoveNext())
				//{
				//	float distance = math.distancesq(posEnumerator.Current.Value, position.Value);
				//	if (distance > 0 && distance < radius)
				//	{
				//		++nearCount;
				//		alignment += math.forward(rotEnumerator.Current.Value);
				//		coheshion += posEnumerator.Current.Value;
				//		separation += (position.Value - posEnumerator.Current.Value);
				//	}
				//}
				//
				//posEnumerator.Dispose();
				//rotEnumerator.Dispose();

				var rot = rotation.Value;

				rotation.Value = math.slerp(
						rotation.Value,
						quaternion.LookRotation(math.normalize(-position.Value), math.forward(rot)/*objectTag.Diretion*/),
						rotateSpeed * deltaTime);

				if (nearCount > 0)
				{
					alignment = math.normalize(alignment / nearCount) * alignmentValue;
					coheshion = math.normalize((coheshion - position.Value) / nearCount) * cohesionValue;
					separation = math.normalize(separation / nearCount) * separationValue;

					//rotation.Value = math.slerp(
					//	rotation.Value,
					//	quaternion.LookRotation(math.normalize(avgPosition - position.Value), objectTag.Diretion),
					//	deltaTime * cohesionValue);

					//rotation.Value.value = math.lerp(
					//	rotation.Value.value,
					//	avgRotation,
					//	deltaTime * alignmentValue);

					//position.Value += seperation * separationValue * moveSpeed * deltaTime * (1 - avgDistance);

					float3 value = alignment + separation;
					if (math.lengthsq(value) != 0)
					{
						rotation.Value = Quaternion.RotateTowards(
							rotation.Value,
							quaternion.LookRotation(position.Value + alignment + separation, math.mul(rot, math.up())),
							limitRotate * deltaTime);
					}
				}

				var forward = math.forward(rotation.Value);

				if (math.lengthsq(coheshion) != 0)
				{
					position.Value += math.normalize(coheshion) * math.min(math.lengthsq(coheshion), moveSpeed * 0.5f) * deltaTime;
				}
				position.Value += math.forward(rotation.Value) * moveSpeed * deltaTime;
			}).ScheduleParallel();
	}
}