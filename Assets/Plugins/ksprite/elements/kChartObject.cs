#define DRAW_SPLITER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class kChartObject : kCustomMeshObject
{
	public enum ChartType {
		LINE = 0,
		PROGRESS,
		BAR,
		HISTOGRAM,
		PIE,
	}
	
	[HideInInspector]
	public ChartType m_chartType = ChartType.BAR;
	
	[HideInInspector]
	public int m_numberOfBars = 1;
	[HideInInspector]
	public float m_barWidth = 568f;
	[HideInInspector]
	public float m_barHeight = 236f;
	[HideInInspector]
	public float m_barSpacing = 6f;
	[HideInInspector]
	public float m_chartWidth = 568f;
	[HideInInspector]
	public int m_numOfDivsForLINE = 13;
	[HideInInspector]
	public float m_ProgressPercent = 0.6f;
	[HideInInspector]
	public float m_ProgressBarWidth = 30f;
	
	protected static Color dividerColor = new Color(1f, 1f, 1f, 0.1f);
	protected static Color dividerFadeColor = new Color(1f, 1f, 1f, 0f);
	protected static Color defaultUpColor = new Color(78/255f, 211/255f, 23/255f);
	protected static Color defaultDownColor = new Color(101/255f, 215/255f, 245/255f);
	protected static Color defaultBorderColor = new Color(99/255f, 127/255f, 69/255f);
	protected static Color defaultShineColor = new Color(157/255f, 217/255f, 0/255f);
	protected static Color shineColor = new Color(85/255f, 198/255f, 255/255f);
	protected static Color diffColor = new Color(defaultUpColor.r - defaultDownColor.r, defaultUpColor.g - defaultDownColor.g, defaultUpColor.b - defaultDownColor.b);

	public void setNumberOfBars(int numberOfBars) {
		m_numberOfBars = numberOfBars;
	}

	public virtual void setWidth(float width) {
		m_chartWidth = width;
	}

	public void setBarHeight(float height) {
		m_barHeight = height;
	}
	
	public void drawChartDividers() {
		
		m_barWidth = (m_chartWidth - (m_numberOfBars+1)*m_barSpacing) / m_numberOfBars;
		
#if DRAW_SPLITER
		switch(m_chartType) {
		case ChartType.LINE:
		{
			float divWidth = 3f;//m_barSpacing;
			float divSpacing = (m_chartWidth - (float)m_numOfDivsForLINE*divWidth) / ((float)m_numOfDivsForLINE-1f);
			for(int i= 0 ; i < m_numOfDivsForLINE ; i++) {
				
				Vector3 divPoint1 = new Vector3((float)i*(divWidth+divSpacing), m_barHeight, -0.5f);
				Vector3 divPoint2 = new Vector3((float)i*(divWidth+divSpacing) + divWidth, m_barHeight, -0.5f);
				Vector3 divPoint3 = new Vector3((float)i*(divWidth+divSpacing) + divWidth, 0, -0.5f);
				Vector3 divPoint4 = new Vector3((float)i*(divWidth+divSpacing), 0, -0.5f);
				
				vertices.Add(divPoint1);
				vertColors.Add(dividerColor);
				
				vertices.Add(divPoint2);
				vertColors.Add(dividerColor);
				
				vertices.Add(divPoint3);
				vertColors.Add(dividerColor);
				
				vertices.Add(divPoint4);
				vertColors.Add(dividerColor);
			}
		}
			break;
		case ChartType.BAR:
		case ChartType.PROGRESS:
		{
			m_ProgressPercent = m_ProgressPercent > 1.0f? 1.0f : m_ProgressPercent;
			int numberOfBarsToDraw = (m_chartType == ChartType.BAR)? m_numberOfBars : (int)(m_ProgressPercent*(float)m_numberOfBars);
			for (int i = 0; i<= numberOfBarsToDraw; i++) {
				
				Vector3 splitPoint1 = new Vector3((float)i*(m_barWidth+m_barSpacing), m_barHeight, -1f);
				Vector3 splitPoint2 = new Vector3((float)i*(m_barWidth+m_barSpacing) + m_barSpacing, m_barHeight, -1f);
				Vector3 splitPoint3 = new Vector3((float)i*(m_barWidth+m_barSpacing) + m_barSpacing, 0, -1f);
				Vector3 splitPoint4 = new Vector3((float)i*(m_barWidth+m_barSpacing), 0, -1f);
				
				vertices.Add(splitPoint1);
				vertColors.Add(dividerColor);
				
				vertices.Add(splitPoint2);
				vertColors.Add(dividerColor);
				
				vertices.Add(splitPoint3);
				vertColors.Add(dividerColor);
				
				vertices.Add(splitPoint4);
				vertColors.Add(dividerColor);
			}
		}
			break;
		default:
			break;
		}
#else
		m_numOfDivsForLINE = 0
#endif	
	}
	
	
	public void drawChartBarBg()
	{
		m_barWidth = (m_chartWidth - (m_numberOfBars-1)*m_barSpacing) / m_numberOfBars;
		
		switch(m_chartType) {
		case ChartType.LINE:
		{
			Debug.LogError("Not implemented!");
		}
			break;
		case ChartType.BAR:
		{
			/*for (int i = 0; i< m_numberOfBars; i++) {
				
				Vector3 bgPoint1 = new Vector3((float)i*(m_barSpacing + m_barWidth), m_barHeight, -1f);
				Vector3 bgPoint2 = new Vector3((float)i*(m_barSpacing + m_barWidth) + m_barWidth, m_barHeight, -1f);
				Vector3 bgPoint3 = new Vector3((float)i*(m_barSpacing + m_barWidth) + m_barWidth, 0, -1f);
				Vector3 bgPoint4 = new Vector3((float)i*(m_barSpacing + m_barWidth), 0, -1f);
				
				vertices.Add(bgPoint1);
				vertColors.Add(dividerColor);
				
				vertices.Add(bgPoint2);
				vertColors.Add(dividerColor);
				
				vertices.Add(bgPoint3);
				vertColors.Add(dividerColor);
				
				vertices.Add(bgPoint4);
				vertColors.Add(dividerColor);
			}*/
		}
			break;
		case ChartType.PROGRESS:
		{
			Vector3 bgPoint1 = new Vector3(0f, m_barHeight, -0.5f);
			Vector3 bgPoint2 = new Vector3(m_chartWidth, m_barHeight, -0.5f);
			Vector3 bgPoint3 = new Vector3(m_chartWidth, 0, -0.5f);
			Vector3 bgPoint4 = new Vector3(0f, 0, -0.5f);
			
			vertices.Add(bgPoint1);
			vertColors.Add(dividerColor);
			
			vertices.Add(bgPoint2);
			vertColors.Add(dividerColor);
			
			vertices.Add(bgPoint3);
			vertColors.Add(dividerColor);
			
			vertices.Add(bgPoint4);
			vertColors.Add(dividerColor);
		}
			break;
		default:
			Debug.LogError("Not implemented!");
			break;
		}
	}
	
	
	public void loadChartObject()
	{
		Debug.Log("loadChartObject()");
		
		if (vertices.Count > 0 )
			return;
		
		
		drawChartDividers();//for drawing splitters (a.k.a dividers)
		
		
		switch (m_chartType)
		{
		case ChartType.LINE:
			
			int resolution = 1;
			for (int x = 0; x < ((int)m_barWidth)/resolution ; x++) {
				float per1 = (float)x*(float)resolution/m_barWidth;
				float y = Mathf.Cos( -3f*Mathf.PI + 2*(float)x/(float)m_barWidth*3f*Mathf.PI)*m_barHeight/2f*per1 + m_barHeight/2;
				
				float percent = y/m_barHeight;
				Color color = new Color(defaultDownColor.r + percent*diffColor.r, defaultDownColor.g + percent*diffColor.g, defaultDownColor.b + percent*diffColor.b);
				vertColors.Add (color);
				
				Vector3 point = new Vector3((float)x, y, -1f);
				vertices.Add(point);
			}
			
			Vector3 base1 = new Vector3(m_barWidth, 0, -1f);
			vertColors.Add (defaultDownColor);
			vertices.Add(base1);
			
			Vector3 base2 = new Vector3(0, 0, -1f);
			vertColors.Add (defaultDownColor);
			vertices.Add(base2);
			
			break;
		case ChartType.PROGRESS:
		{
			Vector3 point1 = new Vector3(0, m_barHeight, -1f);
			Vector3 point2 = new Vector3(m_ProgressPercent*(float)m_chartWidth, m_barHeight, -1f);
			Vector3 point3 = new Vector3(m_ProgressPercent*(float)m_chartWidth, 0f, -1f);
			Vector3 point4 = new Vector3(0, 0f, -1f);
			
			vertices.Add(point1);
			vertColors.Add(defaultUpColor);
				
			vertices.Add(point2);
			vertColors.Add(defaultUpColor);
				
			vertices.Add(point3);
			vertColors.Add(defaultDownColor);
				
			vertices.Add(point4);
			vertColors.Add(defaultDownColor);
			
		}
			break;
		case ChartType.BAR:
			
			m_ProgressPercent = m_ProgressPercent > 1.0f? 1.0f : m_ProgressPercent;
			int numberOfBarsToDraw = (m_chartType == ChartType.BAR)? m_numberOfBars : (int)(m_ProgressPercent*(float)m_numberOfBars);

			for (int i = 0; i< numberOfBarsToDraw; i++) {
				float y = (m_chartType == ChartType.PROGRESS)? m_barHeight : m_barHeight*(float)(i+1)/(float)m_numberOfBars;
				
				Vector3 point1 = new Vector3((float)i*(m_barWidth+m_barSpacing), y, -1f);
				Vector3 point2 = new Vector3((float)i*(m_barWidth+m_barSpacing) + m_barWidth, y, -1f);
				Vector3 point3 = new Vector3((float)i*(m_barWidth+m_barSpacing) + m_barWidth, 0f, -1f);
				Vector3 point4 = new Vector3((float)i*(m_barWidth+m_barSpacing), 0f, -1f);
				
				float percent = y/m_barHeight;
				Color color = new Color(defaultDownColor.r + percent*diffColor.r, defaultDownColor.g + percent*diffColor.g, defaultDownColor.b + percent*diffColor.b);
				
				vertices.Add(point1);
				vertColors.Add(color);
				
				vertices.Add(point2);
				vertColors.Add(color);
				
				vertices.Add(point3);
				vertColors.Add(defaultDownColor);
				
				vertices.Add(point4);
				vertColors.Add(defaultDownColor);
			}
			break;
		case ChartType.HISTOGRAM:
			Debug.LogError("Not implemented!");
			break;
		case ChartType.PIE:
			Debug.LogError("Not implemented!");
			break;
			
		}
		
		updateMesh();
	}
	
	protected void addBarBackground(int idx0, int idx1, float y0, float y1, float y2, Color barColor1, Color barColor2)
	{
		float x0 = idx0 * (m_barWidth + m_barSpacing);
		float x1 = idx1 * (m_barWidth + m_barSpacing) - m_barSpacing;

		vertices.Add(new Vector3(x0, y1, -1f));
		vertColors.Add(barColor1);
		vertices.Add(new Vector3(x1, y1, -1f));
		vertColors.Add(barColor1);
		vertices.Add(new Vector3(x1, y0, -1f));
		vertColors.Add(barColor1);
		vertices.Add(new Vector3(x0, y0, -1f));
		vertColors.Add(barColor1);

		vertices.Add(new Vector3(x0, y2, -1f));
		vertColors.Add(barColor2);
		vertices.Add(new Vector3(x1, y2, -1f));
		vertColors.Add(barColor2);
		vertices.Add(new Vector3(x1, y1, -1f));
		vertColors.Add(barColor1);
		vertices.Add(new Vector3(x0, y1, -1f));
		vertColors.Add(barColor1);
	}
	
	protected void updateChartWithValue(float x, float y0, float y, Color barColor, bool drawBarBorderColor = false, bool drawBarShine = false)
	{
		switch(m_chartType) {
		case ChartType.LINE:
		{
			/*if(vertices.Count>m_numOfDivsForLINE*4) {//remove bases - only if they are added
				vertices.RemoveRange(vertices.Count-2, 2);
				vertColors.RemoveRange(vertColors.Count-2, 2);
			}*/
			
			float percent = y/m_barHeight;
			Color color = new Color(defaultDownColor.r + percent*diffColor.r, defaultDownColor.g + percent*diffColor.g, defaultDownColor.b + percent*diffColor.b);
			vertColors.Add (color);
					
			Vector3 point = new Vector3(x, y, -1f);
			vertices.Add(point);
			
			
			/*Vector3 base1 = new Vector3(x, 0f, -1f);
			vertColors.Add (downColor);
			vertices.Add(base1);
				
			Vector3 base2 = new Vector3(0f, 0f, -1f);
			vertColors.Add (downColor);
			vertices.Add(base2);*/
		}
			break;
		case ChartType.PROGRESS:
		{
			m_ProgressPercent = m_ProgressPercent > 1f? 1f : m_ProgressPercent;
			Vector3 point1 = new Vector3(0, m_barHeight, -1f);
			Vector3 point2 = new Vector3(m_ProgressPercent*(float)m_chartWidth, m_barHeight, -1f);
			Vector3 point3 = new Vector3(m_ProgressPercent*(float)m_chartWidth, 0f, -1f);
			Vector3 point4 = new Vector3(0, 0f, -1f);
			
			vertices.Add(point1);
			vertColors.Add(defaultUpColor);
				
			vertices.Add(point2);
			vertColors.Add(defaultUpColor);
				
			vertices.Add(point3);
			vertColors.Add(defaultDownColor);
				
			vertices.Add(point4);
			vertColors.Add(defaultDownColor);
		}
			break;
		case ChartType.BAR:
		{
			const float PIXELS_BAR_BORDER = 0;//1.5f;
			Vector3 point1, point2, point3, point4;

			if(drawBarBorderColor)
			{
				point1 = new Vector3(x*(m_barWidth+m_barSpacing), y, -1f);
				point2 = new Vector3(x*(m_barWidth+m_barSpacing) + m_barWidth, y, -1f);
				point3 = new Vector3(x*(m_barWidth+m_barSpacing) + m_barWidth, y0, -1f);
				point4 = new Vector3(x*(m_barWidth+m_barSpacing), y0, -1f);

				vertices.Add(point1);
				vertColors.Add(defaultBorderColor);
				vertices.Add(point2);
				vertColors.Add(defaultBorderColor);
				vertices.Add(point3);
				vertColors.Add(defaultBorderColor);
				vertices.Add(point4);
				vertColors.Add(defaultBorderColor);
			}


			point1 = new Vector3(x*(m_barWidth+m_barSpacing) + PIXELS_BAR_BORDER, y - PIXELS_BAR_BORDER, -1f);
			point2 = new Vector3(x*(m_barWidth+m_barSpacing) - PIXELS_BAR_BORDER + m_barWidth, y - PIXELS_BAR_BORDER, -1f);
			point3 = new Vector3(x*(m_barWidth+m_barSpacing) - PIXELS_BAR_BORDER + m_barWidth, y0 + PIXELS_BAR_BORDER, -1f);
			point4 = new Vector3(x*(m_barWidth+m_barSpacing) + PIXELS_BAR_BORDER, y0 + PIXELS_BAR_BORDER, -1f);
			
			float percent = y/m_barHeight;
			Color defaultColor = new Color(defaultDownColor.r + percent*diffColor.r, defaultDownColor.g + percent*diffColor.g, defaultDownColor.b + percent*diffColor.b);
			
			vertices.Add(point1);
			vertColors.Add(barColor == Color.clear? defaultColor : barColor);
			
			vertices.Add(point2);
			vertColors.Add(barColor == Color.clear? defaultColor : barColor);
			
			vertices.Add(point3);
			vertColors.Add(barColor == Color.clear? defaultDownColor : barColor);
			
			vertices.Add(point4);
			vertColors.Add(barColor == Color.clear? defaultDownColor : barColor);

			if (drawBarShine && y-y0 > 5*PIXELS_BAR_BORDER)
			{
				point1 = new Vector3(x*(m_barWidth+m_barSpacing) + PIXELS_BAR_BORDER, y - PIXELS_BAR_BORDER, -1f);
				point2 = new Vector3(x*(m_barWidth+m_barSpacing) - PIXELS_BAR_BORDER + m_barWidth, y - PIXELS_BAR_BORDER, -1f);
				point3 = new Vector3(x*(m_barWidth+m_barSpacing) - PIXELS_BAR_BORDER + m_barWidth, y - 3*PIXELS_BAR_BORDER, -1f);
				point4 = new Vector3(x*(m_barWidth+m_barSpacing) + PIXELS_BAR_BORDER, y - 3*PIXELS_BAR_BORDER, -1f);

				vertices.Add(point1);
				vertColors.Add(barColor == Color.clear? defaultShineColor : shineColor);
				vertices.Add(point2);
				vertColors.Add(barColor == Color.clear? defaultShineColor : shineColor);
				vertices.Add(point3);
				vertColors.Add(barColor == Color.clear? defaultShineColor : shineColor);
				vertices.Add(point4);
				vertColors.Add(barColor == Color.clear? defaultShineColor : shineColor);
			}

		}
			break;
		}
	}
	
	protected void updateChartWithValue(float x, float y)
	{
		updateChartWithValue(x, 0, y, Color.clear);
	}
	
	protected override void onInit()
	{
		//if(!Application.isPlaying)
		//	return;

		loadChartObject();
		base.onInit();
	}
	
	
	// Update is called once per frame
	protected override void onUpdate()
	{
		base.onUpdate();
	}
	
	
	public virtual void resetChart() {
		vertices.Clear();
		vertColors.Clear();
	}	
	
	
	
	protected override int[] calculateMeshTriangles(int startVertex, int numberOfVertices,Vector3[] sVertices) {
		int[] indices;
		switch (m_chartType) {
		case ChartType.LINE:
			int[] lineIndices = base.calculateMeshTriangles(m_numOfDivsForLINE*4, vertices.Count - m_numOfDivsForLINE*4,sVertices);
			int[] barIndices = getIndicesForBAR(/*vertices.Count - m_numOfDivsForLINE*4*/0, m_numOfDivsForLINE*4);
			
			
			indices = new int[lineIndices.Length + barIndices.Length];
			for (int i=0; i< barIndices.Length ; i++)
				indices[i] = barIndices[i];
			for (int i=barIndices.Length; i<barIndices.Length+lineIndices.Length; i++)
				indices[i] = lineIndices[i-barIndices.Length];
			break;
		case ChartType.PROGRESS:
		case ChartType.BAR:
			indices = getIndicesForBAR(0,sVertices.Length);
			break;
		default:
			indices = base.calculateMeshTriangles(0, sVertices.Length,sVertices);
			break;
		}
		
		return indices;
	}
	
	
	protected int[] getIndicesForBAR(int startVertex, int numberOfVertices)
	{
		if (numberOfVertices %4 != 0)
			Debug.LogError("Number of vertices should be multiply of 4! Number of Vertices = " + m_numberOfBars);
		
		int[] indices = new int[numberOfVertices + numberOfVertices/2];
		int k = 0;
		for (int i=startVertex; i<numberOfVertices+startVertex; i++,k++) {
			
			indices[k] = i;
			if ((i+1)%4 == 2) {
				indices[++k] = i+2;
				indices[++k] = i;
			}
		}
		
		return indices;
	}
}

