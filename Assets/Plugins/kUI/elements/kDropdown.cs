using UnityEngine;
using System.Collections;

public enum kDropdownItemState {
	IDLE = 0,
	PRESSED,
	SELECTED,
};

public class kDropdownItem : kSpriteObject
{
	public System.Action<kDropdownItem> onSelect = null;

	//public string pressedAnim = "0x0300000";
	//public string selectedAnim = "0x0300000";
	public kSpriteItem m_pressedAnim = new kSpriteItem();
	public kSpriteItem m_selectedAnim= new kSpriteItem();

	public kText itemText = null;
	public Color defaultTextColor = Color.white;
	public Color selectedTextColor = Color.white;

	kDropdownItemState state = kDropdownItemState.IDLE;

	public void initFromParent(kDropdown parent)
	{
		setSourceSprite(parent.sprite);
		m_defaultAnim 	= parent.m_itemDefaultAnim;
		m_pressedAnim 	= parent.m_itemPressedAnim;
		m_selectedAnim 	= parent.m_itemSelectedAnim;

		GameObject objText = new GameObject("kText");
		objText.transform.parent = transform;
		itemText = objText.AddComponent<kText>();
		itemText.textAlign = kAlignMode.VCENTER_LEFT;
		itemText.font = parent.font;
		defaultTextColor = parent.defaultTextColor;
		selectedTextColor= parent.selectedTextColor;
		
		kTouchable touchable = gameObject.AddComponent<kTouchable>();
		touchable.onTouchEnd = (Touch touch) => {
			if (state == kDropdownItemState.PRESSED)
				setState(playbackDir == PlaybackDir.ANIM_PLAY_BK ? kDropdownItemState.SELECTED : kDropdownItemState.IDLE);
		};
		touchable.onTouchPressed = (Touch touch) => {
			setState(kDropdownItemState.PRESSED);
		};
		touchable.onTouchRelease = (Touch touch) => {
			if (state == kDropdownItemState.PRESSED) {
				setState(kDropdownItemState.SELECTED);
				if (onSelect != null)
					onSelect(this);
			}
		};
		setState(kDropdownItemState.IDLE);
		objText.transform.localScale = new Vector3(parent.fontScale, parent.fontScale, 1);
		objText.transform.localPosition = new Vector3(0.05f * getBounds().width, -0.5f * getBounds().height, -0.1f);
		itemText.setWarpWidth((int)(0.8f * getBounds().width / parent.fontScale));
	}

