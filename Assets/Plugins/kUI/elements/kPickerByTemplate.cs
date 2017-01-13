using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface kPickerItemData{
}

public abstract class kPickerItem:kObject{
}

//todo: add search bar,refresh,next
//sortbar
public class kPickerByTemplate : kPicker, kPickerDataSource {
	//gaphic objects
	//public kPicker 			listPicker;
	public kPickerItem[] 		templateListItems; //can be a friendlistitem or invitelistitem
	
 	List<kPickerItemData>[]	data;
	/** Item setup callback , this callback should make all setup needed to list item based on item data*/
	public System.Action<kPickerItem,kPickerItemData,int> onItemSetup= null;
	
	protected List<kPickerItem>	m_reuseableObjects = new List<kPickerItem>();
	protected bool[] m_updateLayout;

	public kPickerByTemplate(){
		dataSource = this;
	}

	protected override void onInit(){
		base.onInit();
		if(Application.isPlaying)
		{
			if(data == null)
				initData();//initialize data only it wasn't initialized already
			
			if(templateListItems != null){
				for(int i = 0; i < templateListItems.Length;i++){
					if(templateListItems[i] != null)
						templateListItems[i].gameObject.SetActive(false);
				}
				dataSource = this;
			}else
				Debug.LogError("No item template set for list "+gameObject.name);
		}
	}
	
	private void initData()
	{
		data = new List<kPickerItemData>[columns.Length];
		m_updateLayout = new bool[columns.Length];

		for(int i = 0; i < columns.Length;i++){
			data[i] = new List<kPickerItemData>();
		}
	}
	
	public List<kPickerItemData> getListData(int i){
		return data[i];
	}
	
	public void setListData(kPickerItemData[] list,int columnIndex, bool showSearchBar = false,bool forceReload = true)
	{
		if(data == null){
			initData();
		}
#if UNITY_EDITOR
		if(forceReload && showSearchBar)
			Debug.LogWarning("Search box will be hidden because forceReload flag is true");
#endif
		data[columnIndex].Clear();
		data[columnIndex].InsertRange(data[columnIndex].Count,list);
		//reset current selection
		selectedRow[columnIndex] = 0;

		if (columns[columnIndex].getScrollContainer() != null)
			columns[columnIndex].getScrollContainer().resetScrollBar();

		if(cachedElements == null || cachedElements[columnIndex] == null 
		|| cachedElements[columnIndex].Count == 0 || forceReload){//if not initialized load column
			//loadColumn(columnIndex);
			releaseCachedElements(columnIndex);
		}else{
			refreshElements(columnIndex,!showSearchBar);
			precomputeColumnContainerSize(columnIndex);
		}

		if(showSearchBar)
		{
			searchBoxes[columnIndex].showNoResultsText(list.Length == 0);
			columns[columnIndex].setScrollBlocked(list.Length == 0);
		}else
		{
			columns[columnIndex].setScrollBlocked(false);
		}

	}
	
	public void addListData(kPickerItemData[] list,int columnIndex){
		data[columnIndex].InsertRange(data[columnIndex].Count,list);
		precomputeColumnContainerSize(columnIndex);
	}

	public void insertListData(kPickerItemData[] list, int columnIndex, int insertIndex){
		data[columnIndex].InsertRange(insertIndex, list);
		ArrayList columnElements = (ArrayList)cachedElements[columnIndex];
		for (int i = 0; i < columnElements.Count; ++i) {
			kPickerElement elem = (kPickerElement)columnElements[i];
			if (elem.rowIndex >= insertIndex) {
				elem.rowIndex += list.Length;
			}
		}
		precomputeColumnContainerSize(columnIndex);
	}

	public void clearList(int columnIndex){
		data[columnIndex].Clear();
		releaseCachedElements(columnIndex);
		//loadColumn(columnIndex);
	}

