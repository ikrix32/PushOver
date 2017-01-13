using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class kText : kTextField {
	
	public	kFont			font;
	public	kFont			fontOutline;
	
	//hide reference from base object
	
	public	kAlignMode		textAlign = kAlignMode.TOP_CENTER;
	//public string text;
	//public	int textWarpWidth;
	public	int 			lineSpacing = 0;
	public  Color			colorTopLeft = Color.white;
	public  Color			colorTopRight= Color.white;
	public  Color			colorBottomLeft = Color.white;
	public  Color			colorBottomRight=Color.white;

	public Color			outlineColor= Color.white;

#if UNITY_EDITOR
	private string			lastText = null;
	private	kAlignMode		lastTextAlign = kAlignMode.TOP_CENTER;
	private	int 			lastTextWarpWidth = 0;
	private	int 			lastLineSpacing = 0;
	private	Color			lastColorTopLeft = Color.white;
	private	Color			lastColorTopRight= Color.white;
	private Color			lastColorBottomLeft = Color.white;
	private Color			lastColorBottomRight=Color.white;
#endif

	protected override void onInit(){
		base.onInit();
		//m_text = text;
		//m_textWarpWidth = textWarpWidth;
	}
	
	protected override void onStart(){
		base.onStart();
		
		if(font != null) {
			if(!font.isLoaded())	font.load();
			setSourceSprite( font.sprite);
			m_material.mainTexture = sprite.sourceTexture;
			
			//update text mesh
			updateMesh();
#if UNITY_EDITOR
			lastText = m_text;
			lastTextAlign = textAlign;
			lastTextWarpWidth = m_textWarpWidth;
			lastLineSpacing = lineSpacing;
#endif
		}
	}
	
	protected override void onUpdate(){
#if UNITY_EDITOR
		if(font == null /*|| font.isLoaded()*/) return;

		if(!Application.isPlaying){
			sprite = font.sprite;
			//update text mesh if needed
			if(lastText == null || lastText.CompareTo(m_text) != 0 || lastTextAlign != textAlign ||
			   (lastTextWarpWidth != m_textWarpWidth && m_textWarpWidth/100 > 0) || lastLineSpacing != lineSpacing){
				updateMesh();
			}
			if(lastColorTopLeft != colorTopLeft || lastColorTopRight != colorTopRight 
			|| lastColorBottomLeft != colorBottomLeft || lastColorBottomRight!=colorBottomRight){
				updateVertsColor();
			}
		}	
#endif
		base.onUpdate();
	}
	
	public override void setText(string txt){
		if(m_text == null || (m_text != null && txt == null ) || m_text.CompareTo(txt) != 0){
			m_text = txt;
			updateMesh();
		}
	}

	public override string getText(){
		return m_text;
	}
	
	public override void setWarpWidth(int width){
		if(m_textWarpWidth != width){
			m_textWarpWidth = width;
			updateMesh();
		}
	}
	public override void setColor(Color color){
		setColors(new Color[]{color,color,color,color});//can't use blend color because of the outline
	}

	public override Color getColor(){
		return colorTopLeft;
	}

	public void setColors(Color[] colors){
		colorTopLeft	= colors[0];
		colorTopRight	= colors[1];
		colorBottomLeft = colors[2];
		colorBottomRight= colors[3];
		updateVertsColor();
	}

	public override void setAlpha(float alpha){
		Color col = getBlendingColor();
		col.a = alpha;
		setBlendingColor(col);
	}

	string[] warpText(string txt,int width){
		string[] words = txt.Split(new char[]{ ' ' });
		
		int   linesCount = 1;
		float lineWidth = 0;
		float textScale = transform.localScale.x;
		
		for(int i = 0; i < words.Length;i++){
			if(words[i].IndexOf("\n") >= 0 || words[i].IndexOf("\\n") >= 0 
			|| (width > 0 && lineWidth + font.stringWidth(" " + words[i]) * textScale > width
			&& (font.stringWidth(" " + words[i]) * textScale < width || lineWidth != 0)))
			{
				if(words[i].IndexOf("\n") >= 0 || words[i].IndexOf("\\n") >= 0)
				{
					string endLn = words[i].IndexOf("\n") >= 0? "\n":"\\n";
					string tmp = words[i];
					while(tmp.IndexOf(endLn) >= 0){
						linesCount++;
						if(tmp.IndexOf(endLn) < tmp.Length - endLn.Length)
							tmp = tmp.Substring(tmp.IndexOf(endLn) + endLn.Length);
						else
							tmp ="";
					}	
					lineWidth = font.stringWidth(tmp) * textScale;
				}else{
					linesCount ++;
					lineWidth = font.stringWidth(words[i])* textScale;
				}
			}else{
				if(lineWidth == 0)
					lineWidth = font.stringWidth(words[i]) != 0 ? font.stringWidth(words[i]) * textScale : 1;
				else
					lineWidth+= font.stringWidth(" " + words[i]) * textScale;
			}
		}
		string[] textLines = new string[linesCount];
		
		int line = 0;
		lineWidth  = 0;
		for(int i = 0; i < words.Length;i++){
			if(words[i].IndexOf("\n") >= 0 || words[i].IndexOf("\\n") >= 0 
			|| (width > 0 && lineWidth + font.stringWidth(" " + words[i]) * textScale > width) 
			&& (font.stringWidth(" " + words[i]) * textScale < width || lineWidth != 0))
			{
				if(words[i].IndexOf("\n") >= 0 || words[i].IndexOf("\\n") >= 0)
				{
					string endLn = words[i].IndexOf("\n") >= 0? "\n":"\\n";
					string tmp = words[i];
					while(tmp.IndexOf(endLn) >= 0){
						textLines[line] += tmp.IndexOf(endLn) > 0 ? (lineWidth == 0 ? "":" ")+tmp.Substring(0,tmp.IndexOf(endLn)):"";
						line++;
						if(tmp.IndexOf(endLn) < tmp.Length - endLn.Length)
							tmp = tmp.Substring(tmp.IndexOf(endLn) + endLn.Length);
						else
							tmp ="";
					}	
					textLines[line] = tmp;
					lineWidth = font.stringWidth(tmp) * textScale;
				}else{
					line ++;
					textLines[line]  = words[i];
					lineWidth = font.stringWidth(words[i]) * textScale;
				}
			}else{
				if(lineWidth == 0){
					textLines[line] = words[i];
					lineWidth = font.stringWidth(words[i]) != 0 ? font.stringWidth(words[i]) * textScale : 1;
				}else{
					textLines[line]  += " " + words[i];
					lineWidth += font.stringWidth(" " + words[i]) * textScale;
				}
			}	
		}
		
		return textLines;
	}
	
	
	string []warpTextSingleLine(string txt,int width)
	{
		float textScale = transform.localScale.x;
		
		txt = txt.Replace("\n", " ");
		
		string[] textLines = new string[1];
		
		if (font.stringWidth(txt) * textScale <= width) {
			textLines[0] = txt;
		}
		else {
			
			int characterCount = 1;
			
			do {
				textLines[0] = txt.Substring(0, characterCount);
				textLines[0] = string.Concat(textLines[0], "...");
				characterCount += 1;
			}while (characterCount < txt.Length && font.stringWidth(textLines[0]) * textScale < width);
		}
		
		
		
		return textLines;
	}
	
	/*
	public int stringMaxHeight(string word){
		if(word == null) return 0;
		
		float maxHeight = 0;
		for(int i = 0; i < word.Length;i++){
			FrameData frame = (FrameData)font.getMeshForChar(word[i]);
			
			if(frame != null && frame.m_components.Length > 0 && maxHeight < frame.m_components[0].m_component.m_bounds.height){
				maxHeight = frame.m_components[0].m_component.m_bounds.height;
			}
		}
		return (int)maxHeight;
	}
	*/
	protected override void updateMesh(){
		setSourceSprite(font != null ? font.sprite : null);
#if UNITY_EDITOR
		lastText = m_text;
		lastTextAlign = textAlign;
		lastTextWarpWidth = m_textWarpWidth;
		lastLineSpacing = lineSpacing;
#endif
		if(m_text == null || m_text.Length < 1 
			|| font == null || font.sprite == null || !font.sprite.isLoaded()){
			clearObjectMesh();
			objMesh.center = Vector2.zero;
			objMesh.size = Vector2.zero;
			return;
		}else{
			
			string[] textLines = null;
			if(m_linesNumber == 1) {
				textLines = warpTextSingleLine(m_text, m_textWarpWidth);
			}
			else {
				textLines = warpText(m_text, m_textWarpWidth);
			}
			
			int charCount = 0;
			for(int i = 0; i < textLines.Length;i++){
				charCount += textLines[i].Length;
			}
			
			Vector2 textureSize = new Vector2(font.sprite.sourceTexture.width,font.sprite.sourceTexture.height);

			bool hasOutline = fontOutline != null;

			objMesh.vertices = new Vector3[charCount  * 4 * (hasOutline ? 2 : 1)];
			objMesh.triangles= new int[charCount * 6 * (hasOutline ? 2 : 1)];
			objMesh.UVs	  	 = new Vector2[charCount * 4 * (hasOutline ? 2 : 1)];
			objMesh.colors	 = new Color[charCount * 4 * (hasOutline ? 2 : 1)];
			
			
			float lineHeight = font.getHeight();
			
			float x = 0;
			float y = lineHeight;
			
			if(((int)textAlign & (int)kAlign.BOTTOM) != 0){
				y -= textLines.Length * lineHeight + (textLines.Length - 1) * lineSpacing;
			}else if(((int)textAlign & (int)kAlign.VCENTER) != 0){
				y -= (textLines.Length * lineHeight + (textLines.Length - 1) * lineSpacing) / 2;
			}
			Rect meshCorners = new Rect();
			int vertIndex = 0;
			for(int i = 0; i < textLines.Length;i++){
				if(((int)textAlign & (int)kAlign.RIGHT) != 0){
					x = - font.stringWidth(textLines[i]);
				}else if(((int)textAlign & (int)kAlign.HCENTER) != 0){
					x = - font.stringWidth(textLines[i])/2;
				}
				
				for(int j = 0; j < textLines[i].Length;j++){
					FrameData frame = (FrameData)font.getMeshForChar(textLines[i][j]);
						
					if(frame != null && frame.m_components.Length > 0){
						FrameData.FrameComponent fComp = frame.m_components[0];
						ModuleData module =(ModuleData) fComp.m_component;

						if(hasOutline)
						{
							FrameData frameOutline	= (FrameData)fontOutline.getMeshForChar(textLines[i][j]);
							FrameData.FrameComponent fCompOutline = frameOutline.m_components[0];
							//ModuleData moduleOutline = (ModuleData) fCompOutline.m_component;

							createVertsForModule(fCompOutline,x,y ,0.01f,vertIndex,textureSize);
							
							if(vertIndex == 0)
								meshCorners = new Rect(	objMesh.vertices[0].x,objMesh.vertices[0].y,objMesh.vertices[0].x,objMesh.vertices[0].y);
							
							//colors
							objMesh.colors[vertIndex * 4] 	 = outlineColor;		objMesh.colors[vertIndex * 4 + 1] = outlineColor;
							objMesh.colors[vertIndex * 4 + 2]= outlineColor;		objMesh.colors[vertIndex * 4 + 3] = outlineColor;
							if(vertIndex == 0)
								meshCorners = new Rect(	objMesh.vertices[0].x,objMesh.vertices[0].y,objMesh.vertices[0].x,objMesh.vertices[0].y);
							vertIndex++;
						}

						createVertsForModule(fComp,x ,y,0,vertIndex,textureSize);

						if(vertIndex == 0)
							meshCorners = new Rect(	objMesh.vertices[0].x,objMesh.vertices[0].y,objMesh.vertices[0].x,objMesh.vertices[0].y);
						meshCorners = updateCorners(objMesh,meshCorners,vertIndex * 4,4);

						//colors
						objMesh.colors[vertIndex * 4] 	 = colorTopLeft;		objMesh.colors[vertIndex * 4 + 1] = colorTopRight;
						objMesh.colors[vertIndex * 4 + 2]= colorBottomLeft;		objMesh.colors[vertIndex * 4 + 3] = colorBottomRight;

						x += fComp.m_compPos.x + module.m_bounds.width * fComp.m_scaleX;
						vertIndex++;
					}else{
						Debug.Log("Character not found in font file: '"+textLines[i][j]+"'");
						vertIndex++;
					}
				}
				x = 0;
				y += lineHeight + lineSpacing;
			}
			objMesh.center.x = (meshCorners.x + meshCorners.width)/2;
			objMesh.center.y = (meshCorners.y + meshCorners.height)/2;
			objMesh.size.x	 = (objMesh.center.x - meshCorners.x)*2;
			objMesh.size.y	 = (objMesh.center.y - meshCorners.y)*2;
		}
		if(m_material == null
		&& font.sprite != null && font.sprite.isLoaded()){
			m_material.mainTexture = font.sprite.sourceTexture;
		}
		meshChanged = true;
	}

	private void createVertsForModule(FrameData.FrameComponent fComp,float x,float y,float z,int vertIndex,Vector2 textureSize)
	{	
		ModuleData module =(ModuleData) fComp.m_component;
		//Verts
		updateMeshVerts(module,vertIndex * 4,x + fComp.m_compPos.x,y + fComp.m_compPos.y,z,fComp.m_scaleX ,fComp.m_scaleY,fComp.m_angle);
		
		//indices
		objMesh.triangles[vertIndex * 6] = vertIndex* 4; 			objMesh.triangles[vertIndex * 6 + 1] = vertIndex* 4 + 3; 
		objMesh.triangles[vertIndex * 6 + 2] = vertIndex* 4 + 2;	objMesh.triangles[vertIndex * 6 + 3] = vertIndex* 4 + 0;	
		objMesh.triangles[vertIndex * 6 + 4] = vertIndex* 4 + 2;	objMesh.triangles[vertIndex * 6 + 5] = vertIndex* 4 + 1;
		
		//UVs
		Rect mBounds = module.m_bounds;
		mBounds.x = mBounds.x / textureSize.x;
		mBounds.y = 1.0f - (( mBounds.y + mBounds.height) / textureSize.y);
		
		mBounds.width = mBounds.width / textureSize.x;
		mBounds.height= mBounds.height / textureSize.y;
		
		objMesh.UVs[vertIndex * 4 + 3] 	= new Vector2(mBounds.x ,mBounds.y + mBounds.height);
		objMesh.UVs[vertIndex * 4 + 2]	= new Vector2(mBounds.x + mBounds.width,mBounds.y + mBounds.height);
		objMesh.UVs[vertIndex * 4 + 1] 	= new Vector2(mBounds.x + mBounds.width,mBounds.y);
		objMesh.UVs[vertIndex * 4 + 0] 	= new Vector2(mBounds.x,mBounds.y);
	}
	
	private void updateMeshVerts(ModuleData module,int index,float x, float y,float z,float scaleX,float scaleY,int angle){
		objMesh.vertices[index + 3] = new Vector3(0, 0,z);
		objMesh.vertices[index + 2] = new Vector3(module.m_bounds.width,0,z);
		objMesh.vertices[index + 1] = new Vector3(module.m_bounds.width,module.m_bounds.height,z);
		objMesh.vertices[index + 0] = new Vector3(0,module.m_bounds.height,z);
			
		for(int i = 0; i < 4;i++){
			//scale
			float newX = objMesh.vertices[index + i].x * scaleX;
			float newY = objMesh.vertices[index + i].y * scaleY;
		
			//rotate
			objMesh.vertices[index + i].x = Mathf.Cos( angle * Mathf.Deg2Rad) * (newX) - Mathf.Sin(angle*Mathf.Deg2Rad) * (newY);   
        	objMesh.vertices[index + i].y = Mathf.Sin( angle * Mathf.Deg2Rad) * (newX) + Mathf.Cos(angle*Mathf.Deg2Rad) * (newY); 
			//update pos in frame
			objMesh.vertices[index + i].x += x;
			objMesh.vertices[index + i].y  = -1 * (objMesh.vertices[index + i].y + y);
		}
	}

	public Color getOutlineColor(){
		return outlineColor;
	}

	public void setOutlineColor(Color c){
		outlineColor = c;
		updateVertsColor();
	}

	void updateVertsColor(){
#if UNITY_EDITOR
		lastColorTopLeft 	= colorTopLeft;
		lastColorTopRight	= colorTopRight;
		lastColorBottomLeft = colorBottomLeft;
		lastColorBottomRight=colorBottomRight;
#endif
		bool hasOutline = fontOutline != null;
		if(objMesh.colors != null){
			for(int i = 0; i < objMesh.colors.Length/4;i++){
				if(hasOutline && i % 2 == 0){
					objMesh.colors[i * 4] 	 = outlineColor;
					objMesh.colors[i * 4 + 1] = outlineColor;
					objMesh.colors[i * 4 + 2] = outlineColor;
					objMesh.colors[i * 4 + 3] = outlineColor;
				}else{
					objMesh.colors[i * 4] 	 = colorTopLeft;
					objMesh.colors[i * 4 + 1] = colorTopRight;
					objMesh.colors[i * 4 + 2] = colorBottomLeft;
					objMesh.colors[i * 4 + 3] = colorBottomRight;
				}
			}
			colorsChanged = true;
		}
	}

	public override TextAnchor getAnchor(){
		switch(textAlign){
			case kAlignMode.TOP_LEFT: return TextAnchor.UpperLeft;
			case kAlignMode.TOP_CENTER: return TextAnchor.UpperCenter;
			case kAlignMode.TOP_RIGHT: return TextAnchor.UpperRight;

			case kAlignMode.VCENTER_LEFT: return TextAnchor.MiddleLeft;
			case kAlignMode.VCENTER_CENTER: return TextAnchor.MiddleCenter;
			case kAlignMode.VCENTER_RIGHT: return TextAnchor.MiddleRight;

			case kAlignMode.BOTTOM_LEFT: return TextAnchor.LowerLeft;
			case kAlignMode.BOTTOM_CENTER: return TextAnchor.LowerCenter;
			case kAlignMode.BOTTOM_RIGHT: return TextAnchor.LowerRight;
		}
		return TextAnchor.MiddleCenter;
	}
	
	public override void setAnchor(TextAnchor anchor){
		switch(anchor){
			case TextAnchor.UpperLeft : textAlign = kAlignMode.TOP_LEFT ;break;
			case TextAnchor.UpperCenter : textAlign = kAlignMode.TOP_CENTER ;break;
			case TextAnchor.UpperRight : textAlign = kAlignMode.TOP_RIGHT ;break;
					
			case TextAnchor.MiddleLeft: textAlign = kAlignMode.VCENTER_LEFT;break;
			case TextAnchor.MiddleCenter: textAlign =kAlignMode.VCENTER_CENTER;break;
			case TextAnchor.MiddleRight: textAlign = kAlignMode.VCENTER_RIGHT;break;
						
			case TextAnchor.LowerLeft: textAlign = kAlignMode.BOTTOM_LEFT;break;
			case TextAnchor.LowerCenter: textAlign = kAlignMode.BOTTOM_CENTER;break;
			case TextAnchor.LowerRight: textAlign = kAlignMode.BOTTOM_RIGHT;break;
		}
	}
	
	public override TextAlignment getAlignment(){
		return TextAlignment.Left;
	}
	
	public override void setAlignment(TextAlignment align){
		//m_textMesh.alignment = align;
	}

	public override FontStyle getFontStyle(){
		return FontStyle.Normal;
	}

	public override void setFontStyle(FontStyle style){

	}

	public override float stringWidth(string s){
		if(font != null && font.isLoaded()){
			return font.stringWidth(s);
		}
		return 0;
	}
	
#if UNITY_EDITOR	
	void OnDrawGizmos(){
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position,2);
	}
#endif
}
