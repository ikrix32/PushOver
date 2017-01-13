using UnityEngine;
using System.Collections;

public enum kCheckboxState {
	UNCHECKED = 0,
	PRESSED_UNCHECKED,
	PRESSED_CHECKED,
	CHECKED,
	DISABLED_CHECKED,
	DISABLED_UNCHECKED,
};


public class kCheckbox : kSpriteObject
{
	public string textChecked = "", textUnchecked = "";
	public kTextField m_text = null;

	public kSpriteItem m_disabledAnim = new kSpriteItem();
	public kSpriteItem m_disabledCheckedAnim = new kSpriteItem();
	public kSpriteItem m_checkedAnim = new kSpriteItem();
	public kSpriteItem m_transitionAnim = new kSpriteItem();

	public bool m_animated = false;
	public bool m_fadeOnDisable = false;
	public Color m_disabledColor = Color.white;

	public AudioClip m_soundPress;
	
	public System.Action onPress;
	public System.Action onRelease;
	
	public float m_pressedScaleModifier = 0;
	
	private kCheckboxState state = kCheckboxState.UNCHECKED;
	
	protected override void onInit()
	{
		base.onInit();

#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_disabledAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_disabledCheckedAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_checkedAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_transitionAnim,sourceSprite,gameObject);
#endif
		//m_originalBlendingColor = getBlendingColor();
	
		if (Application.isPlaying) {
			kTouchable touchable = GetComponent<kTouchable>();
			if (touchable == null) {
				touchable = gameObject.AddComponent<kTouchable>();
			}
			touchable.onTouchPressed = (Touch touch) => {
				if (state != kCheckboxState.DISABLED_UNCHECKED && state != kCheckboxState.DISABLED_CHECKED) {
					transform.localScale += Vector3.one * m_pressedScaleModifier;
					if (m_soundPress != null) {
						playSound(m_soundPress);
					}
					setState(state == kCheckboxState.UNCHECKED ? kCheckboxState.PRESSED_UNCHECKED : kCheckboxState.PRESSED_CHECKED);
					if (onPress != null) {
						onPress();
					}
				}
			};
			touchable.onTouchEnd = (Touch touch) => {
				if (state == kCheckboxState.PRESSED_CHECKED || state == kCheckboxState.PRESSED_UNCHECKED) {
					transform.localScale -= Vector3.one * m_pressedScaleModifier;
					setState(state == kCheckboxState.PRESSED_CHECKED ? kCheckboxState.CHECKED : kCheckboxState.UNCHECKED);
				}
			};
			touchable.onTouchRelease = touchable.onTouchEnd;
			touchable.onTouchTap = (Touch touch) => {
				if (state != kCheckboxState.DISABLED_UNCHECKED && state != kCheckboxState.DISABLED_CHECKED) {
					setState(state == kCheckboxState.CHECKED ? kCheckboxState.UNCHECKED : kCheckboxState.CHECKED);
					if (onRelease != null)
						onRelease();
				}
			};
		}
	}
	
	public bool isChecked()
	{
		return state == kCheckboxState.DISABLED_CHECKED || state == kCheckboxState.CHECKED;
	}
	
	public void setChecked(bool val)
	{
		if (state == kCheckboxState.DISABLED_CHECKED || state == kCheckboxState.DISABLED_UNCHECKED) {
			setState(val ? kCheckboxState.DISABLED_CHECKED : kCheckboxState.DISABLED_UNCHECKED);
		} else {
			setState(val ? kCheckboxState.CHECKED : kCheckboxState.UNCHECKED);
		}
	}
	
	public bool isEnabled()
	{
		return state != kCheckboxState.DISABLED_CHECKED && state != kCheckboxState.DISABLED_UNCHECKED;
	}
	
	public void enable(bool val)
	{
		if (val) {
			setState((state == kCheckboxState.DISABLED_UNCHECKED || state == kCheckboxState.UNCHECKED) ? kCheckboxState.UNCHECKED : kCheckboxState.CHECKED);
		} else {
			setState((state == kCheckboxState.DISABLED_UNCHECKED || state == kCheckboxState.UNCHECKED) ? kCheckboxState.DISABLED_UNCHECKED : kCheckboxState.DISABLED_CHECKED);
		}
	}
	
	public kCheckboxState getState()
	{
		return state;
	}
	
	public void setState(kCheckboxState newState)
	{
		if (newState == state)
			return;
		//Debug.LogWarning(" * " + gameObject.name + ", setState: " + newState + ", oldState: " + state);
		switch (newState) {
		case kCheckboxState.DISABLED_UNCHECKED:
			play(m_disabledAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
			break;
		case kCheckboxState.DISABLED_CHECKED:
			play(m_disabledCheckedAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
			break;
		case kCheckboxState.UNCHECKED:
			if (m_animated && (state == kCheckboxState.CHECKED || state == kCheckboxState.PRESSED_CHECKED)) {
				play(m_transitionAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_BK);
			} else {
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
			}
			break;
		case kCheckboxState.CHECKED:
			if (m_animated && (state == kCheckboxState.UNCHECKED || state == kCheckboxState.PRESSED_UNCHECKED)) {
				play(m_transitionAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
			} else {
				play(m_checkedAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
			}
			break;
		}
		if (m_text != null) {
			m_text.setText((state == kCheckboxState.DISABLED_CHECKED || state == kCheckboxState.PRESSED_CHECKED || state == kCheckboxState.CHECKED) ? textChecked : textUnchecked);
		}
		setBlendingColor(m_fadeOnDisable && (newState == kCheckboxState.DISABLED_CHECKED || newState == kCheckboxState.DISABLED_UNCHECKED) ? m_disabledColor : Color.white);
		state = newState;
	}
}
