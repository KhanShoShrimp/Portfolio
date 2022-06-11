using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WebCamDropbox : TMPro.TMP_Dropdown
{
	[SerializeField] RawImage m_RawImage;
	[SerializeField] Button m_Button;

	public WebCamTexture WebCamTexture;

	protected override void Awake()
	{
		base.Awake();

		Resize();

		onValueChanged.AddListener(x =>
		{
			if (WebCamTexture != null && WebCamTexture.isPlaying)
			{
				WebCamTexture.Stop();
			}

			if (x > 0)
			{
				WebCamTexture = new WebCamTexture(options[value].text);
				WebCamTexture.Play();

				m_RawImage.texture = WebCamTexture;
				m_Button.interactable = true;
			}
			else
			{
				m_RawImage.texture = null;
				m_Button.interactable = false;
			}

			Resize();
		});
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		options.Clear();
		options.Add(new OptionData("None"));
		for (int i = 0; i < WebCamTexture.devices.Length; i++)
		{
			options.Add(new OptionData(WebCamTexture.devices[i].name));
		}

		base.OnPointerClick(eventData);
	}

	private void Resize()
	{
		if (WebCamTexture == null || !WebCamTexture.isPlaying)
		{
			m_RawImage.rectTransform.sizeDelta = new Vector2(0, 0);
		}
		else
		{
			m_RawImage.rectTransform.sizeDelta = new Vector2(1000, 1000f * WebCamTexture.height / WebCamTexture.width);
		}
	}
}
