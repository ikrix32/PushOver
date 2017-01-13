using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// format: <font id="BebasNeue" color="#80ff9900" size="0.8">custom text</font>
class kTextModifier
{
	public int idxStart;
	public int idxEnd;
	public int fontID;
	public float size;
	public Color color;
	
	public kTextModifier(int idxStart = 0, int idxEnd = int.MaxValue)
	{
		this.idxStart = idxStart;
		this.idxEnd = idxEnd;
		this.fontID = 0;
		this.size = 1;
	}

	public kTextModifier(Color color)
	{
		this.idxStart = 0;
		this.idxEnd = int.MaxValue;
		this.fontID = 0;
		this.size = 1;
		this.color = color;
	}
}

public class kTextLine
{
	public int idxStart, idxEnd;
	public float width;
}

[ExecuteInEditMode]
public class kCustomText : kPickerItem
{
	const int MIN_WRAP_WIDTH = 100;

	public List<kFont> fonts;
	
	public string text;
	public bool processModifiers = true;
	public kAlignMode align = kAlignMode.TOP_LEFT;
	public Color color = Color.white;
	public int lineSpacing = 0;
	public int wrapWidth = 0;
	public int maxLines = 0;
	
	private string lastText = null;
	private bool lastProcessModifiers = true;
	private kAlignMode lastAlign = kAlignMode.TOP_LEFT;
	private Color lastColor = Color.white;
	private int lastLineSpacing = 0;
	private int lastWrapWidth = 0;
	private int lastMaxLines = 0;

	private string parsedText = null;
	private List<kTextModifier> modifiers = new List<kTextModifier>();
	private List<kTextLine> textLines = null;
	
	protected override void onStart()
	{
		base.onStart();
		if (fonts != null && fonts.Count > 0 && fonts[0] != null)
		{
			if (!fonts[0].isLoaded())
				fonts[0].load();
			setSourceSprite(fonts[0].sprite);
			m_material.mainTexture = sprite.sourceTexture;
			updateMesh();
		}
	}
	
	protected override void onUpdate()
	{//TODO - optimize me!!!!
		if (fonts == null || fonts.Count == 0 || fonts[0] == null)
			return;
		setSourceSprite(fonts[0].sprite);
		if (lastText != text || lastProcessModifiers != processModifiers || lastAlign != align || lastLineSpacing != lineSpacing ||
			(lastWrapWidth != wrapWidth && wrapWidth > MIN_WRAP_WIDTH) || lastMaxLines != maxLines)
			updateMesh();
		if (lastColor != color)
			updateVertsColor();
		base.onUpdate();
	}

	public List<kTextLine> getTextLines()
	{
		return textLines;
	}
	
	public void setText(string _text)
	{
		if (text == null || _text == null || text.CompareTo(_text) != 0)
		{
			text = _text;
			updateMesh();
		}
	}

	public void setColor(Color _color)
	{
		if (color != _color)
		{
			color = _color;
			updateVertsColor();
		}
	}

	private string getAttrValue(string txt, string attr)
	{
		int idxAttr = txt.IndexOf(attr + "=\"");
		if (idxAttr < 0)
			return null;
		int idxStart = idxAttr + attr.Length + 2;
		int idxEnd = txt.IndexOf('"', idxStart);
		if (idxEnd < 0)
			return null;
		return txt.Substring(idxStart, idxEnd - idxStart);
	}
	
	private void parseText(string txt)
	{
		parsedText = "";
		txt = txt.Replace("\\n", "\n");
		while (txt.Length > 0)
		{
			int idx = txt.IndexOf("<font ");
			if (idx < 0)
				break;
			int idxStart = txt.IndexOf(">") + 1;
			int idxEnd = txt.IndexOf("</font>", idxStart);
			if (idxStart <= 0 || idxEnd <= 0)
				break;
			parsedText += txt.Substring(0, idx);
			kTextModifier modif = new kTextModifier(parsedText.Length, parsedText.Length + idxEnd - idxStart);
			try {
				string sModif = txt.Substring(idx, idxStart - idx);
				string sFontID = getAttrValue(sModif, "id");
				if (sFontID != null)
				{
					for (int i = 0; i < fonts.Count; ++i)
						if (fonts[i].fontName.ToLower().Contains(sFontID.ToLower()))
						{
							modif.fontID = i;
							break;
						}
				}
				string sSize = getAttrValue(sModif, "size");
				modif.size = (sSize != null) ? float.Parse(sSize) : 1;
				string sColor = getAttrValue(sModif, "color");
				if (sColor != null)
				{
					if (sColor[0] == '#')
						sColor = sColor.Substring(1);
					int color = int.Parse(sColor, System.Globalization.NumberStyles.AllowHexSpecifier);
					if (sColor.Length == 6)
						modif.color = new Color((float)((color >> 16) & 0xFF) / 255, (float)((color >> 8) & 0xFF) / 255, (float)(color & 0xFF) / 255);
					else
						modif.color = new Color((float)((color >> 16) & 0xFF) / 255, (float)((color >> 8) & 0xFF) / 255, (float)(color & 0xFF) / 255, (float)((color >> 24) & 0xFF) / 255);
				}
				else
					modif.color = this.color;
			} catch (System.Exception) {
			}
			parsedText += txt.Substring(idxStart, idxEnd - idxStart);
			txt = txt.Substring(idxEnd + 7);
			modifiers.Add(modif);
		}
		parsedText += txt;
	}
	
