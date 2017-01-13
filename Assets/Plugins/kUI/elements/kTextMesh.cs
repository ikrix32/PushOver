using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[ExecuteInEditMode]
public class kTextMesh : kTextField
{
	public TextMesh m_textMesh;
	public bool 	m_richText = true;
	public bool 	m_emojiiSupport = false;
	public bool 	m_typewriterText = false;
	public float	m_typewriterCharTime = 0.05f;

	private string m_typewriterFullText = "";

	private kEmojiFontHelper m_emojiFont;

	private static Shader s_shader = null;
	private static Shader s_shader_no_clip = null;

	public string m_textureSize;
	
	protected override Shader shader{
		get{ return s_shader;}
		set{ s_shader = value;}
	}
	
	protected override Shader shader_no_clip{
		get{ return s_shader_no_clip;}
		set{ s_shader_no_clip = value;}
	}

	private bool m_dirty = false;

	void setDirty()
	{
		m_dirty = true;
	}
	
	private Material[] m_materials{
		get{
			if(!Application.isPlaying){
				return GetComponent<Renderer>().sharedMaterials;
			}else
				return GetComponent<Renderer>().materials;
		}
		set{
			if(!Application.isPlaying)
				GetComponent<Renderer>().sharedMaterials = value;
			else
				GetComponent<Renderer>().materials = value;
		}
	}
	
	protected override void onInit()
	{
		base.onInit();

		if(s_shader == null)
			s_shader = Shader.Find("kSprite/Text Shader");
		
		if(s_shader_no_clip == null)
			s_shader_no_clip = Shader.Find("kSprite/Text ShaderN");

		if ((m_textMesh = GetComponent<TextMesh>()) == null){
			MeshFilter filter = GetComponent<MeshFilter>();
			if(filter != null)
				DestroyImmediate(filter);
			m_textMesh = gameObject.AddComponent<TextMesh>();
		}

		m_material.shader = shader;//Shader.Find("kSprite/Text Shader");
		m_crtShader = SHADER_CLIP;
		m_material.name = "Font material";
	}
	
	protected override void onStart()
	{
		base.onStart();
		m_material.mainTexture = m_textMesh.font.material.mainTexture;
	
		if (m_emojiiSupport) {
			m_emojiFont = new kEmojiFontHelper();
		}
		updateTextMesh();
		setDirty();

		/*if (Application.isPlaying && m_typewriterText) {
			StartCoroutine(playTypewriterAni());
		}*/
	}

	protected override void onEnable()
	{
		base.onEnable();
		setDirty();
		if (Application.isPlaying && m_typewriterText) {
			m_typewriterFullText = m_text;
			StartCoroutine(playTypewriterAni());
		}
	}

	protected override void onDisable()
	{
		base.onDisable();
		if (Application.isPlaying && m_typewriterText) {
			StopAllCoroutines();
			m_text = m_typewriterFullText;
		}
	}

	protected override void onUpdate()
	{
		#if UNITY_EDITOR
		if(m_material != null && m_material.mainTexture != null)
			m_textureSize = ""+ m_material.mainTexture.width+"x"+m_material.mainTexture.height;
		#endif
		if (m_textMesh == null)
			return;
		
		if (m_dirty)
		{
			m_dirty = false;
			updateTextMesh();
			/*// quick hack to force font to redraw
			string txt = m_textMesh.text;
			m_textMesh.text = " ";
			m_textMesh.text = txt;
			objMesh.center = new Vector2((renderer.bounds.center.x - transform.position.x) / transform.lossyScale.x, (renderer.bounds.center.y - transform.position.y) / transform.lossyScale.y);
			objMesh.size = new Vector2(renderer.bounds.size.x / transform.lossyScale.x, renderer.bounds.size.y / transform.lossyScale.y);
			*/
		}
		base.onUpdate();
	}

	public void setup(Font font, int fontSize, float characterSize, float lineSpacing)
	{
		if(m_textMesh != null)
		{
			m_textMesh.font = font;
			m_textMesh.fontSize = fontSize;
			m_textMesh.characterSize = characterSize;
			m_textMesh.lineSpacing = lineSpacing;
		}
	}

	public void setFontSize(int fontSize)
	{
		m_textMesh.fontSize = fontSize;
	}

	public override string getText()
	{
		return m_typewriterText ? m_typewriterFullText : m_text;
	}

	public override void setText(string txt)
	{
		if(m_text != txt){
			base.setText(txt);
			updateTextMesh();
			setDirty();
			if (m_typewriterText) {
				m_typewriterFullText = m_text;
				if (gameObject.activeInHierarchy) {
					StopAllCoroutines();
					StartCoroutine(playTypewriterAni());
				}
			}
		}
	}

