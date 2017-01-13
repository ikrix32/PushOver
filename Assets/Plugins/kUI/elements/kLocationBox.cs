using UnityEngine;

public class kLocationBox : kEditBox
{
	public bool m_autoSetToCurrentLocation = false;
	
	protected override void onStart()
	{
		base.onStart();
		gameObject.name = "locationEdit";

		if(Application.isPlaying && m_autoSetToCurrentLocation)
			updateGeoLocation();
	}

	public void updateGeoLocation(System.Action<string> onSuccess = null, System.Action<string> onFail = null) 
	{
		/*GeoLocationController.GetGeoLocation((string location)=>{
			setText(location);
			if ( onSuccess != null )
				onSuccess(location);
		}, onFail);*/
		if(onSuccess != null)
			onSuccess("fix me");
	}	
}