	public float getTextWidth(int idxStart, int idxEnd)
	{
		kTextModifier modif = null, nextModif = null;
		int modIndex = 0;
		for (; modIndex < modifiers.Count; ++modIndex)
		{
			if (modifiers[modIndex].idxStart <= idxStart && modifiers[modIndex].idxEnd > idxStart)
				modif = modifiers[modIndex];
			else if (modifiers[modIndex].idxStart > idxStart)
			{
				nextModif = modifiers[modIndex];
				break;
			}
		}
		if (modif == null)
			modif = new kTextModifier();
		float textWidth = 0;
		for (int i = idxStart; i < idxEnd; ++i)
		{
			if (nextModif != null && nextModif.idxStart == i)
			{
				modif = nextModif;
				nextModif = (++modIndex < modifiers.Count) ? modifiers[modIndex] : null;
			}
			if (modif.idxEnd == i)
				modif = new kTextModifier();
			if (parsedText[i] == '\n')
				continue;
			FrameData frame = (FrameData)fonts[modif.fontID].getMeshForChar(parsedText[i]);
			if (frame != null && frame.m_components.Length > 0)
				textWidth += (frame.m_components[0].m_compPos.x + frame.m_components[0].m_component.m_bounds.width * frame.m_components[0].m_scaleX) * modif.size;
		}
		return textWidth;
	}
	
	private void wrapText(string text, int wrapWidth)
	{
		modifiers.Clear();
		if (processModifiers)
			parseText(text);
		else
			parsedText = text;
		textLines = new List<kTextLine>();
		kTextLine line = new kTextLine();
		textLines.Add(line);
		int wordStartIndex = 0;
		while (wordStartIndex < parsedText.Length)
		{
			int wordEndIndex = parsedText.IndexOf(' ', wordStartIndex);
			if (wordEndIndex < 0)
				wordEndIndex = parsedText.Length;
			int lineEndIndex = parsedText.IndexOf("\n", wordStartIndex, wordEndIndex - wordStartIndex);
			if (lineEndIndex >= 0)
				wordEndIndex = lineEndIndex;
			float wordWidth = getTextWidth(wordStartIndex, wordEndIndex);
			if (wrapWidth > 0 && line.width + wordWidth > wrapWidth)
			{
				if (wordWidth > wrapWidth)
				{
					while (wordWidth > wrapWidth)
					{
						int cutIndex = wordStartIndex;
						while (line.width + getTextWidth(wordStartIndex, cutIndex + 1) <= wrapWidth)
							++cutIndex;
						if (cutIndex == wordStartIndex && parsedText[line.idxEnd] == ' ') // remove last space
							line.width -= getTextWidth(line.idxEnd, line.idxEnd + 1);
						line.idxEnd = cutIndex;
						line.width += getTextWidth(wordStartIndex, cutIndex);
						if (textLines.Count == maxLines)
							return;
						line = new kTextLine();
						line.idxStart = cutIndex;
						wordStartIndex = cutIndex;
						wordWidth = getTextWidth(wordStartIndex, wordEndIndex);
						textLines.Add(line);
					}
				}
				else
				{
					if (parsedText[line.idxEnd] == ' ') // remove last space
						line.width -= getTextWidth(line.idxEnd, line.idxEnd + 1);
					if (textLines.Count == maxLines)
						return;
					line = new kTextLine();
					line.idxStart = wordStartIndex;
					textLines.Add(line);
				}
			}
			line.idxEnd = wordEndIndex;
			line.width += wordWidth;
			if (wordEndIndex < parsedText.Length && parsedText[wordEndIndex] == ' ') // add space
				line.width += getTextWidth(wordEndIndex, wordEndIndex + 1);
			wordStartIndex = wordEndIndex + 1;
			if (wordEndIndex == lineEndIndex)
			{
				if (textLines.Count == maxLines)
					return;
				line = new kTextLine();
				line.idxStart = wordStartIndex;
				line.idxEnd = wordStartIndex;
				textLines.Add(line);
			}
		}
	}
	