	public void moveElement(int columnIndex, int currentRowIndex, int newRowIndex, System.Action onComplete = null)
	{
		if (currentRowIndex < 0 || currentRowIndex >= data[columnIndex].Count ||
		    newRowIndex < 0 || newRowIndex >= data[columnIndex].Count)
			return;

		int cacheIndex = -1;
		ArrayList columnElements = (ArrayList)cachedElements[columnIndex];
		for (int i = 0; i < columnElements.Count && cacheIndex == -1; i++) {
			if (currentRowIndex == ((kPickerElement)columnElements[i]).rowIndex)
				cacheIndex = i;
		}

		kPickerItemData tempItemData = data[columnIndex][currentRowIndex];
		data[columnIndex].RemoveAt(currentRowIndex);

		if (cacheIndex >= 0) {
			kPickerElement element = (kPickerElement)columnElements[cacheIndex];
			element.transform.localPosition += 5 * Vector3.back + 30 * Vector3.right;
			float rowHeight = element.GetComponent<kObject>().getBoundsWorld().height;

			System.Action moveAction = () => {
				element.transform.parent = columns[columnIndex].transform.parent;
				columnElements.RemoveAt(cacheIndex);
				for (int i = cacheIndex; i < columnElements.Count; i++) {
					((kPickerElement)columnElements[i]).rowIndex--;
				}
				refreshElements(columnIndex, false);
				columns[columnIndex].markLayoutChanged();
				StartCoroutine(scrollToRowIndex(columnIndex, newRowIndex, 0.1f * Mathf.Abs(newRowIndex - currentRowIndex), () => {
					cacheIndex = -1;
					for (int i = 0; i < columnElements.Count; i++) {
						if (newRowIndex == ((kPickerElement)columnElements[i]).rowIndex)
							cacheIndex = i;
					}
					if (cacheIndex >= 0) {
						kPickerElement nextElement = (kPickerElement)columnElements[cacheIndex];
						StartCoroutine(moveNextElements(columnIndex, nextElement, -rowHeight, 0.2f, () => {
							data[columnIndex].Insert(newRowIndex, tempItemData);
							refreshElements(columnIndex, false);
							columns[columnIndex].markLayoutChanged();
							DestroyObject(element.gameObject);
							if (onComplete != null) {
								onComplete();
							}
						}));
					} else {
						data[columnIndex].Insert(newRowIndex, tempItemData);
						refreshElements(columnIndex, false);
						columns[columnIndex].markLayoutChanged();
						DestroyObject(element.gameObject);
						if (onComplete != null) {
							onComplete();
						}
					}
				}));
			};

			if (cacheIndex < columnElements.Count - 1) {
				kPickerElement nextElement = (kPickerElement)columnElements[cacheIndex + 1];
				StartCoroutine(moveNextElements(columnIndex, nextElement, rowHeight, 0.2f, moveAction));
			} else {
				moveAction();
			}
		}
	}

	public void removeElement(int columnIndex,int rowIndex,float animDuration = 0f,System.Action onComplete=null){
		if(rowIndex < 0 || rowIndex >= data[columnIndex].Count)
			return ;

		int cacheIndex = -1;
		ArrayList columnElements= (ArrayList)cachedElements[columnIndex];
		for(int i = 0 ; i< columnElements.Count && cacheIndex == -1; i++) {
			if(rowIndex == ((kPickerElement)columnElements[i]).rowIndex)
				cacheIndex = i;
		}

		data[columnIndex].RemoveAt(rowIndex);
		if(cacheIndex >= 0){
			kPickerElement element = (kPickerElement)columnElements[cacheIndex];
			//bool isVisible = element.renderer.enabled;
			//rem because element is hidden when remove is called
			if(/*isVisible &&*/ cacheIndex < columnElements.Count - 1)
			{
				kPickerElement nextElement = (kPickerElement)columnElements[cacheIndex + 1];
				float yOffset = element.transform.localPosition.y - nextElement.transform.localPosition.y;
				StartCoroutine(moveNextElements(columnIndex,nextElement,yOffset,animDuration,()=>{
					/*destroyElement(element.gameObject,columnIndex);
					cachedElements[columnIndex].RemoveAt(cacheIndex);//removing cache*/
					refreshElements(columnIndex,false);
					columns[columnIndex].markLayoutChanged();
					if(columns[columnIndex].getScrollContainer() !=  null)
						columns[columnIndex].getScrollContainer().updateContentBounds();
					if(onComplete != null)
						onComplete();
				}));
			}else{
				/*destroyElement(element.gameObject,columnIndex);
				cachedElements[columnIndex].RemoveAt(cacheIndex);//removing cache*/
				refreshElements(columnIndex,false);
				columns[columnIndex].markLayoutChanged();
				if(columns[columnIndex].getScrollContainer() !=  null)
					columns[columnIndex].getScrollContainer().updateContentBounds();
				if(onComplete != null)
					onComplete();
			}
		}

	}

