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

	public float Range;
	public float LimitRotate;
	public float RotateSpeed;
	public float MoveSpeed;
}