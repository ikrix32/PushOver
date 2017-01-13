using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

[CustomEditor(typeof(Door))]

[CanEditMultipleObjects]
public class DoorObjectEditor : kSpriteObjectEditor {
}

[CustomEditor(typeof(Domino))]

[CanEditMultipleObjects]
public class DominoObjectEditor : kSpriteObjectEditor {
}

[CustomEditor(typeof(Platform))]

[CanEditMultipleObjects]
public class PlatformObjectEditor : kSpriteObjectEditor {

}

[CustomEditor(typeof(Ladder))]

[CanEditMultipleObjects]
public class LadderObjectEditor : kSpriteObjectEditor {

}