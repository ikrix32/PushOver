using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class BaseItemData 
{
	public const uint ITEM_OWNER_MASK		= 0xFF000000;
	public const uint ITEM_TYPE_FLAG_MASK	= 0x00F00000;
	public const uint ITEM_INDEX_MASK		= 0x000FFFFF;
	public const int  ITEM_OWNER_SHIFT   	= 24;
	public const int  ITEM_TYPE_FLAG_SHIFT	= 20;
	
	public const uint FLIP_VERTICAL_FLAG	= 1;
	public const uint FLIP_HORIZONTAL_FLAG	= 2;

	public const uint FLAG_TYPE_IMAGE			= 1<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_MODULE			= 2<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_FRAME			= 3<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_FRAMECOMPONENT	= 4<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_SEQUENCE		= 5<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_SEQUENCECOMPONENT= 6<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_ANIMATION		= 7<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_ANIMATIONCOMPONENT=8<<ITEM_TYPE_FLAG_SHIFT;

	public const uint FLAG_TYPE_SOBJECT			= 9<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_OBJECTSTATE		= 10<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_SLAYER			= 11<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_LAYEROBJECT		= 12<<ITEM_TYPE_FLAG_SHIFT;
	//bla bla
	public const uint FLAG_TYPE_SCENEOBJECT		= 13<<ITEM_TYPE_FLAG_SHIFT;
	public const uint FLAG_TYPE_SCENELAYER		= 14<<ITEM_TYPE_FLAG_SHIFT;
	
	public 	kDataPackage m_owner;
	private uint		 m_ID; //index in owner item list

	public	string	m_name;
	public	Rect	m_bounds;

	private BaseItemData(){}
	
	public BaseItemData(kDataPackage owner,uint ID){
			m_owner = owner;
			m_ID	= packID(owner.getID(),getType(),ID);
			m_name	= null;
	}
	
	public abstract void	read(kBinaryReader stream,int dataFormat);
	public abstract	uint	getType();
	
	public virtual uint	getID(){
		return (m_owner.getID()<<ITEM_OWNER_SHIFT)|getType()|m_ID;
	}			

	public static uint dispatchOwner(uint objectID){
		return (objectID&ITEM_OWNER_MASK)>>ITEM_OWNER_SHIFT;
	}

	public static uint dispatchType(uint objectID) {
		return (objectID&ITEM_TYPE_FLAG_MASK);
	}

	public static uint dispatchIndex(uint objectID){
		return (objectID&ITEM_INDEX_MASK);
	}

	public static uint packID(uint owner,uint type,uint objectIndex)
	{
		return	((owner&0xFF)<<ITEM_OWNER_SHIFT)
		|	(type&ITEM_TYPE_FLAG_MASK)|(objectIndex&ITEM_INDEX_MASK);
	}
};

public abstract class BaseComponentData: BaseItemData{
	public	BaseItemData	m_component;
	public	Vector2 		m_compPos;
	
	public BaseComponentData(kDataPackage owner,uint ID):base(owner,ID){}
};

public class ModuleData:BaseItemData
{
	//public p_Image2D	m_image;
	
	public ModuleData(kDataPackage owner,uint ID):base(owner,ID){}
	
	public override void read(kBinaryReader stream,int dataFormat)
	{
		m_name 		= stream.ReadString();
		int x		= stream.ReadInt32();
		int y		= stream.ReadInt32();
		int width 	= stream.ReadInt32();
		int height	= stream.ReadInt32();
		m_bounds 	= new  Rect(x,y,width,height);
		
#if DEBUG_SPRITE_LOADING
			Debug.Log("[" + m_name + "]["+x+","+y+","+width+","+height+"");
#endif
	}
	public override  uint	 getType(){return FLAG_TYPE_MODULE;}
};


public class FrameData : BaseItemData
{
		/************************************************/
		/************* Frame components *****************/
		/************************************************/
		public class FrameComponent: BaseComponentData
		{
			public int 		m_angle;
			public float	m_scaleX;
			public float	m_scaleY;
			public int 		m_blendingColor;
			public int		m_transformFlag;
			
