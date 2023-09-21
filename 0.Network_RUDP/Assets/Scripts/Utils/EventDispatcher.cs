using System;
using UnityEngine;

namespace Khansho
{
	public class EventDispatcher : MonoBehaviour
	{
		private static EventDispatcher s_Instance;
		public static EventDispatcher Instance
		{
			get
			{
				Initialize();
				return s_Instance;
			}
		}

		private event Action m_UpdateEvent = () => { };
		public static event Action UpdateEvent
		{
			add
			{
				Instance.m_UpdateEvent += value;
			}
			remove
			{
				Instance.m_UpdateEvent -= value;
			}
		}

		private event Action<float> m_CountingEvent = x => { };
		public static event Action<float> CountingEvent
		{
			add
			{
				Instance.m_CountingEvent += value;
			}
			remove
			{
				Instance.m_CountingEvent -= value;
			}
		}

		private event Action m_DestroyEvent = () => { };
		public static event Action DestroyEvent
		{
			add
			{
				Instance.m_DestroyEvent += value;
			}
			remove
			{
				Instance.m_DestroyEvent -= value;
			}
		}

		private static bool s_Initialized = false;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize()
		{
			if (s_Initialized)
			{
				return;
			}
			var gameobject = new GameObject("EventDispatcher");
			DontDestroyOnLoad(gameobject);
			s_Instance = gameobject.AddComponent<EventDispatcher>();
			s_Initialized = true;
		}

		private void Update()
		{
			m_UpdateEvent();
			m_CountingEvent(Time.deltaTime);
		}

		private void OnDestroy()
		{
			m_DestroyEvent();
		}
	}
}