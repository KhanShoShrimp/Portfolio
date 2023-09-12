using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Khansho.Example1
{
	public enum DataType : byte
	{
		None,
		BeginStream,
		DataStream,
		EndStream,
		TryConnect,
		Accept,
		Block
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Datagram
	{
		public const int MAXSIZE = 65507;
		public const int NORMALSIZE = 1024;
		public const int SIZE = MAXSIZE - Packet.HEAD_SIZE;
		public const int HEADLENGTH = 1;
		public const int BUFFERLENGTH = SIZE - HEADLENGTH;

		public DataType DataType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = BUFFERLENGTH)]
		public byte[] Bytes;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DataType : ");
			sb.Append(DataType);
			sb.Append(", ");
			sb.Append("Values : ");
			if (Bytes == null)
			{
				sb.Append("null");
			}
			else
			{
				sb.Append(Bytes.ToStr());
			}
			return sb.ToString();
		}
	}
}