			public FrameComponent(kDataPackage owner,uint ID):base(owner,ID)
			{
				m_component = null;
				m_angle = 0;
				m_scaleX= 1.0f;
				m_scaleY = 1.0f;
				m_blendingColor = 255;
			}
			public override uint	 getType(){ return FLAG_TYPE_FRAMECOMPONENT;}
			
			public override void  read(kBinaryReader stream,int dataFormat){
				uint	compID = (uint)stream.ReadUInt32();
			
				m_component		= m_owner.get(compID);
				m_name			= m_component.m_name;
				m_compPos.x		= stream.ReadInt32();
				m_compPos.y		= stream.ReadInt32();
				m_angle			= stream.ReadInt32();
				m_scaleX		= stream.ReadFloat();
				m_scaleY		= stream.ReadFloat();
				m_blendingColor = stream.ReadInt32();
				m_transformFlag	= stream.ReadInt32();
			
#if DEBUG_SPRITE_LOADING
					Debug.Log("\t\t\tRead FrameComp: "+ m_name+" ["+m_compPos.x+","+m_compPos.y+"]" + "Scale:["+m_scaleX+","+m_scaleY+"] angle:"+m_angle+" blending:"+m_blendingColor);//+m_bounds,getHeight(),m_angle,m_scaleX,m_scaleY,m_blendingColor);
#endif
			}
		};
	
	public  FrameComponent[] m_components;
	//RectangleList	m_collisionBounds;
	
	public FrameData(kDataPackage owner,uint ID):base(owner,ID){}

	public override void read(kBinaryReader stream,int dataFormat)
	{
		m_name			= stream.ReadString();
		int compCount	= stream.ReadInt32();

#if DEBUG_SPRITE_LOADING
			Debug.Log("\t\tRead Frame:" + m_name+ " No modules:" + compCount);
#endif
		
		m_components = new FrameComponent[compCount];
		
		m_bounds = new Rect();
		for(uint i = 0; i < compCount;i++){
			FrameComponent comp = new FrameComponent(m_owner,i);
			comp.read(stream,dataFormat);
			m_components[i] = comp;
			m_bounds = RectUtil.union(m_bounds,new Rect(comp.m_compPos.x,comp.m_compPos.y,comp.m_component.m_bounds.width,comp.m_component.m_bounds.height));
		}
	}
	
	public override uint	 getType(){return FLAG_TYPE_FRAME;}	
};


public class SequenceData: BaseItemData
{
	public SequenceComponent[] m_components;
	
	public SequenceData(kDataPackage owner,uint ID):base(owner,ID){}
	
	public override void read(kBinaryReader stream,int dataFormat){
		m_name = stream.ReadString();

		int compCount = stream.ReadInt32();
			
#if DEBUG_SPRITE_LOADING
		Debug.Log("\t\tRead Sequence: " + m_name + "No frames:" + compCount);
#endif	
		m_components = new SequenceComponent[compCount];
		for(uint i = 0; i < compCount;i++){
			SequenceComponent comp = new SequenceComponent(m_owner,i);
			comp.read(stream,dataFormat);
			m_components[i] = comp;
		}
	}
	
	public override uint	 getType(){return FLAG_TYPE_SEQUENCE;}
	/************************************************/
	/************* Sequence components *****************/
	/************************************************/
	
	public class SequenceComponent: BaseComponentData
	{
		public int 		m_duration;
		public float	m_scaleX;
		public float	m_scaleY;
		//public float	m_scaleXEnd;
		//public float	m_scaleYEnd;
		public int 		m_blend;
		//public int 		m_blendEnd;
		public int	 	m_angle;
		//public int		m_rotation;
		public bool 	m_interpTransforms;
	