	public void setState(kDropdownItemState newState)
	{
		switch (newState)
		{
			case kDropdownItemState.IDLE:
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				itemText.setColor(defaultTextColor);
				break;
			case kDropdownItemState.PRESSED:
				if (state == kDropdownItemState.SELECTED)
					play(m_selectedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_BK);
				else
					play(m_pressedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				itemText.setColor(selectedTextColor);
				break;
			case kDropdownItemState.SELECTED:
				play(m_selectedAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				itemText.setColor(selectedTextColor);
				break;
		}
		state = newState;
	}
}

public enum kDropdownState {
	INACTIVE = 0,
	ACTIVE,
	DISABLED,
};

public class kDropdown : kSpriteObject
{
	public kSpriteItem m_activeAnim = new kSpriteItem();
	public kSpriteItem m_itemDefaultAnim = new kSpriteItem();
	public kSpriteItem m_itemPressedAnim = new kSpriteItem();
	public kSpriteItem m_itemSelectedAnim= new kSpriteItem();
	
	public kFont font;
	public float fontScale = 1.0f;
	public string[] items;
	public Color defaultTextColor = Color.white;
	public Color selectedTextColor = Color.white;
	public Color disabledTextColor = Color.white;

	private kText _text;
	private kView _clipView;
	private kDropdownItem[] _items;
	private kDropdownItem _selectedItem = null;

	private kDropdownState state = kDropdownState.INACTIVE;

	protected override void onInit()
	{
		base.onInit();

#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_activeAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_itemDefaultAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_itemPressedAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_itemSelectedAnim,sourceSprite,gameObject);
#endif

		if (Application.isPlaying)
		{
			Rect bkgBounds = getBounds();
			GameObject objText = new GameObject("_text");
			objText.transform.parent = transform;
			objText.transform.localScale = new Vector3(fontScale, fontScale, 1);
			objText.transform.localPosition = new Vector3(0.05f * bkgBounds.width, -0.5f * bkgBounds.height, -0.1f);
			_text = objText.AddComponent<kText>();
			_text.textAlign = kAlignMode.VCENTER_LEFT;
			_text.setWarpWidth((int)(0.8f * bkgBounds.width / fontScale));
			_text.setColor(defaultTextColor);
			_text.font = font;
			
			GameObject objClip = new GameObject("clipView");
			objClip.transform.parent = transform;
			objClip.transform.localPosition = new Vector3(0, -bkgBounds.height, 0);
			_clipView = objClip.AddComponent<kView>();
			_clipView.viewSize = new Vector2(bkgBounds.width, 0);
			kTouchable touchable = gameObject.AddComponent<kTouchable>();
			touchable.onFocus = onDropdownFocus;
			touchable.onFocusLost = onDropdownUnfocus;
			touchable.onTouchPressed = (Touch t) => onDropdownTouchPressed();
			
			_items = new kDropdownItem[items.Length];
			for (int i = 0; i < items.Length; ++i)
			{
				GameObject objItem = new GameObject("item" + i);
				objItem.transform.parent = (i == 0) ? objClip.transform : _items[0].transform;
				objItem.transform.localPosition = new Vector3(0, -_clipView.viewSize.y, 0);
				kDropdownItem ddItem = _items[i] = objItem.AddComponent<kDropdownItem>();
				ddItem.initFromParent(this);
				ddItem.itemText.setText(items[i]);
				ddItem.onSelect = onItemSelected;
				ddItem.GetComponent<kTouchable>().onFocus = onItemFocus;
				_clipView.viewSize.y += ddItem.getBounds().height;
			}
			
			if (_items.Length > 0)
				_items[0].transform.localPosition = new Vector3(0, _clipView.viewSize.y, 0);
		}
	}
	
	private void onDropdownFocus()
	{
		if (state != kDropdownState.DISABLED && _items.Length > 0)
		{
			setState(kDropdownState.ACTIVE);
			iTween.MoveTo(_items[0].gameObject, iTween.Hash("position", _clipView.transform.position, "time", 0.3f, "easeType", iTween.EaseType.easeInOutCubic));
		}
	}

	private void onDropdownUnfocus()
	{
		if (state == kDropdownState.ACTIVE)
		{
			setState(kDropdownState.INACTIVE);
			iTween.MoveTo(_items[0].gameObject, iTween.Hash("position", _clipView.transform.position + Vector3.up * _clipView.viewSize.y,
				"time", 0.3f, "easeType", iTween.EaseType.easeInOutCubic));
		}
	}

	private void onDropdownTouchPressed()
	{
		if (state == kDropdownState.ACTIVE)
			onDropdownUnfocus();
		else
			onDropdownFocus();
	}

	private void onItemFocus()
	{
		GetComponent<kTouchable>().setFocused();
	}
	
	private void onItemSelected(kDropdownItem item)
	{
		if (_selectedItem != null)
			_selectedItem.setState(kDropdownItemState.IDLE);
		item.setState(kDropdownItemState.SELECTED);
		_text.setText(item.itemText.getText());
		_selectedItem = item;
		onDropdownUnfocus();
	}

	public string getText()
	{
		return _text.getText();
	}

	public int getSelection()
	{
		for (int i = 0; i < _items.Length; ++i)
			if (_selectedItem == _items[i])
				return i;
		return -1;
	}
	
	public void setSelection(int index)
	{
		if (index < 0)
			index = 0;
		if (index >= _items.Length)
			index = _items.Length - 1;
		onItemSelected(_items[index]);
	}

	public kDropdownState getState()
	{
		return state;
	}
	
	public void setState(kDropdownState newState)
	{
		switch (newState)
		{
			case kDropdownState.INACTIVE:
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				_text.setColor(defaultTextColor);
				break;
			case kDropdownState.ACTIVE:
				play(m_activeAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				_text.setColor(defaultTextColor);
				break;
			case kDropdownState.DISABLED:
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				_text.setColor(disabledTextColor);
				break;
		}
		state = newState;
	}
}
