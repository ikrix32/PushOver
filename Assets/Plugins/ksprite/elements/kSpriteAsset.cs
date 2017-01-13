using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kSpriteAsset : ScriptableObject
{
	public 	const int   kSpriteLayer = 30;

	public TextAsset	spriteBinary;
	public Texture	 	sourceTexture;

	private kSpriteData m_spriteData;

	public bool 	enableLoadTextureOnDeman;
	public string	textureOnDeman;

#if UNITY_EDITOR	
	[System.NonSerialized]
	private string[]	allAvailableGraphicItems = null;

	private string[]	frameNames = null;
	private string[]	sequenceNames = null;
#endif	

	public bool isLoaded(){
		return m_spriteData != null;
	}

	void OnEnable() {
		loadSprite();
	}

	void OnDestroy()
	{
		if(m_spriteData != null){
			m_spriteData.dropReference();
			m_spriteData = null;
		}
		destroyTexture();
		spriteBinary = null;
	}

	public void setSourceTexture(Texture texture){
		sourceTexture = texture;
		//material.mainTexture = texture;
	}

	public void loadSprite(){
		if(m_spriteData != null){
			m_spriteData.dropReference();
			m_spriteData = null;
		}
		if(spriteBinary != null)
			m_spriteData = kSpriteData.getSpriteData(spriteBinary);

		if(m_spriteData != null){
			//name = spriteBinary.name;
			m_spriteData.grabReference();
		}
		
#if UNITY_EDITOR
		updateGraphicNames();
#endif
	}

	public uint getID(){
		return m_spriteData != null ? m_spriteData.getID() : 0;
	}

	protected int m_referenceCount = 0;
	public void grabReference(){
		m_referenceCount++;
		if(enableLoadTextureOnDeman)
		{
			if(sourceTexture == null)
			{
				Texture tex = (Texture)Resources.Load(textureOnDeman);
				#if !DISABLE_CONSOLE_LOG
				if(tex != null)
					Debug.Log("Sprite texture loaded: "+textureOnDeman);
				else
					Debug.LogError("Sprite texture can't be loaded: "+textureOnDeman);
				#endif
				setSourceTexture(tex);
			}
		}
	}

	public void dropReference(){
		m_referenceCount--;
		if(m_referenceCount <= 0){
			m_referenceCount= 0;
			if(enableLoadTextureOnDeman){
				destroyTexture();
			}
		}
	}

	public void destroyTexture(){
		if(enableLoadTextureOnDeman && sourceTexture != null){
			if (Application.isEditor){
				//DestroyImmediate(sourceTexture);
			}else{
				//DestroyImmediate(sourceTexture);
			}
			sourceTexture = null;
		}
	}

	public FrameData[] GetAllFrames(){
		return m_spriteData.getFrames();
	}

#if UNITY_EDITOR	
	public void updateGraphicNames(){
		if(isLoaded()){
			//update available items
			allAvailableGraphicItems = new string[m_spriteData.getFrames().Length + m_spriteData.getSequences().Length];
			frameNames = new string[m_spriteData.getFrames().Length];
			for(int i = 0; i < m_spriteData.getFrames().Length;i++){
				allAvailableGraphicItems[i] = "FRAME: " + m_spriteData.getFrames()[i].m_name;
				frameNames[i] = m_spriteData.getFrames()[i].m_name;
			}
			System.Array.Sort(allAvailableGraphicItems,0,m_spriteData.getFrames().Length,new MyComparer()/*delegate(string str1, string str2){
				return str1.CompareTo(str2);
			}*/);
			System.Array.Sort(frameNames, delegate(string str1, string str2){
				return str1.CompareTo(str2);
			});
			sequenceNames = new string[m_spriteData.getSequences().Length];
			for(int i = 0; i < m_spriteData.getSequences().Length; i++){
				allAvailableGraphicItems[m_spriteData.getFrames().Length + i] = "ANIM: " + m_spriteData.getSequences()[i].m_name;
				sequenceNames[i] = m_spriteData.getSequences()[i].m_name;
			}
			System.Array.Sort(sequenceNames, delegate(string str1, string str2){
				return str1.CompareTo(str2);
			});
		}else{
			allAvailableGraphicItems = null;
			frameNames = null;
			sequenceNames = null;
		}
	}
#endif

	#if UNITY_EDITOR
	public class MyComparer : IComparer  {
		// Calls CaseInsensitiveComparer.Compare with the parameters reversed. 
		int IComparer.Compare( object x, object y )  {
			string str1 = (string) x;
			string str2 = (string) y;
			return str1.CompareTo(str2);
		}
	}
	#endif
	
	public BaseItemData get(uint ID)
	{
		if(!isLoaded()) return null;

		BaseItemData item = m_spriteData.get(ID);
		#if DEBUG_LOG_SPRITES_USAGE
		if(item != null && isLoaded ()){
		string sceneName = null;
		GameObject obj = gameObject;
		while(obj.transform.parent != null && sceneName == null){
		obj = obj.transform.parent.gameObject;
		if(obj.GetComponent<kScene>() != null){
		sceneName = obj.name;
		}
		}
		kAppManager.usedItem(sceneName,spriteBinary.name,item);
		}
		#endif
		return item;
	}


	public BaseItemData getItemByName(uint itemType,string name){
		if(!isLoaded()) return null;

		return m_spriteData.getItemByName(itemType,name);	
	}

	public BaseItemData[] getAllContainig(uint itemType,string name){
		if(!isLoaded()) return new BaseItemData[0];
		return m_spriteData.getAllContainig(itemType,name);
	}
	
	#if UNITY_EDITOR	
	public string[] getGraphicResNames(int type){
		switch((uint)type){
		case BaseItemData.FLAG_TYPE_FRAME: 		return frameNames;
		case BaseItemData.FLAG_TYPE_SEQUENCE: 	return sequenceNames;
		}
		return null;
	}

	public int getNameIndex(int itemID){
		BaseItemData item = get((uint)itemID);
		if(item != null){
			uint itemType = BaseItemData.dispatchType((uint)itemID);
			string[] namesList = null;
			switch(itemType){
			case BaseItemData.FLAG_TYPE_FRAME: 		namesList = frameNames;break;
			case BaseItemData.FLAG_TYPE_SEQUENCE: 	namesList = sequenceNames;break;
			}
			if(namesList != null)
				for(int i = 0; i < namesList.Length;i++)
					if(item.m_name.CompareTo(namesList[i]) == 0)
						return i;
		}
		return 0;
	}
	/** Returns item ID from type and name index in ordered list of item names */
	public int getGraphicResID(int type,int nameIndex){
		BaseItemData item = getItemByName((uint)type,getGraphicResNames(type)[nameIndex]);
		if(item != null)
			return (int)item.getID();
		return 0;
	}

	public string[] getAvailableGraphicItems(){
		return allAvailableGraphicItems;
	}

	public string stringIDToItemName(string id){
		uint itemID = (uint)kSpriteObject.StringToInt(id);
		uint itemType 	= BaseItemData.dispatchType(itemID);

		switch(itemType){
		case BaseItemData.FLAG_TYPE_FRAME: 		return "FRAME: " + m_spriteData.get(itemID).m_name;
		case BaseItemData.FLAG_TYPE_SEQUENCE: 	return "ANIM: " + m_spriteData.get(itemID).m_name;
		}
		return null;
	}

	public int indexOfGraphicItem(string itemID){
		if(itemID.Contains("0x"))
			return indexOfGraphicItem((uint)kSpriteObject.StringToInt(itemID));
		else
			return indexOfGraphicItem(itemIDFor(itemID));
	}

	public int indexOfGraphicItem(uint itemID){
		BaseItemData item = get (itemID);
		if (item == null)
			return 0;
		string itemName = (item.getType() == BaseItemData.FLAG_TYPE_FRAME ? "FRAME: " : "ANIM: ") + item.m_name;
		for(int i = 0; i < allAvailableGraphicItems.Length;i++){
			if(allAvailableGraphicItems[i].CompareTo(itemName) == 0)
				return i;
		}
		return 0;
	}

	public uint itemIDFor(string item){
		if(item.Contains("FRAME: ")){
			string name = item.Substring("FRAME: ".Length,item.Length - "FRAME: ".Length);
			return getItemByName(BaseItemData.FLAG_TYPE_FRAME,name).getID();
		}
		if(item.Contains("ANIM: ")){
			string name = item.Substring("ANIM: ".Length,item.Length - "ANIM: ".Length);
			return getItemByName(BaseItemData.FLAG_TYPE_SEQUENCE,name).getID();
		}
		return 0x0300000;
	}

	public uint itemIDAtIndex(int index){
		if(index < m_spriteData.getFrames().Length){
			return m_spriteData.getFrames()[index].getID();
		}else if(index - m_spriteData.getFrames().Length < m_spriteData.getSequences().Length){
			return m_spriteData.getSequences()[index - m_spriteData.getFrames().Length].getID();
		}
		return 0;
	}
	#endif	
}
