using UnityEngine;
using System.Collections;

public class kSliderToggleOnOff : kSlider
{
	public kTextField ON;
	public kTextField OFF;

	public kSpriteItem knobOnFrame;
	public kSpriteItem knobOffFrame;

	protected override void onInit()
	{
		base.onInit();
		valueChanged += toggle_ON_OFF;
		kTouchable touchable = GetComponent<kTouchable>();
		if (touchable != null) {
			touchable.onTouchRelease += snapToValue;
			touchable.onTouchEnd += snapToValue;
		}
	}

	protected override void onTouchMoved (Touch touch)
	{
		if (state != kSliderState.PRESSED)
			return;

		// The toggle is centered.
		float toggleCenterX = transform.position.x ;
		//Debug.LogWarning("toggleCenterX = " + toggleCenterX);

		float touchX = touch.position.x;
		//Debug.LogWarning("touchX = " + touchX);

		int newValue = 0;
		if (touchX < toggleCenterX)
		{
			newValue = 0;
		}
		else if (touchX > toggleCenterX)
		{
			newValue = sliderSteps - 1;
		}

		if (newValue != sliderValue)
		{
			setValue(newValue, true);
		}
	}

	private void snapToValue(Touch t)
	{
		if (sliderSteps > 2) {
			setValue(getValue() < sliderSteps / 2 ? 0 : sliderSteps);
		}
	}

	private void toggle_ON_OFF(kSlider slider, int value)
	{
		if (knobOnFrame.id != 0 && knobOffFrame.id != 0)
		{
			knob.play( value != 0 ? knobOnFrame.id : knobOffFrame.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
		}
		if (ON != null) {
			ON.gameObject.SetActive(value != 0);
		}
		if (OFF != null) {
			OFF.gameObject.SetActive(value == 0);
		}
	}
}