	protected override void updateMesh()
	{
		kFont baseFont = fonts != null && fonts.Count > 0 ? fonts[0] : null;
		sprite = baseFont != null ? baseFont.sprite : null;
		lastText = text;
		lastAlign = align;
		lastProcessModifiers = processModifiers;
		lastLineSpacing = lineSpacing;
		lastWrapWidth = wrapWidth;
		lastMaxLines = maxLines;
		
		if (text == null || baseFont == null || sprite == null || !sprite.isLoaded())
		{
			clearObjectMesh();
			objMesh.center = Vector2.zero;
			objMesh.size = Vector2.zero;
			return;
		}
		else
		{
			wrapText(text, (int)(wrapWidth / transform.localScale.x));
			
			int charCount = 0;
			for (int i = 0; i < textLines.Count; ++i)
				charCount += textLines[i].idxEnd - textLines[i].idxStart;
			bool isTruncated = maxLines > 0 && textLines[textLines.Count - 1].idxEnd < parsedText.Length;
			if (isTruncated)
				charCount += 3;
			objMesh.vertices = new Vector3[charCount * 4];
			objMesh.triangles = new int[charCount * 6];
			objMesh.UVs = new Vector2[charCount * 4];
			objMesh.colors = new Color[charCount * 4];
			
			Vector2 textureSize = new Vector2(baseFont.sprite.sourceTexture.width, baseFont.sprite.sourceTexture.height);
			float lineHeight = baseFont.getHeight(); // todo: get height for each line?
			float x = 0, y = lineHeight;
			if (((int)align & (int)kAlign.BOTTOM) != 0)
				y -= textLines.Count * lineHeight + (textLines.Count - 1) * lineSpacing;
			else if (((int)align & (int)kAlign.VCENTER) != 0)
				y -= (textLines.Count * lineHeight + (textLines.Count - 1) * lineSpacing) / 2;
			
			kTextModifier modif = new kTextModifier(color);
			kTextModifier nextModif = modifiers.Count > 0 ? modifiers[0] : null;
			int modIndex = 0, vertIndex = 0;
			Rect meshCorners = new Rect();
			for (int i = 0; i < textLines.Count; ++i)
			{
				if (((int)align & (int)kAlign.RIGHT) != 0)
					x = -textLines[i].width;
				else if (((int)align & (int)kAlign.HCENTER) != 0)
					x = -textLines[i].width / 2;
				int nbDots = (isTruncated && i == textLines.Count - 1) ? 3 : 0;
				for (int j = textLines[i].idxStart; j < textLines[i].idxEnd + nbDots; ++j)
				{
					if (nextModif != null && nextModif.idxStart == j)
					{
						modif = nextModif;
						nextModif = (++modIndex < modifiers.Count) ? modifiers[modIndex] : null;
					}
					if (modif.idxEnd == j)
						modif = new kTextModifier(color);
					
					FrameData frame = (FrameData)fonts[modif.fontID].getMeshForChar(j < textLines[i].idxEnd ? parsedText[j] : '.');
					if (frame != null && frame.m_components.Length > 0)
					{
						FrameData.FrameComponent fComp = frame.m_components[0];
						ModuleData module = (ModuleData)fComp.m_component;
						
						// vertices
						updateMeshVerts(module, vertIndex * 4, x + fComp.m_compPos.x * modif.size, y + fComp.m_compPos.y * modif.size, fComp.m_scaleX * modif.size, fComp.m_scaleY * modif.size, fComp.m_angle);
						
						// indices
						objMesh.triangles[vertIndex * 6] = vertIndex * 4;
						objMesh.triangles[vertIndex * 6 + 1] = vertIndex * 4 + 3;
						objMesh.triangles[vertIndex * 6 + 2] = vertIndex * 4 + 2;
						objMesh.triangles[vertIndex * 6 + 3] = vertIndex * 4 + 0;
						objMesh.triangles[vertIndex * 6 + 4] = vertIndex * 4 + 2;
						objMesh.triangles[vertIndex * 6 + 5] = vertIndex * 4 + 1;
						
						// UVs
						Rect mBounds = module.m_bounds;
						mBounds.x = mBounds.x / textureSize.x;
						mBounds.y = 1.0f - (mBounds.y + mBounds.height) / textureSize.y;
						mBounds.width = mBounds.width / textureSize.x;
						mBounds.height = mBounds.height / textureSize.y;
						objMesh.UVs[vertIndex * 4 + 3] = new Vector2(mBounds.x, mBounds.y + mBounds.height);
						objMesh.UVs[vertIndex * 4 + 2] = new Vector2(mBounds.x + mBounds.width, mBounds.y + mBounds.height);
						objMesh.UVs[vertIndex * 4 + 1] = new Vector2(mBounds.x + mBounds.width, mBounds.y);
						objMesh.UVs[vertIndex * 4 + 0] = new Vector2(mBounds.x, mBounds.y);
						
						// colors
						objMesh.colors[vertIndex * 4] = modif.color;
						objMesh.colors[vertIndex * 4 + 1] = modif.color;
						objMesh.colors[vertIndex * 4 + 2] = modif.color;
						objMesh.colors[vertIndex * 4 + 3] = modif.color;
						
						if (vertIndex == 0)
							meshCorners = new Rect(objMesh.vertices[0].x, objMesh.vertices[0].y, objMesh.vertices[0].x, objMesh.vertices[0].y);
						meshCorners = updateCorners(objMesh, meshCorners, vertIndex * 4, 4);
						x += fComp.m_compPos.x + module.m_bounds.width * fComp.m_scaleX * modif.size;
					}
					else
					{
						Debug.Log("kCustomText char not found: '" + parsedText[j] + "'");
					}
					vertIndex++;
				}
				x = 0;
				y += lineHeight + lineSpacing;
			}
			objMesh.center.x = (meshCorners.x + meshCorners.width) / 2;
			objMesh.center.y = (meshCorners.y + meshCorners.height) / 2;
			objMesh.size.x = (objMesh.center.x - meshCorners.x) * 2;
			objMesh.size.y = (objMesh.center.y - meshCorners.y) * 2;
		}
		if (m_material == null 
		&&	baseFont.sprite != null && baseFont.sprite.isLoaded()){
			m_material.mainTexture = baseFont.sprite.sourceTexture;
		}
		meshChanged = true;
	}
	
