using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;


public enum PlaybackMode{
	ANIM_PLAY_ONCE = 0,
	ANIM_PLAY_LOOP,
};

public enum PlaybackDir{
	ANIM_PLAY_FW = 0,
	ANIM_PLAY_BK,
};

[ExecuteInEditMode]
public class kSpriteObject : kPickerItem
{
	public const int MAX_ANIMATION_FRAME_TIME = 100;//in ms
	public kSpriteItem 	m_defaultAnim = new kSpriteItem();

	protected int	currentGraphicID = 0;
	
	private int		currentFrame	  		= 0;
	private kMesh 	currentFrameMesh = new kMesh();
	private float 	currentFrameTime 	= 0;
	private float 	currentFrameAlpha = 1.0f;
	
	private bool	animPlaying			= false;
	
	protected PlaybackMode 	playbackMode = PlaybackMode.ANIM_PLAY_ONCE;
	protected PlaybackDir	playbackDir  = PlaybackDir.ANIM_PLAY_FW;
	
	protected System.Action onAnimationComplete = null;
	
	protected override void onInit(){
		base.onInit();
#if UPDATE_SPRITE_FRAME_IDS
		validateSpriteItem(m_defaultAnim,sourceSprite,gameObject);
#endif
		play(m_defaultAnim.id,PlaybackMode.ANIM_PLAY_LOOP,PlaybackDir.ANIM_PLAY_FW);
	}

	public static bool validateSpriteItem(kSpriteItem gRes,kSpriteAsset sourceSprite,GameObject gObject)
	{
		if(Application.isPlaying)
			return false;

		//Debug.Log("Validate sprite item");
		if(gRes.name == null || gRes.name.Length == 0 || sourceSprite == null){
			Debug.Log("Null res name:"+gObject.name);
			return false;
		}
		//todo: optimize
		BaseItemData gData = sourceSprite.getItemByName((uint)gRes.type,gRes.name);
		BaseItemData gData1 = sourceSprite.get((uint)gRes.id);

		if( gData != gData1)
		{
			if(gData == null){
				if(gData1 == null)
					gData1 = sourceSprite.get(0x0300000);

				if(gData1 != null)
				{
					Debug.LogWarning(	"Can't find "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "("+gRes.id.ToString("x")+")"+
					                 " referenced by "+ gObject.name+".\n Reference updated to use "+
					               (gData1.getType() == BaseItemData.FLAG_TYPE_FRAME ? "frame ": "sequence ")+gData1.m_name);
					gRes.type = (int)gData1.getType();
					gRes.id = (int)gData1.getID();
					gRes.name = gData1.m_name;
					return true;
				}
			}else{
				if(gData1 == null){
					Debug.LogWarning("Updated object "+gObject.name+" graphic id for "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "(oldId:"+gRes.id.ToString("x")+",newId:"+gData.getID()+")");
					gRes.id = (int)gData.getID();
					return true;
				}else{
					Debug.LogWarning("Updated "+gObject.name+" graphic id? for "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "(oldId:"+gRes.id.ToString("x")+",newId:"+gData.getID()+")");
					gRes.id = (int)gData.getID();
					return true;
				}
			}
		}
		return false;
	}

	public int getCurrentAnim(){
		return currentGraphicID;
	}
	
	public bool isPlaying(){
		return currentGraphicID != 0 && animPlaying;
	}
	
	public void pause(){
		animPlaying = false;
	}
	
	public void resume(){
		animPlaying = true;
	}

	public void playOnce(int graphicID){
		play(graphicID);
	}

	public void playInLoop(int graphicID){
		play(graphicID,PlaybackMode.ANIM_PLAY_LOOP);
	}

