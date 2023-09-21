using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Khansho.Example1.UI
{
	public class Text_IP : MonoBehaviour
	{
		[SerializeField] private TMPro.TMP_Text m_Text;

		private void Start()
		{
			m_Text.text = $"IP : {NetworkInfo.ExternalIPAddress}";
		}
	}
}