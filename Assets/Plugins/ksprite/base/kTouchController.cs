using UnityEngine;
using System;
using System.Collections;
using System.Reflection;


public enum kMouseState
{
	Up,
	Down,
	HeldDown
};

public struct kTouchMaker
{
	public static Touch createTouch( int finderId, int tapCount, Vector2 position, Vector2 deltaPos, float timeDelta, TouchPhase phase )
	{
		var self = new Touch();
		ValueType valueSelf = self;
		var type = typeof( Touch );
		
		type.GetField( "m_FingerId", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, finderId );
		type.GetField( "m_TapCount", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, tapCount );
		type.GetField( "m_Position", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, position );
		type.GetField( "m_PositionDelta", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, deltaPos );
		type.GetField( "m_TimeDelta", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, timeDelta );
		type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, phase );
		
		return (Touch)valueSelf;
	}
	
	
	public static Touch createTouchFromInput( kMouseState mouseState, ref Vector2? lastMousePosition )
	{
		var self = new Touch();
	
		ValueType valueSelf = self;
		var type = typeof( Touch );
		
		var currentMousePosition = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
		
		// if we have a lastMousePosition use it to get a delta
		if( lastMousePosition.HasValue )
			type.GetField( "m_PositionDelta", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, currentMousePosition - lastMousePosition );
		
		if( mouseState == kMouseState.Down) // equivalent to touchBegan
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Began );
			lastMousePosition = Input.mousePosition;
		}
		else if( mouseState == kMouseState.Up ) // equivalent to touchEnded
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Ended );
			lastMousePosition = null;
		}
		else // UIMouseState.HeldDown - equivalent to touchMoved/Stationary
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Moved );
			lastMousePosition = Input.mousePosition;
		}
		
		type.GetField( "m_Position", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, currentMousePosition );
		type.GetField( "m_TimeDelta", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, Time.deltaTime );
		
		return (Touch)valueSelf;
	}
}

public class kTouchController: kBehaviourScript
{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	[System.NonSerialized]
	private Vector2? lastMousePosition;
#endif
	private Vector3 lastWorldPos;
	private kTouchable lastTouchingObject = null;
	private kTouchable focusedObject = null;

#if ILDEBUG_CONSOLE
	public static System.Action consoleGestureCallback = null;
#endif

	public kCamera m_camera;

