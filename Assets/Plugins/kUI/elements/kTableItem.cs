using UnityEngine;
using System.Collections;

public enum kTableItemState {
	IDLE = 0,
	PRESSED,
	SELECTED,
};

public class kTableItem : kSpriteObject
{
	public static int MAX_TAP_OFFSET_SQUARED = 20 * 20;

	public System.Action<kTableItem> onSelect = null;

	public kCheckbox m_checkbox;
	public kTextField m_itemText;
	public Color m_selectedTextColor = Color.white;
	public Color m_selectedColor = Color.white;

	private Color m_defaultTextColor, m_defaultColor;
	private kTableItemState m_state = kTableItemState.IDLE;

	//private kTableItemState m_pressState;
	//private Vector2 m_pressPos;

	protected override void onInit()
	{
		base.onInit();
		if (!Application.isPlaying)
			return;

		m_defaultColor = getBlendingColor();
		if (m_itemText != null)
			m_defaultTextColor = m_itemText.getColor();

		kTouchable touchable = GetComponent<kTouchable>();
		if (touchable == null) {
			touchable = gameObject.AddComponent<kTouchable>();
		}
		touchable.onTouchPressed = (Touch touch) => {
			//m_pressState = m_state;
			//m_pressPos = touch.position;
			setState(kTableItemState.PRESSED);
		};
		touchable.onTouchRelease = (Touch touch) => {
			if (m_state == kTableItemState.PRESSED) {
				setState(kTableItemState.SELECTED);
				if (onSelect != null)
					onSelect(this);
			}
		};
		/*touchable.onTouching = (Touch touch) => {
			if (m_state == kTableItemState.PRESSED && (touch.position - m_pressPos).sqrMagnitude > MAX_TAP_OFFSET_SQUARED)
				setState(m_pressState);
		};
		touchable.onTouchEnd = (Touch touch) => {
			setState(m_pressState);
		};*/

		setState(kTableItemState.IDLE);
	}

	protected override void onStart()
	{
		base.onStart();
		if (!Application.isPlaying)
			return;

		if (m_checkbox != null) {
			//m_checkbox.enable(false);
			Destroy(m_checkbox.GetComponent<kTouchable>());
			Destroy(m_checkbox.GetComponent<BoxCollider>());
		}
	}

	public void setState(kTableItemState newState)
	{
		setBlendingColor(newState == kTableItemState.IDLE ? m_defaultColor : m_selectedColor);
		if (m_itemText != null)
			m_itemText.setColor(newState == kTableItemState.IDLE ? m_defaultTextColor : m_selectedTextColor);
		if (m_checkbox != null && newState != kTableItemState.PRESSED)
			m_checkbox.setChecked(newState == kTableItemState.SELECTED);
		m_state = newState;
	}
}
