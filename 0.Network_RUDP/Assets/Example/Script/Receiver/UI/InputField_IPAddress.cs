using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Ping = System.Net.NetworkInformation.Ping;

public class InputField_IPAddress : MonoBehaviour
{
	[SerializeField] private TMPro.TMP_InputField m_InputField;
	[SerializeField] private Button m_Button;
	[SerializeField] private UnityEvent<IPAddress> m_OnClick;

	public IPAddress m_IPAddress { get; private set; }
	private Ping m_Ping;

	private void Awake()
	{
		m_Ping = new Ping();
		m_InputField.onEndEdit.AddListener(x => 
		{
			m_Button.interactable = Ping(x);
		});
		m_Button.onClick.AddListener(() => m_OnClick.Invoke(m_IPAddress));
	}

	private void OnDestroy()
	{
		m_Ping?.Dispose();
		m_Ping = null;
	}

	private bool Ping(string ipaddress)
	{
		const int PINGLOOP = 5;

		if (IPAddress.TryParse(ipaddress, out IPAddress address))
		{
			for (int i = 0; i < PINGLOOP; i++)
			{
				var result = m_Ping.Send(ipaddress, 500);
				if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
				{
					m_IPAddress = address;
					return true;
				}
			}
		}
		return false;
	}
}
