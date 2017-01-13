using UnityEngine;
using System.Collections;

public interface kPickerDataSource
{
	int			noRowsInColumn(kPicker picker,int columnIndex);
	GameObject 	elementAt(kPicker picker, int column, int row);
};

public class kPickerElement:MonoBehaviour{
	public int 	rowIndex;
}

[ExecuteInEditMode]
public class kPicker: kSpriteObject
{
	//public float MAX_AUTO_SCROLL_VELOCITY = 2500f;
	public float SELECTION_SNAP_TIME = 0.15f;

	public const int SEARCH_BOX_ROW_INDEX = -1;
	
	[SerializeField]
	public kPickerDataSource 	dataSource;
	//params: picker, column, old selection, new selection
	public System.Action<kPicker, int, int, int> selectionChanged = null;
	
	public bool snapSelectionOneByOne = false;
	public bool isCircular = false;
	public int 	rowSpacing = 0;
	
	public kView[] 	 		columns;
	public kObject[]		selectors;
	public bool[]	computeScrollSizeRealTime = null;

	protected int[] selectedRow = new int[20];
	protected bool[] m_forceSelectionCallback = new bool[20];

	private bool[] m_selectionChangedWhileDragging;
	protected ArrayList[]	cachedElements = null;
	public int	minChachedElements = 2;
	public int 	maxChachedElements = 5;
	
	public kSpriteObject[] searchBars = null;
	public kSearchBox[]	searchBoxes = null;
	public kSpriteObject[] activityIndicators = null;
	public bool activityIndicatorsOnTop = false;

	public bool m_autoCheckContentOutOfBounds = false;
	public bool m_useRaycastSelection = false;
	
	//activity start trigger ,param activity complete callback 
	protected System.Action<System.Action<bool>>[] m_activityTriggerCallbacks = null;
	protected bool m_addSearchBarToList = true;
	
	private int m_firstVisibleElem;
	private int m_lastVisibleElem;
	
	protected override void onInit(){
		base.onInit();
		if(Application.isPlaying){
			if(activityIndicators != null) {
				m_activityTriggerCallbacks = new System.Action<System.Action<bool>>[activityIndicators.Length];
				for(int i = 0; i < activityIndicators.Length;i++){
					if(activityIndicators[i] != null)
						activityIndicators[i].gameObject.SetActive(false);
					else
						m_activityTriggerCallbacks[i] = null;
				}
			}
			
			if(searchBars != null) {
				for(int i = 0; i < searchBars.Length;i++){
					if(searchBars[i] != null)
						searchBars[i].gameObject.SetActive(false);
				}
			}
		}
		if (computeScrollSizeRealTime == null || computeScrollSizeRealTime.Length < columns.Length) {
			computeScrollSizeRealTime = new bool[columns.Length];
		}
	}
	
	protected override void onStart(){
		base.onStart();
		if(Application.isPlaying){
			if(dataSource != null && columns != null && columns.Length > 0){
				loadPicker();
			}
			
			m_selectionChangedWhileDragging = new bool[selectors.Length];
		}
	}
	
	protected override void onUpdate(){
		base.onUpdate();
#if UNITY_EDITOR
		if(computeScrollSizeRealTime == null){
			computeScrollSizeRealTime = new bool[columns.Length];
		}else if(computeScrollSizeRealTime.Length != columns.Length){
			bool[] newVal = new bool[columns.Length];
			for(int i = 0; i < Mathf.Min(columns.Length,computeScrollSizeRealTime.Length);i++){
				newVal[i] = computeScrollSizeRealTime[i];
			}
			computeScrollSizeRealTime = newVal;
		}
#endif
	
		if(Application.isPlaying && selectors != null)
		{
			for(int i = 0 ; i< columns.Length; i++) {
				if(!updatePickerElements(i)){
					updateSelection(i);
				}
			}
		}
	}

	protected override void onEnable(){
		base.onEnable();

		if(cachedElements == null)
			return;

		if(snapSelectionOneByOne) {
			for(int i = 0 ; i < columns.Length ; i++)
				snapInstantToCurrentSelection(i);
		}
	}
	
