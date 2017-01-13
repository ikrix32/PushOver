using UnityEngine;
using System.Collections;

public class kScrollBar : kCustomMeshObject
{
	public enum ScrollBarType {
		VERTICAL = 0,
		HORIZONTAL,
		BOTH_AXES,
	}
	
	private const float SCROLL_BAR_WIDTH = 5f;
	private const float SCROLL_SHOWN_TIME = 0.6f;
	private const float SCROLL_DISAPEAR_TIME = 0.45f;
	
	//private Color SCROLLBAR_COLOR = new Color(3f / 255f, 97f / 255f, 124f / 255f);
	private Color SCROLLBAR_COLOR = Color.white;
	
	private Vector2 m_viewSize = Vector2.zero;
	private Vector2 m_contentSize = Vector2.zero;
	private float m_scrollBarHeight = 0f;
	private float m_xOffset = 5f;
	
	private bool m_isShown = false;
	
	private float m_showTime = -1f, m_disapearTime = -1f;
	
	private void loadScrollBar()
	{
		
		//vertical scroll bar
		vertices.Add(new Vector3(m_viewSize.x - SCROLL_BAR_WIDTH/2f,0f,0f));
		vertices.Add(new Vector3(m_viewSize.x + SCROLL_BAR_WIDTH/2f,0f,0f));
		vertices.Add(new Vector3(m_viewSize.x + SCROLL_BAR_WIDTH/2f,-1f,0f));
		vertices.Add(new Vector3(m_viewSize.x - SCROLL_BAR_WIDTH/2f,-1f,0f));
		
		for (int i = 0; i < vertices.Count; ++i)
			vertColors.Add(SCROLLBAR_COLOR);
		
		//TODO - add horizontal scroll bar
		/*vertices.Add(new Vector3(m_viewSize.y - SCROLL_BAR_WIDTH/2f,0f,0f));
		vertices.Add(new Vector3(m_viewSize.y + SCROLL_BAR_WIDTH/2f,0f,0f));
		vertices.Add(new Vector3(m_viewSize.y + SCROLL_BAR_WIDTH/2f,-1f,0f));
		vertices.Add(new Vector3(m_viewSize.y - SCROLL_BAR_WIDTH/2f,-1f,0f));
		
		for (int i = 0; i < 4; ++i)
			vertColors.Add(SCROLLBAR_COLOR);
		*/
	}
	
	protected override void onInit()
	{
		loadScrollBar();
		base.onInit();
		//initially move the scrollbar outside the screen.
		transform.localPosition = new Vector2 (-Screen.width, -Screen.height);
	}
	
	protected override void onUpdate() 
	{
		if (m_showTime > 0)
		{
			m_showTime -= Time.deltaTime;
			if (m_showTime < 0)
				m_disapearTime = SCROLL_DISAPEAR_TIME;
		}
		
		if(m_disapearTime > 0) {
			m_disapearTime -= Time.deltaTime;
			
			if(m_disapearTime <= 0) {
				gameObject.SetActive(false);
				updateScrollBarAlphaColor(1f);
			}
			else {
				updateScrollBarAlphaColor(m_disapearTime);
			}
		}
		
		base.onUpdate();
	}
	
	public void setViewSize(Vector2 viewSize)
	{
		m_viewSize = viewSize;
		calculateBarHeight();
	}
	
	public void setContentSize(Vector2 contentSize)
	{
		m_contentSize = contentSize;
		calculateBarHeight();
	}
	
	public void setIsShown(bool isShown) {
		m_isShown = isShown;
	}
	
	public bool isShown() {
		return m_isShown;
	}
	
	
	public void calculateScrollBarPosition(Vector3 contentSizeOffset)
	{
		if( m_contentSize.y == 0 || !m_isShown || m_scrollBarHeight == 0f || m_scrollBarHeight >= m_viewSize.y)
			return;
		
		gameObject.SetActive(true);
		
		m_showTime = SCROLL_SHOWN_TIME;
		
		if(m_disapearTime > 0) {
			m_disapearTime = -1f;
			updateScrollBarAlphaColor(1f);
		}
		
		float procent = m_viewSize.y / m_contentSize.y;
		transform.localPosition = contentSizeOffset.y * procent * Vector3.down + m_xOffset * Vector3.left + 1.5f * Vector3.back;
		setBlendingColor(Color.white);
	}
	
	private void calculateBarHeight()
	{	
		float barHeight = m_contentSize.y == 0f? 0f :  m_viewSize.y * m_viewSize.y / m_contentSize.y;

		if(m_scrollBarHeight == barHeight) return;

		m_scrollBarHeight = barHeight;

		if(!m_isShown || m_scrollBarHeight >= m_viewSize.y || m_scrollBarHeight == 0f) {
			gameObject.SetActive(false);
			return;
		}
		
		gameObject.SetActive(true);
		vertices.Clear();
		vertices.Add(new Vector3(m_viewSize.x - SCROLL_BAR_WIDTH/2f, 0f, 0f));
		vertices.Add(new Vector3(m_viewSize.x + SCROLL_BAR_WIDTH/2f, 0f, 0f));
		vertices.Add(new Vector3(m_viewSize.x + SCROLL_BAR_WIDTH/2f, -m_scrollBarHeight, 0f));
		vertices.Add(new Vector3(m_viewSize.x - SCROLL_BAR_WIDTH/2f, -m_scrollBarHeight, 0f));
					
		updateMesh();
	}
	
	private void updateScrollBarAlphaColor(float alpha) 
	{
		SCROLLBAR_COLOR.a = alpha;
		setVertColors(SCROLLBAR_COLOR);
		setBlendingColor(Color.white);
	}
}
