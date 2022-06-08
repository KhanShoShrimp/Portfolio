using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WebCamDropbox : TMPro.TMP_Dropdown
{
	public override void OnPointerClick(PointerEventData eventData)
	{
		List<OptionData> optionlist = new List<OptionData>();
		optionlist.Add(new OptionData("None"));
		for (int i = 0; i < WebCamTexture.devices.Length; i++)
		{
			optionlist.Add(new OptionData(WebCamTexture.devices[i].name));
		}
		options = optionlist;

		base.OnPointerClick(eventData);
	}
}
