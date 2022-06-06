using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System.Diagnostics;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BoidsSettingComponent : IComponentData
{
	public float Coheshion;
	public float Alignment;
	public float Separation;

	public float Radius;

	public float MoveSpeed;
	public float RotateSpeed;
}