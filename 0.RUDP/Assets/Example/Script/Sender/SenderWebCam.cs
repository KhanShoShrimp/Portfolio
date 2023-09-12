using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Khansho.Example1
{
    public class SenderWebCam : MonoBehaviour
    {
		[SerializeField] private RawImage m_RawImage;
		
		private WebCamTexture m_WebCamTexture;
		private Color32[] m_Colors;
		private GCHandle m_Handle;

		public IntPtr Ptr { get; private set; }
		public int Length { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		private void OnDestroy()
		{
			if (m_Handle.IsAllocated)
			{
				m_Handle.Free();
			}
			m_WebCamTexture?.Stop();
			m_WebCamTexture = null;
			m_RawImage.texture = null;
		}

		public void OnWebCamChanged(string devicename)
		{
			OnDestroy();

			if (!string.IsNullOrWhiteSpace(devicename))
			{
				const int REQUESTWIDTH = 160;
				const int REQUESTHEIGHT = 90;

				m_WebCamTexture = new WebCamTexture(devicename, REQUESTWIDTH, REQUESTHEIGHT);
				m_WebCamTexture.Play();
				m_RawImage.texture = m_WebCamTexture;

				m_Colors = m_WebCamTexture.GetPixels32();
				m_Handle = GCHandle.Alloc(m_Colors, GCHandleType.Pinned);

				Ptr = m_Handle.AddrOfPinnedObject();
				Length = m_Colors.Length * 4;
				Width = m_WebCamTexture.width;
				Height = m_WebCamTexture.height;
			}
		}

		public bool TryUpdateBytes()
		{
			if (m_WebCamTexture == null)
			{
				return false;
			}
			else
			{
				m_WebCamTexture.GetPixels32(m_Colors);
				return true;
			}
		}
	}
}