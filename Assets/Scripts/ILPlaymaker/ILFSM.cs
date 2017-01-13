using UnityEngine;
using System.Collections;

public class ILFSM : PlayMakerFSM 
{
	private bool disabledByUser = false;
	public void OnApplicationPause(bool pause)
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
		#endif

		if (pause) {
			disabledByUser = !enabled;
			enabled = false;
		} else {
			if (!disabledByUser)
				enabled = true;
		}
	}

	protected void SetupDone(){
		SetState("Setup");
	}
}
