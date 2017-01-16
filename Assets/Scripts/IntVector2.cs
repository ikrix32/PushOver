using UnityEngine;
using System.Collections;

[System.Serializable]
public struct IntVector2{
	public int x;
	public int y;

	public IntVector2(int x,int y){
		this.x = x;
		this.y = y;
	}

	public override bool Equals(object obj)
	{
		return this.Equals((IntVector2)obj);
	}

	public bool Equals(IntVector2 other)
	{
		return x == other.x && y == other.y;
	}

	public override string ToString(){
		return "["+x+","+y+"]";
	}
}
