using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum kProgressBarType
{
	Mesh = 0,
	Sprite,
	CircularMesh,
}

public class kProgressBar : kObject
{
	const float EPSILON = 0.001f;

	public kTextField m_text;

	public kProgressBarType m_type = kProgressBarType.Mesh;

	public kSpriteObject m_progressFrame;

	public float m_barWidth = 27f;
	public int m_roundEndPoints = 8;
	public Color m_startColor = Color.white;
	public Color m_endColor = Color.white;
	public Vector3 m_startPos = 100 * Vector3.left;
	public Vector3 m_endPos = 100 * Vector3.right;

	public float m_barRadius = 100;
	public float m_startAngle = 1.23f * Mathf.PI;
	public float m_endAngle = -0.23f * Mathf.PI;
	public float m_angleStep = 0.15f;

	private float m_percent = -1f;

	private kCustomMeshObject m_progressMesh;
	private kView m_progressClip;

	public kCustomMeshObject progressMesh {
		get {
			if (m_progressMesh == null)
				createProgressMesh();
			return m_progressMesh;
		}
	}

	protected override void onInit()
	{
		base.onInit();
		if (!Application.isPlaying)
			return;
		createProgressMesh();
		createProgressClip();
	}

	private void createProgressMesh()
	{
		if (m_type == kProgressBarType.Mesh || m_type == kProgressBarType.CircularMesh) {
			if (m_progressMesh == null) {
				GameObject obj = new GameObject("progressMesh");
				obj.transform.parent = transform;
				obj.transform.localScale = Vector3.one;
				obj.transform.localPosition = Vector3.zero;
				m_progressMesh = obj.AddComponent<kCustomMeshObject>();
				m_progressMesh.gameObject.layer = gameObject.layer;
			}
		}
	}

	private void createProgressClip()
	{
		if (m_type == kProgressBarType.Sprite) {
			if (m_progressClip == null && m_progressFrame != null) {
				Rect frameBounds = m_progressFrame.getBounds();
				GameObject objClip = new GameObject("progressClip");
				objClip.transform.parent = transform;
				objClip.transform.localPosition = new Vector3(frameBounds.x, frameBounds.y + frameBounds.height, 0);
				objClip.transform.localEulerAngles = Vector3.zero;
				objClip.transform.localScale = Vector3.one;
				m_progressClip = objClip.AddComponent<kView>();
				m_progressClip.viewSize = new Vector2(frameBounds.width, frameBounds.height);
				m_progressFrame.transform.parent = m_progressClip.transform;
			}
		}
	}