	public void play(int graphicID,PlaybackMode pMode = PlaybackMode.ANIM_PLAY_ONCE,PlaybackDir pDir = PlaybackDir.ANIM_PLAY_FW,System.Action onComplete=null,int timeOffset = 0){
		if(sprite == null /*|| !sourceSprite.isLoaded()*/|| sprite.get((uint)graphicID) == null){
			currentGraphicID = 0;
			return;
		}
#if UNITY_EDITOR
		if(lastSourceSprite != sprite){
			lastSourceSprite = sprite;
			updateTexture();
		}
#endif
		//BaseItemData graphicData = sourceSprite.get(graphicID);
		
		currentGraphicID = graphicID;
		playbackMode = pMode;
		playbackDir	 = pDir;
		
		uint itemType 	= BaseItemData.dispatchType((uint)graphicID);
		animPlaying = true;
		onAnimationComplete = onComplete;
		
		currentFrameAlpha = 1.0f;
		waitOneFrame = true;
		switch(itemType){
			case BaseItemData.FLAG_TYPE_SEQUENCE: 
			{
				currentFrame = getFrameAt(timeOffset,playbackMode,playbackDir);
				currentFrameTime = 0;//Time.time;
			}break;		
		}
		updateMesh();
	}
	
	protected int getFrameAt(int timeOffset,PlaybackMode pMode,PlaybackDir pDir){
		int frame = 0;
		if(BaseItemData.dispatchType((uint)currentGraphicID) == BaseItemData.FLAG_TYPE_SEQUENCE)
		{
			SequenceData sequence = (SequenceData)sprite.get((uint)currentGraphicID);
			frame = pDir == PlaybackDir.ANIM_PLAY_FW ? 0 : sequence.m_components.Length - 1;
			SequenceData.SequenceComponent frameInfo = (SequenceData.SequenceComponent)sequence.m_components[frame];
			
			while(timeOffset >= frameInfo.m_duration){
				timeOffset -= frameInfo.m_duration;
				//go to next frame
				if((pDir == PlaybackDir.ANIM_PLAY_FW && frame < sequence.m_components.Length - 1)
				|| (pDir == PlaybackDir.ANIM_PLAY_BK && frame > 0)){
					frame += (pDir == PlaybackDir.ANIM_PLAY_FW ? 1: -1);
				}else{
					//anim ended
					if(pMode == PlaybackMode.ANIM_PLAY_LOOP){
						frame = (pDir == PlaybackDir.ANIM_PLAY_FW ? 0: sequence.m_components.Length - 1);
					}else
						return frame;
				}
				frameInfo = (SequenceData.SequenceComponent)sequence.m_components[frame];
			}
		}
		return frame;
	}
	
	public virtual void setAlpha(float alpha){
		Color x = getBlendingColor();
		x.a = alpha;
		setBlendingColor(x);
	}

#if UNITY_EDITOR
	protected override void reset()
	{//source sprite changed stop animation
		if(!Application.isPlaying){
			currentGraphicID = 0;//stop animation
		}
		//defaultAnim = "0x0300000";
	}
#endif
	
	protected override void onUpdate()
	{
		base.onUpdate();
		
		if(sprite == null || !sprite.isLoaded()) return;
		
		//if(gameObject.name == "female_avatar")
		//	Debug.Log("KKT:"+m_defaultAnim.id.ToString("x"));
		
		if(currentGraphicID != 0 && animPlaying)
		{
			uint itemType 	= BaseItemData.dispatchType((uint)currentGraphicID);
		
			switch(itemType){
				case BaseItemData.FLAG_TYPE_FRAME: 
				{
					if(onAnimationComplete != null && animPlaying && playbackMode != PlaybackMode.ANIM_PLAY_LOOP)
						onAnimationComplete();
					animPlaying = (playbackMode == PlaybackMode.ANIM_PLAY_LOOP);
 				}break;
			
				case BaseItemData.FLAG_TYPE_SEQUENCE: 
				{
					updateSequence();
				}break;
				
				case BaseItemData.FLAG_TYPE_ANIMATION: 
				{	//updateAnimation(ID);
				}break;
			}
		}
	}

	private bool waitOneFrame = false;
	protected override void onLateUpdate(){
		base.onLateUpdate();
		waitOneFrame = false;
	}

