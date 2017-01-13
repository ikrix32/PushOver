using UnityEngine;
using System.Collections;

public class kTextBubble : kSpriteObject
{
	public const int IDX_TOP_LEFT = 0;
	public const int IDX_SIDE_TOP = 1;
	public const int IDX_TOP_RIGHT = 2;
	public const int IDX_SIDE_LEFT = 3;
	public const int IDX_MIDDLE = 4;
	public const int IDX_SIDE_RIGHT = 5;
	public const int IDX_BOTTOM_LEFT = 6;
	public const int IDX_SIDE_BOTTOM = 7;
	public const int IDX_BOTTOM_RIGHT = 8;

	public kSpriteItem m_topLeftCorner;
	public kSpriteItem m_topSide;
	public kSpriteItem m_topRightCorner;

	public kSpriteItem m_leftSide;
	public kSpriteItem m_middle;
	public kSpriteItem m_rightSide;

	public kSpriteItem m_bottomLeftCorner;
	public kSpriteItem m_bottomSide;
	public kSpriteItem m_bottomRightCorner;

	[HideInInspector]
	private int[] bubbleSegments = null;
#if UNITY_EDITOR
	[HideInInspector][System.NonSerialized]
	public bool foldoutSegments = true;
#endif

	public kAlignMode alignMode = kAlignMode.VCENTER_CENTER;
	public bool useScaling = false;
	public float edgeInset = 0;
	public kTextField m_text;

	private kAlignMode lastAlignMode;
	private float lastEdgeInset;

	protected override void onInit()
	{
		base.onInit();

		if (!Application.isPlaying)
			return;
		
		if (m_text == null)
		{
			GameObject obj = new GameObject("kTextMesh");
			m_text = obj.AddComponent<kTextMesh>();
			obj.transform.parent = transform;
		}

		updateMesh();
	}
	
	protected override void onUpdate()
	{
		base.onUpdate();
		if (!Application.isPlaying || sprite == null)
			return;
		
		bool updateBubble = false;
		if (lastAlignMode != alignMode)
		{
			lastAlignMode = alignMode;
			updateBubble = true;
		}
		if (lastEdgeInset != edgeInset)
		{
			lastEdgeInset = edgeInset;
			updateBubble = true;
		}
		if (updateBubble || !Application.isPlaying)
			updateMesh();
	}
	
	public override void childMeshChanged(kObject child)
	{
		if (child == m_text)
			updateMesh();
	}
	
