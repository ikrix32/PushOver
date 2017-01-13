using UnityEngine;
using System.Collections;

public enum kPopupType {
	ALERT = 0,
	CONFIRM,
	ACTIVITY_INDICATOR,
	PROGRESS,
	ERROR,
	ERROR_NOTIFICATION,
	BACKGROUND_REFRESH,
	WEEKLY_PLAN,
	NATIVE,
	FACEBOOK,
	FIT_POINTS,
	MYSTERY_BOX_PORTRAIT,
	MYSTERY_BOX_LANDSCAPE,
	LEVEL_UP_NEW_MISSION,
	BIG_BUTTONS_1,
	BIG_BUTTONS_2,
	ACTIVITY_UPDATE,
	COUNT,
};

public class kPopup : kSpriteObject
{
	public System.Action onOK = null;
	public System.Action onCancel = null;
	public System.Action onClose = null;
	
	public System.Action onNativeCallback = null;
	
	public kPopupType type;
	//[HideInInspector]
	//public kSpriteObject background = null;
	//public kSpriteObject window = null;

	//used to touch outside(close the popup)
	public kObject popupBlackBackground = null;
	//public kView contentView = null;
	//public kSpriteObject scrollBar = null;
	public kPicture		popupPicture = null;
	public kPicture		popupBackground = null;
	
	public kTextField title = null;
	public kTextField content = null;
	public kProgressBar	progressBar = null;
	
	public kButton okButton;
	public kButton cancelButton;
	public kButton closeButton;

	public AudioClip m_sndShow;
	public AudioClip m_sndHide;

	[HideInInspector]
	public bool disableTapOutside = false;
	
	//private float scrollInset;
	private static int m_noActivePopup;

	protected override void onEnable() {
	}
	protected override void onDisable() {
	}

	public static void IncrementActivePopups() {
		m_noActivePopup ++;
		//Debug.LogError("m_noActivePopup =" + m_noActivePopup);
	}

	public static void DecrementActivePopups() {
		m_noActivePopup --;
		//Debug.LogError("m_noActivePopup =" + m_noActivePopup);
	}
	
	public static bool isAnyPopupActive(){
		return m_noActivePopup != 0;
	}

	/*
	protected override void onAppPause (bool pause)
	{
		base.onAppPause (pause);
		if (pause) {
			if (onClose != null) {
				onClose ();
			} else 
			if (onCancel != null) {
				onCancel ();
			}
			
			hide (0f, true);
		}
	}
	*/

	protected override void onInit()
	{
		//background = this;
		base.onInit();
		if (Application.isPlaying)
		{
			DontDestroyOnLoad (this);

			if (okButton != null)
				okButton.onRelease += () => {
					if (onOK != null)
						onOK.Invoke();
				};
			if (cancelButton != null)
				cancelButton.onRelease += () => {
					if (onCancel != null)
						onCancel.Invoke();
				};
			//the close button has no standalone functionality. 
			//It will act as a cancel ( or ok button if no cancel button is available)
			if (closeButton != null){
				closeButton.onRelease += () => {   
					if (onClose != null){
						onClose.Invoke();
					}
					else if (cancelButton != null){
							if (onCancel != null){
								onCancel.Invoke();
							}
					}
					else if (okButton != null){
							if (onOK != null){
								onOK.Invoke();
							}
					}
				};
			}

			if ( popupBlackBackground!= null ){
				kTouchable touchable = popupBlackBackground.GetComponent<kTouchable>();
				if (touchable == null)
					touchable = popupBlackBackground.gameObject.AddComponent<kTouchable>();
				
				touchable.onTouchRelease = (Touch touch) => {
					if (disableTapOutside)
						return;

					if (onClose != null){
						onClose.Invoke();
					}
					else if (cancelButton != null){
						if (onCancel != null)
							onCancel.Invoke();
					}
					else if (okButton != null){
						if (onOK != null){
							onOK.Invoke();
						}
					}
				};
			}
			//if (scrollBar != null)
			//	scrollInset = -scrollBar.gameObject.transform.localPosition.y;
			//gameObject.SetActive(false);
		}
	}
	
