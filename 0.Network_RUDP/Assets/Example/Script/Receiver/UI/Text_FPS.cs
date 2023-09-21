using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Khansho.Example1
{
	public class Text_FPS : MonoBehaviour
	{
		[SerializeField] private TMPro.TMP_Text m_FPS;
		private StringBuilder m_FPS_SB;
		long m_LastRecvTick;

		private void Start()
		{
			m_FPS_SB = new StringBuilder(24);
			m_LastRecvTick = DateTime.Now.Ticks;
		}

		public void UpdateFPS()
		{
			var sec = DateTime.Now.Ticks - m_LastRecvTick;
			m_FPS_SB.Clear();
			m_FPS_SB.Append(10000000 / sec);
			m_FPS_SB.Append("fps");
			m_FPS.SetText(m_FPS_SB);
			m_LastRecvTick = DateTime.Now.Ticks;
		}
	}
}