	protected override void updateMesh()
	{
		if (sprite == null || !sprite.isLoaded() || m_text == null)
			return;

		if(bubbleSegments == null){
			bubbleSegments = new int[] {	
				m_topLeftCorner.id, m_topSide.id, m_topRightCorner.id,
				m_leftSide.id, m_middle.id, m_rightSide.id,
				m_bottomLeftCorner.id, m_bottomSide.id, m_bottomRightCorner.id
			};
		}

		int rowFrames = 0, colFrames = 0, nbModules = 0;
		bool multiLine = (bubbleSegments[IDX_SIDE_TOP] != 0);
		float textWidth = (m_text != null ? m_text.getBounds().width : 0) + 2 * edgeInset;
		float textHeight = (m_text != null ? m_text.getBounds().height : 0) + 2 * edgeInset;
		float bubbleWidth = 0, bubbleHeight = 0;
		Vector2 middleScale = Vector2.one;
		
		while (rowFrames < 2 || bubbleWidth < textWidth)
		{
			int graphicID = (rowFrames == 0) ? bubbleSegments[IDX_SIDE_LEFT] : (rowFrames == 1 ? bubbleSegments[IDX_SIDE_RIGHT] : bubbleSegments[IDX_MIDDLE]);
			FrameData frame = (FrameData)sprite.get((uint)graphicID);

			if (frame != null) {
				if (useScaling && rowFrames == 2) {
					float a = frame != null ? frame.m_bounds.width : 1;
					middleScale.x = (textWidth - bubbleWidth) / a;
					bubbleWidth = textWidth;
				} else
					bubbleWidth += frame != null ? frame.m_bounds.width : 0;
			
				if (!multiLine) {
					colFrames = 1;
					bubbleHeight = frame != null ? frame.m_bounds.height : 0;
					nbModules += (frame != null && frame.m_components != null) ? frame.m_components.Length : 0;
				}
				rowFrames++;
			} else
				break;
		}

		if (rowFrames > 0) {
			if (multiLine) {
				while (colFrames < 2 || bubbleHeight < textHeight) {
					int graphicID = (colFrames == 0) ? bubbleSegments [IDX_SIDE_TOP] : (colFrames == 1 ? bubbleSegments [IDX_SIDE_BOTTOM] : bubbleSegments [IDX_MIDDLE]);
					FrameData frame = (FrameData)sprite.get ((uint)graphicID);
					if (useScaling && colFrames == 2) {
						middleScale.y = (textHeight - bubbleHeight) / frame.m_bounds.height;
						bubbleHeight = textHeight;
					} else
						bubbleHeight += frame.m_bounds.height;
					colFrames++;
				}

				for (int idx = 0; idx < bubbleSegments.Length; ++idx) {
					FrameData frame = (FrameData)sprite.get ((uint)bubbleSegments [idx]);
					int frameModules = frame.m_components != null ? frame.m_components.Length : 0;
					switch (idx) {
					case IDX_MIDDLE:
						nbModules += (rowFrames - 2) * (colFrames - 2) * frameModules;
						break;
					case IDX_SIDE_TOP:
					case IDX_SIDE_BOTTOM:
						nbModules += (rowFrames - 2) * frameModules;
						break;
					case IDX_SIDE_LEFT:
					case IDX_SIDE_RIGHT:
						nbModules += (colFrames - 2) * frameModules;
						break;
					default:
						nbModules += frameModules;
						break;
					}
				}
			}

			objMesh.vertices = new Vector3[nbModules * 4];
			objMesh.triangles = new int[nbModules * 6];
			objMesh.UVs = new Vector2[nbModules * 4];
			objMesh.colors = new Color[nbModules * 4];

			int verticesOffset = 0, trianglesOffset = 0;
			Vector2 textureSize = new Vector2 (sprite.sourceTexture.width, sprite.sourceTexture.height);
			int[][] segments = new int[colFrames][];
			if (!multiLine) {
				segments [0] = new int[rowFrames];
				segments [0] [0] = bubbleSegments [IDX_SIDE_LEFT];
				segments [0] [rowFrames - 1] = bubbleSegments [IDX_SIDE_RIGHT];
				for (int j = 1; j < rowFrames - 1; ++j)
					segments [0] [j] = bubbleSegments [IDX_MIDDLE];
			} else {
				for (int i = 0; i < colFrames; ++i) {
					segments [i] = new int[rowFrames];
					segments [i] [0] = bubbleSegments [i == 0 ? IDX_TOP_LEFT : (i == colFrames - 1 ? IDX_BOTTOM_LEFT : IDX_SIDE_LEFT)];
					segments [i] [rowFrames - 1] = bubbleSegments [i == 0 ? IDX_TOP_RIGHT : (i == colFrames - 1 ? IDX_BOTTOM_RIGHT : IDX_SIDE_RIGHT)];
					for (int j = 1; j < rowFrames - 1; ++j)
						segments [i] [j] = bubbleSegments [i == 0 ? IDX_SIDE_TOP : (i == colFrames - 1 ? IDX_SIDE_BOTTOM : IDX_MIDDLE)];
				}
			}
		
			Rect meshCorners = new Rect ();
			Vector2 segmentScale = Vector2.one;
			float frameY = 0;
			if (((int)alignMode & (int)kAlign.BOTTOM) != 0)
				frameY = -bubbleHeight;
			else if (((int)alignMode & (int)kAlign.VCENTER) != 0)
				frameY = -bubbleHeight / 2;
			for (int row = 0; row < segments.Length; ++row) {
				segmentScale.y = (row > 0 && row < segments.Length - 1) ? middleScale.y : 1;
				float frameX = 0;
				if (((int)alignMode & (int)kAlign.RIGHT) != 0)
					frameX = -bubbleWidth;
				else if (((int)alignMode & (int)kAlign.HCENTER) != 0)
					frameX = -bubbleWidth / 2;
				for (int col = 0; col < segments [row].Length; ++col) {
					segmentScale.x = (col > 0 && col < segments [row].Length - 1) ? middleScale.x : 1;
					FrameData frame = (FrameData)sprite.get ((uint)segments [row] [col]);
					int frameNbModules = (frame != null && frame.m_components != null) ? frame.m_components.Length : 0;
					for (int i = 0; i < frameNbModules; ++i, verticesOffset += 4, trianglesOffset += 6) {
						FrameData.FrameComponent modInfo = frame.m_components [i];
						ModuleData module = (ModuleData)modInfo.m_component;
					
						//vertices
						updateMeshVerts (module, verticesOffset, frameX, frameY, modInfo.m_scaleX * segmentScale.x, modInfo.m_scaleY * segmentScale.y);
					
						//indices
						objMesh.triangles [trianglesOffset] = verticesOffset;
						objMesh.triangles [trianglesOffset + 1] = verticesOffset + 3;
						objMesh.triangles [trianglesOffset + 2] = verticesOffset + 2;
						objMesh.triangles [trianglesOffset + 3] = verticesOffset + 0;
						objMesh.triangles [trianglesOffset + 4] = verticesOffset + 2;
						objMesh.triangles [trianglesOffset + 5] = verticesOffset + 1;
					
						//UVs
						Rect mBounds = module.m_bounds;
						mBounds.x = mBounds.x / textureSize.x;
						mBounds.y = 1.0f - ((mBounds.y + mBounds.height) / textureSize.y);
						mBounds.width = mBounds.width / textureSize.x;
						mBounds.height = mBounds.height / textureSize.y;
						objMesh.UVs [verticesOffset + 3] = new Vector2 (mBounds.x, mBounds.y + mBounds.height);
						objMesh.UVs [verticesOffset + 2] = new Vector2 (mBounds.x + mBounds.width, mBounds.y + mBounds.height);
						objMesh.UVs [verticesOffset + 1] = new Vector2 (mBounds.x + mBounds.width, mBounds.y);
						objMesh.UVs [verticesOffset + 0] = new Vector2 (mBounds.x, mBounds.y);
						if ((modInfo.m_transformFlag & BaseItemData.FLIP_VERTICAL_FLAG) != 0) {
							var temp = objMesh.UVs [verticesOffset];
							objMesh.UVs [verticesOffset] = objMesh.UVs [verticesOffset + 3];
							objMesh.UVs [verticesOffset + 3] = temp;
							temp = objMesh.UVs [verticesOffset + 1];
							objMesh.UVs [verticesOffset + 1] = objMesh.UVs [verticesOffset + 2];
							objMesh.UVs [verticesOffset + 2] = temp;
						}
						if ((modInfo.m_transformFlag & BaseItemData.FLIP_HORIZONTAL_FLAG) != 0) {
							var temp = objMesh.UVs [verticesOffset];
							objMesh.UVs [verticesOffset] = objMesh.UVs [verticesOffset + 1];
							objMesh.UVs [verticesOffset + 1] = temp;
							temp = objMesh.UVs [verticesOffset + 2];
							objMesh.UVs [verticesOffset + 2] = objMesh.UVs [verticesOffset + 3];
							objMesh.UVs [verticesOffset + 3] = temp;
						}
					
						//colors
						objMesh.colors [verticesOffset] = Color.white;// blendingColor;
						objMesh.colors [verticesOffset + 1] = Color.white;// blendingColor;
						objMesh.colors [verticesOffset + 2] = Color.white;// blendingColor;
						objMesh.colors [verticesOffset + 3] = Color.white;// blendingColor;
						if (verticesOffset == 0)
							meshCorners = new Rect (objMesh.vertices [verticesOffset].x, objMesh.vertices [verticesOffset].y, objMesh.vertices [verticesOffset].x, objMesh.vertices [verticesOffset].y);
						meshCorners = updateCorners (objMesh, meshCorners, verticesOffset, 4);
					}
					frameX += frame != null ? frame.m_bounds.width * segmentScale.x : 0;
					if (col == segments [row].Length - 1)
						frameY += frame != null ? frame.m_bounds.height * segmentScale.y : 0;
				}
			}
			objMesh.center.x = (meshCorners.x + meshCorners.width) / 2;
			objMesh.center.y = (meshCorners.y + meshCorners.height) / 2;
			objMesh.size.x = (objMesh.center.x - meshCorners.x) * 2;
			objMesh.size.y = (objMesh.center.y - meshCorners.y) * 2;
			meshChanged = true;
		
			float centerX = 0, centerY = 0;
			if (((int)alignMode & (int)kAlign.LEFT) != 0)
				centerX += bubbleWidth / 2;
			else if (((int)alignMode & (int)kAlign.RIGHT) != 0)
				centerX -= bubbleWidth / 2;
			if (((int)alignMode & (int)kAlign.TOP) != 0)
				centerY -= bubbleHeight / 2;
			else if (((int)alignMode & (int)kAlign.BOTTOM) != 0)
				centerY += bubbleHeight / 2;
			float textX = centerX, textY = centerY;
			if (m_text.getAnchor () == TextAnchor.LowerLeft || m_text.getAnchor () == TextAnchor.MiddleLeft || m_text.getAnchor () == TextAnchor.UpperLeft) {
				textX -= textWidth / 2 - edgeInset;
				if (textX > centerX - bubbleWidth / 2 + edgeInset)
					textX = centerX - bubbleWidth / 2 + edgeInset;
			} else if (m_text.getAnchor () == TextAnchor.LowerRight || m_text.getAnchor () == TextAnchor.MiddleRight || m_text.getAnchor () == TextAnchor.UpperRight) {
				textX += textWidth / 2 - edgeInset;
				if (textX < centerX + bubbleWidth / 2 - edgeInset)
					textX = centerX + bubbleWidth / 2 - edgeInset;
			}
			if (m_text.getAnchor () == TextAnchor.UpperLeft || m_text.getAnchor () == TextAnchor.UpperCenter || m_text.getAnchor () == TextAnchor.UpperRight)
				textY += textHeight / 2 - edgeInset;
			else if (m_text.getAnchor () == TextAnchor.LowerLeft || m_text.getAnchor () == TextAnchor.LowerCenter || m_text.getAnchor () == TextAnchor.LowerRight)
				textY -= textHeight / 2 - edgeInset;
			m_text.transform.localPosition = new Vector3 (textX, textY, -0.1f);
		}
	}
	
	private void updateMeshVerts(ModuleData module, int index, float x, float y, float scaleX, float scaleY)
	{
		objMesh.vertices[index + 3] = new Vector3(0, 0, 0);
		objMesh.vertices[index + 2] = new Vector3(module.m_bounds.width, 0, 0);
		objMesh.vertices[index + 1] = new Vector3(module.m_bounds.width, -module.m_bounds.height, 0);
		objMesh.vertices[index + 0] = new Vector3(0, -module.m_bounds.height, 0);
		
		for (int i = 0; i < 4; ++i)
		{
			//scale
			objMesh.vertices[index + i].x *= scaleX;
			objMesh.vertices[index + i].y *= scaleY;
			//update pos in frame
			objMesh.vertices[index + i].x += x;
			objMesh.vertices[index + i].y -= y;
		}
	}
	
	public void setText(string _text)
	{
		m_text.setText(_text);
		updateMesh();
	}
	
	public override void setAlpha(float alpha)
	{
		base.setAlpha(alpha);
		if (m_text != null) {
			m_text.setAlpha(alpha);
		}
	}
}