	private void updateMeshVerts(ModuleData module, int index, float x, float y, float scaleX, float scaleY, int angle)
	{
		objMesh.vertices[index + 3] = new Vector3(0, 0, 0);
		objMesh.vertices[index + 2] = new Vector3(module.m_bounds.width, 0, 0);
		objMesh.vertices[index + 1] = new Vector3(module.m_bounds.width, module.m_bounds.height, 0);
		objMesh.vertices[index + 0] = new Vector3(0, module.m_bounds.height, 0);
		
		for (int i = 0; i < 4; ++i)
		{
			// scale
			float newX = scaleX * objMesh.vertices[index + i].x;
			float newY = scaleY * objMesh.vertices[index + i].y;
			// rotate
			objMesh.vertices[index + i].x = newX * Mathf.Cos(angle * Mathf.Deg2Rad) - newY * Mathf.Sin(angle * Mathf.Deg2Rad);
			objMesh.vertices[index + i].y = newX * Mathf.Sin(angle * Mathf.Deg2Rad) + newY * Mathf.Cos(angle * Mathf.Deg2Rad);
			// update position
			objMesh.vertices[index + i].x += x;
			objMesh.vertices[index + i].y = -objMesh.vertices[index + i].y - y;
		}
	}
	
	private void updateVertsColor()
	{
		lastColor = color;
		if (objMesh.colors != null)
		{
			kTextModifier modif = new kTextModifier(color);
			kTextModifier nextModif = modifiers.Count > 0 ? modifiers[0] : null;
			int modIndex = 0, colIndex = 0;
			for (int i = 0; i < textLines.Count; ++i)
				for (int j = textLines[i].idxStart; j < textLines[i].idxEnd; ++j)
				{
					if (nextModif != null && nextModif.idxStart == j)
					{
						modif = nextModif;
						nextModif = (++modIndex < modifiers.Count) ? modifiers[modIndex] : null;
					}
					if (modif.idxEnd == j)
						modif = new kTextModifier(color);
					for (int k = 0; k < 4; ++k, ++colIndex)
						objMesh.colors[colIndex] = modif.color;
				}
			colorsChanged = true;
		}
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, 2);
	}
#endif
}
