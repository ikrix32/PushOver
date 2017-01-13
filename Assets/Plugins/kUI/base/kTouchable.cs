using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class kTouchable: kBehaviourScript
{
	public static bool 	TOUCH_ENABLED = true;
	private const float	TIMEOUT_TOUCH_INPUT_DISABLED = 3;//seconds
	private static System.DateTime touchDisableTimer = System.DateTime.MinValue;
	public static System.Action<GameObject> objectTouchedCallback = null;
	public static System.Action<GameObject> objectReleasedCallback = null;

	public const int TAP_DRAG_TOLERANCE = 15;//pixels
	public const int SQR_TAP_TOLERANCE = TAP_DRAG_TOLERANCE * TAP_DRAG_TOLERANCE;
	public const float LONG_PRESS_TIME = 0.8f;//seconds
	public const int DEFAULT_SIZE_INC  = 100;

	public bool m_enableSizeIncrease = true;
	public bool m_forwardTouchToParent = true;
	//touch pressed on object
	public System.Action<Touch> onTouchPressed = null;
	//touch dragged over the object -- TODO
	public System.Action<Touch> onTouchBegan = null;
	//touch over the object
	public System.Action<Touch> onTouching = null;
	//touch dragged out of the object bound
	public System.Action<Touch> onTouchEnd = null;
	//touch released while still over the object
	public System.Action<Touch> onTouchRelease = null;
	//tap on item (press & release ) 
	public System.Action<Touch> onTouchTap = null;
	//touch pinched to zoom in
	public System.Action<float> onTouchPinchZoomIn = null;	
	//touch pinched to zoom in
	public System.Action<float> onTouchPinchZoomOut = null;
	//touch swipe
	public System.Action<Vector2> onTouchSwipe = null;
	//touch double tap
	public System.Action onDoubleTap = null;
	//focus callbacks
	public System.Action onFocus 	= null;
	public System.Action onFocusLost= null;
	
	public AudioClip m_soundOnPress = null;
	public AudioClip m_soundOnTap = null;
	
	TouchPhase objState = TouchPhase.Ended;

	//public kFSMEvent[] m_onTouchTapEvents;
	public kPlayMakerEvent m_playmakerTouchPressEvent;
	public kPlayMakerEvent m_playmakerTouchReleaseEvent;
	public kPlayMakerEvent m_playmakerTouchTapEvent;

	private Vector2 m_pressPos = Vector2.zero;
	private bool m_wasPressed = false;
	private bool m_isTap = false;
	
	private BoxCollider m_collider;

	private bool m_receiveTouchEventsAfterTouchEnd = true;
	public bool receiveTouchEventsAfterTouchEnd {
		get{ return m_receiveTouchEventsAfterTouchEnd;}
	}

	private bool m_playPressSounds = true;
	public bool setPlayPressSounds 
	{
		get
		{
			return m_playPressSounds;
		}
		set
		{
			m_playPressSounds = value;
		}
	}
	
	protected override void onInit(){
		base.onInit();
		if((m_collider = GetComponent<BoxCollider>()) == null){
			m_collider = (BoxCollider)gameObject.AddComponent<BoxCollider>();
			m_collider.isTrigger = true;
		}
	}
	
	protected override void onStart(){
		base.onStart();
		updateTouchArea();
	}

	protected override void onAppPause (bool pause)
	{
		base.onAppPause (pause);
		if (pause) {
			Touch cancelTouch = kTouchMaker.createTouch(0, 0, Vector3.zero,Vector3.zero,0f,TouchPhase.Canceled);
			touchUpdate(cancelTouch);
		}
	}
	
	// Update is called once per frame
	protected override void onUpdate(){
		base.onUpdate();
		//updateTouchArea();
		if(!TOUCH_ENABLED){
			//avoid block touch input forever
			if(touchDisableTimer == System.DateTime.MinValue){
				touchDisableTimer = System.DateTime.Now;
				//Debug.Log(" * Touch input disabled!");
			}else{
				System.TimeSpan diff = System.DateTime.Now - touchDisableTimer;
				if(diff.TotalSeconds > TIMEOUT_TOUCH_INPUT_DISABLED){
					Debug.LogError(" * Touch input enabled on time out!");
					touchDisableTimer = System.DateTime.MinValue;
					TOUCH_ENABLED = true;
					iTween.CancelAllTouchBlockingTweens();
				}
			}
		}else{
			touchDisableTimer = System.DateTime.MinValue;
		}
	}
	
	public virtual void updateTouchArea(){
		kObject 	obj = null;
		if(enabled && m_collider != null && (obj = GetComponent<kObject>()) != null){
			Rect meshBounds = obj.getClippedBounds();
			m_receiveTouchEventsAfterTouchEnd = obj.isReceivingTouchEventsAfterLoosingFocus();
			if(meshBounds.width != 0 && meshBounds.height != 0 && gameObject.GetComponent<Renderer>().enabled){
				m_collider.enabled = true;
				m_collider.isTrigger = true;
				Vector2 invLocalScale = new Vector2(1/transform.localScale.x,1/transform.localScale.y);
				Vector3 newCenter =(Vector3)Vector2.Scale(meshBounds.center,invLocalScale);

				int sizeInc = ((m_enableSizeIncrease) && (objState == TouchPhase.Began || objState == TouchPhase.Moved)) ? DEFAULT_SIZE_INC:0;
				if(objState == TouchPhase.Began || objState == TouchPhase.Moved || objState == TouchPhase.Stationary)
					sizeInc += obj.getSizeIncreaseForTouch();

				Vector3 newSize = new Vector3(( Mathf.Abs(meshBounds.width) + sizeInc) * invLocalScale.x,(Mathf.Abs(meshBounds.height) + sizeInc) * invLocalScale.y, 1);
				if((m_collider.center - newCenter).magnitude > 9 || (m_collider.size - newSize).magnitude > 20)
				{
					m_collider.center = newCenter;
					m_collider.size = newSize;
				}
			}else{
				m_collider.enabled = false;
			}
		}
	}
	
	public void setFocused(){
		kScreen.instance.kTouchController.setFocusedObject(this);
	}
	
	public bool isFocused(){
		return kScreen.instance.kTouchController.getFocusedObject() == this;
	}

	public void enableTouch(bool enable){
		enabled = enable;
		m_collider.enabled = enable;
		updateTouchArea ();
	}
	
	public void touchUpdate(Touch touch){
		if (/*!TOUCH_ENABLED || */!enabled) {
			return;
		}
		bool shouldUpdateTouchArea = false;
		//if(touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Stationary)
		//	Debug.LogError("Touch update: " + name + ", id = " + touch.fingerId + ", phase = " + touch.phase);
		switch (touch.phase){
			case TouchPhase.Began:{
				if (TOUCH_ENABLED) {
					m_isTap = true;
					m_wasPressed = true;
					m_pressPos = touch.position;
					if (onTouchPressed != null) {
						if (objectTouchedCallback != null)
							objectTouchedCallback(gameObject);
						onTouchPressed(touch);
					}

					if(m_playmakerTouchPressEvent != null){
						m_playmakerTouchPressEvent.SendEvent();
					}

					if (m_soundOnPress != null && m_playPressSounds) {
						PlayOneShotSound(m_soundOnPress);
					}

					if(touch.tapCount == 2 && onDoubleTap != null)
					{
						onDoubleTap();
					}
				}
				shouldUpdateTouchArea = true;
			}break;
			case TouchPhase.Stationary:
			case TouchPhase.Moved:
			{
				if (TOUCH_ENABLED) {
					if (m_isTap && (touch.position - m_pressPos).sqrMagnitude > SQR_TAP_TOLERANCE) {
						m_isTap = false;
						if ( onTouchSwipe != null )
							onTouchSwipe(touch.position - m_pressPos);
					}
					if (objState != TouchPhase.Began && objState != touch.phase) {
						//Debug.LogError("Touch update: " + name + ", id = " + touch.fingerId + ", phase = " + TouchPhase.Began);
						if (onTouchBegan != null)
							onTouchBegan(touch);
					} else {
						if (onTouching != null)
						{
							onTouching(touch);
						}

//						if(Input.touchCount == 2)
//						{
//							pinchZoom();
//						}	
					}
				}
				if (objState != TouchPhase.Began && objState != touch.phase) {
					shouldUpdateTouchArea = true;
				}
			}break;
			case TouchPhase.Canceled:{
				if (TOUCH_ENABLED) {
					m_isTap = false;
					m_wasPressed = false;
					if (onTouchEnd != null)
						onTouchEnd(touch);
				}
				shouldUpdateTouchArea = true;	
			}break;
			case TouchPhase.Ended:{
				if (TOUCH_ENABLED) {
					if (m_wasPressed && objectReleasedCallback != null)
						objectReleasedCallback(gameObject);
					if (onTouchRelease != null) {
						onTouchRelease(touch);
					}

					if(m_playmakerTouchReleaseEvent != null){
						m_playmakerTouchReleaseEvent.SendEvent();
					}

					if (m_isTap) {
						if (onTouchTap != null)
							onTouchTap(touch);
						
						if(m_playmakerTouchTapEvent != null){
							m_playmakerTouchTapEvent.SendEvent();
						}

						if (m_soundOnTap != null && m_playPressSounds){
							PlayOneShotSound(m_soundOnTap);
						}
					}
					m_wasPressed = false;
				}
				shouldUpdateTouchArea = true;
			}break;
		}
		objState = touch.phase;
		if (shouldUpdateTouchArea) {
			//increase bounding box of the touched object or decrease it when losing focus
			updateTouchArea();
		}
	}
	public void cancelTap(){
		m_isTap = false;
	}

	public BoxCollider getCollider(){
		return m_collider;
	}

	private void PlayOneShotSound(AudioClip clip){
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		audioSource.spatialBlend = 0;
		audioSource.PlayOneShot(clip, kScreen.FX_VOLUME);
	}

//	private void pinchZoom()
//	{
//		Touch touchZero = Input.GetTouch(0);
//		Touch touchOne = Input.GetTouch(1);
//		
//		//find the position in the previous frame of each touch.
//		Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
//		Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
//		
//		//find the magnitude of the vector (the distance) between the touches in each frame.
//		float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
//		float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
//		
//		//find the difference in the distances between each frame.
//		float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
//		Debug.Log("deltaMagnitudeDiff ============" + deltaMagnitudeDiff);
//
//		if(deltaMagnitudeDiff > 0)
//		{
//			if(onTouchPinchZoomOut != null)
//			{
//				onTouchPinchZoomOut(deltaMagnitudeDiff);
//			};
//		}
//		else
//		{
//			if(onTouchPinchZoomIn != null)
//			{
//				onTouchPinchZoomIn(deltaMagnitudeDiff);
//			}
//		}
//	}
}
