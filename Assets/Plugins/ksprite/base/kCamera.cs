using UnityEngine;
using System.Collections;

public class kCamera : kBehaviourScript
{
	const float SPHERE_CAST_RADIUS = 10f;
	public Camera camera;

	protected override void onInit(){
		base.onInit();

		if(camera == null){
			camera = GetComponent<Camera>();
			if(camera == null)
				camera = gameObject.AddComponent<Camera>();
		}
	}

	public virtual Vector2 RayCast(Vector3 position,out RaycastHit hit,float maxDistance){
		Ray ray = camera.ScreenPointToRay(position);

		Physics.SphereCast (ray.origin,SPHERE_CAST_RADIUS,ray.direction, out hit,maxDistance,camera.cullingMask);

		return position;
	}

	public virtual Vector3 ToWorldPoint(Vector3 position,RaycastHit hit){
		return ToWorldPoint(position);
	}

	public virtual Vector3 ToWorldPoint(Vector3 position){
		return camera.ScreenToWorldPoint(position);
	}
}
