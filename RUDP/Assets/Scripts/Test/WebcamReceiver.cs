using Khansho;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebcamReceiver : MonoBehaviour
{
	private const int SERVERPORT = 15000;
	private const int CLIENTPORT = 15001;

	private Client m_Client;

	private void Start()
	{
		m_Client = new Client(CLIENTPORT, SERVERPORT);
	}

	private void OnDestroy()
	{
		m_Client.Dispose();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_Client.TryConnect(NetworkInfo.ExternalIPAddress);//IPAddress.Loopback);
		}
	}
}
