using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kTabBar : kSpriteObject 
{
	private const float Z_OFFSET_HIGHLIGHT_TAB = 1f;
//	private Color TAB_ITEM_SELECTED  = new Color(17/255f, 83/255f, 196/255f);
	public  Color TAB_ITEM_DESELECTED  = new Color(21/255f, 151/255f, 248/255f);
	public bool m_bringFrontHighlightedTab = false;
	
	// highlight changed callback
	// params: new highlighted object, old highlighted object
	public System.Action<kTabItem, kTabItem> onHighlightChanged = null;
	
	private List<kTabItem> items = new List<kTabItem>();
	private kTabItem  currentHighlightedObj = null;
	
	public void setBringFrontHighlightedTab(bool value) {
		m_bringFrontHighlightedTab = value;
	}
	
	protected override void onInit(){
		base.onInit();
		if(Application.isPlaying){
			updateCurrentItems();
		}
	}
	
	protected override void onUpdate(){
		base.onUpdate();
		
		if(!Application.isPlaying){
			updateCurrentItems();
		}
	}
	
	protected void updateCurrentItems()
	{
		kTabItem[] childs = GetComponentsInChildren<kTabItem>();
			 
		if(childs.Length != items.Count){
			for(int i = 0; i < items.Count;i++){
				kTabItem item = (kTabItem)items[i];
				item.onHighlight -= highlightChanged;
			}
			items.Clear();
			//kScrollableContainer scroll = gameObject.GetComponent<kScrollableContainer>();
			foreach(kTabItem child in childs){	
				if(child.gameObject != gameObject/* 
				&&(child.transform.parent == transform
				||(scroll != null && child.transform.parent == scroll.scrollContainer.transform))*/)
				{
					items.Add(child);
					child.onHighlight += highlightChanged;
				}
			}
		}
	}
	
	public kTabItem[] getTabItems(){
		return items.ToArray();
	}
	
	public void setHighlightedItem(kTabItem tab){
		tab.setState(kItemState.HIGHLIGHTED);
		highlightChanged(tab);
	}
	
	public kTabItem getHighlightedItem(){
		return currentHighlightedObj;
	}
	
	protected void highlightChanged(kTabItem item){
		if(currentHighlightedObj != null && currentHighlightedObj != item && currentHighlightedObj.getState() == kItemState.HIGHLIGHTED){
			currentHighlightedObj.setState(kItemState.IDLE);
		}
		kTabItem oldTab = currentHighlightedObj;
		currentHighlightedObj = item;

		if (Application.isPlaying) {
			if(oldTab != null) {
				if(oldTab.m_background != null)
				{
					oldTab.setState (kItemState.IDLE);
				}
			}
			if(currentHighlightedObj != null) {
				if(currentHighlightedObj.m_background != null)
				{
					currentHighlightedObj.setState (kItemState.PRESSED);
				}
			}
		}

		
		if(onHighlightChanged != null){
			onHighlightChanged(currentHighlightedObj,oldTab);
		}
	}
	
	public void resetBarHighlight(){
		highlightChanged(null);
	}
}