	// Update is called once per frame
	private void updateSequence() {
		if(!animPlaying)
			return;

		SequenceData sequence = (SequenceData)sprite.get((uint)currentGraphicID);
		SequenceData.SequenceComponent frameInfo = (SequenceData.SequenceComponent)sequence.m_components[currentFrame];

		float dt = Mathf.Min(Time.deltaTime * 1000, MAX_ANIMATION_FRAME_TIME);

		if(!waitOneFrame)//wait one frame before updating animation time,this will fix synchronization issues between animations played on multiple gameObjects
			currentFrameTime = currentFrameTime + dt; //(Time.time - currentFrameTime) * 1000;

		//go to next frame
		if(currentFrameTime >= frameInfo.m_duration){
			if((playbackDir == PlaybackDir.ANIM_PLAY_FW && currentFrame < sequence.m_components.Length - 1)
			|| (playbackDir == PlaybackDir.ANIM_PLAY_BK && currentFrame > 0)){
				currentFrame += (playbackDir == PlaybackDir.ANIM_PLAY_FW ? 1: -1);
				currentFrameTime = currentFrameTime - frameInfo.m_duration;//Time.time;
				updateMesh();
			}else{
				//anim ended
				if(playbackMode == PlaybackMode.ANIM_PLAY_LOOP){
					currentFrame = (playbackDir == PlaybackDir.ANIM_PLAY_FW ? 0: sequence.m_components.Length - 1);
					currentFrameTime = currentFrameTime - frameInfo.m_duration;// Time.time;
					updateMesh();
				}else{
					if(animPlaying) {
						animPlaying = false;
						if (onAnimationComplete != null)
							onAnimationComplete();
					}
				}
			}
		}
		//if(animPlaying)
		updateFrameTransforms();
	}
	//apply current frame transform to mesh
	private void updateFrameTransforms()
	{
		if (!(sprite.get((uint)currentGraphicID) is SequenceData))
			return;
		SequenceData s = (SequenceData)sprite.get((uint)currentGraphicID);
		SequenceData.SequenceComponent frameInfo	= (SequenceData.SequenceComponent)s.m_components[currentFrame];
		SequenceData.SequenceComponent nextFrameInfo= null;
		
		//update frame transform only if the sequence use interpolations between 
		//frames or the mesh was changed
		if(!meshChanged && !frameInfo.m_interpTransforms)
			return;
		
		if(frameInfo.m_interpTransforms && currentFrame + 1 < s.m_components.Length){
			nextFrameInfo = (SequenceData.SequenceComponent)s.m_components[currentFrame + 1];
		}
		
		float easPos 	 = 0;
		
		if(playbackDir == PlaybackDir.ANIM_PLAY_FW){
			easPos = Mathf.Clamp01(currentFrameTime * 1.0f/ frameInfo.m_duration);
		}else{
			easPos = Mathf.Clamp01((frameInfo.m_duration - currentFrameTime) * 1.0f / frameInfo.m_duration);
		}
		
		float 	posX	= (frameInfo.m_compPos.x + easPos * (nextFrameInfo != null ? (nextFrameInfo.m_compPos.x - frameInfo.m_compPos.x):0));
		float 	posY	= (frameInfo.m_compPos.y + easPos * (nextFrameInfo != null ? (nextFrameInfo.m_compPos.y - frameInfo.m_compPos.y):0));
		float 	scaleX 	= frameInfo.m_scaleX 	 + easPos * (nextFrameInfo != null ? (nextFrameInfo.m_scaleX - frameInfo.m_scaleX) : 0);
		float 	scaleY 	= frameInfo.m_scaleY 	 + easPos * (nextFrameInfo != null ? (nextFrameInfo.m_scaleY - frameInfo.m_scaleY) : 0);
		float 	angle	= -1f * (frameInfo.m_angle+ easPos * (nextFrameInfo != null ? (nextFrameInfo.m_angle - frameInfo.m_angle) : 0));
		float alpha = (frameInfo.m_blend + easPos * (nextFrameInfo != null ? (nextFrameInfo.m_blend - frameInfo.m_blend): 0))/255.0f;
			
		if(currentFrameMesh.vertices != null)
		{
			if(objMesh.vertices == currentFrameMesh.vertices)
				objMesh.vertices = new Vector3[currentFrameMesh.vertices.Length];
			
			Rect meshCorners = new Rect();
			for(int i = 0; i < currentFrameMesh.vertices.Length;i++){
				objMesh.vertices[i] = currentFrameMesh.vertices[i];
				
				//scale
				float newX = currentFrameMesh.vertices[i].x * scaleX;
				float newY = currentFrameMesh.vertices[i].y * scaleY;
			
				//rotate
				objMesh.vertices[i].x = Mathf.Cos(angle * Mathf.Deg2Rad) * (newX) - Mathf.Sin( angle * Mathf.Deg2Rad) * (newY);   
	        	objMesh.vertices[i].y = Mathf.Sin(angle * Mathf.Deg2Rad) * (newX) + Mathf.Cos( angle * Mathf.Deg2Rad) * (newY); 
				
				//update pos in frame
				objMesh.vertices[i].x += posX;//frameInfo.m_compPos.x;
				objMesh.vertices[i].y -= posY;//frameInfo.m_compPos.y;
				
				if(i == 0)
					meshCorners = new Rect(	objMesh.vertices[i].x,objMesh.vertices[i].y,objMesh.vertices[i].x,objMesh.vertices[i].y);
				meshCorners = updateCorners(objMesh,meshCorners,i,1);
			}
			if(currentFrameAlpha != alpha)
				updateVertsColor(/*blendingColor*/Color.white,alpha);
			currentFrameAlpha = alpha;
			objMesh.center.x = (meshCorners.x + meshCorners.width)/2;
			objMesh.center.y = (meshCorners.y + meshCorners.height)/2;
			objMesh.size.x	 = (objMesh.center.x - meshCorners.x)*2;
			objMesh.size.y	 = (objMesh.center.y - meshCorners.y)*2;
			
			meshChanged = true;
		}
	}
	
