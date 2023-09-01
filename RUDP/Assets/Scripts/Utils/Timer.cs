using System;
using UnityEngine;

public class Timer
{
	private float m_Time;
	private float m_Target;

	public bool IsDone { get; private set; }
	public event Action Completed;

	public Timer(float target)
	{
		m_Target = target;
		
		Reset();

		EventDispatcher.UpdateEvent += Counting;
	}

	~Timer()
	{
		EventDispatcher.UpdateEvent -= Counting;
	}

	public void Reset()
	{
		m_Time = 0;
		IsDone = false;
	}

	public void Stop()
	{
		m_Time = 0;
		IsDone = true;
	}

	private void Counting()
	{
		if (IsDone)
		{
			return;
		}

		if (m_Time < m_Target)
		{
			m_Time += Time.deltaTime;
		}
		else
		{
			IsDone = true;
			Completed();
		}
	}
}
