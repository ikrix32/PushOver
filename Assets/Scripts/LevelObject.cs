using UnityEngine;
using System.Collections;

public abstract class LevelObject : kSpriteObject {
	public enum Type{
		Door,
		Ladder,
		Platform,
		Domino
	}

	public Type type;

	public abstract void SetDefaultState();
}