		public  SequenceComponent(kDataPackage owner,uint ID):base(owner,ID){
			m_duration	= 0;
			m_scaleX 	= 1.0f;
			m_scaleY 	= 1.0f;
			m_blend		= 255;
			m_angle	= 0;
			m_interpTransforms = false;
		}
		
		public override void read(kBinaryReader stream,int dataFormat){
			m_component		= m_owner.get(stream.ReadUInt32());
			m_compPos.x		= stream.ReadInt32();
			m_compPos.y		= stream.ReadInt32();
			m_duration		= stream.ReadInt32();
			m_scaleX		= stream.ReadFloat();
			if(dataFormat < 6)stream.ReadFloat();
			m_scaleY		= stream.ReadFloat();
			if(dataFormat < 6)stream.ReadFloat();
			m_blend			= stream.ReadInt32();
			if(dataFormat < 6)stream.ReadInt32();
			m_angle			= stream.ReadInt32();
			if(dataFormat < 6)stream.ReadInt32();
			if(dataFormat>= 6) m_interpTransforms = stream.ReadBoolean();
				
#if DEBUG_SPRITE_LOADING
				Debug.Log("\t\t Sequence component pos ["+m_compPos.x+","+m_compPos.y+"] duration "+m_duration+" scaleStart["+ m_scaleX+"," + m_scaleY+"] blend["+m_blend+"] angle "+m_angle);
#endif
		}

		public override uint	 getType(){return FLAG_TYPE_SEQUENCECOMPONENT;}
	};
};

public class AnimationData:BaseItemData {
	
	public  AnimationComponent[]	m_components;
	
	public AnimationData(kDataPackage owner,uint ID):base(owner,ID){}
	
	public override void read(kBinaryReader stream,int dataFormat){
		m_name = stream.ReadString();
		int compCount = stream.ReadInt32();
	
#if DEBUG_SPRITE_LOADING
			Debug.Log("\t\tRead Animation:"+m_name+"comp count" + compCount);
#endif
		m_components = new AnimationComponent[compCount];
		for(uint i = 0; i < compCount;i++){
			AnimationComponent comp = new AnimationComponent(m_owner,i);
			comp.read(stream,dataFormat);
			m_components[i] = comp;
		}
	}
		
	public override uint		getType(){return FLAG_TYPE_ANIMATION;}
													
		public class AnimationComponent:BaseComponentData
		{	
			public AnimationComponent(kDataPackage owner,uint ID):base(owner,ID){
				m_component = null;
			}
		
			public override void read(kBinaryReader stream,int dataFormat){
				m_component = m_owner.get(stream.ReadUInt32());
				m_name		= m_component.m_name;
				m_compPos.x	= stream.ReadInt32();
				m_compPos.y	= stream.ReadInt32();
		
#if DEBUG_SPRITE_LOADING
					Debug.Log("\t\t\tRead Anim comp:" + m_name);
#endif
			}
			public override uint	getType(){return FLAG_TYPE_ANIMATIONCOMPONENT;}
		};
};


public class kSpriteData: kDataPackage{
	private string 	m_file;
	
	private ModuleData[] 	m_modulesData = null;
	private FrameData[]		m_framesData  = null;
	private SequenceData[]	m_sequencesData = null;
	private AnimationData[]	m_animationsData = null;
	
	private int referenceCount = 0;
	
	private static Hashtable loadedSprites = new Hashtable();
	
	public static kSpriteData getSpriteData(TextAsset spriteBinary)
	{
		if (spriteBinary == null)
			return null;
		kSpriteData sprite = null;
		if (loadedSprites.ContainsKey(spriteBinary.name)) {
			sprite = (kSpriteData)loadedSprites[spriteBinary.name];
		} else {
			sprite = load(spriteBinary);
			if (sprite != null)
				loadedSprites.Add(spriteBinary.name, sprite);
		}
		return sprite;
	}
	
