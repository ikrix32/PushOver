using UnityEngine;
using System.Collections;

public class kEmojiFontHelper{
	protected SysFontTexture _texture;

	protected Material _material;

	private string 	m_emojiCharset = "";
	private float 	m_charWidth;
	private float 	m_charHeight;

	public kEmojiFontHelper(){
		_material= new Material(Shader.Find ("kSprite/Sprite Shader"));
		_material.name = "emoji material";
		_texture = new SysFontTexture();
		_texture.AppleFontName = "";
		_texture.FontSize = 32;
		_texture.Alignment = SysFont.Alignment.Left;
		_texture.IsMultiLine = false;
	}
	
	public string getDinamicFontText(string text,int fontSize,bool redraw = true){
		if(text == null) return null;

		string result = text;
	
		updateEmojiCharset(result,redraw);

		for(int i = 0; i < result.Length - 1;i++){
			if(System.Char.IsSurrogatePair(result,i)){
				string emoji = result.Substring(i,2);
				result = result.Replace(emoji,getQuadStrFor(emoji,fontSize));
			}
		} 
		return result;
	}

	public Material getMaterial(){
		return _material;
	}

	public string getQuadStrFor(string emojiCharPair,int fontSize){
		int index = m_emojiCharset.IndexOf(emojiCharPair);
		index = index/2;

		float x = index * m_charWidth;
		float y = 0.07f;
		//fontSize= (int)(fontSize * 1.3f);

		return "<color=#ffffffff><quad material=1 size="+fontSize+" x="+x+" y="+y+" width="+m_charWidth+" height="+m_charHeight+" /></color>";
	}

	public void updateEmojiCharset(string text,bool redraw){
		string newCharset = "";
		bool needsToRedrawCharset = false;
		for(int i = 0; i < text.Length - 1;i++){
			if(System.Char.IsSurrogatePair(text,i)){
				string emoji = text.Substring(i,2);
				int index = newCharset.IndexOf(emoji);
				if(index < 0)
					newCharset += emoji;
				if(!needsToRedrawCharset)
					needsToRedrawCharset = m_emojiCharset.IndexOf(emoji) < 0;
			}
		}

		if(needsToRedrawCharset || (redraw && Mathf.Abs(m_emojiCharset.Length - newCharset.Length) > 5)){
			setCharset(newCharset);
		}
	}

	private void setCharset(string newCharset){
		Debug.Log("Update charset!!");
		m_emojiCharset = newCharset;
		_texture.FontName = "Apple Color Emoji";
		_texture.FontSize = 32;
		_texture.AndroidFontName = null;
		_texture.AppleFontName = "Apple Color Emoji";
		_texture.IsBold = false;
		_texture.IsItalic = false;
		_texture.IsMultiLine = false;
		_texture.Text = m_emojiCharset;
		_texture.Update();
		_material.mainTexture = _texture.Texture;

		Debug.Log(" Charset :"+m_emojiCharset +" Text width:"+_texture.TextWidthPixels+" texture width:"+_texture.WidthPixels);

		float textWidth = _texture.TextWidthPixels * 1.0f/_texture.WidthPixels;

		m_charHeight= _texture.TextHeightPixels * 1.0f /_texture.HeightPixels;
		m_charWidth = textWidth /(m_emojiCharset.Length/2);
	}

	public void destroy()
	{
		if (_texture != null)
		{
			_texture.Destroy();
			_texture = null;
		}
	}
}
