using UnityEngine;
using System.Collections;

[System.Serializable]
public class kSpriteItem{
	public int 		type;
	public int 		id;
	public string 	name;
	public kSpriteItem(){}
	public kSpriteItem(int i,int t,string n){id = i;type = t;name = n;}
}