	private static kSpriteData load(TextAsset binary)
	{
		kSpriteData sprite = null;
		if (binary == null)
			return null;
		
		//Debug.Log("Loading sprite: " + binary.name);
		sprite = new kSpriteData();
		sprite.m_file = binary.name;
		sprite.m_name = binary.name;
		
		Stream s = new MemoryStream(binary.bytes);
		kBinaryReader stream = new kBinaryReader(s, System.Text.Encoding.UTF8, false);
		
		int version = stream.ReadInt32();
		sprite.m_ID = stream.ReadUInt32();
		if(version <= 7)
			sprite.m_ID = sprite.m_ID & 0xF;

#if DEBUG_SPRITE_LOADING
		int imagesCount = stream.ReadInt32();
		Debug.Log("Sprite version: " + version + " | ID: " +sprite.m_ID + " | No images: " + imagesCount);
#else
		stream.ReadInt32(); // images count
#endif
		
		//only one texture supported
		//for (int i = 0; i < imagesCount; i++)
		{
#if DEBUG_SPRITE_LOADING
			string imagePath	= stream.ReadString();
			string imageName	= stream.ReadString();
			Debug.Log("Original texture name: " + imageName + " path: " + imagePath);
#else
			stream.ReadString(); // image path
			stream.ReadString(); // image name
#endif
			//p_Image2D image = Image2D::loadImage(imageName->c_str());
			//sprite->m_images.push_back(image);

			int  modulesCount = stream.ReadInt32();
#if DEBUG_SPRITE_LOADING
			Debug.Log("\tModules - " + modulesCount);
#endif
			sprite.m_modulesData = new ModuleData[modulesCount];
			for (uint j = 0; j < modulesCount; j++)
			{
				ModuleData module = new ModuleData(sprite, j);
				//module->setImage(image);
				module.read(stream, version);
				sprite.m_modulesData[j] = module;
			}
		}
	
		int framesCount = stream.ReadInt32();
#if DEBUG_SPRITE_LOADING
		Debug.Log("\tFrames - " + framesCount);
#endif
		sprite.m_framesData = new FrameData[framesCount];
		for (uint i = 0; i < framesCount; i++)
		{
			FrameData frame = new FrameData(sprite, i);
			frame.read(stream, version);
			sprite.m_framesData[i] = frame;
		}
			
		int sequencesCount = stream.ReadInt32();
#if DEBUG_SPRITE_LOADING
		Debug.Log("\tSequences - " + sequencesCount);
#endif
		sprite.m_sequencesData = new SequenceData[sequencesCount];
		for (uint i = 0; i < sequencesCount; i++)
		{
			SequenceData anim = new SequenceData(sprite, i);
			anim.read(stream, version);
			sprite.m_sequencesData[i] = anim;
		}
			
		int animCount = stream.ReadInt32();
#if DEBUG_SPRITE_LOADING
		Debug.Log("\tAnimations - " + animCount);
#endif
		sprite.m_animationsData = new AnimationData[animCount];
		for (uint i = 0; i < animCount; i++)
		{
			AnimationData anim = new AnimationData(sprite, i);
			anim.read(stream, version);
			sprite.m_animationsData[i] = anim;
		}
		stream.Close();
		s.Close();
		return sprite;
	}
	
	public void grabReference(){
		referenceCount++;
	}
	
	public void dropReference(){
		referenceCount--;
		if(referenceCount <= 0 && loadedSprites.ContainsKey(m_file))
			loadedSprites.Remove(m_file);
	}
	
	public FrameData[] getFrames(){
		return m_framesData;
	}
	
	public SequenceData[] getSequences(){
		return m_sequencesData;
	}
	
	public AnimationData[] getAnimations(){
		return m_animationsData;
	}
	