	public void setProgress(float percent, bool fadedOutside = false, bool forceRefresh = false)
	{
		if (float.IsNaN(percent)) {
			Debug.LogError("Wrong progress bar percent!!!");
			return;
		}
		if (m_progressMesh == null && m_progressClip == null) {
			//Debug.LogError("setProgress before init!!!");
			return;
		}
		if (!forceRefresh && (Mathf.Abs(m_percent - percent) < EPSILON))
			return;
		
		m_percent = Mathf.Clamp(percent, EPSILON, 100);

		int endPoints = Mathf.Clamp(m_roundEndPoints, 1, 100);

		switch (m_type) {
			case kProgressBarType.Mesh: {
				progressMesh.vertices = new List<Vector3>();
				progressMesh.vertColors = new List<Color>();
				float barAngle = Mathf.Atan2((m_endPos - m_startPos).y, (m_endPos - m_startPos).x);
				for (float pAngle = barAngle + Mathf.PI * 3 / 2; pAngle > barAngle + Mathf.PI / 2 - EPSILON; pAngle -= Mathf.PI / endPoints) {
					progressMesh.vertices.Add(m_startPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(pAngle), 0.5f * m_barWidth * Mathf.Sin(pAngle), 0));
					progressMesh.vertColors.Add(m_startColor);
				}
				if (m_percent > 100 - EPSILON) {
					for (float pAngle = barAngle + Mathf.PI / 2; pAngle > barAngle - Mathf.PI / 2 - EPSILON; pAngle -= Mathf.PI / endPoints) {
						progressMesh.vertices.Add(m_endPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(pAngle), 0.5f * m_barWidth * Mathf.Sin(pAngle), 0));
						progressMesh.vertColors.Add(m_endColor);
					}
				} else {
					Vector3 endPos = m_startPos + (m_endPos - m_startPos) * m_percent / 100;
					progressMesh.vertices.Add(endPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(barAngle + Mathf.PI / 2), 0.5f * m_barWidth * Mathf.Sin(barAngle + Mathf.PI / 2), 0));
					progressMesh.vertices.Add(endPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(barAngle - Mathf.PI / 2), 0.5f * m_barWidth * Mathf.Sin(barAngle - Mathf.PI / 2), 0));
					progressMesh.vertColors.Add(getColor(m_percent / 100));
					progressMesh.vertColors.Add(getColor(m_percent / 100));
				}
				progressMesh.setDirty();
			} break;
			case kProgressBarType.CircularMesh: {
				progressMesh.vertices = new List<Vector3>();
				progressMesh.vertColors = new List<Color>();
				if (m_percent > EPSILON) {
					Vector3 pivotPos = new Vector3(m_barRadius * Mathf.Cos(m_startAngle), m_barRadius * Mathf.Sin(m_startAngle), 0);
					for (float pAngle = m_startAngle + Mathf.PI * (endPoints - 1) / endPoints; pAngle > m_startAngle + EPSILON; pAngle -= Mathf.PI / endPoints) {
						progressMesh.vertices.Add(pivotPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(pAngle), 0.5f * m_barWidth * Mathf.Sin(pAngle), 0));
						progressMesh.vertColors.Add(m_startColor);
					}
				}
				float angle = m_startAngle;
				float radius = m_barRadius + m_barWidth / 2;
				bool done = false;
				while (!done) {
					if (angle < m_startAngle + (m_endAngle - m_startAngle) * m_percent / 100) {
						angle = m_startAngle + (m_endAngle - m_startAngle) * m_percent / 100;
						done = true;
					}
					progressMesh.vertices.Add(new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0));
					progressMesh.vertColors.Add(getColor((angle - m_startAngle) / (m_endAngle - m_startAngle)));
					if (!done) {
						angle -= m_angleStep;
					}
				}
				if (m_percent > 100 - EPSILON) {
					Vector3 pivotPos = new Vector3(m_barRadius * Mathf.Cos(m_endAngle), m_barRadius * Mathf.Sin(m_endAngle), 0);
					for (float pAngle = m_endAngle - Mathf.PI / endPoints; pAngle > m_endAngle - Mathf.PI + EPSILON; pAngle -= Mathf.PI / endPoints) {
						progressMesh.vertices.Add(pivotPos + new Vector3(0.5f * m_barWidth * Mathf.Cos(pAngle), 0.5f * m_barWidth * Mathf.Sin(pAngle), 0));
						progressMesh.vertColors.Add(m_endColor);
					}
				}
				radius = m_barRadius - m_barWidth / 2;
				done = false;
				while (!done) {
					if (angle > m_startAngle) {
						angle = m_startAngle;
						done = true;
					}
					m_progressMesh.vertices.Add(new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0));
					m_progressMesh.vertColors.Add(getColor((angle - m_startAngle) / (m_endAngle - m_startAngle)));
					angle += m_angleStep;
				}

				if (fadedOutside) {
					for (int i = 0; i < progressMesh.vertColors.Count / 2; i++) {
						Color newColor = new Color(progressMesh.vertColors [i].r,progressMesh.vertColors [i].g,progressMesh.vertColors [i].b,0.5f);
						progressMesh.vertColors [i] = newColor;
					}
				}
				progressMesh.setDirty();
			} break;
			case kProgressBarType.Sprite: {
				if (m_progressClip != null) {
					m_progressClip.viewSize.x = m_progressFrame.getBounds().width * m_percent / 100;
					m_progressClip.updateClip();
				}
			} break;
		}
	}

	public float getProgress()
	{
		return m_percent;
	}

	private Color getColor(float percent)
	{
		return new Color(m_startColor.r + (m_endColor.r - m_startColor.r) * percent,
		                 m_startColor.g + (m_endColor.g - m_startColor.g) * percent,
		                 m_startColor.b + (m_endColor.b - m_startColor.b) * percent,
		                 m_startColor.a + (m_endColor.a - m_startColor.a) * percent);
	}
}
