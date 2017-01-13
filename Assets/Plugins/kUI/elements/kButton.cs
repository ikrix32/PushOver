using UnityEngine;
using System.Collections;

public enum kButtonState{
	INVALID = -1 ,
	RELEASED ,
	TRANSITION,
	PRESSED,
	DISABLED,
};


public class kButton : kSpriteObject {

	public System.Action onPress;
	public System.Action onRelease;
	public System.Action onTap;

	public kPlayMakerEvent m_playmakerPressEvent;
	public kPlayMakerEvent m_playmakerReleaseEvent;
	public kPlayMakerEvent m_playmakerTapEvent;

	public kTextField m_text;

	public float m_pressedScaleModifier = 0;
	public bool m_fadeOnDisable = false;

	public kSpriteItem m_transitionAnim = new kSpriteItem();
	public kSpriteItem m_pressedAnim = new kSpriteItem();
	public kSpriteItem m_disabledAnim = new kSpriteItem();
	
	private kButtonState state = kButtonState.RELEASED;

	public Color m_releasedColor;
	public Color m_pressedColor;
	public Color m_disabledColor;

	protected override void onInit(){
		base.onInit();

		if(!Application.isPlaying)
		{//TODO remove
			if(m_releasedColor == Color.clear)
				m_releasedColor = m_blendingColor;//getBlendingColor();

			if(m_disabledColor == Color.clear){
				if(m_fadeOnDisable)
					m_disabledColor = new Color(m_releasedColor.r,m_releasedColor.g,m_releasedColor.b,m_releasedColor.a * 0.25f);
				else
					m_disabledColor = m_releasedColor;
			}

			if(m_pressedColor == Color.clear){
				m_pressedScaleModifier = 0;
				m_pressedColor = new Color(m_releasedColor.r * 0.75f, m_releasedColor.g * 0.75f, m_releasedColor.b * 0.75f, m_releasedColor.a);
			}
		}
		//state = kButtonState.RELEASED;
		setState(state);

#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_transitionAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_pressedAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_disabledAnim,sourceSprite,gameObject);
#endif
		kTouchable touchable = GetComponent<kTouchable>();
		if (touchable == null)
			touchable = gameObject.AddComponent<kTouchable>();
		
		if (Application.isPlaying)
		{
			touchable.onTouchPressed = (Touch touch) => {
				if (enabled && getState() != kButtonState.DISABLED) {
					setState(kButtonState.TRANSITION);
					if (onPress != null)
						onPress();
					if(m_playmakerPressEvent != null){
						m_playmakerPressEvent.SendEvent();
					}
				}
			};
			touchable.onTouchRelease = (Touch touch) => {
				if (getState() == kButtonState.TRANSITION || getState() == kButtonState.PRESSED) {
					setState(kButtonState.TRANSITION);
					if (onRelease != null)
						onRelease();
					if(m_playmakerReleaseEvent != null){
						m_playmakerReleaseEvent.SendEvent();
					}
				}
			};
			touchable.onTouchEnd = (Touch touch) => {
				if (getState() == kButtonState.TRANSITION || getState() == kButtonState.PRESSED)
					setState(kButtonState.TRANSITION);
			};
			touchable.onTouchTap = (Touch touch) => {
				if (getState() == kButtonState.TRANSITION || getState() == kButtonState.PRESSED) {
					//setState(kButtonState.TRANSITION);
					if (onTap != null)
						onTap();

					if(m_playmakerTapEvent != null){
						m_playmakerTapEvent.SendEvent();
					}
				}
			};
		}
	}
	
	protected override void onStart(){
		base.onStart();
	}
	
	public kButtonState getState(){
		return state;
	}
	
	protected override void onUpdate(){
		base.onUpdate();
		
		if(state == kButtonState.TRANSITION && !isPlaying()){
			if(playbackDir == PlaybackDir.ANIM_PLAY_FW){
				setState(kButtonState.PRESSED);
			}else{
				setState(kButtonState.RELEASED);
			}
		}
	}
	
	public virtual void setState(kButtonState newState,bool allowDisableTouchable = true)
	{
		switch (newState) {
			case kButtonState.RELEASED: {
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);

				updateBlendColor(m_releasedColor);
				kTouchable touchable = GetComponent<kTouchable>();
				if(touchable != null)
				{
					touchable.setPlayPressSounds = true;
				}
			} break;
			case kButtonState.TRANSITION: {
				if (state == kButtonState.RELEASED) {
					transform.localScale += Vector3.one * m_pressedScaleModifier;
					play(m_transitionAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
				} else {
					transform.localScale -= Vector3.one * m_pressedScaleModifier;
					play(m_transitionAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_BK);
				}
			} break;
			case kButtonState.PRESSED: {
				play(m_pressedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				updateBlendColor(m_pressedColor);
			} break;
			case kButtonState.DISABLED: {
				play(m_disabledAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				updateBlendColor(m_disabledColor);
				if (state == kButtonState.PRESSED) {
					transform.localScale -= Vector3.one * m_pressedScaleModifier;
				}
				kTouchable touchable = GetComponent<kTouchable>();
				if(touchable != null)
				{
					touchable.setPlayPressSounds = false;
				}
			} break;
		}
		//if (m_fadeOnDisable)
		//	setAlpha(newState == kButtonState.DISABLED ? 0.5f : 1f);

		if(m_text != null && m_fadeOnDisable){
			m_text.setAlpha(m_blendingColor.a);
		}

		if (allowDisableTouchable) {
			kTouchable touchable = GetComponent<kTouchable>();
			if (touchable != null){
				touchable.enabled = newState != kButtonState.DISABLED;
				if(touchable.getCollider() != null){
					touchable.getCollider().enabled = touchable.enabled;
				}
			}
		}
		state = newState;
	}

	private void updateBlendColor(Color color){
		if(!Application.isPlaying)
			return;
	
		setBlendingColor(color);
	}
	
	public void setButtonFrames(int defaultFrameId, int pressedFrameId = 0, int disabledFrameId = 0, int transitionFrameId = 0)
	{
		m_defaultAnim.id = defaultFrameId;
		m_pressedAnim.id = pressedFrameId == 0 ? defaultFrameId : pressedFrameId;
		m_disabledAnim.id = disabledFrameId == 0 ? defaultFrameId : disabledFrameId;
		m_transitionAnim.id = transitionFrameId == 0 ? m_pressedAnim.id : transitionFrameId;

		play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_ONCE, PlaybackDir.ANIM_PLAY_FW);
	}

	public void setButtonColor(Color c)
	{
		m_releasedColor = c;
		m_pressedColor = new Color(0.8f * c.r, 0.8f * c.g, 0.8f * c.b, c.a);
	}

	public override void setAlpha(float alpha){
		base.setAlpha(alpha);
		if (m_text != null)
			m_text.setAlpha(alpha);
	}
}
