using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class kObjectData : BaseItemData
{
	public class StateData : BaseComponentData
	{
		public StateData(kDataPackage owner,uint ID):base(owner,ID){
				m_component = null;
		}
		
		public override void read(kBinaryReader stream,int dataFormat){
			m_name			= stream.ReadString();
			
			uint 	compType 	= (uint)stream.ReadInt32();
			String 	compName 	= stream.ReadString();
			uint 	compOwnerID = (uint)stream.ReadInt32();
			m_component 	  	= ((kObjectDataPackage) m_owner).getSprite(compOwnerID).getItemByName(compType,compName);
			
			stream.ReadInt32(); // playback (unused)
			
			//if(((kObjectPackage)m_owner).debugObjPackageLoding)
			//	Debug.Log("\t\t ObjectState component name :"+m_name);
		}
		public override uint getType(){return FLAG_TYPE_OBJECTSTATE;}
	};
	
	
	public  StateData[] m_objectStates;
	public int[] m_linkWithIds = null;
	public int m_objType;
	
	public kObjectData (kDataPackage owner,uint ID):base(owner,ID){}
	
	
	public override void read(kBinaryReader stream,int dataFormatVersion)
	{
		m_name = stream.ReadString();
		
		int compCount = stream.ReadInt32();
		m_objectStates = new StateData[compCount];
		for(int i = 0; i < compCount;i++)
		{
			StateData comp = new StateData(m_owner,(uint)i);
			comp.read(stream, dataFormatVersion);
			m_objectStates[i] = comp;
		}
		
		//ignore rigid body type,should be none
		stream.ReadInt32();

		string linkWith = stream.ReadString(); //for linking the parts of the avatar
		m_objType = stream.ReadInt32();//for Object Type (body, face, hand, t-shirt,etc)
		
		//if(((kObjectDataPackage)m_owner).debugObjPackageLoding)
		//	Debug.Log("____________ objDataType =  " + m_objType + "linkWithString = "+linkWith);
		
		if (linkWith.Trim().CompareTo("") != 0)
		{
			string[] parts = linkWith.Split(",".ToCharArray());
			m_linkWithIds = new int[parts.Length];
			
			for(int i=0 ; i < parts.Length; i++) {
				m_linkWithIds[i] =  Convert.ToInt32(parts[i].Trim());
			}
		}
	}
	
	public override uint getType(){return FLAG_TYPE_SOBJECT;}
}

public class kObjectDataPackage : kDataPackage {
	private string 			m_file;
	private kSpriteAsset[] 	m_sprites;
	private kObjectData[] 	m_objectsData;
	
	public kObjectData[] getObjectsData(){
		return m_objectsData;
	}
	
	public void setSourceSprites(kSpriteAsset[] sprites){
		m_sprites = sprites;
	}
	
	public kSpriteAsset getSprite(uint packageID){
		int packType = kDataPackage.dispatchType(packageID);

		if(packType == kDataPackage.DATA_PACKAGE_TYPE_SPRITE){
			int index = kDataPackage.dispatchIndex(packageID);
			index -= 1; //ugly - becaouse 0 is a invalid Sprite ID in SpriteEditor
			
			if (index >= m_sprites.Length || index < 0 || m_sprites[index] == null || !m_sprites[index].isLoaded())
				Debug.LogError("Sprite " + index + " not loaded");
				
			if(index >= 0 && index < m_sprites.Length) return m_sprites[index];	
		}
		return null;
	}
	
	
	public kSpriteAsset getSourceSpriteFor(uint objectID){
		uint uidOwner = BaseItemData.dispatchOwner(objectID);
		
		return getSprite(uidOwner);
	}
	
	public uint getGraphicID(uint objectID,uint state)
	{
		return ((kObjectData)get(objectID)).m_objectStates[(int)state].m_component.getID();
	}
	
