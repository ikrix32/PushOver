using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class kScrollableContainer : kBehaviourScript 
{
#if UNITY_EDITOR	
	[System.NonSerialized]
	public 	bool		debug = true;
#endif	
	
	//Scroll variables
	protected kContainer container = null;
	[HideInInspector]
	public GameObject scrollContainer = null;
	
	protected override void onInit(){
		base.onInit();
		if(container == null){
			container = new kContainer(gameObject);	
			scrollContainer = container.gameObject;
			
			foreach (Transform child in transform)
				container.addObject(child.gameObject);
		}
	}
	
	// Update is called once per frame
	protected override void onUpdate () {
		base.onUpdate();
#if UNITY_EDITOR
		//check new child added ,move it in container and update his clip
		if(!Application.isPlaying && container != null)
		{
			foreach (Transform child in transform){
				container.addObject(child.gameObject);
			}	
		}
#endif
		container.computeBounds();
	}
	
	public void addObject(GameObject obj){
		//if(container != null)
			container.addObject(obj);
	}
	
	public void removeObject(GameObject obj, bool recalculateSize = false){
		//if(container != null)
			container.removeObject(obj, recalculateSize);
	}
	
	public void childMeshChanged(kObject child){
		updateContentBounds();
	}
	public void updateContentBounds(){
		container.markContentChange();//computeBounds();
	}
	
	public Vector3 getScrollPos(){
		return container.gameObject.transform.localPosition ;
	}
	
	public Vector2 getContentSize(){
		return container.size;
	}
	
	public void scroll(Vector2 offset){
		container.changePosition(offset);
	}
	
	protected override void onDestroy(){
		container = null;
	}
	
	public void setShowScrollBar(bool value) {
		container.setShowScrollBar(value);
	}
	
	public void resetScrollBar() {
		container.resetScrollBar();
	}
	
	public void setContentSize(Vector2 contentSize) {
		container.setScrollSize(contentSize);
	}
	
#if UNITY_EDITOR	
	void OnDrawGizmos(){
		if(debug){
			Vector3 containerPos = transform.position + getScrollPos();
			Vector3 center = new Vector3(containerPos.x + getContentSize().x/2,
										 containerPos.y - getContentSize().y/2, 0);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(center, new Vector3(getContentSize().x, getContentSize().y, 2));

			containerPos +=  (Vector3)container.minPosition;
			center = new Vector3(containerPos.x + container.lifetimeSize.x/2,
			                             containerPos.y - container.lifetimeSize.y/2, 0);
			Gizmos.color = Color.black;
			Gizmos.DrawWireCube(center, new Vector3(container.lifetimeSize.x, container.lifetimeSize.y, 2));
		}
	}
#endif
}

public class kContainer{
	public 	GameObject 	gameObject = null;
	public 	Vector2  	size = Vector2.zero;
	public Vector2		minPosition = Vector2.zero;//used for scrollbar
	public Vector2		lifetimeSize = Vector2.zero;//used for scrollbar

	private kScrollBar m_scrollBar = null;
	
	//private Rect 	m_bounds = new Rect(0f,0f,0f,0f);
	private bool 	m_precalculatedScrollSize = false;
	
	public kContainer(GameObject parent){
		gameObject = new GameObject();
		gameObject.transform.parent = parent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.name = "kContainer_" + parent.name;
		//add kobject component to allow him receive notifications when child mesh is changed
		gameObject.AddComponent<kObject>();
		
		//add the scroll bar
		GameObject scrollBar = new GameObject("scrollBar");
		scrollBar.transform.parent = parent.transform;
		scrollBar.transform.localPosition = new Vector3(0f, 0f, -1.5f);
		scrollBar.transform.localScale = Vector3.one;
		m_scrollBar = scrollBar.AddComponent<kScrollBar>();
		m_scrollBar.setViewSize(gameObject.transform.parent.GetComponent<kView>().viewSize);
	}
	
	public void resetScrollBar() {
		lifetimeSize = Vector2.zero;
		markContentChange();

		m_precalculatedScrollSize = false;
		 m_scrollBar.calculateScrollBarPosition(Vector2.zero);
	}
	
	public void setScrollSize(Vector2 contentSize) {
		m_precalculatedScrollSize = true;
		m_scrollBar.setContentSize(contentSize);
		changeScrollBarPosition();
	}
	