	protected override void updateMesh()
	{
		FrameData frame = getCurrentFrameData();
		if(frame != null){
			updateFrameMesh(frame);
			if(BaseItemData.dispatchType((uint)currentGraphicID) == BaseItemData.FLAG_TYPE_SEQUENCE)
				updateFrameTransforms();
		}
	}
		
	public void updateFrameMesh(FrameData frame)
	{	
		if(sprite.sourceTexture == null || frame == null) return;
	
		Vector2 textureSize = new Vector2(sprite.sourceTexture.width,sprite.sourceTexture.height);
		
		int noModules = frame.m_components != null? frame.m_components.Length : 0;
		
		currentFrameMesh.vertices = new Vector3[noModules * 4];
		currentFrameMesh.triangles= new int[noModules * 6];
		currentFrameMesh.UVs	  = new Vector2[noModules * 4];
		currentFrameMesh.colors	  = new Color[noModules * 4];
		
		Rect meshCorners = new Rect();
		for(int i = 0; i < noModules;i++){
			FrameData.FrameComponent modInfo = (FrameData.FrameComponent)frame.m_components[i];
			ModuleData module = (ModuleData)modInfo.m_component;
			//Verts
			updateMeshVerts(modInfo,i * 4);
			
			//indices
			currentFrameMesh.triangles[i * 6] = i* 4; 			currentFrameMesh.triangles[i * 6 + 1] = i* 4 + 3; 
			currentFrameMesh.triangles[i * 6 + 2] = i* 4 + 2;	currentFrameMesh.triangles[i * 6 + 3] = i* 4 + 0;
			currentFrameMesh.triangles[i * 6 + 4] = i* 4 + 2;	currentFrameMesh.triangles[i * 6 + 5] = i* 4 + 1;
			
			//UVs
			Rect mBounds = module.m_bounds;
			mBounds.x = mBounds.x / textureSize.x;
			mBounds.y = 1.0f - (( mBounds.y + mBounds.height) / textureSize.y);
			
			mBounds.width = mBounds.width / textureSize.x;
			mBounds.height= mBounds.height / textureSize.y;
		
			currentFrameMesh.UVs[i * 4 + 3] = new Vector2(mBounds.x ,mBounds.y + mBounds.height);
			currentFrameMesh.UVs[i * 4 + 2] = new Vector2(mBounds.x + mBounds.width,mBounds.y + mBounds.height);
			currentFrameMesh.UVs[i * 4 + 1] = new Vector2(mBounds.x + mBounds.width,mBounds.y);
			currentFrameMesh.UVs[i * 4 + 0] = new Vector2(mBounds.x,mBounds.y);
			
			if((modInfo.m_transformFlag & BaseItemData.FLIP_VERTICAL_FLAG) != 0){
				var temp = currentFrameMesh.UVs[i * 4];
				currentFrameMesh.UVs[i * 4] = currentFrameMesh.UVs[i * 4 + 3];
				currentFrameMesh.UVs[i * 4 + 3] = temp;		temp = currentFrameMesh.UVs[i * 4 + 1];
				currentFrameMesh.UVs[i * 4 + 1] = currentFrameMesh.UVs[i * 4 + 2];
				currentFrameMesh.UVs[i * 4 + 2] = temp;
			}
			if((modInfo.m_transformFlag & BaseItemData.FLIP_HORIZONTAL_FLAG) != 0){
				var temp = currentFrameMesh.UVs[i * 4];
				currentFrameMesh.UVs[i * 4] = currentFrameMesh.UVs[i * 4 + 1];
				currentFrameMesh.UVs[i * 4 + 1] = temp;		temp = currentFrameMesh.UVs[i * 4 + 2];
				currentFrameMesh.UVs[i * 4 + 2] = currentFrameMesh.UVs[i * 4 + 3];
				currentFrameMesh.UVs[i * 4 + 3] = temp;
			}
			float modAlpha = modInfo.m_blendingColor * 1.0f/255;
			Color comp = new Color(1/*blendingColor.r*/,1/*blendingColor.g*/,1/*blendingColor.b*/,1/*blendingColor.a*/ * modAlpha);
			//colors
			currentFrameMesh.colors[i * 4] 		= comp;	currentFrameMesh.colors[i * 4 + 1] 	= comp;
			currentFrameMesh.colors[i * 4 + 2] 	= comp;	currentFrameMesh.colors[i * 4 + 3] 	= comp;
			
			if(i == 0)
				meshCorners = new Rect(currentFrameMesh.vertices[i].x,currentFrameMesh.vertices[i].y,
							currentFrameMesh.vertices[i].x,currentFrameMesh.vertices[i].y);
			meshCorners = updateCorners(currentFrameMesh,meshCorners,i * 4,4);
		}
		currentFrameMesh.center.x = (meshCorners.x + meshCorners.width)/2;
		currentFrameMesh.center.y = (meshCorners.y + meshCorners.height)/2;
		currentFrameMesh.size.x	 = (currentFrameMesh.center.x - meshCorners.x)*2;
		currentFrameMesh.size.y	 = (currentFrameMesh.center.y - meshCorners.y)*2;
		
		objMesh.vertices	= currentFrameMesh.vertices;
		objMesh.triangles 	= currentFrameMesh.triangles;
		objMesh.UVs			= currentFrameMesh.UVs;
		objMesh.colors		= currentFrameMesh.colors;
		objMesh.center		= currentFrameMesh.center;
		objMesh.size		= currentFrameMesh.size;
		
		meshChanged = true;
	}
	