	public override BaseItemData get(uint ID) 
	{
		uint uidOwner = BaseItemData.dispatchOwner(ID);
		int packType = kDataPackage.dispatchType(uidOwner);
			
		if(packType == kDataPackage.DATA_PACKAGE_TYPE_SPRITE){
			kSpriteAsset sprite = getSprite(uidOwner);
			if(sprite != null)	return sprite.get(ID);
		}else{
			uint itemType = BaseItemData.dispatchType(ID);
			uint itemIndex= BaseItemData.dispatchIndex(ID);
			
			if(itemType == BaseItemData.FLAG_TYPE_SOBJECT && itemIndex >=0 && itemIndex < m_objectsData.Length) return m_objectsData[itemIndex];
		}
		return null;
	}
	
	public static kObjectDataPackage getObjectPackage(TextAsset binaryFile,kSpriteAsset[] sprites,bool loadSprites) {
		//if(debugObjPackageLoding)
		//	Debug.Log("loadObjectPackage method called");
			
		if (binaryFile != null ) 
		{
			TextAsset asset = binaryFile;
					
			if( asset != null ){
				Stream s = new MemoryStream(asset.bytes);
				kBinaryReader stream = new kBinaryReader(s,System.Text.Encoding.UTF8,false);
					
				int dataFromatVersion= stream.ReadInt32();
				
				int spritesCount = stream.ReadInt32();
#if !DISABLE_CONSOLE_LOG
				Debug.Log("Sprites count:"+spritesCount);
#endif
#if UNITY_EDITOR
				for(int i = 0; i < spritesCount;i++){
					string spriteFullName = stream.ReadString();
					if(loadSprites)
					{
						string spriteName = spriteFullName.Replace(".ksprite",".bytes");
						//spriteName = spriteName.Replace(".bytes","");

						TextAsset spriteBinary = (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/avatars/"+spriteName, typeof(TextAsset));
						if(spriteBinary == null)
							Debug.Log("Failed to load sprite: Assets/Resources/avatars/"+spriteName);

						string path = AssetDatabase.GetAssetPath(spriteBinary);
						if(!string.IsNullOrEmpty(path))
							path = path.Replace(".bytes",".ksprite.asset");
						
						kSpriteAsset sprite = ScriptableObject.CreateInstance<kSpriteAsset>();//new kSpriteNew(Shader.Find("Mobile/Particles/Alpha Blended"));  //scriptable object
						AssetDatabase.CreateAsset(asset,path);
						EditorUtility.SetDirty(asset);

						sprite.spriteBinary = spriteBinary;
						sprite.loadSprite();
						sprite.enableLoadTextureOnDeman = true;
						sprite.textureOnDeman = "avatars/tex_"+spriteName.Replace(".bytes","");
						int idx = kDataPackage.dispatchIndex(sprite.getID()) - 1;
						Debug.Log("Sprite "+spriteName+" loaded at position "+idx);
						sprites[idx] = sprite;
						//if(debugObjPackageLoding)
							//Debug.Log("Create sprite : " + spriteName);
					}
				}
#else
				if(sprites == null || spritesCount > sprites.Length){
					Debug.LogError("ObjectPackage: Some of the needed sprites are not set, set all needed sprites before loading object package.");
					return null;
				}
				for(int i = 0; i < spritesCount;i++){
					string spriteName = stream.ReadString();
				}
#endif
				for(int i = 0; i < sprites.Length;i++)
					if(sprites[i] != null && !sprites[i].isLoaded())				
						sprites[i].loadSprite();

				kObjectDataPackage package = new kObjectDataPackage();
				
				package.m_ID =  stream.ReadUInt32();
				if(dataFromatVersion <= 17)
					package.m_ID = package.m_ID & 0xF;

				package.m_sprites = sprites;
				
				int objectsCount =  stream.ReadInt32();
				package.m_objectsData = new kObjectData[objectsCount];
				for(uint i = 0; i < objectsCount;i++) {
					kObjectData objData = new kObjectData(package,i);
					objData.read(stream,dataFromatVersion);
					package.m_objectsData[i] = objData;
				}
				//if(debugObjPackageLoding)
				//	Debug.Log("ObjectData ID:" + m_ID + "spritesCount:"+spritesCount + "   objectsCount:"+objectsCount );
				stream.Close();
				s.Close();
				return package;
			}else{				
				//if(debugObjPackageLoding)
				Debug.LogError( "Could not open TextAsset");// with filename: " + fileName );
			}
			
		}
		return null;
	}
	
}