	/*protected override void onUpdate()
	{
		base.onUpdate();

		if (Application.isPlaying && type == kPopupType.LARGE)
		{
			kScrollableContainer container = contentView.GetComponentInChildren<kScrollableContainer>();
			float viewHeight = contentView.viewSize.y;
			float contentHeight = container.getContentSize().y;
			//if (contentHeight <= viewHeight)
			//	scrollBar.gameObject.transform.parent.gameObject.SetActive(false);

			float scrollBarBkgHeight = scrollBar.transform.parent.GetComponent<kSpriteObject>().getBounds().height - 2 * scrollInset;
			float scrollBarHeight = scrollBar.getBounds().height;
			Vector3 barPos = scrollBar.transform.localPosition;
			barPos.y = -scrollInset + (scrollBarHeight - scrollBarBkgHeight) * container.getScrollPos().y / (contentHeight - viewHeight);
			if (barPos.y > -scrollInset)
				barPos.y = -scrollInset;
			else if (barPos.y < scrollBarHeight - scrollBarBkgHeight - scrollInset)
				barPos.y = scrollBarHeight - scrollBarBkgHeight - scrollInset;
			scrollBar.transform.localPosition = barPos;
		}
	}*/

	public IEnumerator WaitAndFadeIn(float waitDuration)
	{
		iTween.FadeUpdate(gameObject, iTween.Hash("alpha", 0f, "time", 0f, "includechildren", true, "easeType", iTween.EaseType.linear));
		yield return new WaitForSeconds(waitDuration);
		if (this != null)
		{
			iTween.FadeTo(gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true, "easeType", iTween.EaseType.linear));
		}
	}
	
	public virtual void show(float animDuration)
	{
		gameObject.SetActive(true);

		StopCoroutine ("WaitAndFadeIn");
		iTween.FadeUpdate(gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true, "easeType", iTween.EaseType.linear));
		if (animDuration > 0) {
			iTween.FadeFrom(gameObject, iTween.Hash("alpha", 0f, "time", animDuration, "includechildren", true, "easeType", iTween.EaseType.linear));
			iTween.MoveFrom(gameObject, iTween.Hash("position", gameObject.transform.position + 300f * Vector3.down, "time", animDuration, "easeType", iTween.EaseType.easeOutCubic));
		}
	}

	private System.Action m_onHideComplete = null;
	public virtual void hide(float animDuration, bool destroyWhenAnimEnds, System.Action onHide = null)
	{
		m_onHideComplete = onHide;
		if (animDuration > 0) {
			iTween.FadeUpdate(gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true));
			iTween.FadeTo(gameObject, iTween.Hash("alpha", 0f, "time", animDuration, "includechildren", true, "easeType", iTween.EaseType.linear));
			if (destroyWhenAnimEnds) {
				iTween.MoveTo(gameObject, iTween.Hash("position", gameObject.transform.position + 300f * Vector3.down, "time", animDuration, "easeType", iTween.EaseType.easeOutCubic,
					"oncomplete", "destroy", "oncompletetarget", gameObject));
			} else {
				iTween.MoveTo(gameObject, iTween.Hash("position", gameObject.transform.position + 300f * Vector3.down, "time", animDuration, "easeType", iTween.EaseType.easeOutCubic,
					"oncomplete", "hideRecusively", "oncompletetarget", gameObject, "oncompleteparams", gameObject.transform.position));
			}
			playSound(m_sndHide);
		} else if(destroyWhenAnimEnds) {
			destroy();
		} else {
			hideRecusively(gameObject.transform.position);
		}
	}
	/*
	public void waitForOpComplete(AsyncOperation op, System.Action onComplete)
	{
		if (op == null) {
			if (onComplete != null)
				onComplete();
			return;
		}
		StartCoroutine(wait(op, onComplete));
	}
	
	protected IEnumerator wait(AsyncOperation op, System.Action onComplete)
	{
		yield return op;
		
		if (onComplete != null)
			onComplete();
	}
	*/
	public void hideRecusively(Vector3 resetPos)
	{
		gameObject.SetActive(false);
		gameObject.transform.position = resetPos;

		if (m_onHideComplete != null) {
			m_onHideComplete ();
		}
	}

	private void destroy()
	{
		destroy(this);

		if (m_onHideComplete != null) {
			m_onHideComplete ();
		}
	}

	public static void destroy(kPopup popup)
	{
		if (popup != null && popup.gameObject != null) {
#if !DISABLE_CONSOLE_LOG
			Debug.Log("Destroy callstack: \n" + UnityEngine.StackTraceUtility.ExtractStackTrace());
			Debug.Log("Destroy popup: " + popup.name);
#endif
			Destroy(popup.gameObject);
			DecrementActivePopups();
		}
	}

	public static void WaitThenDestroy(kPopup popup, float seconds)
	{
		if (popup != null) {
			popup.StartCoroutine(PopupDestroyWithDelay(popup, seconds));
		}
	}
	
	public void NativeAlertCallback()
	{
		if (onNativeCallback != null) {
			onNativeCallback();
		}
	}

	private static IEnumerator PopupDestroyWithDelay(kPopup popup, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		if (popup != null) {
			kPopup.destroy(popup);
		}
	}
}
