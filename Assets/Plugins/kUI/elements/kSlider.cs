using UnityEngine;
using System.Collections;

public enum kSliderState {
	IDLE = 0,
	PRESSED,
};

public class kSlider : kSpriteObject
{
	public kObject fill = null;
	public kSpriteObject knob = null;
	public int sliderSteps = 2;
	
	public System.Action<kSlider, int> valueChanged;
	
	private float fillSize;
	private Vector2 knobPosition;
	public AudioClip m_soundPress;
	public float knobStartPosOffsetX;

	protected kSliderState state = kSliderState.IDLE;
	protected int sliderValue = -1;
	
	protected override void onInit()
	{
		base.onInit();
		state = kSliderState.IDLE;
		if (Application.isPlaying)
		{
			kTouchable touchable = GetComponent<kTouchable>();
			if (touchable == null)
				touchable = gameObject.AddComponent<kTouchable>();
			touchable.onTouchPressed = (Touch touch) => {
				setState(kSliderState.PRESSED);
				if ( m_soundPress != null )
					playSound(m_soundPress);
			};
			touchable.onTouchRelease = (Touch touch) => {
				setState(kSliderState.IDLE);
			};
			touchable.onTouchEnd = (Touch touch) => {
				setState(kSliderState.IDLE);
			};
			touchable.onTouching = onTouchMoved;
		}
	}

	protected override void onStart()
	{
		base.onStart();
		if (knob != null)
			knobPosition = knob.transform.localPosition;
		if (fill != null)
			fillSize = fill.getBounds().width / fill.transform.localScale.x;
		setValue(sliderValue, true);
	}

	protected virtual void onTouchMoved(Touch touch)
	{
		if (state != kSliderState.PRESSED)
			return;
		
		Vector2 touchLocalPosition = touch.position - (Vector2)transform.position;
		Rect bounds = getBounds();

		float minX = Mathf.Max( knobPosition.x, bounds.x) ; 
		float maxX = bounds.x +  bounds.width;//getBounds().width - minX - knob.getBounds().width * transform.localScale.x;

		float touchX = touchLocalPosition.x;// - minX;//touch.position.x - transform.position.x - knob.getBounds().width * transform.localScale.x / 2;
		int newValue = 0;
		if (touchX < minX)
			newValue = 0;
		else if (touchX > maxX)
			newValue = sliderSteps - 1;
		else{
			newValue = (int)Mathf.Round((sliderSteps - 1) * (touchX - minX) / (maxX - minX));
		}

		if (newValue != sliderValue)
			setValue(newValue, true);
	}

	public int getValue()
	{
		return sliderValue;
	}

	public void setValue(int newValue, bool forceSelect = false)
	{
		if (newValue < 0)
			newValue = 0;
		else if (newValue >= sliderSteps)
			newValue = sliderSteps - 1;
		if (newValue == sliderValue && !forceSelect)
			return;
		int oldValue = sliderValue;
		sliderValue = newValue;

		float knobPosPercent = sliderValue * 1.0f / (sliderSteps - 1);

		Rect sliderBounds = getBounds();
		//Rect knobBounds = knob.getBounds();

		Vector2 knobCenterStartPosInsideSlider = sliderBounds.center + Vector2.left *  (sliderBounds.width/2 - knobStartPosOffsetX);

		Vector2 knobCenterCrtPosInsideSlider = knobCenterStartPosInsideSlider + Vector2.right * ((sliderBounds.width - 2 * knobStartPosOffsetX) * knobPosPercent);

		knob.transform.localPosition = (Vector3)knobCenterCrtPosInsideSlider + Vector3.forward * knob.transform.localPosition.z;

//		knob.transform.localPosition = new Vector3(knobPosition.x + (getBounds().width / transform.localScale.x - 2 * knobPosition.x - knob.getBounds().width) * sliderValue / (sliderSteps - 1),
//			knob.transform.localPosition.y, knob.transform.localPosition.z);
		if (fill != null)
		{
			fill.transform.localScale = new Vector3((knob.transform.localPosition.x + knob.getBounds().width * transform.localScale.x / 2 - fill.transform.localPosition.x) / fillSize,
				fill.transform.localScale.y, fill.transform.localScale.z);
		}
		if (valueChanged != null && (oldValue != -1 || forceSelect))
			valueChanged(this, sliderValue);
	}

	public kSliderState getState()
	{
		return state;
	}
	
	public void setState(kSliderState newState)
	{
		state = newState;
	}

	public override int getSizeIncreaseForTouch(){
		return 100;
	}
	/*
	public override bool isReceivingTouchEventsAfterLoosingFocus(){
		return false;
	}*/
}
