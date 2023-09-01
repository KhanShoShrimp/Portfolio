using Khansho;
using UnityEngine;

public class WebCamSender : MonoBehaviour
{
	private const int SERVERPORT = 15000;
	private const int CLIENTPORT = 15001;

	private Server m_Server;

	private void Start()
	{
		Application.targetFrameRate = 30;
		m_Server = new Server(SERVERPORT, CLIENTPORT);
	}

	private void OnDestroy()
	{
		m_Server.Dispose();
	}
}