	public override TextAnchor getAnchor(){
		return m_textMesh.anchor ;
	}

	public override void setAnchor(TextAnchor anchor){
		m_textMesh.anchor = anchor;
	}

	public override TextAlignment getAlignment(){
		return m_textMesh.alignment;
	}

	public override void setAlignment(TextAlignment align){
		m_textMesh.alignment = align;
	}

	public override FontStyle getFontStyle(){
		return m_textMesh.fontStyle;
	}

	public override void setFontStyle(FontStyle style){
		m_textMesh.fontStyle = style;
	}
	
	public override void setWarpWidth(int width)
	{
		if (m_textWarpWidth != width)
		{
			base.setWarpWidth(width);
			//updateTextMesh();
			setDirty();
		}
	}

	public override void setBlendingColor(Color color){
		base.setBlendingColor(color);
		//if(m_textMesh.richText)
		setDirty();
	}
	
	public override void setColor(Color color){
		setBlendingColor(color);
	}

	public override Color getColor (){
		return getBlendingColor();
	}

	public override void setAlpha(float alpha){
		Color x = getBlendingColor();
		x.a = alpha;
		setBlendingColor(x);
	}

	public static string colorToHtmlColor(Color color)
	{
		int intColor = ((int)(color.r * 255) << 24) | ((int)(color.g * 255) << 16) | ((int)(color.b * 255) << 8) | (int)(color.a * 255);
		return intColor.ToString("x8");
	}

	public void updateTextMesh()
	{
		if (m_text == null)
			m_text = "";
		if (m_textWarpWidth < MIN_WRAP_WIDTH)
			m_textWarpWidth = 0;
		if (m_spaceWidth == 0)
			m_spaceWidth = stringWidth("a a") - stringWidth("aa");
		
		m_textMesh.text = "";

		bool isRichText = parseText(m_text);
		bool hasEmoji = hasEmojiChars(m_text);
		m_textMesh.richText = m_richText && (isRichText || hasEmoji);

		Material[] mat = null;
		if (Application.isPlaying && m_emojiiSupport && m_emojiFont != null && hasEmoji) {
			if (m_materials.Length < 2)
				mat = new Material[2];
			else
				mat = m_materials;
			mat[0] = m_material;
			mat[1] = m_emojiFont.getMaterial();
			mat[1].SetVector("_Clip", m_material.GetVector("_Clip"));
		} else {
			if (m_materials.Length != 1)
				mat = new Material[1];
			else
				mat = m_materials;
			mat[0] = m_material;
		}
		m_materials = mat;

		string renderText = wrapText(m_text, m_textWarpWidth);

		setTextMeshText(renderText, hasEmoji);
	}

	private void setTextMeshText(string renderText, bool hasEmoji = false)
	{
		if (m_textMesh.richText) {
			m_material.color = Color.white;
			renderText = "<color=#" + colorToHtmlColor(getBlendingColor()) + ">" + renderText + "</color>";
		} else {
			m_material.color = getBlendingColor();
		}
		if (m_emojiiSupport && m_emojiFont != null && hasEmoji)
			m_textMesh.text = m_emojiFont.getDinamicFontText(renderText, m_textMesh.fontSize);
		else
			m_textMesh.text = renderText;

		objMesh.center = new Vector2((GetComponent<Renderer>().bounds.center.x - transform.position.x) / transform.lossyScale.x, (GetComponent<Renderer>().bounds.center.y - transform.position.y) / transform.lossyScale.y);
		objMesh.size = new Vector2(Mathf.Abs(GetComponent<Renderer>().bounds.size.x / transform.lossyScale.x), Mathf.Abs(GetComponent<Renderer>().bounds.size.y / transform.lossyScale.y));
		meshChanged = true;
	}

	public override void updateObjectMesh(bool forceClipUpdate = false)
	{
		if (meshChanged || (m_transform != null && m_transform.hasChanged) ||
		    hasChangedOnLastFrame || clipChanged || forceClipUpdate) {
			clipMesh(meshChanged);
		}
		//clipMesh(meshChanged);

		if (meshChanged || clipChanged || forceClipUpdate)
		{
			if (m_materials.Length > 1 && m_materials[1] != null) {
				m_materials[1].SetVector("_Clip",m_material.GetVector("_Clip"));
			}

			clipChanged = false;
			meshChanged = false;
			kTouchable touch = GetComponent<kTouchable>();
			if (touch != null)
				touch.updateTouchArea();
		}
		if (meshChanged)
			sendMeshChangedNotif();//childMeshChanged(this);
	}

	public float getTextWidth()
	{
		return stringWidth (m_text);
	}
	
