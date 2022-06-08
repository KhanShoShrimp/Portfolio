using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Reference] //Reference : 클래스를 정적 변수에 저장합니다.
public class Example : GameObj
{
	//SetDefault : 인스펙터에 컴포넌트를 미리 채워줍니다.
	[SetDefault] public SomeComponent Components1;
	[SetDefault] [SerializeField] SomeComponent Components2;
	[SetDefault(getChild = true)] public SomeComponent[] Components3;
	[SetDefault(getChild = false)] public List<SomeComponent> Components4;

	protected override void UpdateMethod() //UpdateMethod : 매 프레임마다 실행될 메소드입니다.
	{
		Debug.Log(Ref<Example>.Ins.Components2); //Ref<T>.Ins를 이용하여 인스턴스에 접근합니다.
	}
}