	public void addObject(GameObject obj){
		if(obj == gameObject || obj == m_scrollBar.gameObject) return;
		
		obj.transform.parent = gameObject.transform;
		
		kObject kObj = obj.GetComponent<kObject>();	
		if(kObj != null) {
			kObj.onParentChange();//notify object tree changes
			
			/*if(m_scrollBar.isShown()) {
				Rect b = kObj.getBounds();
				Vector3 childOffset = obj.gameObject.transform.localPosition;
				b.x += childOffset.x;
				b.y += childOffset.y;
				//Debug.Log("ADD obj = " + obj.name + "  b.x = " + b.x +"   b.y = " + b.y + "  b.width = " + b.width + "  b.height = " + b.height + "  localpos.y = " + childOffset.y);
				
				if(m_init){
					m_bounds = new Rect(b.x, b.y + b.height, b.width, b.height);
					m_init = false;
				}
				
				if(m_bounds.x > b.x){
					m_bounds.width += m_bounds.x - b.x;
					m_bounds.x = b.x;
				}
				if(m_bounds.y < b.y + b.height ){
					float aaa = (b.y + b.height) - m_bounds.y;
					m_bounds.height += (b.y + b.height) - m_bounds.y;
					m_bounds.y = b.y + b.height;
				}
				if(m_bounds.x + m_bounds.width < b.x + b.width){
					m_bounds.width += (b.x + b.width) - (m_bounds.x + m_bounds.width);
				}
				if(m_bounds.height < -b.y) {
					m_bounds.height = - b.y;
				}
				
				size.x = m_bounds.width; 
				
				if(size.y != m_bounds.height) {
					//Debug.Log("m_bounds.height = " + m_bounds.height);
					size.y = m_bounds.height;
					
					
					if (!m_precalculatedContentSize) {
						m_scrollBar.setContentSize(size);
						changeScrollBarPosition();
					}
				}
				
				if(childOffset.y > 0) {
				
					gameObject.transform.localPosition += Vector3.up * childOffset.y;
					foreach (Transform child in gameObject.transform){
						kObject obj1 = child.GetComponent<kObject>();
						if(obj1 != null){
							obj1.gameObject.transform.localPosition += Vector3.down * childOffset.y;
							//clipMesh?
						}
					}
				}
			}*/
		}
		
		markContentChange();//computeBounds();
	}
	
	public void removeObject(GameObject obj, bool recalculateSize = false){
		
		obj.transform.parent = null;
		

		kObject kObj = obj.GetComponent<kObject>();
		if(kObj != null) { 
			kObj.onParentChange();//notify object tree changes
		}	
		markContentChange();//computeBounds();
	}
	
	private bool contentChanged = false;
	public void markContentChange(){
		contentChanged = true;
	}
	
	public void computeBounds(){
		
		//if(m_scrollBar.isShown())return;
		
		if(!contentChanged)return;

		contentChanged = false;
		
		Rect bounds=new Rect(0,0,0,0);
		bool init=true;

		foreach (Transform child in gameObject.transform){
			kObject obj = child.GetComponent<kObject>();
			if(obj != null)
			{
				Rect b = obj.getBounds();
				Vector3 childOffset = obj.gameObject.transform.localPosition;
				b.x += childOffset.x;
				b.y += childOffset.y;
				
				if(init){
					bounds = new Rect(b.x, b.y + b.height, b.width, b.height);
					init = false;
					continue;
				}
				
				if(bounds.x > b.x){
					bounds.width += bounds.x - b.x;
					bounds.x = b.x;
				}
				if(bounds.y < b.y + b.height ){
					bounds.height += (b.y + b.height) - bounds.y;
					bounds.y = b.y + b.height;
				}
				if(bounds.x + bounds.width < b.x + b.width){
					bounds.width += (b.x + b.width) - (bounds.x + bounds.width);
				}
				if(bounds.y - bounds.height > b.y){
					bounds.height += (bounds.y - bounds.height) - b.y;
				}
			}
		}

		Vector3 componentsOffset = new Vector3(bounds.x,bounds.y,0);
		size.x = bounds.width; size.y = bounds.height;
		
		if(componentsOffset != Vector3.zero){
			gameObject.transform.localPosition += componentsOffset;
			
			foreach (Transform child in gameObject.transform){
				kObject obj = child.GetComponent<kObject>();
				if(obj != null){
					obj.gameObject.transform.localPosition -= componentsOffset;
					//clipMesh?
				}
			}
		}

		if(lifetimeSize == Vector2.zero || !m_scrollBar.isShown()){
			minPosition = Vector2.zero;
			lifetimeSize = size;
		}else{
			minPosition -= (Vector2)componentsOffset;
			if (minPosition.x > 0) {
				lifetimeSize.x += minPosition.x;
				minPosition.x = 0;
			}
			if (minPosition.y < 0) {
				lifetimeSize.y -= minPosition.y;
				minPosition.y = 0;
			}
			if (minPosition.x + lifetimeSize.x < size.x) {
				lifetimeSize.x = size.x - minPosition.x;
			}
			if (minPosition.y - lifetimeSize.y > 0 - size.y) {
				lifetimeSize.y = size.y + minPosition.y;
			}
		}
		if(m_scrollBar.isShown()) {
			if (!m_precalculatedScrollSize) {
				m_scrollBar.setViewSize(gameObject.transform.parent.GetComponent<kView>().viewSize);
				m_scrollBar.setContentSize(lifetimeSize);
				changeScrollBarPosition();
			}
		}
	}
	
	public void changePosition(Vector3 offset){
		if(offset.x < 0.1f && offset.x > -0.1f && offset.y < 0.1f && offset.y > -0.1f)
			return;
		gameObject.transform.localPosition += offset;
		
		changeScrollBarPosition();
	}
	
	public void setShowScrollBar(bool value) {
		m_scrollBar.setIsShown(value);
	}
	
	private void changeScrollBarPosition() {
		m_scrollBar.calculateScrollBarPosition(gameObject.transform.localPosition + (Vector3)minPosition);
	}
};
