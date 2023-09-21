using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Khansho.Example1
{
	public class ExampleRUDP : RUDP
	{
		public new event Action<Datagram> RecvCallBack = x => { };
		public new event Action<Datagram> SendCallBack = x => { };

		private readonly IntPtr m_StructPtr;

		public ExampleRUDP() : base(Datagram.SIZE)
		{
			m_StructPtr = GameUtils.GetIntPtr(Datagram.SIZE);

			base.RecvCallBack += Receive;
			base.SendCallBack += Sended;
		}

		private void Sended(Packet packet)
		{
			Datagram data = Marshal.PtrToStructure<Datagram>(packet.Ptr);
			SendCallBack(data);
		}

		private void Receive(Packet packet)
		{
			Datagram data = Marshal.PtrToStructure<Datagram>(packet.Ptr);
			RecvCallBack(data);
		}

		public void RequestSend(ref Datagram data, bool reliable = true)
		{
			int byteslength = data.Bytes == null ? 0 : data.Bytes.Length;
			RequestSend(ref data, Datagram.HEADLENGTH + byteslength, reliable);
		}

		public void RequestSend(ref Datagram data, int length, bool reliable = true)
		{
			Marshal.StructureToPtr(data, m_StructPtr, true);
			Send(m_StructPtr, length, reliable);
		}
	}
}