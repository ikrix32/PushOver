using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class kFont : kBehaviourScript 
{
#if	ENABLE_CREATE_KSPRITE_ASSET
	public kSprite 	sourceSprite;
#endif
	public kSpriteAsset sprite;

	public string	fontName;
	
	protected uint[] spritesTable = null;
	
#if UNITY_EDITOR
	[System.NonSerialized]
	private string oldFontName;
#endif
	
	protected override void onInit(){
		base.onInit();
#if	ENABLE_CREATE_KSPRITE_ASSET
		if(!Application.isPlaying && sourceSprite != null){
			sprite = kSpriteAssetHelper.getSpriteAsset(sourceSprite);
		}
#endif
		if(fontName != null && fontName.Length > 0){
			gameObject.name = "font_"+fontName;
			load();
		}
	}
	
	// Update is called once per frame
	protected override void onUpdate () {
		base.onUpdate();
#if UNITY_EDITOR
		if(!Application.isPlaying){
			if(fontName != null && fontName.Length > 0){
				if(oldFontName == null || fontName.CompareTo(oldFontName) != 0){
					gameObject.name = "font_"+fontName;
					oldFontName = fontName;
					load();
				}
			}
		}
		if(Application.isPlaying)
#endif
			if(isLoaded())
				gameObject.SetActive(false);
	}
	
	public void load()
	{
		if(sprite == null)
			return;
		
		if(!sprite.isLoaded())
			sprite.loadSprite();
		
		if(spritesTable == null){
			spritesTable = new uint[char.MaxValue + 1];
			for(int i = 0; i < char.MaxValue + 1;i++)
				spritesTable[i] = 0xFFFFFFFF;
		}
		BaseItemData[] sprites = sprite.getAllContainig(BaseItemData.FLAG_TYPE_FRAME,fontName + "_");
		
		for(int i = 0; i < sprites.Length;i++){
			FrameData frame = (FrameData)sprites[i];
			//try{
			uint charID = 0;
			if(UInt32.TryParse(frame.m_name.Replace(fontName + "_",""),out charID)){
				//	uint charID =  UInt32.Parse();
				spritesTable[charID] = frame.getID();
			}
			//}catch(Exception ex){}
		}
	}
	
	public bool isLoaded(){
		return sprite != null && sprite.isLoaded() && fontName != null && spritesTable != null;
	}
	
	public BaseItemData getMeshForChar(char ch){
		if(isLoaded() && spritesTable[(int)ch] != 0xFFFFFFFF){
			return sprite.get(spritesTable[(int)ch]);// getItemByName(BaseItemData.FLAG_TYPE_FRAME,fontName + "_" + c);
		}else{
			if(sprite == null || !sprite.isLoaded())
				Debug.Log("Font errorr:Source sprite not loaded.");
			
			if(fontName == null)
				Debug.Log("Font errorr:Font name not set.");
			
			if(spritesTable == null)
				Debug.Log("Font sprites not loaded.");
			
			if(spritesTable != null && spritesTable[(int)ch] == 0xFFFFFFFF){
				load ();
				Debug.Log("Character "+ch+" not found in font "+fontName);
				if(ch != 32)return getMeshForChar(' ');
			}
		}
		return null;
	}
	
	public float stringWidth(string word)
	{
		if (word == null)
			return 0;
		float wordLength = 0;
		for (int i = 0; i < word.Length; i++)
		{
			if (word[i] == '\n')
				continue;
			FrameData frame = (FrameData)getMeshForChar(word[i]);
			if (frame != null && frame.m_components.Length > 0)
				wordLength += frame.m_components[0].m_compPos.x + (frame.m_components[0].m_component.m_bounds.width * frame.m_components[0].m_scaleX);
		}
		return (int)wordLength;
	}
	
	public float getHeight()
	{
		FrameData frame = (FrameData)getMeshForChar('A');
		if (frame != null && frame.m_components.Length > 0)
			return (frame.m_components[0].m_component.m_bounds.height * frame.m_components[0].m_scaleY);
		return 0;
	}
}
