using UnityEngine;
using System.Collections;

[System.Serializable]
public class kPlayMakerEvent{
	public PlayMakerFSM m_controller;
	public string m_eventName;

	public void SendEvent()
	{
		if(m_controller != null && !string.IsNullOrEmpty(m_eventName))
			m_controller.SendEvent(m_eventName);
	}
}