	protected void updateSelection(int col)
	{
		int i = col;
		if (selectors != null && selectors.Length > col && selectors[i] != null &&
			(columns[i].scrollMoving != kView.kScrollDirection.NO_MOVE || (m_forceSelectionCallback[i] && m_firstVisibleElem >= 0)))
		{
			if (!m_useRaycastSelection)
			{
				Vector2 selectorCenter = selectors[i].getBoundsWorld().center;
				ArrayList columnElements = (ArrayList)cachedElements[i];
				
				int firstElem = Mathf.Max(m_firstVisibleElem - 1, 0);
				int lastElem = Mathf.Min(m_lastVisibleElem + 1, columnElements.Count);
				for (int j = firstElem; j < lastElem ; j++)
				{
					kPickerElement elem = (kPickerElement)columnElements[j];
					Rect elemRect = elem.GetComponent<kObject>().getBoundsWorld();
					if (RectUtil.isRectContainingPoint(elemRect, selectorCenter)) {
						int newSel = elem.rowIndex;
						if (selectedRow[i] != newSel || m_forceSelectionCallback[i]) {
							m_forceSelectionCallback[i] = false;
							if (selectionChanged != null) {
								AudioSource audioSource = GetComponent<AudioSource>() ;
								if (audioSource != null && audioSource.clip != null) {
									if ( selectedRow[i] != newSel )
										playSound(audioSource.clip);
								}

								selectionChanged(this, i, selectedRow[i], newSel);
							}
							selectedRow[i] = newSel;
							m_selectionChangedWhileDragging[i] = columns[i].isDragging;
						}
						break;
					}
				}
			}else{
				Vector3 rayOrigin = selectors[i].transform.position + new Vector3(selectors[i].getBounds().center.x, selectors[i].getBounds().center.y, 0);
				Ray ray = new Ray(rayOrigin, Vector3.forward);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit))
				{
					if(hit.collider.GetComponent<kPickerElement>() != null)
					{
						int newSel = hit.collider.GetComponent<kPickerElement>().rowIndex;
						if(selectedRow[i] != newSel || m_forceSelectionCallback[i]){
							m_forceSelectionCallback[i] = false;
							//if(gameObject.audio != null)
							//	gameObject.audio.PlayOneShot(gameObject.GetComponent<AudioSource>().clip);
							if(selectionChanged != null){
								selectionChanged(this, i, selectedRow[i], newSel);
							}
							selectedRow[i] = newSel;
							m_selectionChangedWhileDragging[i] = columns[i].isDragging;
						}
					}
				}
			}
		}
			
			/*
			if(selectors[i] != null)
			{
				Vector3 rayOrigin = selectors[i].transform.position + new Vector3(selectors[i].getBounds().center.x, selectors[i].getBounds().center.y, -3);
				Ray ray = new Ray(rayOrigin, Vector3.forward);
				RaycastHit hit;
	            if (Physics.Raycast(ray, out hit))
				{
					if(hit.collider.gameObject.GetComponent<kPickerElement>() != null)
					{
						int newSel = hit.collider.gameObject.GetComponent<kPickerElement>().rowIndex;
						if(selectedRow[i] != newSel){
							if(gameObject.audio != null)
								gameObject.audio.PlayOneShot(gameObject.GetComponent<AudioSource>().clip);
							if(selectionChanged != null){
								selectionChanged(this, i, selectedRow[i], newSel);
							}
							selectedRow[i] = newSel;
							m_selectionChangedWhileDragging[i] = columns[i].isDragging;
						}
					}
				}
			}*/
	}

	//int frameCounter = 0;
	/** Returns true if new object was added to picker list */
	protected bool updatePickerElements(int column){
		if (cachedElements == null)
			return true;

		int noRowsInColumn = dataSource.noRowsInColumn(this,column);

		if(cachedElements[column].Count == 0 && noRowsInColumn > 0)
			loadColumn(column);

		//frameCounter = (frameCounter + 1)%5;
		//if(frameCounter != 1)
		//	return false;

		bool addedNewObjects = false;
		if(columns[column] != null && cachedElements[column].Count > 0)
		{
			bool addNewObject = true;
			while(addNewObject)
			{
				addNewObject = false;

				ArrayList columnElements= (ArrayList)cachedElements[column];
				
				kPickerElement firstElem= (kPickerElement)columnElements[0];
				kPickerElement lastElem = (kPickerElement)columnElements[columnElements.Count - 1];
				
				m_firstVisibleElem	= -1;
				m_lastVisibleElem	= -1;
				
				for(int j = 0;j < columnElements.Count;j++){
					kPickerElement elem = (kPickerElement)columnElements[j];
					if(m_firstVisibleElem == -1 && elem.GetComponent<Renderer>().enabled)
						m_firstVisibleElem= j;
					if(m_lastVisibleElem < j && elem.GetComponent<Renderer>().enabled)
						m_lastVisibleElem = j;
				}
			
				//tmp hardcode
				int minCacheSize = minChachedElements;
				int maxCacheSize = maxChachedElements;
				//if(firstVisibleElem == -1)
				//	Debug.Log("No Visible element");
				
				int nextTopElemIndex 	= firstElem.rowIndex - 1;
				int nextBottomElemIndex = lastElem.rowIndex + 1;
				if(isCircular){
					if(nextTopElemIndex < 0)
						nextTopElemIndex = noRowsInColumn + nextTopElemIndex; 
					if(nextBottomElemIndex >= noRowsInColumn)
						nextBottomElemIndex = nextBottomElemIndex % noRowsInColumn; 
				}
				
				if(m_firstVisibleElem != SEARCH_BOX_ROW_INDEX && m_firstVisibleElem < minCacheSize && nextTopElemIndex == SEARCH_BOX_ROW_INDEX 
				&& searchBars != null && searchBars.Length > column && searchBars[column] != null && m_addSearchBarToList) {
					searchBars[column].gameObject.SetActive(true);
					if(columns[column].allowVerticalScroll)
						setupElemPos(searchBars[column].gameObject,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, firstElem.gameObject,kAlignMode.TOP_CENTER);
					else
						setupElemPos(searchBars[column].gameObject,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, firstElem.gameObject,kAlignMode.VCENTER_LEFT);

					addElement(searchBars[column].gameObject,column,nextTopElemIndex, true);
					addNewObject = true;
				}
				if(m_firstVisibleElem != -1 && m_firstVisibleElem < minCacheSize && nextTopElemIndex >= 0){
					GameObject obj = dataSource.elementAt(this,column, nextTopElemIndex);
					if(columns[column].allowVerticalScroll)
						setupElemPos(obj,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, firstElem.gameObject,kAlignMode.TOP_CENTER);
					else
						setupElemPos(obj,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, firstElem.gameObject,kAlignMode.VCENTER_LEFT);
					addElement(obj,column,nextTopElemIndex,true);
					addNewObject = true;
				}else if(m_firstVisibleElem > maxCacheSize && !firstElem.GetComponent<Renderer>().enabled){
					destroyElement(firstElem.gameObject,column);//Destroy(firstElem.gameObject);
					columnElements.RemoveAt(0);
				}
				else if (activityIndicatorsOnTop && isActivityTriggerEnabled(column) && noRowsInColumn != 0
				&& m_firstVisibleElem == 0 && nextTopElemIndex == -1)
				{
					activityIndicators[column].gameObject.SetActive(true);
					if(columns[column].allowVerticalScroll)
						setupElemPos(activityIndicators[column].gameObject,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, firstElem.gameObject,kAlignMode.TOP_CENTER);
					else
						setupElemPos(activityIndicators[column].gameObject,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, firstElem.gameObject,kAlignMode.VCENTER_LEFT);
					addElement(activityIndicators[column].gameObject,column,nextTopElemIndex,true);
					addNewObject = true;
				}

				if(m_lastVisibleElem != -1 && columnElements.Count - 1 - m_lastVisibleElem < minCacheSize 
				&& nextBottomElemIndex < noRowsInColumn){
					GameObject obj = dataSource.elementAt(this,column, nextBottomElemIndex);
					if(columns[column].allowVerticalScroll)
						setupElemPos(obj,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, lastElem.gameObject,kAlignMode.BOTTOM_CENTER);
					else
						setupElemPos(obj,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, lastElem.gameObject,kAlignMode.VCENTER_RIGHT);
					addElement(obj,column,nextBottomElemIndex);
					addNewObject = true;
				}else if(columnElements.Count - 1 - m_lastVisibleElem > maxCacheSize && !lastElem.GetComponent<Renderer>().enabled){
					destroyElement(lastElem.gameObject,column);//Destroy(lastElem.gameObject);
					columnElements.RemoveAt(columnElements.Count - 1);
				}
				else if (!activityIndicatorsOnTop && isActivityTriggerEnabled(column) && noRowsInColumn != 0
				&& m_lastVisibleElem == columnElements.Count - 1 && nextBottomElemIndex == noRowsInColumn)
				{
					activityIndicators[column].gameObject.SetActive(true);
					if(columns[column].allowVerticalScroll)
						setupElemPos(activityIndicators[column].gameObject,columns[column].transform.position + Vector3.right * columns[column].getBounds().width/2, lastElem.gameObject,kAlignMode.BOTTOM_CENTER);
					else
						setupElemPos(activityIndicators[column].gameObject,columns[column].transform.position + Vector3.down * columns[column].getBounds().height/2, lastElem.gameObject,kAlignMode.VCENTER_RIGHT);
					addElement(activityIndicators[column].gameObject,column,nextBottomElemIndex);
					addNewObject = true;
				}
				
				if (activityIndicatorsOnTop && m_firstVisibleElem >= 0 && m_firstVisibleElem < columnElements.Count) {
					kPickerElement firstElement = (kPickerElement)columnElements[m_firstVisibleElem];
					updateActivityTrigger(column, firstElement);
				}
				if (!activityIndicatorsOnTop && m_lastVisibleElem >= 0 && m_lastVisibleElem < columnElements.Count) {
					kPickerElement lastElement = (kPickerElement)columnElements[m_lastVisibleElem];
					updateActivityTrigger(column, lastElement);
				}
				addedNewObjects = addedNewObjects || addNewObject;
			}
			if(m_autoCheckContentOutOfBounds){
				//update container layout when finish addind alements
				if(m_needUpdateLayout && !addedNewObjects){
					columns[column].markLayoutChanged();
				}
				m_needUpdateLayout = addedNewObjects;
			}
		}
		//if(addedNewObjects) frameCounter = 0;

		return addedNewObjects;
	}
	private bool m_needUpdateLayout = false;
	
	protected bool m_isActivityEnabled = false;
	public bool isActivityTriggerEnabled(int column){
		return	m_isActivityEnabled && activityIndicators != null && activityIndicators.Length > column 
			&&	activityIndicators[column] != null 
			&&  m_activityTriggerCallbacks != null && m_activityTriggerCallbacks[column] != null;
	}
	
	protected void updateActivityTrigger(int column,kPickerElement lastColumnElement){
		if(isActivityTriggerEnabled(column)){
			kPickerElement spinerElem = activityIndicators[column].GetComponent<kPickerElement>();
			if(lastColumnElement == spinerElem) {
				StartCoroutine(activityTriggerCallbacks(column));
				m_isActivityEnabled  = false;
				//m_activityTriggerCallbacks[column] = null;
			}
		}
	}
	
	protected IEnumerator activityTriggerCallbacks(int column) {
		yield return new WaitForSeconds(0.7f);
		if(m_activityTriggerCallbacks[column] != null) {//because in 0.7 seconds it's possible to change the TAB
			m_activityTriggerCallbacks[column]((bool isTriggerEnabled)=>{
				m_isActivityEnabled = isTriggerEnabled;
				removeActivityIndicator(column, isTriggerEnabled);
			});
		}
	}
	
	
	public void setSearchBarTrigger(int column, System.Action<string> triggerCallback){
		if (searchBoxes != null && searchBoxes.Length > column && searchBoxes[column] != null) {
			searchBoxes[column].triggerSearchCallback =  triggerCallback;
		}
	}

	public void setSearchBarCloseCallback(int column, System.Action closeCallback){
		if (searchBoxes != null && searchBoxes.Length > column && searchBoxes[column] != null) {
			searchBoxes[column].onCloseCallback =  closeCallback;
		}
	}
	
	/** Adds an activity indicator to the list of items and triggers the callback method when the indicator gets visible */
	public void enableActivityTrigger(int column,System.Action<System.Action<bool>> triggerCallback){
		if(activityIndicators != null && activityIndicators.Length > column 
		&& activityIndicators[column] != null){
			m_activityTriggerCallbacks[column] = triggerCallback;
			m_isActivityEnabled = true;
		}
	}
	
	public void disableActivityTrigger(int column){
		removeActivityIndicator(column, false);
		m_activityTriggerCallbacks[column] = null;
		m_isActivityEnabled = false;
	}
	
	public void removeActivityIndicator(int column, bool isTriggerEnabled) 
	{
		if(columns[column].getScrollContainer() != null)	columns[column].getScrollContainer().removeObject(activityIndicators[column].gameObject, !isTriggerEnabled);
		
		kPickerElement spinerElem = activityIndicators[column].GetComponent<kPickerElement>();
		if(spinerElem != null)
			((ArrayList)(cachedElements[column])).Remove(spinerElem);
		
		activityIndicators[column].transform.parent = transform;
		activityIndicators[column].gameObject.SetActive(false);
	}
	
	public void setupElemPos(GameObject obj,Vector3 basePos, GameObject anchorElem,kAlignMode alignMode)
	{
		Vector3 pos = new Vector3(basePos.x, basePos.y, basePos.z - 1);
		
		kObject anchorkObj = anchorElem == null? null : anchorElem.GetComponent<kObject>();
		bool isAnchorValid = anchorkObj != null;
		
		kObject kObj = obj.GetComponent<kObject>();
		bool iskObjValid = kObj != null;
		
		int align = (int)alignMode;
		if((align & (int)kAlign.TOP) != 0)
		{
			if(isAnchorValid){
				pos.y = anchorElem.transform.position.y;
				pos.y += anchorkObj.getBounds().y 
					  +  anchorkObj.getBounds().height + rowSpacing;
			}
			if(iskObjValid){
				pos.y -= kObj.getBounds().y;
			}
		}else if((align & (int)kAlign.BOTTOM) != 0){
			if(isAnchorValid){
				pos.y = anchorElem.transform.position.y;
				pos.y += anchorkObj.getBounds().y - rowSpacing;
			}
			if(iskObjValid){
				pos.y -= (kObj.getBounds().y + kObj.getBounds().height);
			}
		}else if((align & (int)kAlign.VCENTER) != 0){
			if(isAnchorValid){
				pos.y = anchorElem.transform.position.y;
				pos.y += anchorkObj.getBounds().center.y;
			}
			if(iskObjValid){
				pos.y -= kObj.getBounds().center.y;
			}
		}
		
		if((align & (int)kAlign.LEFT) != 0)
		{
			if(isAnchorValid){
				pos.x = anchorElem.transform.position.x;
				pos.x += (anchorkObj.getBounds().center.x - anchorkObj.getBounds().width/2) - rowSpacing;
			}
			if(iskObjValid){
				pos.x -= (kObj.getBounds().x + kObj.getBounds().width);
			}
		}else if((align & (int)kAlign.RIGHT) != 0){
			if(isAnchorValid){
				pos.x = anchorElem.transform.position.x;
				pos.x += anchorkObj.getBounds().center.x +  anchorkObj.getBounds().width/2 + rowSpacing;
			}
			if(iskObjValid){
				pos.x -= kObj.getBounds().x;//(obj.GetComponent<kObject>().getBounds().center.x + obj.GetComponent<kObject>().getBounds().width/2);
			}
			obj.transform.position = pos;
		}else if((align & (int)kAlign.HCENTER) != 0){
			if(isAnchorValid){
				pos.x = anchorElem.transform.position.x;
				pos.x += anchorkObj.getBounds().center.x;
			}
			if(iskObjValid){
				pos.x -= kObj.getBounds().center.x;
			}
		}
		obj.transform.position = pos;
	}
	
	public void loadPicker(){
		if(cachedElements == null)
			cachedElements = new ArrayList[columns.Length];
		for(int i = 0; i < columns.Length;i++){
			loadColumn(i);
		}
	}
	
	public void releaseCachedElements(int column){
		if(cachedElements != null&& cachedElements[column] != null){
			while(cachedElements[column].Count > 0){//destroy all loaded elements
				destroyElement(((kPickerElement)cachedElements[column][0]).gameObject,column); //Destroy(((kPickerElement)cachedElements[column][0]).gameObject);
				cachedElements[column].RemoveAt(0);
			}
		}
	}
	
	public void loadColumn(int column){
		if(!gameObject.activeInHierarchy){
			releaseCachedElements(column);
			return;
		}
		
		if(cachedElements == null){
			loadPicker();
			return;
		}else if(cachedElements[column] == null){
			cachedElements[column] = new ArrayList();
		}else{
			stopScrolling();//columns[column].blockCoroutines = true; //bug fix: when the scrollview is still moving coroutines should end
			releaseCachedElements(column);
		}
		
		if(columns[column] != null)
		{
			//columns[column].MAX_AUTO_SCROLL_VELOCITY = MAX_AUTO_SCROLL_VELOCITY;
			if(columns[column].getScrollContainer() != null)
				columns[column].getScrollContainer().resetScrollBar();

			if(searchBoxes != null && column < searchBoxes.Length && searchBoxes[column] != null)
				searchBoxes[column].showNoResultsText(false);
			
			//columns[column].edgeInset = new Vector2(columns[column].viewSize.x/2, columns[column].viewSize.y/2 - selectors[column].getBounds().height/2);
			if (selectors.Length > column && selectors[column] != null)
				columns[column].onScrollEnd = OnColumnScrollEnd;
			else
				columns[column].onScrollEnd = null;

			columns[column].setPagingEnable(snapSelectionOneByOne);
			if(snapSelectionOneByOne)
				columns[column].onSnapToSelection = onSnapToSelection;
			else
				columns[column].onSnapToSelection = null;

			/*if(m_showSearchBar)
			{
				GameObject obj = searchBars[column].gameObject;
				obj.SetActive(true);
				
				if(selectors.Length > column && selectors[column] != null){
					setupElemPos(obj, new Vector3(0, 0, columns[column].transform.position.z),
					             selectors[column].gameObject, kAlignMode.VCENTER_CENTER);
				}else{
					if(columns[column].allowVerticalScroll){
						setupElemPos(obj,new Vector3(columns[column].transform.position.x + columns[column].getBounds().width/2,
						                             columns[column].transform.position.y - columns[column].edgeInset.y,
						                             columns[column].transform.position.z), null,kAlignMode.BOTTOM_CENTER);
					}else{
						setupElemPos(obj,new Vector3(columns[column].transform.position.x,
						                             columns[column].transform.position.y + columns[column].getBounds().center.y,
						                             columns[column].transform.position.z), null,kAlignMode.VCENTER_RIGHT);
					}
				}
				addElement(obj,column,SEARCH_BOX_ROW_INDEX,true);//selectedRow[column]);
			}*/

			if(searchBoxes != null && searchBoxes.Length > column && searchBoxes[column] != null 
			&& searchBoxes[column].getText() != null && searchBoxes[column].getText().Length > 0) {
				searchBoxes[column].setText("");
			}

			int noRowsInColumn = dataSource.noRowsInColumn(this,column);
			if(noRowsInColumn > 0){
				if(selectedRow[column] >= noRowsInColumn){
					Debug.LogError("Wrong selection set on picker "+name+" current selected row ("+selectedRow[column]+") is greater than elements count.");
					selectedRow[column] = 0;
				}
				GameObject obj = dataSource.elementAt(this,column, selectedRow[column]);
				
				if(!computeScrollSizeRealTime[column]){
					computeContentSize(column, obj.GetComponent<kObject>());
				}
				
				if(selectors.Length > column && selectors[column] != null){
					setupElemPos(obj, new Vector3(0, 0, columns[column].transform.position.z),
								selectors[column].gameObject, kAlignMode.VCENTER_CENTER);
				}else{
					if(columns[column].allowVerticalScroll){
						setupElemPos(obj,new Vector3(columns[column].transform.position.x + columns[column].getBounds().width/2,
									 columns[column].transform.position.y - columns[column].edgeInset.y,
									 columns[column].transform.position.z), null,kAlignMode.BOTTOM_CENTER);
					}else{
						setupElemPos(obj,new Vector3(columns[column].transform.position.x + columns[column].edgeInset.x,
									 columns[column].transform.position.y + columns[column].getBounds().center.y,
									 columns[column].transform.position.z), null,kAlignMode.VCENTER_RIGHT);
					}
				}
				addElement(obj,column,selectedRow[column]);
			}
			//selectedRow[column] = -1;//reset selection to force selectionCallback => bug: it happens selection to be 0 => no callback
			//Debug.Log("Force selection change on column:"+column);
			//m_forceSelectionCallback[column] = true;
		}
	}

	protected void computeContentSize(int column, kObject obj){
		if(columns[column].getScrollContainer() != null && obj != null){
			Rect r = obj.getBounds();
			int noRowsInColumn = dataSource.noRowsInColumn(this,column);
			columns[column].getScrollContainer().setContentSize(new Vector2(r.width, noRowsInColumn * (r.height + rowSpacing)));
		}
	}
	
	protected void addElement(GameObject obj,int column,int row,bool insertAtTop = false){
		//Debug.Log("Add element:"+row);
		if(columns[column].getScrollContainer() != null)
			columns[column].getScrollContainer().addObject(obj);
		else{
			Debug.Log("Add element on view");
			obj.transform.parent = columns[column].transform;
		}
		
		obj.transform.localRotation = Quaternion.identity;
		//add box collider to allow selector highlight elements
		kTouchable t = obj.GetComponent<kTouchable>();
		if (t == null){
			t = obj.AddComponent<kTouchable>();
		}
		//Fixed wrong selection
		t.m_enableSizeIncrease = false;
		
		kTouchable[] touchables = obj.GetComponentsInChildren<kTouchable>(true);
		foreach (kTouchable touchable in touchables) 
		{
			//skip cotainer object
			if(touchable.transform.parent != columns[column].transform)
			{
				touchable.onTouchBegan  -= columns[column].onTouchBegan;
	  			touchable.onTouchBegan	+= columns[column].onTouchBegan;
				touchable.onTouchPressed-= columns[column].onTouchBegan;
				touchable.onTouchPressed+= columns[column].onTouchBegan;
				touchable.onTouching 	-= columns[column].onTouchDrag;
				touchable.onTouching 	+= columns[column].onTouchDrag;
				touchable.onTouchEnd 	-= columns[column].onTouchEnd;
				touchable.onTouchEnd 	+= columns[column].onTouchEnd;
				touchable.onTouchRelease-= columns[column].onTouchEnd;
				touchable.onTouchRelease+= columns[column].onTouchEnd;
			}
    	}
		/*
		t.onTouchBegan	+= columns[column].onTouchBegan;
		t.onTouchPressed+= columns[column].onTouchBegan;
		t.onTouching 	+= columns[column].onTouchDrag;
		t.onTouchEnd 	+= columns[column].onTouchEnd;
		t.onTouchRelease+= columns[column].onTouchEnd;
		*/
		
		obj.GetComponent<Renderer>().enabled = false;//disable renderer one frame, it will be enabled by clipping
		//if(obj.GetComponent<kObject>() != null)
		//	obj.GetComponent<kObject>().updateObjectMesh(true);
		
		kPickerElement elem = obj.GetComponent<kPickerElement>();
		if(elem == null)
			elem = obj.AddComponent<kPickerElement>();
		elem.rowIndex = row;
		
		if(!insertAtTop)
			cachedElements[column].Add(elem);
		else
			cachedElements[column].Insert(0,elem);
	}

	public int getColumnSelection(int col)
	{
		if(col >= columns.Length)
			return -1;
		return selectedRow[col];
	}
	
	public void setColumnSelection(int col, int row, bool releaseCache = true,bool callSelectionChangedCallback = true)
	{
		if (columns != null && col < columns.Length && columns[col] != null) {
			if(dataSource != null)
				row = Mathf.Clamp(row, 0, dataSource.noRowsInColumn(this, col) - 1);
			//Debug.LogError("setColumnSelection .......... selectedRow[col] = " + selectedRow[col] + "   row = " + row);
			if (selectedRow[col] != row) {
				int oldRow = selectedRow[col];
				selectedRow[col] = row;
				if (releaseCache) {
					releaseCachedElements(col);
					m_forceSelectionCallback[col] = callSelectionChangedCallback;
				} else {
					//m_forceSelectionCallback[col] = true;
					StartCoroutine(selectElement(col, oldRow, row));
				}
			}
		}
	}
	
	public kObject getObject(int col, int row)
	{
		kObject resObject = null;
		float resDistance = float.MaxValue;
		if (col < columns.Length && cachedElements[col] != null)
		{
			for (int i = 0; i < cachedElements[col].Count; i++)
			{
				kPickerElement elem = (kPickerElement)cachedElements[col][i];
				if (elem.rowIndex == row)
				{
					if (isCircular && selectors != null && selectors.Length > col && selectors[col] != null)
					{
						float distance = (selectors[col].transform.position - elem.transform.position).sqrMagnitude;
						if (distance < resDistance)
						{
							resObject = elem.gameObject.GetComponent<kObject>();
							resDistance = distance;
						}
					}
					else
						return elem.GetComponent<kObject>();
				}
			}
		}
		return resObject;
	}
	
	public int getColumnIndex(kView column){
		for(int i = 0; i < columns.Length;i++){
			if(columns[i] == column)
				return i;
			
		}
		return -1;//not a column of this picker
	}

	private void onSnapToSelection(kView column, int direction)
	{
		int columnIdx = getColumnIndex(column);
		updateSelection(columnIdx);
		int newSelection = selectedRow[columnIdx];


		if (!m_selectionChangedWhileDragging[columnIdx]) {
			newSelection -= direction;
		}
		int noRowsInColumn = dataSource.noRowsInColumn(this, columnIdx);

		if (newSelection < 0) {
			newSelection = isCircular ? noRowsInColumn - 1 : 0;
		}
		if (newSelection >= noRowsInColumn) {
			newSelection = isCircular ? 0 : noRowsInColumn - 1;
		}
		//selectedRow[columnIdx] = newSelection;
		snapToSelection(columnIdx, newSelection);
		m_selectionChangedWhileDragging[columnIdx] = false;
	}

	private void snapToSelection(int col, int selIndex){
		if(snapSelectionOneByOne)
			StartCoroutine(snapToSelectionOneByOne(col,selIndex));
		else
			StartCoroutine(snapToSelectionDefault(col,selIndex));
	}

	protected IEnumerator snapToSelectionDefault(int col, int selIndex)
	{
		kView column = columns[col];
		kScrollableContainer scrollObj = column.getScrollContainer();
		kObject selObject = getObject(col, selIndex);

		if (selObject != null && col < selectors.Length)
		{
			float selectorCenter = 	column.allowHorizontalScroll ?
									(selectors[col].transform.position.x + selectors[col].getBounds().center.x) :
									(selectors[col].transform.position.y + selectors[col].getBounds().center.y);
			float snapTime = SELECTION_SNAP_TIME;
			while (!column.isDragging && selectedRow[col] == selIndex)//stop snap when changing selection
			{
				if (selObject == null)
					break;
				Vector3 scrollOffset = Vector3.zero;
				float selObjCenter = column.allowHorizontalScroll ?
									(selObject.transform.position.x + selObject.getBounds().center.x) :
									(selObject.transform.position.y + selObject.getBounds().center.y);
				
				var distanceFromTarget = selectorCenter - selObjCenter;
				var snapDistance = (distanceFromTarget / snapTime) * Time.deltaTime;//(snapOneByOne? 5.5f : SELECTION_SNAP_STEP_COUNT);
				snapTime -= Time.deltaTime;

				bool isSnapped = snapTime < Time.smoothDeltaTime;
				snapDistance = isSnapped? distanceFromTarget : snapDistance;

				if (column.allowHorizontalScroll)
					scrollOffset.x = snapDistance;
				else
					scrollOffset.y = snapDistance;
					
				scrollObj.scroll(scrollOffset);

				if (isSnapped) {
					column.setScrollMoveFinished();
					break;
				}
				yield return null;
			}
		}
	}

	public void OnColumnScrollEnd(kView column)
	{
		int columnIdx = getColumnIndex(column);
		snapToSelection(columnIdx, selectedRow[columnIdx]);
	}

	public void stopScrolling(){
		//StopCoroutine("snapToSelection");
		StopAllCoroutines();
		for(int i = 0; i < columns.Length;i++){
			if(columns[i] != null)
				columns[i].stopScrolling();
		}
	}

	private float easeOutCubic(float start, float end, float value)
	{
		value--;
		end -= start;
		return end * (value * value * value + 1) + start;
	}

	protected IEnumerator snapToSelectionOneByOne(int col, int selIndex)
	{
		kView column = columns[col];
		kScrollableContainer scrollObj = column.getScrollContainer();
		kObject selObject = getObject(col, selIndex);

		if (selObject != null && col < selectors.Length) 
		{
			float selectorCenter = column.allowHorizontalScroll ?
				(selectors[col].transform.position.x + selectors[col].getBounds().center.x) :
				(selectors[col].transform.position.y + selectors[col].getBounds().center.y);
			float selObjCenter = column.allowHorizontalScroll ?
				(selObject.transform.position.x + selObject.getBounds().center.x) :
				(selObject.transform.position.y + selObject.getBounds().center.y);
			float totalDistance = selectorCenter - selObjCenter, movedDistance = 0f;
			float snapTime = SELECTION_SNAP_TIME, startTime = Time.time;
			while (!column.isDragging && Time.time - startTime < snapTime) {
				float newDistance = easeOutCubic(0, totalDistance, (Time.time - startTime) / snapTime);
				scrollObj.scroll((newDistance - movedDistance) * (column.allowHorizontalScroll ? Vector3.right : Vector3.up));
				movedDistance = newDistance;

				yield return null;
			}
			if (Time.time - startTime >= snapTime) {
				scrollObj.scroll((totalDistance - movedDistance) * (column.allowHorizontalScroll ? Vector3.right : Vector3.up));
			}
			column.setScrollMoveFinished();
		}
	}

	protected IEnumerator selectElement(int col, int oldIndex, int newIndex)
	{
		kView column = columns[col];
		kScrollableContainer scrollObj = column.getScrollContainer();
		kObject selObject = getObject(col, newIndex);
		if (selObject == null) {
			int nbElements = dataSource.noRowsInColumn(this, col);

			kObject refObject = getObject(col, oldIndex);
			if (refObject == null) {
				refObject = ((kPickerElement)cachedElements[col][0]).GetComponent<kObject>();
			}
			Rect refBounds = refObject.getBounds();
			float scrollSpeed = 0;
			if(isCircular){
				int distLeft = oldIndex - (newIndex > oldIndex ? newIndex - nbElements : newIndex);
				int distRight = (newIndex < oldIndex ? newIndex + nbElements : newIndex) - oldIndex;
				scrollSpeed = distLeft > distRight ? -0.2f : 0.2f;
			}else
				scrollSpeed = newIndex > oldIndex ? -0.2f : 0.2f;
					
			Vector3 scrollOffset = Vector3.zero;
			if (column.allowHorizontalScroll) {
				scrollOffset.x = scrollSpeed * refBounds.width;
			} else {
				scrollOffset.y = scrollSpeed * refBounds.height;
			}
			do {
				scrollObj.scroll(scrollOffset);
				yield return null;
				selObject = getObject(col, newIndex);
			} while (selObject == null && selectedRow[col] == newIndex);
		}

		if (selectionChanged != null && selectedRow[col] == newIndex) {
			selectionChanged(this, col, oldIndex, newIndex);
		}
		if(selectedRow[col] == newIndex){
			snapToSelection(col, newIndex);
		}
	}

	protected void snapInstantToCurrentSelection(int col)
	{
		kView column = columns[col];
		kScrollableContainer scrollObj = column.getScrollContainer();
		kObject selObject = getObject(col, selectedRow[col]);

		if (scrollObj != null && selObject != null)
		{
			Vector3 scrollOffset = Vector3.zero;

			float selectorCenter = column.allowHorizontalScroll ?
				(selectors[col].transform.position.x + selectors[col].getBounds().center.x) :
				(selectors[col].transform.position.y + selectors[col].getBounds().center.y);
			float selObjCenter = column.allowHorizontalScroll ?
				(selObject.transform.position.x + selObject.getBounds().center.x) :
				(selObject.transform.position.y + selObject.getBounds().center.y);

			var distanceFromTarget = selectorCenter - selObjCenter;

			if (column.allowHorizontalScroll)
				scrollOffset.x = distanceFromTarget;
			else
				scrollOffset.y = distanceFromTarget;

			scrollObj.scroll(scrollOffset);
			column.setScrollMoveFinished();
		}
	}

	public void objectClick(kObject obj){
	}
	
	protected virtual void destroyElement(GameObject obj,int column){
		if(columns[column].getScrollContainer() != null)	columns[column].getScrollContainer().removeObject(obj);
		
		if((activityIndicators == null || activityIndicators.Length <= column || obj != activityIndicators[column].gameObject)
			&& (searchBars == null || searchBars.Length <= column || obj != searchBars[column].gameObject))
			Destroy(obj);
		else{
			obj.SetActive(false);
			obj.transform.parent = transform;
		}
	}

#if UNITY_EDITOR	
	void OnDrawGizmos()
	{
		/*
		for (int i = 0; i < selectors.Length; i++)
			if (selectors[i] != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(selectors[i].transform.position + new Vector3(selectors[i].getBounds().center.x, selectors[i].getBounds().center.y, -3), 3);
			}
		*/
		/*Rect pickerViewPortWorldCoord = new Rect(	transform.position.x, transform.position.y - pickerSize.y * transform.lossyScale.y,
													pickerSize.x * transform.lossyScale.x,pickerSize.y * transform.lossyScale.y);
			
		if(pickerViewPortWorldCoord.width != 0 && pickerViewPortWorldCoord.height != 0){
			Vector3 center = new Vector3(pickerViewPortWorldCoord.x + pickerViewPortWorldCoord.width/2,pickerViewPortWorldCoord.y + pickerViewPortWorldCoord.height/2,0);
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(center,new Vector3(pickerViewPortWorldCoord.width,pickerViewPortWorldCoord.height,2));
		}*/		
	}
#endif
}
