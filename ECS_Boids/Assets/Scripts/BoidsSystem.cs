using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(SpawnSystem))]
public partial class BoidsSystem : SystemBase
{
	BoidsSettingComponent m_BoidsSetting;
	EntityQuery m_ObjectQuery;

	bool m_IsInit = false;
	NativeArray<Translation> m_Positions;
	NativeArray<Rotation> m_Rotations;

	protected override void OnStartRunning()
	{
		m_BoidsSetting = GetSingleton<BoidsSettingComponent>();
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
		float alignmentValue = m_BoidsSetting.Alignment;
		float cohesionValue = m_BoidsSetting.Coheshion;
		float separationValue = m_BoidsSetting.Separation;
		
		float radius = m_BoidsSetting.Radius;

		float moveSpeed = m_BoidsSetting.MoveSpeed;
		float rotateSpeed = m_BoidsSetting.RotateSpeed;

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
		float deltaTime = Time.DeltaTime;
		Entities
			.WithBurst()
			.WithReadOnly(positions)
			.WithReadOnly(rotations)
			.ForEach((ref Translation position, ref Rotation rotation, in ObjectTag objectTag) =>
		{
			float3 alignment = new float3(0, 0, 0);
			float3 coheshion = new float3(0, 0, 0);
			float3 separation = new float3(0, 0, 0);
			float3 force = float3.zero;

			var posEnumerator = positions.GetEnumerator();
			var rotEnumerator = rotations.GetEnumerator();

			int nearCount = 0;
			while (posEnumerator.MoveNext() & rotEnumerator.MoveNext())
			{
				float distance = math.distance(posEnumerator.Current.Value, position.Value);
				if (distance > 0 && distance < radius)
				{
					++nearCount;
					alignment += math.forward(rotEnumerator.Current.Value);
					coheshion += posEnumerator.Current.Value;
					separation += (position.Value - posEnumerator.Current.Value) / distance;
				}
			}
			posEnumerator.Dispose();
			rotEnumerator.Dispose();

			if (nearCount > 0)
			{
				alignment /= nearCount;
				coheshion /= nearCount;
				separation /= nearCount;

				coheshion -= position.Value;

				alignment *= alignmentValue;
				coheshion *= cohesionValue;
				separation *= separationValue;

				//rotation.Value = math.slerp(
				//	rotation.Value,
				//	quaternion.LookRotation(math.normalize(avgPosition - position.Value), objectTag.Diretion),
				//	deltaTime * cohesionValue);

				//rotation.Value.value = math.lerp(
				//	rotation.Value.value,
				//	avgRotation,
				//	deltaTime * alignmentValue);

				//position.Value += seperation * separationValue * moveSpeed * deltaTime * (1 - avgDistance);
			}
			else
			{
				rotation.Value = math.slerp(
						rotation.Value,
						quaternion.LookRotation(math.normalize(-position.Value), objectTag.Diretion),
						deltaTime * rotateSpeed);
			}
			var velocity = math.normalize(math.forward(rotation.Value) + (alignment + coheshion + separation));
			rotation.Value = quaternion.LookRotation(velocity, math.mul(rotation.Value, math.up()));
			position.Value = position.Value + velocity * moveSpeed * deltaTime;
		}).ScheduleParallel();
	}
}