	private void updateMeshVerts(FrameData.FrameComponent modInfo,int index){
		ModuleData module = (ModuleData)modInfo.m_component;
		
		currentFrameMesh.vertices[index + 3] = new Vector3(0, 0,0);
		currentFrameMesh.vertices[index + 2] = new Vector3(module.m_bounds.width,0,0);
		currentFrameMesh.vertices[index + 1] = new Vector3(module.m_bounds.width,module.m_bounds.height,0);
		currentFrameMesh.vertices[index + 0] = new Vector3(0,module.m_bounds.height,0);
			
		for(int i = 0; i < 4;i++){
			//scale
			float newX = currentFrameMesh.vertices[index + i].x * modInfo.m_scaleX;
			float newY = currentFrameMesh.vertices[index + i].y * modInfo.m_scaleY;
		
			//rotate
			currentFrameMesh.vertices[index + i].x = Mathf.Cos( modInfo.m_angle * Mathf.Deg2Rad) * (newX) - Mathf.Sin(modInfo.m_angle*Mathf.Deg2Rad) * (newY);   
        	currentFrameMesh.vertices[index + i].y = Mathf.Sin( modInfo.m_angle * Mathf.Deg2Rad) * (newX) + Mathf.Cos(modInfo.m_angle*Mathf.Deg2Rad) * (newY); 
			//update pos in frame
			currentFrameMesh.vertices[index + i].x += modInfo.m_compPos.x;
			currentFrameMesh.vertices[index + i].y  = -1 * (currentFrameMesh.vertices[index + i].y + modInfo.m_compPos.y);
		}
	}
	
