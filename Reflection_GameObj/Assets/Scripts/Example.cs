using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Reference] //Reference : 클래스를 인스턴스에 저장합니다.
public class Example : GameObj
{
	[SetDefault] //SetDefult : 인스펙터에서 컴포넌트의 기본값을 넣어줍니다.
	[SerializeField] SomeComponent Components1;
	[SetDefault] public SomeComponent Components2;
	[SetDefault] public SomeComponent[] Components3;
	[SetDefault] public List<SomeComponent> Components4;

	protected override void UpdateMethod() //UpdateMethod : 매 프레임마다 실행될 메소드입니다.
	{
		Debug.Log(Ref<Example>.Ins.Components2); //Ref<T>.Ins를 이용하여 인스턴스에 접근합니다.
	}
}
