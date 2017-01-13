using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class kObjectPackage : kBehaviourScript
{
	[HideInInspector]
	public TextAsset 	objPackBinary;
		
	public 	kSpriteAsset[]	sprites;
	
	private kObjectDataPackage m_dataPackage;
	
#if UNITY_EDITOR
	public bool objPackageLoaded 	= false;
#endif
	[System.NonSerialized]//debug loding option
	public bool debugObjPackageLoding = false;
	
	public bool isLoaded(){
		return m_dataPackage != null;
	}
	
	public void loadObjectPackage(bool loadSprites = false) {
		m_dataPackage = kObjectDataPackage.getObjectPackage(objPackBinary,sprites,loadSprites);
	}
	
	public kSpriteAsset getSourceSprite(uint objectID){
		return m_dataPackage.getSourceSpriteFor(objectID);
	}
	
	public void setSprite(int id, kSpriteAsset sprite) {
		if(sprites != null) sprites[id] = sprite;
	}
	
	public uint getGraphicID(uint objectID,uint state){
		return m_dataPackage.getGraphicID(objectID,state);
	}
	
	public BaseItemData get(uint ID){
		return m_dataPackage.get(ID);
	}
	
	public int getObjectType (uint ID)
	{
		int objType = -1; //-1 means not a component of the avatar
		BaseItemData item = m_dataPackage.get(ID);
		if(item is kObjectData){
			return ((kObjectData)item).m_objType;
		}
		return objType; //body, face, shoes, etc
	}
	
	public uint getFirstIdOfType(int type)
	{
		for (int i = 0; i< m_dataPackage.getObjectsData().Length; i++) {
			if (m_dataPackage.getObjectsData()[i].m_objType == type) {
				if(debugObjPackageLoding)
					Debug.Log("m_objectsData[i].getID()  = "+ m_dataPackage.getObjectsData()[i].getID().ToString("x"));
				return m_dataPackage.getObjectsData()[i].getID();
			}
		}
		if(debugObjPackageLoding)
			Debug.Log("m_objectsData[i].getID()  = " + 0.ToString("x"));
		return 0;
	}
	
	public int[] getLinkedIds(uint ID) {
		return ((kObjectData)m_dataPackage.get(ID)).m_linkWithIds;
	}
		
	public uint getIDFromIndex(int index) {
		return m_dataPackage.getObjectsData()[index].getID();
	}
		
	protected override void onInit() {
		base.onInit();
		/*if (sprites != null) {//todo
			for (int i = 0 ; i < sprites.Length ; i++) {
				if (sprites[i] != null && !sprites[i].isLoaded())
					sprites[i].loadSprite();
			}
		}*/
		loadObjectPackage();
	}

	// Update is called once per frame
	protected override void onUpdate ()
	{	
#if UNITY_EDITOR
		base.onUpdate();
		objPackageLoaded = isLoaded();
		if(objPackageLoaded)
			m_dataPackage.setSourceSprites(sprites);
		
		if(!Application.isPlaying)
		{
			if(isLoaded()){
				/*string sName = objPackBinary;
				int idx = sName.LastIndexOfAny(new char[]{'/','\\'});
				idx = idx >= 0 ? idx + 1 : 0; 
				sName = sName.Substring(idx, sName.Length - idx);
				sName += " (" + objPackBinary + ")";
				if(gameObject.name.CompareTo(sName) != 0)*/
					gameObject.name = objPackBinary.name;
				
				/*if (sprites.Length != spritesCount) {
					Debug.LogError("You set manually " + sprites.Length + " sprites and the ObjPackage with the name " + sName + " expects " + spritesCount);
				}*/
			}
		}
#endif

	}
}