	private IEnumerator moveNextElements(int columnIndex,kPickerElement startElement,float offset,float duration,System.Action onComplete)
	{	//avoid division by zero
		duration = Mathf.Max(duration, 0.0001f);
		float tmpOffset = offset;
		while(Math.Abs(tmpOffset) > 0.1f){
			if(cachedElements == null || cachedElements[columnIndex] == null || cachedElements[columnIndex].Count == 0)
				break;

			float crtOffset = Time.deltaTime/duration * offset;
			crtOffset = Math.Abs(crtOffset) < Math.Abs(tmpOffset) ? crtOffset : tmpOffset;
			tmpOffset -= crtOffset;
			ArrayList columnElements= (ArrayList)cachedElements[columnIndex];
			bool move = false;
			for(int i = 0;i < columnElements.Count;i++){
				if(startElement == columnElements[i])
					move = true;
				if(move){
					kPickerElement element = (kPickerElement)columnElements[i];
					element.transform.localPosition += Vector3.up * crtOffset;
				}
			}
			yield return null;
		}

		if(onComplete != null)
			onComplete();
	}

	private IEnumerator scrollToRowIndex(int columnIndex, int rowIndex, float duration, System.Action onComplete)
	{
		setColumnSelection(columnIndex, rowIndex, false);

		yield return new WaitForSeconds(duration);

		if (onComplete != null) {
			onComplete();
		}
	}

	private void precomputeColumnContainerSize(int columnIndex){
		if(cachedElements == null || cachedElements[columnIndex] == null 
		|| cachedElements[columnIndex].Count == 0 || computeScrollSizeRealTime[columnIndex])
			return;
	
		ArrayList columnElements= (ArrayList)cachedElements[columnIndex];
		kPickerElement element = (kPickerElement)columnElements[0];
		kObject obj = element.GetComponent<kObject>();
		if(obj != null){
			computeContentSize(columnIndex,obj);
		}
	}
	
	public void refreshElements(int columnIndex = 0,bool removeSearchBar=true) {
		if(cachedElements == null || cachedElements.Length <= columnIndex || cachedElements[columnIndex] == null)
			return;

		ArrayList columnElements= (ArrayList)cachedElements[columnIndex];
		for(int i=0 ; i< columnElements.Count ; i++) {
			kPickerElement element = (kPickerElement)columnElements[i];
			kPickerItem item = element.GetComponent<kPickerItem>();
			if(data[columnIndex].Count > element.rowIndex && element.rowIndex != SEARCH_BOX_ROW_INDEX){
				onItemSetup(item, data[columnIndex][element.rowIndex], element.rowIndex);
			}else if(element.rowIndex == SEARCH_BOX_ROW_INDEX && !removeSearchBar){
			}else{
				destroyElement(element.gameObject,columnIndex);
				cachedElements[columnIndex].RemoveAt(i);
				i--;
			}

		}
		if(selectedRow[columnIndex] >= data[columnIndex].Count)
			selectedRow[columnIndex] = 0;
		markUpdateLayout(columnIndex);
	}
	protected override void onUpdate(){
		base.onUpdate();
		if (!Application.isPlaying)
			return;

		for(int i = 0; i < m_updateLayout.Length;i++)
			if(m_updateLayout[i])
				updateLayout(i);
	}

