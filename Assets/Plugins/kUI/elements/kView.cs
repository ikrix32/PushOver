using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class kView : kSpriteObject,ClipSource 
{
	public enum kScrollDirection {
		BACKWARD = -1,
		NO_MOVE = 0,//this actually signify that the scroll will stop immediatly(after bringBackToBounds) and no other snap to selection will be done.
		FORWARD = 1,
	}
	
	public float AUTO_SCROLL_MAX_VELOCITY = 2500f;
	public float AUTO_SCROLL_STOP_VELOCITY = 50f;
	public float AUTO_SCROLL_IMPULSE_AMPLIFIER = 1.0f;
	public float AUTO_SCROLL_DECELERATION_FACTOR = 2.6f;
	public float MAX_DRAG_PERCENT_FROM_SOURCE= 1.0f;

	public float SPRING_BACK_ELASTICITY = 1.0f;

	public bool 	clipChilds = true;
	public Vector2 	viewSize = Vector2.zero;
	
	public IntRect clipRect = RectUtil.RECT_NO_CLIP;
	
	private BoxCollider touchCollider = null;
	public 	Vector2		edgeInset = new Vector2(10, 10);
	
	public bool	 allowVerticalScroll = false;
	public bool	 allowHorizontalScroll = false;
	public bool  showScrollBar = false;
	private bool m_pagingEnable = false;
	private kScrollDirection m_scrollMoving;
	public kAlignMode alignMode = kAlignMode.TOP_LEFT;
	
	//todo kkk [HideInInspector]
	private kScrollableContainer m_scrollContainer = null;
	public kView	m_superClipSource;

	private bool m_layoutChanged = false;
	private bool m_isTransparent = false;

	//touch began
	private System.Action<Touch> m_onTouchBegan = null;
	//touch over the object
	private System.Action<Touch> m_onTouchDrag = null;
	//touch dragged out of the object bound
	private System.Action<Touch> m_onTouchEnd = null;
	//scroll ended
	public System.Action<kView> onScrollEnd = null;
	//snap to next item - used in corelation with paging
	public System.Action<kView, int> onSnapToSelection = null;
	
	public System.Action<Touch> onTouchBegan {
		get { return m_onTouchBegan;}
	}
	public System.Action<Touch> onTouchDrag {
		get { return m_onTouchDrag;}
	}
	public System.Action<Touch> onTouchEnd {
		get { return m_onTouchEnd;}
	}
	
	public void setPagingEnable(bool theValue)
	{
		m_pagingEnable = theValue;
	}
	
	public kScrollDirection scrollMoving {
		get {return m_scrollMoving;}
	}
	
	public void setScrollMoveFinished() {
		m_scrollMoving = kScrollDirection.NO_MOVE;
	}

	public void stopScrolling(){
		/*StopCoroutine("decelerate");
		StopCoroutine("springBackToBounds");*/
		StopAllCoroutines();
	}

	protected bool m_scrollBlocked = false;
	public void setScrollBlocked(bool blocked){
		m_scrollBlocked = blocked;
		if (blocked) {
			m_isDragging = false; 
			checkContainerOutOfBounds ();
		}
	}

	protected System.Action m_onScaleComplete;
	public void ScaleOut(System.Action onComplete = null)
	{
		m_onScaleComplete = onComplete;
		iTween.ScaleTo (gameObject,iTween.Hash("scale", Vector3.one *0.001f, "time", 0.3f,"includechildren",false,"onComplete","onScaleComplete","onCompleteTarget",gameObject));
	}

	public void ScaleIn(System.Action onComplete = null)
	{
		m_onScaleComplete = onComplete;
		iTween.ScaleTo (gameObject,iTween.Hash("scale", Vector3.one, "time", 0.3f,"includechildren",false,"onComplete","onScaleComplete","onCompleteTarget",gameObject));
	}

	private void onScaleComplete()
	{
		if (m_onScaleComplete != null) {
			m_onScaleComplete ();
		}
	}

	protected override void onInit ()
	{
		base.onInit();
		if(Application.isPlaying && (allowVerticalScroll || allowHorizontalScroll))
		{
			if(GetComponent<kScrollableContainer>() == null) {
				m_scrollContainer =  gameObject.AddComponent<kScrollableContainer>();
				m_scrollContainer.setShowScrollBar(showScrollBar);
			}
				
			
			if(GetComponent<kTouchable>() == null){
				gameObject.AddComponent<kTouchable>();
				//kBehaviourScript.initObjectSubTree(transform);
			}
		}
		m_clipSource = this;
		updateClipSourceOnSubTree(gameObject,m_clipSource);
		updateClip();
	}
	
	protected override void onStart(){
		base.onStart();
		
		if(objMesh.size == Vector2.one){
			objMesh.size 	= viewSize;
			objMesh.center.x = viewSize.x * 0.5f;
			objMesh.center.y = -viewSize.y * 0.5f;
		}

		if(Application.isPlaying && (allowVerticalScroll || allowHorizontalScroll))
		{
			m_scrollMoving = kScrollDirection.NO_MOVE;
			
			//ensure the view collider won't be disabled by clipping
			touchCollider = GetComponent<BoxCollider>();
			touchCollider.isTrigger = true;
			
			touchCollider.center= new Vector3(getBounds().center.x,getBounds().center.y,0);
			touchCollider.size 	= new Vector3(getBounds().width,getBounds().height, 1);
			
			if(m_onTouchBegan == null) m_onTouchBegan = defaultDragBeganImpl;
			if(m_onTouchDrag == null) m_onTouchDrag = defaultDragImpl;
			if(m_onTouchEnd == null) m_onTouchEnd = defaultDragEndImpl;
			
			kTouchable[] touchables = GetComponentsInChildren<kTouchable>();
			foreach (kTouchable touchable in touchables) {
				//skip cotainer object
				if(touchable.transform.parent != transform)
				{
	  				if(m_onTouchBegan != null){
						touchable.onTouchBegan	+= m_onTouchBegan;
						touchable.onTouchPressed+= m_onTouchBegan;
					}
					if(m_onTouchDrag != null){
						touchable.onTouching += m_onTouchDrag;
					}
					if(m_onTouchEnd != null){
						//if(touchable.gameObject == gameObject)
							touchable.onTouchEnd += m_onTouchEnd;
						touchable.onTouchRelease += m_onTouchEnd;
					}
				}
    		}
			//m_containerPosUpdate = true;
		}
	}
	
	protected override void onLateUpdate(){
		if(m_layoutChanged && m_scrollContainer != null){
			m_layoutChanged = false;
			checkContainerOutOfBounds();
		}
		if(m_transform.hasChanged || hasChangedOnLastFrame)
		{
			updateClip();
		}
		base.onLateUpdate();
	}
	
	protected override ClipSource getClipSource(){
		return this;
	}
	
	protected override void setClipSource(ClipSource source){
		if(m_superClipSource != source){
			m_superClipSource = (kView)source;
			clipChanged = true;
			updateClip();
		}
	}
	
	public kScrollableContainer getScrollContainer()
	{
		return m_scrollContainer;
	}
	
	public IntRect getClip(){
		return clipRect;
	}
	
	bool ignoreUpdateClip = false;
   	public void updateClip(){
		if (ignoreUpdateClip)
		{
			ignoreUpdateClip = false;
			return;
		}
		IntRect parentClip = m_superClipSource != null ? m_superClipSource.getClip() : RectUtil.RECT_NO_CLIP;
		
		if(viewSize.x > 0 && viewSize.y > 0 
		&& parentClip.width > 0 && parentClip.height > 0 && clipChilds){
			Vector2 vSize = Vector2.Scale(viewSize,transform.lossyScale);
			vSize = Quaternion.Inverse(transform.rotation) * vSize;
			IntRect viewPortWorldCoord = RectUtil.newRectangle((int)transform.position.x,(int)(transform.position.y - vSize.y),
																   (int)vSize.x ,(int)vSize.y);
				
			RectUtil.intersect(ref parentClip,ref viewPortWorldCoord,ref clipRect);
			if(clipRect.width <= 0 || clipRect.height <= 0)
				clipRect = RectUtil.RECT_CLIP_ZERO;
		}else if(viewSize.x <= 0 || viewSize.y <= 0 || !clipChilds){
			clipRect = parentClip;
		}else{
			Vector2 vSize = Vector2.Scale(viewSize,transform.lossyScale);
			vSize = Quaternion.Inverse(transform.rotation) * vSize;
			IntRect viewPortWorldCoord = RectUtil.newRectangle( (int)transform.position.x,(int)(transform.position.y - vSize.y),
																(int)vSize.x ,(int)vSize.y);
			clipRect = viewPortWorldCoord;
		}
		if (Application.isPlaying)
		{
			ignoreUpdateClip = true;
			BroadcastMessage("updateClip", true, SendMessageOptions.DontRequireReceiver);
			BroadcastMessage("updateObjectMesh", true, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	private bool isOutOfBouns(kScrollableContainer scrollObj,float pos,bool xDir){
		float minPos = xDir ? (viewSize.x - m_scrollContainer.getContentSize().x - edgeInset.x) : -edgeInset.y;
		float maxPos = xDir ? edgeInset.x : (-viewSize.y + m_scrollContainer.getContentSize().y + edgeInset.y);
		float targetPos = getContainerTargetPos(minPos,maxPos,pos,xDir);
		return (pos < minPos || pos > maxPos) && pos != targetPos;
	}
	
	private bool m_xOutOfBounds = false;
	private bool m_yOutOfBounds = false;
	private bool m_isDragging = false;

	public bool isDragging {
		get { return m_isDragging; }
	}
	
	//private Vector2 m_lastTouchDeltaPos;
	//private float	m_lastTouchDeltaTime;
	private Vector2 m_dragVelocity = Vector2.zero;
	
	private Vector2 m_dragStartPos = Vector2.zero;
	
	//default drag methods
	private void defaultDragBeganImpl(Touch t)
	{
		m_dragStartPos = t.position;
	}

	private void defaultDragImpl(Touch t)
	{
		if (!gameObject.activeInHierarchy || m_scrollBlocked)
			return;
		if (!m_isDragging && scrollMoving == kScrollDirection.NO_MOVE && (t.position - m_dragStartPos).sqrMagnitude < kTouchable.SQR_TAP_TOLERANCE)
			return;

		if (m_scrollContainer != null && m_scrollContainer.getContentSize().y > 0)
		{
			Vector2 scrollOffset = Vector2.zero;
			m_isDragging = true;
			if (allowHorizontalScroll)
			{
				m_scrollMoving = t.deltaPosition.x > 0? kScrollDirection.FORWARD : kScrollDirection.BACKWARD;
				
				float minX = viewSize.x - m_scrollContainer.getContentSize().x - edgeInset.x, maxX = edgeInset.x;
				float newLeft = m_scrollContainer.getScrollPos().x + t.deltaPosition.x;
				m_xOutOfBounds = isOutOfBouns(m_scrollContainer,newLeft,true);
				if (m_xOutOfBounds)
				{
					var distanceFromSource = (newLeft < minX) ? (minX - newLeft) : (newLeft - maxX);
					var percentFromSource = distanceFromSource / viewSize.x;
					percentFromSource = Mathf.Abs(percentFromSource);
					percentFromSource = percentFromSource > 1f? 0.9f : percentFromSource;

					if(percentFromSource < MAX_DRAG_PERCENT_FROM_SOURCE){
						scrollOffset.x = t.deltaPosition.x / Mathf.Pow(25f, percentFromSource);
					}else{
						scrollOffset = Vector2.zero;
					}
				}
				else
					scrollOffset.x = t.deltaPosition.x;
			}
			if (allowVerticalScroll)
			{
				m_scrollMoving = t.deltaPosition.y > 0? kScrollDirection.FORWARD : kScrollDirection.BACKWARD;
				
				float minY = -edgeInset.y, maxY = -viewSize.y + m_scrollContainer.getContentSize().y + edgeInset.y;
				float newTop = m_scrollContainer.getScrollPos().y + t.deltaPosition.y;
				m_yOutOfBounds = isOutOfBouns(m_scrollContainer,newTop,false);
				if (m_yOutOfBounds)
				{
					var distanceFromSource = (newTop < minY) ? (minY - newTop) : (newTop - maxY);
					var percentFromSource = distanceFromSource / viewSize.y;
					percentFromSource = Mathf.Abs(percentFromSource);
					percentFromSource = percentFromSource > 1f? 0.9f : percentFromSource;

					if(percentFromSource < MAX_DRAG_PERCENT_FROM_SOURCE)
					{
						scrollOffset.y = t.deltaPosition.y / Mathf.Pow(25f, percentFromSource);
					}
					else
					{
						scrollOffset = Vector2.zero;
					}
				}
				else
					scrollOffset.y = t.deltaPosition.y;
			}
			if(t.deltaTime != 0)
			{
				if(Mathf.Sign(m_dragVelocity.y) == Mathf.Sign(scrollOffset.y) 
				&& Mathf.Sign(m_dragVelocity.x) == Mathf.Sign(scrollOffset.x)){
					m_dragVelocity = (m_dragVelocity + scrollOffset / t.deltaTime) * 0.5f;
				}else
					m_dragVelocity = scrollOffset / t.deltaTime;
			}
			//m_lastTouchDeltaPos = t.deltaPosition;
			//m_lastTouchDeltaTime = t.deltaTime;
			//m_scrollContainer.scroll(m_dragVelocity * t.deltaTime);

			float viewScale = gameObject.transform.localScale.x;
			m_scrollContainer.scroll(scrollOffset/viewScale);
//			m_scrollContainer.scroll(scrollOffset);
		}
	}
	
	private void defaultDragEndImpl(Touch t)
	{
		if (!gameObject.activeInHierarchy || m_scrollBlocked)
			return;

		if (m_scrollContainer != null && m_scrollContainer.getContentSize().y > 0 && m_isDragging)
		{
			float velocity = 0f;

			m_isDragging = false;
			
			if (allowHorizontalScroll){
				//if(Mathf.Abs(m_dragVelocity.x * Time.smoothDeltaTime) >= (float)kTouchable.TAP_DRAG_TOLERANCE)
					velocity = m_dragVelocity.x;
				StartCoroutine(decelerate(velocity, true));
			}
			
			if (allowVerticalScroll)
			{
				//if(Mathf.Abs(m_dragVelocity.y * Time.smoothDeltaTime) >= (float)kTouchable.TAP_DRAG_TOLERANCE) 
					velocity = m_dragVelocity.y;
				
				StartCoroutine(decelerate(m_dragVelocity.y, false));
			}
		}
		m_dragVelocity = Vector2.zero;
	}

	public void markLayoutChanged()
	{
		m_layoutChanged = true;
		if (m_scrollContainer != null) {
			m_scrollContainer.updateContentBounds();
		}
	}

	private void checkContainerOutOfBounds()
	{
		if(m_scrollContainer != null){
			bool outOfBoundsX = isOutOfBouns(m_scrollContainer,m_scrollContainer.getScrollPos().x,true);
			bool outOfBoundsY = isOutOfBouns(m_scrollContainer,m_scrollContainer.getScrollPos().y,false);
		
			if (allowHorizontalScroll && outOfBoundsX)
				StartCoroutine(springBackToBounds(1.0f,true));
			else if(allowVerticalScroll && outOfBoundsY)
				StartCoroutine(springBackToBounds(1.0f,false));
		}
	}

	private float getContainerTargetPos(float minPos,float maxPos,float pos,bool xdir){
		float targetPos = pos <= minPos ? minPos : maxPos;
		if (xdir && m_scrollContainer.getContentSize().x < viewSize.x)
		{
			if (((int)alignMode & (int)kAlign.LEFT) != 0)
				targetPos = maxPos;
			else if (((int)alignMode & (int)kAlign.HCENTER) != 0)
				targetPos = (minPos + maxPos) / 2;
			else if (((int)alignMode & (int)kAlign.RIGHT) != 0)
				targetPos = minPos;
		}
		else if (!xdir && m_scrollContainer.getContentSize().y < viewSize.y)
		{
			if (((int)alignMode & (int)kAlign.TOP) != 0)
				targetPos = minPos;
			else if (((int)alignMode & (int)kAlign.VCENTER) != 0)
				targetPos = (minPos + maxPos) / 2;
			else if (((int)alignMode & (int)kAlign.BOTTOM) != 0)
				targetPos = maxPos;
		}
		return targetPos;
	}

	protected IEnumerator springBackToBounds(float elasticityModifier, bool xdir = false)
	{
		while (!m_isDragging)
		{ //recompute target position every frame because scroll position and size changes when adding or removing new objects
			float scrollPos = xdir ? m_scrollContainer.getScrollPos().x : m_scrollContainer.getScrollPos().y;
			float minPos = xdir ? (viewSize.x - m_scrollContainer.getContentSize().x - edgeInset.x) : -edgeInset.y;
			float maxPos = xdir ? edgeInset.x : (-viewSize.y + m_scrollContainer.getContentSize().y + edgeInset.y);

			float targetPos = getContainerTargetPos(minPos,maxPos,scrollPos,xdir);

			float distanceFromTarget = scrollPos - targetPos;
			float snapDistance = distanceFromTarget / 4f;
			bool isSnapped = Mathf.Abs(snapDistance) < 0.1f;
			snapDistance = isSnapped? -distanceFromTarget : -snapDistance;

			m_scrollContainer.scroll((xdir ? Vector3.right : Vector3.up) * snapDistance * SPRING_BACK_ELASTICITY);

			scrollPos = xdir ? m_scrollContainer.getScrollPos().x : m_scrollContainer.getScrollPos().y;
			if(!isOutOfBouns(m_scrollContainer,scrollPos,xdir))
			{
				if (onScrollEnd != null)
					onScrollEnd(this);
				break;
			}
			yield return null;
		}

		if (xdir)
			m_xOutOfBounds = false;
		else
			m_yOutOfBounds = false;
	}
	
	protected IEnumerator decelerate(float initVelocity, bool xdir = false)
	{
		// start decelerate on next frame to avoid drag interruptions - except when paging is enabled
		if (!m_pagingEnable && initVelocity != 0f)
			yield return null;

		initVelocity *= AUTO_SCROLL_IMPULSE_AMPLIFIER;

		float scrollPos = (xdir ? m_scrollContainer.getScrollPos().x : m_scrollContainer.getScrollPos().y);
		bool isOutOfBounds = isOutOfBouns(m_scrollContainer, scrollPos, xdir);
		if (isOutOfBounds)
		{
			StartCoroutine(springBackToBounds(3.0f, xdir));
			m_scrollMoving = kScrollDirection.NO_MOVE;
		}
		else if (m_pagingEnable && onSnapToSelection != null) 
		{
			int direction = Mathf.Abs(initVelocity) < 500f ? 0 : (int)Mathf.Sign(initVelocity);
			m_scrollMoving = initVelocity == 0 ? kScrollDirection.NO_MOVE : (initVelocity > 0 ? kScrollDirection.FORWARD : kScrollDirection.BACKWARD);
			onSnapToSelection(this, direction);
		}
		else
		{
			if (initVelocity < -AUTO_SCROLL_MAX_VELOCITY)
				initVelocity = -AUTO_SCROLL_MAX_VELOCITY;
			else if (initVelocity > AUTO_SCROLL_MAX_VELOCITY)
				initVelocity = AUTO_SCROLL_MAX_VELOCITY;

			m_scrollMoving =  initVelocity == 0 ? kScrollDirection.NO_MOVE : (initVelocity > 0 ? kScrollDirection.FORWARD : kScrollDirection.BACKWARD);

			float velocity = initVelocity;
			//float decelerationModifier = 0.94f;
			float elasticDecelerationModifier = 0.7f;

			while (!m_isDragging)
			{
				float offset = (velocity * Time.smoothDeltaTime);
				m_scrollContainer.scroll((xdir ? Vector3.right : Vector3.up) * offset);
				float newPos = (xdir ? m_scrollContainer.getScrollPos().x : m_scrollContainer.getScrollPos().y);
				
				isOutOfBounds = isOutOfBouns(m_scrollContainer,newPos,xdir);
				
				if (isOutOfBounds)
				{
					velocity *= elasticDecelerationModifier;
					elasticDecelerationModifier -= 0.1f;
					if (elasticDecelerationModifier <= 0)
						break;
				}
				else
				{
					velocity -= (velocity * AUTO_SCROLL_DECELERATION_FACTOR * Time.smoothDeltaTime);
					if (Mathf.Abs(velocity) < AUTO_SCROLL_STOP_VELOCITY)
						break;
				}
				yield return null;
			}
			m_scrollMoving = kScrollDirection.NO_MOVE;
			if (isOutOfBounds)
			{
				if (xdir)
				{
					m_xOutOfBounds = true;
					StartCoroutine(springBackToBounds(1.5f, true));
				}
				else
				{
					m_yOutOfBounds = true;
					StartCoroutine(springBackToBounds(1.5f, false));
				}
			}
			else if (onScrollEnd != null)
				onScrollEnd(this);
		}
	}

	public override Rect getBoundsWorld(){
		Vector2 center = new Vector2(viewSize.x/2,-viewSize.y/2);
		center = Vector2.Scale(center,transform.lossyScale);
		Vector2 size   = Vector2.Scale(viewSize,transform.lossyScale);
		
		center = center + (Vector2)transform.position;
		return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
	}
	
//	public override Rect getClippedBoundsWorld(){
//		return getBoundsWorld();
//	}
	
	public override Rect getBounds(){
		Vector2 center = new Vector2(viewSize.x/2,-viewSize.y/2);
		center = Vector2.Scale(center,transform.localScale);
		Vector2 size   = Vector2.Scale(viewSize,transform.localScale);
		
		return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
	}
	
//	public override Rect getClippedBounds(){
//		return getBounds();
//	}

	public override bool isReceivingTouchEventsAfterLoosingFocus(){
		return true;
	}

	public void DragView(Touch touch)
	{
		defaultDragImpl(touch);
	}

	public void setIsFaded(bool faded){
		m_isTransparent = faded;
	}

	public bool IsFaded(){
		return m_isTransparent;
	}

#if UNITY_EDITOR	
	void OnDrawGizmos(){
		Vector2 vSize = Vector2.Scale(viewSize,transform.lossyScale);
		vSize = Quaternion.Inverse(transform.rotation) * vSize;
		Rect viewPortWorldCoord = RectUtil.newRectangle(transform.position.x, transform.position.y - vSize.y,vSize.x ,vSize.y);
		
		if(viewPortWorldCoord.width != 0 && viewPortWorldCoord.height != 0){
			Vector3 center = new Vector3(viewPortWorldCoord.x + viewPortWorldCoord.width/2,viewPortWorldCoord.y + viewPortWorldCoord.height/2,0);
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(center,new Vector3(viewPortWorldCoord.width,viewPortWorldCoord.height,2));
		}
	}
#endif
}
