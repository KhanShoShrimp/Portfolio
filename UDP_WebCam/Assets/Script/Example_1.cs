using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class Example_1 : MonoBehaviour
{
	public SendWebCam Send = null;
	public RecvWebCam Recv = null;

	[SerializeField] RawImage SendRawImage;
	[SerializeField] RawImage RecvRawImage;

	[SerializeField] Button SendButton;
	[SerializeField] Button RecvButton;
	[SerializeField] WebCamDropbox Dropdown;

	public void StartSend()
	{
		if (Dropdown.value == 0)
		{
			Debug.Log("WebCam이 선택되지 않았습니다.");
			return;
		}
		Dropdown.gameObject.SetActive(false);
		SendButton.gameObject.SetActive(false);

		Send = new SendWebCam(SendRawImage);
	}

	public void StartRecv()
	{
		RecvButton.gameObject.SetActive(false);

		Recv = new RecvWebCam(RecvRawImage, "127.0.0.1");
	}
}
