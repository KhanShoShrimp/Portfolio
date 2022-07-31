using UnityEngine;

public class CameraMove : MonoBehaviour
{
	private readonly Vector3 m_Point = new Vector3(125, 0, 125);
	private readonly Vector3 m_Axis = new Vector3(0, 1, 0);

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Update()
	{
		transform.RotateAround(m_Point, m_Axis, Time.deltaTime * 45);
		transform.LookAt(m_Point);
	}
}