	//private Touch m_lastLouchInst;
	// Update is called once per frame
	protected override void onUpdate() {
		base.onUpdate();

		if(m_camera != null)
		{
			
			int tochCount = Input.touchCount;		
#if !USE_MULTITOUCH
			tochCount = Input.touchCount > 0? 1 : tochCount;
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
			tochCount = Input.touchCount <= 0? 1 : tochCount;
#endif
		
			
			for(int i = 0; i < tochCount; i++)
			{
				Touch touch;
				
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
				 touch = new Touch();
				bool rayCast = false;
				
				// check for each mouse state in turn, no elses here. They can all occur on the same frame!
				if( Input.GetMouseButtonDown( i ) ){
					touch =	kTouchMaker.createTouchFromInput( kMouseState.Down, ref lastMousePosition );
					rayCast = true;
				}else if( Input.GetMouseButton( i ) ){
					touch = kTouchMaker.createTouchFromInput( kMouseState.HeldDown, ref lastMousePosition );
					rayCast = true;
				}else if( Input.GetMouseButtonUp( i ) ){
					touch =	kTouchMaker.createTouchFromInput( kMouseState.Up, ref lastMousePosition );
					rayCast = true;
				}
				
				if(!rayCast)
					return;
#else 
				touch = Input.GetTouch(i);
#endif
				//if(touch.Equals(m_lastLouchInst)){
				//Already processed
				//	return;
				//}
				//m_lastLouchInst = touch;
				RaycastHit hit  = new RaycastHit();

				kTouchable touchObject = null;


				Vector2 hitPos = m_camera.RayCast(touch.position,out hit,float.PositiveInfinity);
				bool isHit = hit.collider != null;

				Vector3 touchWorldPos = m_camera.ToWorldPoint(hitPos,hit);
				Vector3 worldDeltaPos = touchWorldPos - lastWorldPos;
				lastWorldPos = touchWorldPos;

				if(lastTouchingObject == null && isHit){
					Touch 	currentTouch = kTouchMaker.createTouch(touch.fingerId,touch.tapCount,touchWorldPos,worldDeltaPos,touch.deltaTime,touch.phase);

					touchObject = hit.collider.GetComponent<kTouchable> ();
					
					if(touchObject != null){
						touchObject.touchUpdate(currentTouch);
					}
					lastTouchingObject = touchObject;
					if(currentTouch.phase == TouchPhase.Began)
					{
						if(focusedObject != null && focusedObject.gameObject != hit.collider.gameObject
						&& focusedObject.onFocusLost != null)
								focusedObject.onFocusLost();
						
						if(touchObject!= null && touchObject != focusedObject && touchObject.onFocus != null)
								touchObject.onFocus();
						focusedObject = touchObject;
					}
				}else{
					Touch 	currentTouch = kTouchMaker.createTouch(touch.fingerId,touch.tapCount,touchWorldPos,worldDeltaPos,touch.deltaTime,touch.phase);
					if(lastTouchingObject != null){
						if((currentTouch.phase != TouchPhase.Ended && lastTouchingObject.receiveTouchEventsAfterTouchEnd)
						|| (isHit && lastTouchingObject.gameObject == hit.collider.gameObject))
							lastTouchingObject.touchUpdate(currentTouch);
						else {
							Touch cancelTouch = kTouchMaker.createTouch(touch.fingerId, 0, touchWorldPos,worldDeltaPos,touch.deltaTime,TouchPhase.Canceled);
							lastTouchingObject.touchUpdate(cancelTouch);
							lastTouchingObject = null;
						}
					}
					
					if(currentTouch.phase == TouchPhase.Canceled || currentTouch.phase == TouchPhase.Ended)
						lastTouchingObject = null;
					
					if(touch.phase == TouchPhase.Began){
						if(focusedObject != null && focusedObject.onFocusLost != null){
							focusedObject.onFocusLost();
						}
						focusedObject = null;
					}
				}/*
				if (isHit)//Physics.Raycast (ray, out hit)) 
				{
					Touch 	currentTouch = kTouchMaker.createTouch(touch.fingerId,touch.tapCount,touchWorldPos,worldDeltaPos,touch.deltaTime,touch.phase);
					
					kTouchable touchObject = hit.collider.GetComponent<kTouchable>();
					
					if(lastTouchingObject != null && lastTouchingObject.gameObject != hit.collider.gameObject){
						Touch cancelTouch = kTouchMaker.createTouch(touch.fingerId, 0, touchWorldPos,worldDeltaPos,touch.deltaTime,TouchPhase.Canceled);
						
						lastTouchingObject.touchUpdate(cancelTouch);
					}
					if(touchObject != null){
						touchObject.touchUpdate(currentTouch);
					}
					if(currentTouch.phase == TouchPhase.Canceled || currentTouch.phase == TouchPhase.Ended)
						lastTouchingObject = null;
					else
						lastTouchingObject = touchObject;
					if(currentTouch.phase == TouchPhase.Began)
					{
						if(focusedObject != null && focusedObject.gameObject != hit.collider.gameObject
						&& focusedObject.onFocusLost != null)
								focusedObject.onFocusLost();
						
						if(touchObject != focusedObject && touchObject.onFocus != null)
								touchObject.onFocus();	
						focusedObject = touchObject;
					}
				}else{
					if(lastTouchingObject != null){
						Touch t = kTouchMaker.createTouch(	touch.fingerId, 0, touchWorldPos,worldDeltaPos,
															touch.deltaTime, TouchPhase.Canceled);
						
							lastTouchingObject.touchUpdate(t);
					}
					lastTouchingObject = null;
					if(touch.phase == TouchPhase.Began){
						if(focusedObject != null && focusedObject.onFocusLost != null){
							focusedObject.onFocusLost();
						}
						focusedObject = null;
					}
				}*/
			}
		}

#if DEBUG_ENABLE_CONSOLE
		for(int i = 0; i < Input.touchCount;i++)
		{
			Touch t = Input.GetTouch(i);
			if(t.tapCount >= 15){
#if DEBUG_ENABLE_CONSOLE
				if(consoleGestureCallback != null)
					consoleGestureCallback();
#endif
			}
		}
#endif
	}

	public void setFocusedObject(kTouchable obj){
		if(focusedObject != null && focusedObject != obj && focusedObject.onFocusLost != null)
			focusedObject.onFocusLost();
		if(obj != null && obj.onFocus != null)
			obj.onFocus();	
		focusedObject = obj;
	}
	
	public kTouchable getFocusedObject(){
		return focusedObject;
	}
};