	public override BaseItemData get(uint ID){
		uint itemType 	= BaseItemData.dispatchType(ID);
		uint itemIndex	= BaseItemData.dispatchIndex(ID);
	
		switch(itemType){
		/*	case Image.ITEM_TYPE_FLAG: 
				if(itemIndex < getImages().size())
					return getImages().get(itemIndex);
				else
					return getImages().get(0);*/
			case BaseItemData.FLAG_TYPE_MODULE: 
			{	
				if(m_modulesData != null && itemIndex < m_modulesData.Length)
					return m_modulesData[itemIndex];
			}break;
			
			case BaseItemData.FLAG_TYPE_FRAME:
			{
				if(m_framesData != null && itemIndex < m_framesData.Length)
					return m_framesData[itemIndex];
			}break;
		
			case BaseItemData.FLAG_TYPE_SEQUENCE: 
			{
				if(m_sequencesData != null && itemIndex < m_sequencesData.Length)
					return m_sequencesData[itemIndex];
			}break;
		
			case BaseItemData.FLAG_TYPE_ANIMATION: 
			{
				if(m_animationsData != null && itemIndex < m_animationsData.Length)
					return m_animationsData[itemIndex];
			}break;
		}
	
		return null;//DataController.get(ID);
	}
	
	public BaseItemData getItemByName(uint itemType,string name)
	{
		switch(itemType){
		/*	case Image.ITEM_TYPE_FLAG: 
				if(itemIndex < getImages().size())
					return getImages().get(itemIndex);
				else
					return getImages().get(0);*/
			case BaseItemData.FLAG_TYPE_MODULE: 
			{	
				if(m_modulesData != null){
					for(int i = 0; i < m_modulesData.Length;i++){
						if(string.Compare(m_modulesData[i].m_name, name, true) == 0)
							return m_modulesData[i];
					}
				}
			}break;
			
			case BaseItemData.FLAG_TYPE_FRAME: 
			{	
				if(m_framesData != null){
					for(int i = 0; i < m_framesData.Length;i++){
						if(string.Compare(m_framesData[i].m_name, name, true) == 0)
							return m_framesData[i];
					}
				}
			}break;

			case BaseItemData.FLAG_TYPE_SEQUENCE: 
			{	
				if(m_sequencesData != null){
					for(int i = 0; i < m_sequencesData.Length;i++){
						if(string.Compare(m_sequencesData[i].m_name, name, true) == 0)
							return m_sequencesData[i];
					}
				}
			}break;
			case BaseItemData.FLAG_TYPE_ANIMATION: 
			{	
				if(m_animationsData != null){
					for(int i = 0; i < m_animationsData.Length;i++){
						if(string.Compare(m_animationsData[i].m_name, name, true) == 0)
							return m_animationsData[i];
					}
				}
			}break;
		}
	
		return null;//DataController.get(ID);
	}

	public BaseItemData[] getAllContainig(uint itemType,string name){
		List<BaseItemData> result = new List<BaseItemData>();
		
		switch(itemType){
			case BaseItemData.FLAG_TYPE_MODULE: 
			{	
				if(m_modulesData != null){
					for(int i = 0; i < m_modulesData.Length;i++){
						if(m_modulesData[i].m_name.StartsWith(name))
							result.Add(m_modulesData[i]);
					}
				}
			}break;
			
			case BaseItemData.FLAG_TYPE_FRAME: 
			{	
				if(m_framesData != null){
					for(int i = 0; i < m_framesData.Length;i++){
						if(m_framesData[i].m_name.StartsWith(name))
							result.Add(m_framesData[i]);
					}
				}
			}break;
	
			case BaseItemData.FLAG_TYPE_SEQUENCE: 
			{	
				if(m_sequencesData != null){
					for(int i = 0; i < m_sequencesData.Length;i++){
						if(m_sequencesData[i].m_name.StartsWith(name))
							result.Add(m_sequencesData[i]);
					}
				}
			}break;
			case BaseItemData.FLAG_TYPE_ANIMATION: 
			{	
				if(m_animationsData != null){
					for(int i = 0; i < m_animationsData.Length;i++){
						if(m_animationsData[i].m_name.StartsWith(name))
							result.Add(m_animationsData[i]);
					}
				}
			}break;
		}
		return result.ToArray();
	 }
}
