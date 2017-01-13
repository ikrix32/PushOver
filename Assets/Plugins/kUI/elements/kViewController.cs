using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/** Singleton class used to implement unique view controllers */
[ExecuteInEditMode]
public abstract class kViewController: kBehaviourScript {
	[SerializeField]
	public List<kView> views = new List<kView>();
	
	public virtual void viewWillShow(kView view){}
	public virtual void viewShown(kView view){}
	public virtual void viewWillHide(kView view){}
	public virtual void viewHidden(kView view){}
	
	//todo: onInit...
	protected virtual void init(){}
	protected virtual void start(){}
	protected virtual void update () {}

	public kView getView(string viewName){
		if( views == null )
			return null;

		for(int i = 0; i < views.Count;i++){
			if(views[i] == null)//TODO - find another fix!!!
				continue;
			if(views[i].gameObject.name.CompareTo(viewName) == 0)
				return views[i];
		}
		return null;
	}
	
	public void disableAllViews(){
		for(int i = 0; i < views.Count;i++){
			if(views[i] != null)
				views[i].gameObject.SetActive(false);
		}
	}
	
	protected override void onInit(){
		base.onInit();
		var instances = Resources.FindObjectsOfTypeAll(GetType());//GameObject.FindObjectsOfType(GetType());
		if(instances != null && instances.Length > 1)
			DestroyImmediate(gameObject);
		if(Application.isPlaying)
			init();
	}
	
	protected override void onStart(){
		base.onStart();
		if(Application.isPlaying)
			start();
	}
	
	protected override void onUpdate(){
		base.onUpdate();
		if(Application.isPlaying)
			update();
#if UNITY_EDITOR
		if(!Application.isPlaying){//this should happens only in edit mode
			if( GetType().Name.CompareTo(gameObject.name) != 0){
				gameObject.name = GetType().Name;
			}
			updateViewList();
		}
#endif
	}

	public void viewStartTransition(kView view, kAppManager.ViewTransitionType transitionType, bool show)
	{
		if(show) {
			viewWillShow(view);
		} else {
			viewWillHide(view);
		}
		if (view != null) {
			playTransitionSound(transitionType, view);
		}
	}
	public void viewEndTransition(kView view, kAppManager.ViewTransitionType transitionType, bool show){
		if(show) {
			viewShown(view);
		} else {
			viewHidden(view);
		}
	}

	private void playTransitionSound(kAppManager.ViewTransitionType transitionType, kView view) 
	{
		switch(transitionType) {
			case kAppManager.ViewTransitionType.SLIDE_DOWN:
			case kAppManager.ViewTransitionType.SLIDE_OVER_DOWN: {
			}break;

			case kAppManager.ViewTransitionType.SLIDE_RIGHT:
			case kAppManager.ViewTransitionType.SLIDE_OVER_RIGHT: {
			}break;

			case kAppManager.ViewTransitionType.SLIDE_LEFT:
			case kAppManager.ViewTransitionType.SLIDE_OVER_LEFT: {
			}break;

			case kAppManager.ViewTransitionType.SLIDE_UP:
			case kAppManager.ViewTransitionType.SLIDE_OVER_UP: {
			}break;
		}
	}
	
#if UNITY_EDITOR	
	private void updateViewList(){
		foreach (kView view in Resources.FindObjectsOfTypeAll(typeof(kView))){
			if(view.transform.parent == transform)
			{
				int i = 0;
				for(; i < views.Count;i++)
					if (views[i] == view)
						break;	

				if(i == views.Count)
					views.Add(view);
			}
		}
	}
#endif
}
