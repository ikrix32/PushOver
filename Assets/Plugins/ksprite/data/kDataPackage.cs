using UnityEngine;
using System.Collections;



public abstract class kDataPackage{//: MonoBehaviour{
	public	const int DATA_PACKAGE_TYPE_SPRITE  = 0;
	public 	const int DATA_PACKAGE_TYPE_OBJPACK = 1;
	//public 	const int DATA_PACKAGE_TYPE_SCENE   = 2;
	
	public  const int DATA_PACKAGE_TYPE_MASK = 0x80;//0xF0;
	public  const int DATA_PACKAGE_ID_MASK	 = 0x7F;//0x0F;
	public  const int DATA_PACKAGE_TYPE_SHIFT= 7;//4;
	public  const int DATA_PACKAGE_ID_SHIFT	 = 0;
	
	public	string	m_name;
	protected uint	m_ID;
	
	//KDataPackage(string name){ m_name = name; m_ID = 0;}
		
	public virtual uint	getID()	 {return m_ID;}
	
	public abstract	BaseItemData get(uint ID);
	
	public static int dispatchType(uint ID) {
		return (int)((ID & DATA_PACKAGE_TYPE_MASK)>>DATA_PACKAGE_TYPE_SHIFT);
	}
	
	public static int dispatchIndex(uint ID)	{
		return (int)(ID & DATA_PACKAGE_ID_MASK)>>DATA_PACKAGE_ID_SHIFT;
	}
};

