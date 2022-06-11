using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class Example : MonoBehaviour
{
	public SendWebCam Send = null;
	public RecvWebCam Recv = null;

	[SerializeField] RawImage RecvRawImage;

	[SerializeField] Button SendButton;
	[SerializeField] Button RecvButton;
	[SerializeField] WebCamDropbox Dropdown;

	private void Awake()
	{
		RecvRawImage.rectTransform.sizeDelta = new Vector2(0, 0);
	}

	public void StartSend()
	{
		SendButton.gameObject.SetActive(false);
		Dropdown.gameObject.SetActive(false);

		Send = new SendWebCam(Dropdown.WebCamTexture);
	}

	public void StartRecv()
	{
		RecvButton.gameObject.SetActive(false);

		Recv = new RecvWebCam(new byte[] { 127, 0, 0, 1 }, RecvRawImage);
	}
}
