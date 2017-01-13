using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class kCustomMeshObject : kPickerItem 
{
	public Triangulator.Type triangulatorType;
	public List<Vector3> 	vertices = new List<Vector3>();
	public List<Color> 		vertColors = new List<Color>();
	public List<Vector2> 	uvs;

	public Texture texture;

	protected override void onInit() {
		base.onInit();
		updateMesh();
	}
	
	protected override void onStart() {
		base.onStart();
		updateMesh();
	}
	
	protected override void onUpdate() {
		base.onUpdate();
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			updateMesh();
			m_material.mainTexture = texture;
		}
#endif
	}

	public void setDirty() {
		updateMesh();
	}
	
	protected override void updateMesh() {
		if (vertices.Count == 0) {
			clearObjectMesh();
			return;
		}
		
		objMesh.vertices = vertices.ToArray();
		objMesh.colors = vertColors.ToArray();
		objMesh.triangles = calculateMeshTriangles(0, vertices.Count,vertices.ToArray());
		
		//objMesh.UVs = null;
		if(uvs != null && uvs.Count > 0){
			objMesh.UVs = uvs.ToArray();
		}else if (objMesh.UVs == null || objMesh.UVs.Length != objMesh.vertices.Length) {
			objMesh.UVs = new Vector2[vertices.Count];
			for (int i = 0; i < objMesh.UVs.Length;i++) {
				objMesh.UVs[i] = new Vector2((i % 2 == 0) ? 1f : 0f, (i % 4 <= 1) ? 0f : 1f);
			}
		}
		Rect meshCorners = new Rect();
		if (objMesh.vertices.Length > 0)
			meshCorners = new Rect(objMesh.vertices[0].x, objMesh.vertices[0].y, objMesh.vertices[0].x, objMesh.vertices[0].y);
		meshCorners = updateCorners(objMesh, meshCorners, 0, objMesh.vertices.Length);
		
		objMesh.center.x = (meshCorners.x + meshCorners.width) / 2;
		objMesh.center.y = (meshCorners.y + meshCorners.height) / 2;
		objMesh.size.x = (objMesh.center.x - meshCorners.x) * 2;
		objMesh.size.y = (objMesh.center.y - meshCorners.y) * 2;

		if(m_material != null)
			m_material.mainTexture = texture;
		
		meshChanged = true;
	}
	
	public void setVertColors(Color c)
	{
		for (int i = 0; i < vertColors.Count; ++i) {
			vertColors[i] = c;
		}
		setDirty();
	}
	
	//TODO - remove and use iTween for alpha transition
	public void setAlpha(float alpha) {
		Color col = getBlendingColor();
		col.a = alpha;
		setBlendingColor(col);
		/*if (vertColors != null) {
			Color c = vertColors[vertexId];
			c.a = alpha;
			vertColors[vertexId] = c;
			setDirty();
		}*/
	}
	//TODO - remove and use iTween for alpha transition
	public void increaseAlpha(float alpha) {
		float a = getBlendingColor().a + alpha;
		a = a < 0.0f ? 0.0f : ( a > 1.0f ? 1.0f : a);
		setAlpha(a);
		/*if (vertColors != null) {
			Color col = vertColors[vertexId];
			col.a += alpha;
			col.a = col.a < 0 ? 0 : (col.a > 1 ? 1 : col.a);
			vertColors[vertexId] = col;
			setDirty();
		}*/
	}
	
	protected virtual int[] calculateMeshTriangles(int startVertex, int numberOfVertices, Vector3[] sVertices) {
		
		Vector2[] vertices2D = new Vector2[numberOfVertices];
		for (int i = startVertex; i < numberOfVertices + startVertex; i++)
			vertices2D[i - startVertex] = new Vector2(sVertices[i].x, sVertices[i].y);
		
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate(triangulatorType);
		
		if (startVertex != 0) {
			for (int i=0; i< indices.Length; i++)
				indices[i] += startVertex;
		}
		
		return indices;
	}
}

public class Triangulator
{
	public enum Type {
		Default = 0,
		Circular,
		Stripe,
	}

	private List<Vector2> m_points = new List<Vector2>();

	public Triangulator (Vector2[] points) {
		m_points = new List<Vector2>(points);
	}

	public int[] Triangulate(Type type = Type.Default) {
		if (type == Type.Circular) {
			int[] triangles = new int[3 * (m_points.Count - 1)];
			for (int i = 1; i < m_points.Count - 1; ++i) {
				triangles[3 * i - 3] = 0;
				triangles[3 * i - 2] = i;
				triangles[3 * i - 1] = i + 1;
			}
			return triangles;
		}

		if (type == Type.Stripe) {
			int[] triangles = new int[(m_points.Count - 2) * 3];
			for (int i = 0; i < m_points.Count - 2; i++) {
				if ( i % 2 == 0 ){
					triangles[i*3 ] = i;
					triangles[i*3 + 1] = i + 1;
					triangles[i*3 + 2] = i + 2 ;
				} else {
					triangles[i*3 ] = i;
					triangles[i*3 + 1] = i + 2;
					triangles[i*3 + 2] = i + 1 ;
				}
			}

			return triangles;
		}

		List<int> indices = new List<int>();

		int n = m_points.Count;
		if (n < 3)
			return indices.ToArray();
 		
		int[] V = new int[n];
		if (Area() > 0) {
			for (int v = 0; v < n; v++)
				V[v] = v;
		}
		else {
			for (int v = 0; v < n; v++)
				V[v] = (n - 1) - v;
		}
 		
		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2; ) {
			if ((count--) <= 0)
				return indices.ToArray();
 
			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;
 
			if (Snip(u, v, w, nv, V)) {
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}
 		
		indices.Reverse();
		return indices.ToArray();
	}
	
	private float Area() {
		int n = m_points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++) {
			Vector2 pval = m_points[p];
			Vector2 qval = m_points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}
	
	private bool Snip(int u, int v, int w, int n, int[] V) {
		int p;
		Vector2 A = m_points[V[u]];
		Vector2 B = m_points[V[v]];
		Vector2 C = m_points[V[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++) {
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = m_points[V[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}
	
	private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}
}
