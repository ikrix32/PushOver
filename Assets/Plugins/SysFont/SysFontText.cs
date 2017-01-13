	/*
	 * Copyright (c) 2012 Mario Freitas (imkira@gmail.com)
	 *
	 * Permission is hereby granted, free of charge, to any person obtaining
	 * a copy of this software and associated documentation files (the
	 * "Software"), to deal in the Software without restriction, including
	 * without limitation the rights to use, copy, modify, merge, publish,
	 * distribute, sublicense, and/or sell copies of the Software, and to
	 * permit persons to whom the Software is furnished to do so, subject to
	 * the following conditions:
	 *
	 * The above copyright notice and this permission notice shall be
	 * included in all copies or substantial portions of the Software.
	 *
	 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
	 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
	 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
	 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
	 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
	 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	 */

	using UnityEngine;
using System.Collections.Generic;

	[ExecuteInEditMode]
	[AddComponentMenu("SysFont/Text")]
	public class SysFontText : kTextField
	{
	  [SerializeField]
	  protected SysFontTexture _texture = new SysFontTexture();

	private static SysFontText s_helperInstance = null;

	protected SysFontText HelperText{
		get{
			if(Application.isPlaying){
				if(s_helperInstance == null){
					GameObject scriptHolder;
					scriptHolder = new GameObject();
					scriptHolder.name = "SYSFONT_HELPER_TEXT";
					scriptHolder.transform.position = new Vector3(0,0,0);
					s_helperInstance = scriptHolder.AddComponent<SysFontText>();
				}
				return s_helperInstance;
			}else
				return null;
		}
	}
		
	  #region ISysFontTexturable properties



	public string AppleFontName
	{
		get
		{
			return _texture.AppleFontName;
		}
		set
		{
			_texture.AppleFontName = value;
		}
	}

	public string AndroidFontName
	{
		get
		{
			return _texture.AndroidFontName;
		}
		set
		{
			_texture.AndroidFontName = value;
		}
	}

	public string FontName
	{
		get
		{
			return _texture.FontName;
		}
		set
		{
			_texture.FontName = value;
		}
	}

	public int FontSize
	{
		get
		{
			return _texture.FontSize;
		}
		set
		{
			_texture.FontSize = value;
		}
	}

	protected Color _lastFontColor;
	public Color FontColor
	{
		get{
			return _texture.FontColor;
		}

		set
		{
			if (_texture.FontColor != value){
				_texture.FontColor = value;
			}
		}
	}
	
	public bool IsBold
	{
		get
		{
			return _texture.IsBold;
		}
		set
		{
			_texture.IsBold = value;
		}
	}

	public bool IsItalic
	{
		get
		{
			return _texture.IsItalic;
		}
		set
		{
			_texture.IsItalic = value;
		}
	}

	public SysFont.Alignment Alignment
	{
		get
		{
			return _texture.Alignment;
		}
		set
		{
			_texture.Alignment = value;
		}
	}

	public bool IsMultiLine
	{
		get
		{
			return _texture.IsMultiLine;
		}
		set
		{
			_texture.IsMultiLine = value;
		}
	}

	public int MaxWidthPixels
	{
		get
		{
			return getWarpWidth();
		}
		set
		{
			setWarpWidth(value);
		}
	}

	public int MaxHeightPixels
	{
		get
		{
			return _texture.MaxHeightPixels;
		}
		set
		{
			_texture.MaxHeightPixels = value;
		}
	}

	public int WidthPixels
	{
		get
		{
			return _texture.WidthPixels;
		}
	}

	public int HeightPixels
	{
		get
		{
			return _texture.HeightPixels;
		}
	}

	public int TextWidthPixels
	{
		get
		{
			return _texture.TextWidthPixels;
		}
	}

	public int TextHeightPixels
	{
		get
		{
			return _texture.TextHeightPixels;
		}
	}

	public Texture2D Texture
	{
		get
		{
		return _texture.Texture;
		}
	}
	#endregion
	
	  public enum PivotAlignment
	  {
	    TopLeft,
	    Top,
	    TopRight,
	    Left,
	    Center,
	    Right,
	    BottomLeft,
	    Bottom,
	    BottomRight
	  }

	  [SerializeField]
	protected PivotAlignment _pivot = PivotAlignment.Center;

	  protected PivotAlignment _lastPivot;
	  public PivotAlignment Pivot
	  {
	    get
	    {
	      return _pivot;
	    }
	    set
	    {
	      if (_pivot != value)
	      {
	        _pivot = value;
	      }
	    }
	  }

	protected Vector3[] _vertices{
		get{
			return objMesh.vertices;
		}
		set{
			objMesh.vertices = value;
		}
	}
  
	protected override void updateMesh()
	{
		if (objMesh.UVs == null || objMesh.UVs.Length != objMesh.vertices.Length){
			objMesh.UVs = new Vector2[4];
			objMesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
	    }

	    Vector2 uv = new Vector2(_texture.TextWidthPixels /
	        (float)_texture.WidthPixels, _texture.TextHeightPixels /
	        (float)_texture.HeightPixels);

		objMesh.UVs[0] = Vector2.zero;
		objMesh.UVs[1] = new Vector2(uv.x, 0f);
		objMesh.UVs[2] = new Vector2(0f, uv.y);
		objMesh.UVs[3] = uv;

	    UpdatePivot();
	    UpdateScale();

		Rect meshCorners = new Rect();
		if(objMesh.vertices.Length > 0)
			meshCorners = new Rect(	objMesh.vertices[0].x,objMesh.vertices[0].y,objMesh.vertices[0].x,objMesh.vertices[0].y);
		meshCorners = updateCorners(objMesh,meshCorners,0,objMesh.vertices.Length);
		
		objMesh.center.x = (meshCorners.x + meshCorners.width)/2;
		objMesh.center.y = (meshCorners.y + meshCorners.height)/2;
		objMesh.size.x	 = (objMesh.center.x - meshCorners.x)*2;
		objMesh.size.y	 = (objMesh.center.y - meshCorners.y)*2;
		/*objMesh.center = new Vector2((renderer.bounds.center.x - transform.position.x) / transform.lossyScale.x, (renderer.bounds.center.y - transform.position.y) / transform.lossyScale.y);
		objMesh.size = new Vector2(Mathf.Abs(renderer.bounds.size.x / transform.lossyScale.x), Mathf.Abs(renderer.bounds.size.y / transform.lossyScale.y));
*/
		
		m_material.mainTexture = Texture;

		meshChanged = true;
	}

	protected void UpdatePivot()
	{
		if (_vertices == null || _vertices.Length != 4)
		{
			_vertices = new Vector3[4];
			_vertices[0] = Vector3.zero;
			_vertices[1] = Vector3.zero;
			_vertices[2] = Vector3.zero;
			_vertices[3] = Vector3.zero;
		}

		// horizontal
		if ((_pivot == PivotAlignment.TopLeft) ||
		(_pivot == PivotAlignment.Left) ||
		(_pivot == PivotAlignment.BottomLeft))
		{
			_vertices[0].x = _vertices[2].x = 0f;
			_vertices[1].x = _vertices[3].x = 1f;
		}
		else if ((_pivot == PivotAlignment.TopRight) ||
		(_pivot == PivotAlignment.Right) ||
		(_pivot == PivotAlignment.BottomRight))
		{
			_vertices[0].x = _vertices[2].x = -1f;
			_vertices[1].x = _vertices[3].x = 0f;
		}
		else
		{
			_vertices[0].x = _vertices[2].x = -0.5f;
			_vertices[1].x = _vertices[3].x = 0.5f;
		}

		// vertical
		if ((_pivot == PivotAlignment.TopLeft) ||
		(_pivot == PivotAlignment.Top) ||
		(_pivot == PivotAlignment.TopRight))
		{
			_vertices[0].y = _vertices[1].y = -1f;
			_vertices[2].y = _vertices[3].y = 0f;
		}
		else if ((_pivot == PivotAlignment.BottomLeft) ||
		(_pivot == PivotAlignment.Bottom) ||
		(_pivot == PivotAlignment.BottomRight))
		{
			_vertices[0].y = _vertices[1].y = 0f;
			_vertices[2].y = _vertices[3].y = 1f;
		}
		else
		{
			_vertices[0].y = _vertices[1].y = -0.5f;
			_vertices[2].y = _vertices[3].y = 0.5f;
		}

		/*if (_mesh == null)
		{
		_mesh = new Mesh();
		_mesh.name = "SysFontTextMesh";
		_mesh.hideFlags = HideFlags.DontSave | HideFlags.DontSave;
		}
		_mesh.vertices = _vertices;
		_mesh.uv = _uv;
		_mesh.triangles = _triangles;
		_mesh.RecalculateBounds();
		_filter.mesh = _mesh;*/

		_lastPivot = _pivot;

		meshChanged = true;
	}

	public void UpdateScale()
	{
		Vector3 scale = Vector3.one;
		scale.x = (float)_texture.TextWidthPixels;
		scale.y = (float)_texture.TextHeightPixels;
		//m_transform.localScale = scale;
		for(int i = 0; i < _vertices.Length;i++){
			_vertices[i] = Vector3.Scale(_vertices[i],scale);
		}
	}

	#region MonoBehaviour methods
	protected override void onInit(){
		base.onInit();
	
		updateMesh();
	}

	protected override void onStart(){
		base.onStart();

		updateMesh();
	}

	protected override void onUpdate()
	{
		if (_texture.NeedsRedraw)
		{
			if (_texture.Update() == false){
				return;
			}
			updateMesh();
		}

		if (_texture.IsUpdated == false){
			return;
		}

		if (_lastPivot != _pivot){
			UpdatePivot();
		}
		base.onUpdate();
	}

	protected override void onDestroy()
	{
		if (_texture != null)
		{
			_texture.Destroy();
			_texture = null;
		}
		base.onDestroy();
	}
	#endregion

	#region kTextField

	public override void setText(string txt){
		if(m_text != txt){
			base.setText(txt);
			_texture.Text = wrapText(txt,m_textWarpWidth);
			_texture.MaxWidthPixels = 2048;
			if (_texture.Update())
				updateMesh();
		}
	}
	
	public override TextAnchor getAnchor(){
		return (TextAnchor)Pivot;
	}
	
	public override void setAnchor(TextAnchor anchor){
		Pivot = (PivotAlignment)anchor;
	}
	
	public override TextAlignment getAlignment(){
		return (TextAlignment)Alignment;
	}
	public override void setAlignment(TextAlignment align){
		Alignment = (SysFont.Alignment)align;
	}
	
	public override FontStyle getFontStyle(){
		FontStyle style = FontStyle.Normal;
		if(IsBold){
			style = FontStyle.Bold;
			if(IsItalic)
				style = FontStyle.BoldAndItalic;
		}else if(IsItalic)
			style = FontStyle.Italic;
		return style;
	}
	public override void setFontStyle(FontStyle style){
		IsBold = style == FontStyle.Bold  || style == FontStyle.BoldAndItalic;
		IsItalic = style == FontStyle.Italic  || style == FontStyle.BoldAndItalic;
	}
	
	public override void setWarpWidth(int width){
		base.setWarpWidth(width);
		_texture.MaxWidthPixels = width;
	}
	
	public override void setColor(Color color){
		FontColor = color;
	}

	public override Color getColor ()
	{
		return FontColor;
	} 

	public override void setAlpha(float alpha){}

	public override float stringWidth(string s){
		if(Application.isPlaying && this != HelperText){
			HelperText.FontName = this.FontName;
			HelperText.FontSize = this.FontSize;
			HelperText.AndroidFontName = this.AndroidFontName;
			HelperText.AppleFontName = this.AppleFontName;
			HelperText.IsBold = this.IsBold;
			HelperText.IsItalic = this.IsItalic;
			HelperText.IsMultiLine = false;
			HelperText._texture.Text = s;
			HelperText._texture.Update();
			return HelperText.TextWidthPixels;
		}else
			return 0;
	}
	#endregion	
}
