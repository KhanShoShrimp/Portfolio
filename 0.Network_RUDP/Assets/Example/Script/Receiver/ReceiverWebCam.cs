using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Khansho.Example1
{
	public class ReceiverWebCam : MonoBehaviour
	{
		[SerializeField] private RawImage m_RawImage;
		[SerializeField] private Text_FPS m_FPS; 
		private Texture2D m_Texture;

		private Color32[] m_Pixels;
		private GCHandle m_Handle;
		private int m_Offset;

		public IntPtr Ptr { get; private set; }
		public int Length { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		private void Awake()
		{
			m_Texture = new Texture2D(0, 0);
			m_RawImage.texture = m_Texture;
			Width = 0;
			Height = 0;
		}

		private void OnDestroy()
		{
			if (m_Handle.IsAllocated)
			{
				m_Handle.Free();
			}
		}

		public void Resize(int width, int height)
		{
			if (Width == width & Height == height)
			{
				return;
			}

			if (width == 0 && height == 0)
			{
				OnDestroy();
				return;
			}

			m_Texture.Reinitialize(width, height);

			Width = width;
			Height = height;
			Length = width * height * 4;

			m_Pixels = new Color32[width * height];
			if (m_Handle.IsAllocated)
			{
				m_Handle.Free();
			}
			m_Handle = GCHandle.Alloc(m_Pixels, GCHandleType.Pinned);
			Ptr = m_Handle.AddrOfPinnedObject();
		}

		public void ClearTexture()
		{
			m_Offset = 0;
			GameUtils.MemSet(Ptr, 0, Length);
		}

		public void WriteTexture(byte[] bytes)
		{
			int bufferlength = Datagram.BUFFERLENGTH;
			var next = m_Offset + bufferlength;

			if (next > Length)
			{
				bufferlength = Length - m_Offset;
				next = m_Offset + bufferlength;
			}

			bytes.CopyTo(Ptr + m_Offset, bufferlength);
			m_Offset = next;
		}

		public void UpdateTexture()
		{
			m_Texture.SetPixels32(m_Pixels, 0);
			m_Texture.Apply();
			m_RawImage.SetVerticesDirty();

			m_FPS.UpdateFPS();
		}
	}
}