	public override float stringWidth(string s)
	{
		int buggySpaces = 0;
		string text = m_textMesh.text;
		if (m_emojiiSupport && m_emojiFont != null) {
			// stupid TextMesh counts some spaces twice
			bool foundEmoji = false, foundOther = false;
			for (int i = s.Length - 1; i >= 0; i--)
			{
				if (i > 0 && System.Char.IsSurrogatePair(s, i - 1))
					i--;
				if (s[i] == ' ')
				{
					if (foundEmoji && foundOther)
						++buggySpaces;
				}
				else if (System.Char.IsSurrogatePair(s, i))
				{
					foundEmoji = true;
					if (!foundOther)
						break;
				}
				else
				{
					foundOther = true;
					if (foundEmoji)
						break;
				}
			}
			m_textMesh.text = m_emojiFont.getDinamicFontText(s, m_textMesh.fontSize, false);
		} else {
			m_textMesh.text = s;
		}
		float w = Mathf.Abs(m_textMesh.GetComponent<Renderer>().bounds.size.x / transform.lossyScale.x) - buggySpaces * m_spaceWidth;
		//Debug.Log("stringWidth(" + s + ") = " + w + " (buggySpaces: " + buggySpaces + ")");
		m_textMesh.text = text;
		return w;
	}

	protected override void onDestroy(){
		if(m_emojiFont != null)
			m_emojiFont.destroy();
		if(m_emojiFont != null){
			if(Application.isPlaying)
				Destroy(m_emojiFont.getMaterial());
			else
				DestroyImmediate(m_emojiFont.getMaterial());
		}
		base.onDestroy();
	}

	private IEnumerator playTypewriterAni()
	{
		string fullText = m_text;
		updateTextMesh();

		m_text = "";
		int charsToAdd = 1;
		float lastFrameTime = 0;
		List<kTextLine> lines = m_textLines;
		List<kMarkupTag> tags = m_markupTags;
		for (int i = 0; lines != null && i < lines.Count; ++i) {
			int lineLength = lines[i].idxEnd - lines[i].idxStart;
			for (int j = 1; j <= lineLength; ++j) {
				// skip tags
				int tagIndex = 0;
				while (tagIndex < tags.Count && tags[tagIndex].idxStart < lines[i].idxStart + j) {
					if (tags[tagIndex].idxStart + tags[tagIndex].tagStart.Length >= lines[i].idxStart + j) {
						j += tags[tagIndex].tagStart.Length;
					}
					if (tags[tagIndex].idxEnd < lines[i].idxStart + j && tags[tagIndex].idxEnd + tags[tagIndex].tagEnd.Length >= lines[i].idxStart + j) {
						j += tags[tagIndex].tagEnd.Length;
					}
					++tagIndex;
				}

				j = Mathf.Min(j, lineLength);
				string crtLine = fullText.Substring(lines[i].idxStart, j);

				// close open tags
				tagIndex = 0;
				while (tagIndex < tags.Count && tags[tagIndex].idxStart < lines[i].idxStart + j) {
					++tagIndex;
				}
				while (--tagIndex >= 0) {
					if (tags[tagIndex].idxStart < lines[i].idxStart + j && tags[tagIndex].idxEnd >= lines[i].idxStart + j) {
						crtLine += tags[tagIndex].tagEnd;
					}
				}

				m_textMesh.richText = true;
				setTextMeshText(m_text + crtLine);

				if (--charsToAdd <= 0) {
					yield return new WaitForSeconds(m_typewriterCharTime);
					charsToAdd = lastFrameTime > 0 ? Mathf.RoundToInt((Time.time - lastFrameTime) / m_typewriterCharTime) : 1;
					lastFrameTime = Time.time;
				}
			}
			m_text += fullText.Substring(lines[i].idxStart, lineLength) + "\n";
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (objMesh != null)
		{
			Rect b = getBoundsWorld();
			Vector3 center = new Vector3(b.center.x, b.center.y, 0);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(center, new Vector3(b.width, b.height, 2));
		}
		if (clippedMesh != null)
		{
			Rect b = getClippedBoundsWorld();
			Vector3 center = new Vector3(b.center.x, b.center.y, 0);
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(center, new Vector3(b.width, b.height, 2));
		}
		IntRect clipRect = m_clipSource != null ? m_clipSource.getClip() : RectUtil.RECT_NO_CLIP;
		if (clipRect.width != 0 && clipRect.height != 0)
		{
			Rect b = new Rect(clipRect.x, clipRect.y, clipRect.width, clipRect.height);
			Vector3 center = new Vector3(b.center.x, b.center.y, 0);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(center, new Vector3(b.width, b.height, 2));
		}
	}
#endif
}
