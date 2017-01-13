using UnityEngine;
using System.Collections;

public struct IntRect{
	public int x,y;
	public int width,height;
	public IntRect(int x,int y,int width,int height)
	{
		this.x = x;
		this.y = y;
		this.height = height;
		this.width = width;
	}
	
	public Vector2 getCenter(ref Vector2 center){
		center.x = x + width/2;
		center.y = y + height/2;
		return center;
	}
	
	public Vector2 getSize(ref Vector2 size){
		size.x = width;
		size.y = height;
		return size;
	}

	public void set(int x,int y,int width,int height){
		this.x = x;
		this.y = y;
		this.height = height;
		this.width = width;
	}

	public void set(float x,float y,float width,float height){
		this.x = (int)x;
		this.y = (int)y;
		this.height = (int)height;
		this.width = (int)width;
	}

	public string ToString(){
		return "["+x+","+y+"]["+width+","+height+"]";
	}
}

public abstract class RectUtil{
	public static IntRect RECT_NO_CLIP = new IntRect(0,0,-1,-1);
	public static IntRect RECT_CLIP_ZERO = new IntRect(0,0,0,0);
	
	public static bool includeUnityAxes(Rect big,Rect small){
		
		return 	big.x <= small.x && big.y >= small.y
			&&	big.x + big.width	>= small.x + small.width
			&&	big.y - big.height 	<= small.y - small.height; 
	}
	
	public static bool include(Rect big,Rect small){
		return 	big.x <= small.x && big.y <= small.y	
			&&	big.x + big.width	>= small.x + small.width
			&&	big.y + big.height 	>= small.y + small.height; 
	}
	
	public static bool isRectContainingPoint(Rect r, Vector2 p)
	{
		if(r.x <= p.x && r.x + r.width >= p.x && r.y <= p.y && r.y + r.height >= p.y)
			return true;
		
		return false;
	}
	
	public static Rect intersect(Rect r1,Rect r2)
	{
		Rect intersection = new Rect();
		
		if(r1.width == 0 || r1.height == 0 
		|| r2.width == 0 || r2.height == 0)
			return intersection;
		if(r1.x < r2.x && r1.x + r1.width < r2.x) return intersection;
		if(r2.x < r1.x && r2.x + r2.width < r1.x) return intersection;
		
		if(r1.y < r2.y && r1.y + r1.height < r2.y) return intersection;
		if(r2.y < r1.y && r2.y + r2.height < r1.y) return intersection;
			
		intersection.x 		= Mathf.Max(r1.x,r2.x);
		intersection.width  = Mathf.Abs(intersection.x - Mathf.Min(r1.x + r1.width,r2.x + r2.width));
		
		intersection.y 		= Mathf.Max(r1.y,r2.y);
		intersection.height = Mathf.Abs(intersection.y - Mathf.Min(r1.y + r1.height,r2.y + r2.height));
	
		return intersection;
	}
	
	public static Rect union(Rect r1,Rect r2){
		Rect union = new Rect();
		
		r1.x = Mathf.Min(r1.x, r1.x + r1.width); r1.width = Mathf.Abs(r1.width);
		r1.y = Mathf.Min(r1.y,r1.y + r1.height); r1.height= Mathf.Abs(r1.height);
		
		r2.x = Mathf.Min(r2.x, r2.x + r2.width); r2.width = Mathf.Abs(r2.width);
		r2.y = Mathf.Min(r2.y, r2.y + r2.height);r2.height= Mathf.Abs(r2.height);
		
		union.x = Mathf.Min(r1.x,r2.x);
		union.y = Mathf.Min(r1.y,r2.y);
		union.width = Mathf.Max(r1.x + r1.width,r2.x + r2.width) - union.x;
		union.height = Mathf.Max(r1.y + r1.height,r2.y + r2.height) - union.y;
		
		return union;
	}
	
	public static Rect newRectangle(float x,float y,float w,float h){
		return new Rect(Mathf.Min(x,x + w),Mathf.Min(y,y+h),Mathf.Abs(w),Mathf.Abs(h));
	}
	
	public static IntRect newRectangle(int x,int y,int w,int h){
		return new IntRect(Mathf.Min(x,x + w),Mathf.Min(y,y+h),Mathf.Abs(w),Mathf.Abs(h));
	}
	
	public static void intersect(ref IntRect r1,ref IntRect r2,ref IntRect intersection)
	{
		intersection.x = 0; intersection.y = 0; intersection.width = 0; intersection.height = 0;
		
		if(r1.width == 0 || r1.height == 0 
		|| r2.width == 0 || r2.height == 0)
			return;
		
		if(r1.x < r2.x && r1.x + r1.width < r2.x) return;
		if(r2.x < r1.x && r2.x + r2.width < r1.x) return;
		
		if(r1.y < r2.y && r1.y + r1.height < r2.y) return;
		if(r2.y < r1.y && r2.y + r2.height < r1.y) return;
			
		intersection.x 		= Mathf.Max(r1.x,r2.x);
		intersection.width  = Mathf.Abs(intersection.x - Mathf.Min(r1.x + r1.width,r2.x + r2.width));
		
		intersection.y 		= Mathf.Max(r1.y,r2.y);
		intersection.height = Mathf.Abs(intersection.y - Mathf.Min(r1.y + r1.height,r2.y + r2.height));
	}
	
	public static void intersect(ref IntRect r1,ref Vector2 r2Center,ref Vector2 r2Size,ref IntRect intersection)
	{
		intersection.x = 0; intersection.y = 0; intersection.width = 0; intersection.height = 0;
		int r2X = (int)(r2Center.x - r2Size.x/2), r2Y = (int)(r2Center.y - r2Size.y/2);
		
		if(r1.width == 0 || r1.height == 0 
		|| r2Size.x == 0 || r2Size.y == 0)
			return;
		
		if(r1.x < r2X && r1.x + r1.width < r2X) return;
		if(r2X < r1.x && r2X + r2Size.x < r1.x) return;
		
		if(r1.y < r2Y && r1.y + r1.height < r2Y) return;
		if(r2Y < r1.y && r2Y + r2Size.y < r1.y) return;
			
		intersection.x 		= r1.x > r2X ? r1.x : r2X;
		int minX = (int)(r1.x + r1.width < r2X + r2Size.x ? r1.x + r1.width : r2X + r2Size.x);
		intersection.width  = (int)(intersection.x - minX);
		intersection.width  = intersection.width >= 0 ? intersection.width : intersection.width * -1;
		
		intersection.y 		= r1.y > r2Y ? r1.y : r2Y;
		int minY = (int)(r1.y + r1.height < r2Y + r2Size.y ? r1.y + r1.height : r2Y + r2Size.y);
		intersection.height = (int)(intersection.y - minY);
		intersection.height = intersection.height >= 0 ? intersection.height : intersection.height * -1;
	}
}