	void updateVertsColor(Color color,float alpha){
		FrameData frame = getCurrentFrameData();
		if(frame != null){
			if(objMesh.colors != null && frame.m_components != null)
			{
				for(int i = 0; i < frame.m_components.Length;i++){
					FrameData.FrameComponent modInfo = (FrameData.FrameComponent)frame.m_components[i];
					if(modInfo != null && i * 4 + 3 < objMesh.colors.Length){
						float modAlpha = modInfo.m_blendingColor * 1.0f/255;
						float a = color.a * alpha * modAlpha;
						//Color comp = new Color(color.r,color.g,color.b,color.a * alpha * modAlpha);
						for(int j = 0; j < 4;j++){
							objMesh.colors[i * 4 + j].a = a;
							objMesh.colors[i * 4 + j].r = color.r;
							objMesh.colors[i * 4 + j].g = color.g;
							objMesh.colors[i * 4 + j].b = color.b;
						}
					}
				}
			}
		}else{
			if(objMesh.colors != null){
				for(int i = 0; i < objMesh.colors.Length/4;i++){
					for(int j = 0; j < 4;j++){
						objMesh.colors[i * 4 + j].a = color.a * alpha;
						objMesh.colors[i * 4 + j].r = color.r;
						objMesh.colors[i * 4 + j].g = color.g;
						objMesh.colors[i * 4 + j].b = color.b;
					}
				}
			}
		}
		colorsChanged = true;
	}

	private FrameData getCurrentFrameData(){
		if(currentGraphicID == 0 || sprite == null || !sprite.isLoaded()) 
			return null;
		
		uint itemType 	= BaseItemData.dispatchType((uint)currentGraphicID);
		BaseItemData graphicData = sprite.get((uint)currentGraphicID);
		
		switch(itemType){
			case BaseItemData.FLAG_TYPE_FRAME: 
			{
				return (FrameData)graphicData;
			}//break;
			
			case BaseItemData.FLAG_TYPE_SEQUENCE: 
			{
				SequenceData sequence = (SequenceData)graphicData;
				if(sequence != null && sequence.m_components.Length > currentFrame){
					SequenceData.SequenceComponent frameInfo = (SequenceData.SequenceComponent)sequence.m_components[currentFrame];
					return (FrameData)frameInfo.m_component;
				}
			}break;
		}

		return null;
	}
	
	public int getCurrentAnimID(){
		return currentGraphicID;
	}
	
	public static int StringToInt (string num) {
		if(num == null || num.Length < 1)return 0;
		if(num.Contains("0x"))
			num = num.Substring(num.IndexOf("0x") + 2);
		int a = 0;
		try{
		 a = System.Int32.Parse( num, NumberStyles.AllowHexSpecifier );
		}catch(Exception ex){
			Debug.LogError("Err:"+ex);
		}
		return a;
	}
}
