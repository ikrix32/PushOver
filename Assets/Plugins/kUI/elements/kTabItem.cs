using UnityEngine;
using System.Collections;

public enum kItemState {
	IDLE = 0,
	PRESSED,
	HIGHLIGHTED,
	DISABLED,
};

public class kTabItem : kSpriteObject
{
	public kTextField mainText;
	public kSpriteObject m_background = null;
	
	public System.Action<kTabItem> onHighlight = null;
	public System.Action<kTabItem> onLoseHighlight = null;
	
	public kSpriteItem m_pressedAnim = new kSpriteItem();
	public kSpriteItem m_highlightedAnim = new kSpriteItem();
	
	private kItemState state = kItemState.IDLE;
	
	protected override void onInit()
	{
		base.onInit();

		#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_pressedAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_highlightedAnim,sourceSprite,gameObject);
		#endif

		state = kItemState.IDLE;
		if (GetComponent<kTouchable>() == null) {
			gameObject.AddComponent<kTouchable>();
		}
	}
	
	protected override void onStart()
	{
		base.onStart();
		if (Application.isPlaying)
		{
			kTouchable touchable = GetComponent<kTouchable>();
			touchable.onTouchEnd = (Touch touch) => {
				if (state == kItemState.PRESSED){
					if(playbackDir == PlaybackDir.ANIM_PLAY_FW)
						setState(kItemState.IDLE);
					else
						setState(kItemState.HIGHLIGHTED);
				}
			};
			touchable.onTouchPressed = (Touch touch) => {
				if (state != kItemState.DISABLED) {
					setState(kItemState.PRESSED);
					
					bool callOnHighlight = (onHighlight != null);// && playbackDir == PlaybackDir.ANIM_PLAY_FW);
					setState(kItemState.HIGHLIGHTED);
					if (callOnHighlight)
						onHighlight(this);
				}
			};
			/*touchable.onTouchRelease = (Touch touch) => {
				//avoid dragg from outside of button and release over the button
				if(getState() == kItemState.PRESSED){
					//call onHighlight only when it wasn't highlighted before press
					bool callOnHighlight = (onHighlight != null);// && playbackDir == PlaybackDir.ANIM_PLAY_FW);
					setState(kItemState.HIGHLIGHTED);
					if (callOnHighlight)
						onHighlight(this);
				}
			};*/
		}
	}
	
	public kItemState getState()
	{
		return state;
	}

	public void setState(kItemState newState)
	{
		switch (newState)
		{
			case kItemState.IDLE:
			{
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
			}
			break;
			case kItemState.PRESSED:
			{
				if (state == kItemState.IDLE) // play pressed anim fw & bk to know what the previous state was
					play(m_pressedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				else
					play(m_pressedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_BK);
			}
			break;
			case kItemState.HIGHLIGHTED:
			{
				play(m_highlightedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
			}
			break;
		}
		if (GetComponent<kTouchable>() != null)
			GetComponent<kTouchable>().enabled = newState != kItemState.DISABLED;
		state = newState;
	}
}
