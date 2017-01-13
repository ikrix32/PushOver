using UnityEngine;
using System.Collections;

public class kSearchBox : kEditBox
{
	public System.Action<string> triggerSearchCallback = null;
	
//	public GameObject overlayWhenFocused = null;
	
	private const float SEARCH_TRIGGER_TIME = 0.5f;
	
	private float m_triggerTime = -1f;

	public kTextMesh m_txtNoResults;

	public System.Action onCloseCallback = null;
	
	protected override void onInit()
	{
		base.onInit();
		if (Application.isPlaying) {
			if (m_txtNoResults != null)
			{
				m_txtNoResults.setText("No Results.");
				m_txtNoResults.gameObject.SetActive(false);
			}
		}
	}
	
	protected override void onStart()
	{
		base.onStart();
		//removed in the new UI
//		overlayWhenFocused.SetActive(false);

		onDeleteEditing += () => {
			if (onCloseCallback != null) 
			{
				onCloseCallback();
			}
		};
	}
	
	protected override void onUpdate() 
	{	
		base.onUpdate();
		if (m_triggerTime > 0) {
			m_triggerTime -= Time.deltaTime;
			if (m_triggerTime < 0 && triggerSearchCallback != null)
				triggerSearchCallback(getText());
		}
		//removed in the new UI
//		if (Application.isPlaying && overlayWhenFocused != null) {
//			overlayWhenFocused.SetActive(isFocused() && getText().Length == 0);
//		}
	}
	
	protected override void setCursorText(string text)
	{	
		if (text != null && (text.Length > 0 || (text.Length == 0 && editText.Length > 0)))
			m_triggerTime = SEARCH_TRIGGER_TIME;
		base.setCursorText(text);
	}
	
	public void showNoResultsText(bool show)
	{
		if (m_txtNoResults != null)
			m_txtNoResults.gameObject.SetActive(show);
	}
}
