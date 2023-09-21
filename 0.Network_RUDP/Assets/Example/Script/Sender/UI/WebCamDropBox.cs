using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Khansho.Example1
{
	public class WebCamDropBox : MonoBehaviour
	{
		[SerializeField] private UnityEvent<string> m_OnWebcamChanged;
		[SerializeField] private TMPro.TMP_Dropdown m_Dropdown;
		[SerializeField] private Button m_Button;

		private int m_Index = -1;

		private void Awake()
		{
			m_Dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			UpdateDropdownList();
		}

		private void UpdateDropdownList()
		{
			var webcamlist = WebCamTexture.devices
				.Select(x => new TMPro.TMP_Dropdown.OptionData(x.name))
				.Prepend(new TMPro.TMP_Dropdown.OptionData("None"))
				.ToList();
			m_Dropdown.options = webcamlist;
		}

		private void OnDropdownValueChanged(int n)
		{
			if (n == m_Index)
			{
				return;
			}
			m_Index = n;

			m_Button.interactable = n > 0;
			if (n == 0)
			{
				m_OnWebcamChanged.Invoke(string.Empty);
			}
			else
			{
				var devicename = m_Dropdown.options[m_Dropdown.value].text;
				m_OnWebcamChanged.Invoke(devicename);
			}
		}
	}
}