	public void markUpdateLayout(int column){
		if(column < m_updateLayout.Length)
			m_updateLayout[column] = true;
	}

	private void updateLayout(int column)
	{
		m_updateLayout[column] = false;

		if(column >= columns.Length || columns[column] == null)
			return;

		if(cachedElements[column].Count > 0)
		{
			ArrayList columnElements= (ArrayList)cachedElements[column];
			
			kPickerElement firstElem = (kPickerElement)columnElements[0];
			//kPickerElement lastElem = (kPickerElement)columnElements[columnElements.Count - 1];
			
			if(firstElem != null){
				kPickerElement prevElem = firstElem;
				for(int i = 1; i < columnElements.Count; i++){
					kPickerElement elem = (kPickerElement)columnElements[i];
					if(columns[column].allowVerticalScroll)
						setupElemPos(elem.gameObject,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, prevElem.gameObject,kAlignMode.BOTTOM_CENTER);
					else
						setupElemPos(elem.gameObject,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, prevElem.gameObject,kAlignMode.VCENTER_RIGHT);
					prevElem = elem;
				}
				columns[column].markLayoutChanged();
				if(columns[column].getScrollContainer() !=  null)
					columns[column].getScrollContainer().updateContentBounds();
			}
		}
	}
	
	public GameObject elementAt(kPicker picker,int column, int row)
	{
		int columnIndex = column;//getColumnIndex(column);
		if(columnIndex >= 0)
		{
			kPickerItem item = getReusableItem(templateListItems[columnIndex].GetType());

			if(item == null)
				item = (kPickerItem) GameObject.Instantiate(templateListItems[columnIndex]);
		
			item.name = "ListItem_" + row;
			item.gameObject.SetActive(true);
			
			if (data != null && data[columnIndex] != null && row < data[columnIndex].Count)
			{
				if(onItemSetup != null) {
					if (activityIndicators != null && activityIndicators.Length > columnIndex && activityIndicators[columnIndex].gameObject == item.gameObject) {
						Debug.LogError("pam pam - why?!?!? I removed it!");
					}
					else {
						onItemSetup(item,data[columnIndex][row],row);
					}
				}
			}
			return item.gameObject;
		}
		return null;
	}
	
	//picker methods
	public int noRowsInColumn(kPicker picker, int column){
		return noRowsInColumn(column);
	}

	public int noRowsInColumn(int columnIndex){
		if(columnIndex >= 0){
			if (data != null && data[columnIndex] != null)
				return data[columnIndex].Count;// + 1;
		}
		return 0;
	}
	
	protected override void destroyElement(GameObject obj,int column){
		kScrollableContainer container = columns[column].GetComponent<kScrollableContainer>();
		
		if(container != null)	container.removeObject(obj);
		
		if((activityIndicators == null || activityIndicators.Length <= column || obj != activityIndicators[column].gameObject)
			&& (searchBars == null || searchBars.Length <= column || obj != searchBars[column].gameObject))
		{
			kPickerItem item = obj.GetComponent<kPickerItem>();
			if(item != null){
				obj.transform.parent = transform;
				obj.SetActive(false);
				m_reuseableObjects.Add(item);
			}else
				Destroy(obj);
		}else{
			obj.SetActive(false);
			obj.transform.parent = transform;
		}
	}

	protected kPickerItem getReusableItem(Type type){
		kPickerItem item = null;
		for (int i = 0; i < m_reuseableObjects.Count && item == null; i++) {
			if(m_reuseableObjects[i].GetType() == type){
				item = m_reuseableObjects[i];
				m_reuseableObjects.RemoveAt(i);
			}
		}
		return item;
	}
}
