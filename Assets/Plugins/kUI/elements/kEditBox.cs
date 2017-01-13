using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum kEditState {
	INACTIVE = 0,
	ACTIVE,
};

public enum kEditType {
	TEXT = 0,
	EMAIL,
	PASSWORD,
	MULTILINE,
	NUMBER,
}

public enum kEditAlign {
	LEFT = 0,
	RIGHT,
}

public enum kRegexCheck {
	TYPING = 0,
	FOCUS_LOST,
}

public class kEditBox : kSpriteObject
{
	public System.Action onFocusBegin;
	public System.Action onFocusLost;
	public System.Action onDoneEditing;
	public System.Action onCancelEditing;
	public System.Action onRegexCheckFail = null;
	public System.Action onDeleteEditing = null;

	public kSpriteItem m_activeAnim = new kSpriteItem();
	public kSpriteItem m_cursorAnim = new kSpriteItem();
	public kObject m_bkg;
	public kButton m_btnDelete;
	
	public kEditType editType = kEditType.TEXT;
	public int resizeMaxLines = 0;
	public int textWrapWidth = 0;
	public Font font;
	public float charSize = 10;
	public float lineSpacing = 0;
	public string editText;
	public string hintText;
	public kRegexCheck regexCheck = kRegexCheck.TYPING;
	public string regexString = null;
	public Color editColor = Color.white;
	public Color hintColor = Color.white;
	public int charLimit = 140;
	
	public	kEditAlign textAlign = kEditAlign.LEFT;
	public bool handleKeyboardAutomatically = true;
	public bool m_emojiiSupport = false;
	public float m_clipInsetX = 25f;
	public float m_clipInsetY = 0f;
	public float m_clipWidth = 0f;
	public float m_clipHeight = 0f;
	public float m_currsorOffsetY = 0f;

	private kEditState m_state = kEditState.INACTIVE;
	private kView m_clipView;
	private kSpriteObject m_cursor;
	private	kTextField m_editText;
	private kTextMesh m_hintText;

	private const float PASSWORD_HIDE_TIME = 0.5f;
	private const float BASE_LINE_SPACING = 0.8535f;
	private const float CURSOR_WIDTH = 4f;
	private const int EDGE_INSET = 10;
	protected const int FONT_SIZE = 30;
	
	private float m_passwordTime;
	private int m_cursorPos;

	public System.Action<Vector3> onLongPress;
	private Vector2 m_touchStartPos = Vector2.zero;
	private float m_touchStartTime = 0f;
	private bool m_pressed = false;
	
	public System.Action<Rect> onAutoResize;
	private Vector3 m_baseScale;
	private Rect m_baseRect;
	private int m_nbLines;
	
#if UNITY_EDITOR
#elif UNITY_IPHONE || UNITY_ANDROID
	private TouchScreenKeyboard tsKeyboard = null;
#endif

	private bool wasKeyBoardShown = false;
	protected override void onAppPause(bool pause){
		base.onAppPause(pause);
		if(pause){
			wasKeyBoardShown = this.isInEditMode();
			//hideKeyboard();
		}else{
			if(wasKeyBoardShown)
			{
				showKeyboard();
			}
			else 
				hideKeyboard();
		}
	}

	protected override void onInit()
	{
		base.onInit();

		#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_activeAnim,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_cursorAnim,sourceSprite,gameObject);
		#endif

		if (Application.isPlaying)
		{
			kView oldView = GetComponentInChildren<kView>();
			if (oldView != null)
				GameObject.DestroyObject(oldView.gameObject);
			
			m_nbLines = 1;
			m_baseRect = getBounds();
			m_baseScale = transform.localScale;
			GameObject objClip = new GameObject("clipView");
			objClip.transform.parent = transform;
			objClip.transform.localPosition = new Vector3(m_clipInsetX, -m_clipInsetY, -1);
			m_clipView = objClip.AddComponent<kView>();
			m_clipView.viewSize = new Vector2(m_clipWidth != 0f ? m_clipWidth : (m_baseRect.width - 2 * m_clipInsetX), m_clipHeight != 0f ? m_clipHeight : m_baseRect.height);
			m_clipView.alignMode = kAlignMode.VCENTER_CENTER;
			kTouchable touchable = objClip.AddComponent<kTouchable>();
			touchable.onFocus = onEditFocus;
			touchable.onFocusLost = onEditUnfocus;
			touchable.onTouching = onTouching;
			touchable.onTouchPressed = (Touch t) => {
				m_touchStartPos = t.position;
				m_touchStartTime = Time.time;
				m_pressed = true;
			};
			touchable.onTouching += (Touch t) => {
				if (m_pressed && Vector2.Distance(t.position, m_touchStartPos) > kTouchable.TAP_DRAG_TOLERANCE)
					m_pressed = false;
				if (m_pressed && Time.time - m_touchStartTime > kTouchable.LONG_PRESS_TIME)
				{
					m_pressed = false;
					if (onLongPress != null)
						onLongPress(m_cursor.transform.position);
				}
			};
			touchable.onTouchEnd = (Touch t) => {
				m_pressed = false;
			};
			touchable.onTouchRelease = touchable.onTouchEnd;
			if (m_btnDelete != null) {
				m_btnDelete.setState(kButtonState.DISABLED);
				m_btnDelete.gameObject.SetActive(true);
				m_btnDelete.onPress = () => {
					m_btnDelete.setState(kButtonState.DISABLED);
					if (onDeleteEditing != null) {
						onDeleteEditing();
					}
				};
			}
			
			GameObject objCursor = new GameObject("cursor");
			objCursor.transform.parent = objClip.transform;
			m_cursor = objCursor.AddComponent<kSpriteObject>();
			m_cursor.setSourceSprite(sprite);
			m_cursor.gameObject.SetActive(false);
			m_cursor.m_defaultAnim = m_cursorAnim;

			GameObject objEdit = new GameObject("editText");
			m_editText = objEdit.AddComponent<kTextMesh>();
			m_editText.transform.parent = objClip.transform;
			m_editText.setWarpWidth (textWrapWidth);
			((kTextMesh)m_editText).m_emojiiSupport = m_emojiiSupport;
			((kTextMesh)m_editText).setup(font, FONT_SIZE, charSize, lineSpacing);
			m_editText.setAlignment(TextAlignment.Left);
			m_editText.setAnchor(TextAnchor.MiddleLeft);
			//m_editText.m_richText = false;
			m_editText.setColor(editColor);
			m_editText.setText(editText);
			float x_pos = textAlign == kEditAlign.RIGHT ? m_clipView.viewSize.x - CURSOR_WIDTH - m_editText.substringWidth(m_editText.getText(), 0, editText.Length) : 0;
			m_editText.transform.localPosition = new Vector3(x_pos, -0.5f * m_clipView.viewSize.y, -0.1f);
			if (editType == kEditType.MULTILINE)
			{
				m_editText.setAnchor(TextAnchor.UpperLeft);
				m_editText.transform.localPosition = new Vector3(x_pos, -EDGE_INSET, -0.1f);
				m_editText.setWarpWidth((int)(m_clipView.viewSize.x - CURSOR_WIDTH));
			}

			GameObject objHint = new GameObject("hintText");
			m_hintText = objHint.AddComponent<kTextMesh>();

			m_hintText.transform.parent = objClip.transform;
			m_hintText.transform.localScale = m_editText.transform.localScale;
			m_hintText.setup(font, FONT_SIZE, charSize, lineSpacing);
			m_hintText.setAlignment(m_editText.getAlignment());
			m_hintText.setAnchor(m_editText.getAnchor());
			m_hintText.setColor(hintColor);
			m_hintText.setText(hintText);
			x_pos = textAlign == kEditAlign.RIGHT ? m_clipView.viewSize.x - CURSOR_WIDTH - m_hintText.substringWidth(m_hintText.getText(),0, hintText.Length) : 0;
			m_hintText.transform.localPosition = new Vector3(x_pos, m_editText.transform.localPosition.y, m_editText.transform.localPosition.z);
		}
	}
	
	protected override void onStart()
	{
		base.onStart();
		if (Application.isPlaying)
		{
			m_cursor.transform.localPosition = new Vector3(0, -0.5f * getBounds().height + m_currsorOffsetY, -0.2f);
			updateCursorPos();
		}
	}
	
	public kEditState getState()
	{
		return m_state;
	}

	public bool isInEditMode(){
		return m_state == kEditState.ACTIVE;
	}

	protected override void onUpdate()
	{
		base.onUpdate();
		if (m_state != kEditState.ACTIVE)
			return;
		if (m_passwordTime > 0)
		{
			m_passwordTime -= Time.deltaTime;
			if (m_passwordTime < 0)
				hidePasswordLetter();
		}
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.UpArrow) || (Input.inputString.Length > 0 && (int)Input.inputString[0] == 63232))
		{
			System.ValueType touch = new Touch();
			Vector2 pos = (Vector2)m_cursor.transform.position + 1.2f * FONT_SIZE * Vector2.up + CURSOR_WIDTH * Vector2.right;
			typeof(Touch).GetField("m_Position", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(touch, pos);
			onTouching((Touch)touch);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) || (Input.inputString.Length > 0 && (int)Input.inputString[0] == 63233))
		{
			System.ValueType touch = new Touch();
			Vector2 pos = (Vector2)m_cursor.transform.position - 0.6f * FONT_SIZE * Vector2.up + CURSOR_WIDTH * Vector2.right;
			typeof(Touch).GetField("m_Position", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(touch, pos);
			onTouching((Touch)touch);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow) || (Input.inputString.Length > 0 && (int)Input.inputString[0] == 63234))
		{
			setCursorPos(m_cursorPos - 1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || (Input.inputString.Length > 0 && (int)Input.inputString[0] == 63235))
		{
			setCursorPos(m_cursorPos + ((m_cursorPos < editText.Length && System.Char.IsSurrogatePair(editText, m_cursorPos)) ? 2 : 1));
		}
		else
		{
			string text = getCursorText();
			if (Input.inputString.Length > 0)
			{
				if (Input.inputString[0] == (char)KeyCode.Backspace)
				{
					if (text.Length > 0)
						text = text.Substring(0, text.Length - ((text.Length > 1 && System.Char.IsSurrogatePair(text, text.Length - 2)) ? 2 : 1));
				}
				else
					text = text + Input.inputString[0];
			}
			if (getCursorText().CompareTo(text) != 0)
			{
				if (charLimit > 0 && getCursorText().Length < text.Length && text.Length + editText.Length - m_cursorPos > charLimit)
					updateCursorPos();
				else
					setCursorText(text);
			}
		}
#elif UNITY_IPHONE || UNITY_ANDROID
		if (tsKeyboard != null && getCursorText().Length != tsKeyboard.text.Length)
		{
			if (charLimit > 0 && getCursorText().Length < tsKeyboard.text.Length && tsKeyboard.text.Length + editText.Length - m_cursorPos > charLimit)
			{
				tsKeyboard.text = tsKeyboard.text.Substring(0, m_cursorPos);
				updateCursorPos();
			}
			else
				setCursorText(tsKeyboard.text);
		}
		
		if (tsKeyboard != null && (tsKeyboard.done || tsKeyboard.wasCanceled) && m_state == kEditState.ACTIVE) {
			if(tsKeyboard.done && onDoneEditing != null){
				onDoneEditing();
			}

			if(tsKeyboard != null && tsKeyboard.wasCanceled && onCancelEditing != null){
				onCancelEditing();
			}
			kScreen.instance.kTouchController.setFocusedObject(null);
		}
#endif
		if (editText != null && m_btnDelete != null) {
			m_btnDelete.setState(editText.Length > 0 ? kButtonState.RELEASED : kButtonState.DISABLED);
		}
	}
	
	public void setState(kEditState newState)
	{
		switch (newState)
		{
			case kEditState.INACTIVE:
				play(m_defaultAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				m_cursor.gameObject.SetActive(false);
				m_hintText.gameObject.SetActive(editText == null || editText.Length == 0);
				
				break;
			case kEditState.ACTIVE:
				play(m_activeAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				m_cursor.play(m_cursorAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
				m_cursor.gameObject.SetActive(true);
				m_hintText.gameObject.SetActive(false);
				break;
		}
		m_state = newState;
	}

	public void setFocused(bool focused)
	{
		if (focused) {
			m_clipView.GetComponent<kTouchable>().setFocused();
		} else {
			kScreen.instance.kTouchController.setFocusedObject(null);
		}
	}
	
	public bool isFocused()
	{
		return m_clipView.GetComponent<kTouchable>().isFocused();
	}
	
	private void onEditFocus()
	{
		setCursorPos(editText.Length);
		setState(kEditState.ACTIVE);
		if (onFocusBegin != null)
			onFocusBegin();
		if (handleKeyboardAutomatically)
			showKeyboard();
	}

	private void onEditUnfocus()
	{
		hidePasswordLetter();
		setState(kEditState.INACTIVE);
		m_cursorPos = textAlign == kEditAlign.LEFT ? 0 : editText.Length;
		updateCursorPos();

		if (onFocusLost != null)
			onFocusLost();
		if (regexCheck == kRegexCheck.FOCUS_LOST && !checkRegexMatching(getText()))
			return;
		if (handleKeyboardAutomatically)
			hideKeyboard();
	}
	
	private void onTouching(Touch t)
	{
		Vector2 relPos = t.position - (Vector2)m_clipView.transform.position - (Vector2)m_editText.transform.localPosition;
		if (editType == kEditType.MULTILINE)
		{
			List<kTextLine> textLines = m_editText.getTextLines();
			float lineHeight = (FONT_SIZE * charSize / 10f) * lineSpacing / BASE_LINE_SPACING;
			int cursorLine = Mathf.Clamp(Mathf.RoundToInt((-relPos.y - EDGE_INSET) / lineHeight), 0, textLines.Count - 1);
			int cursorPos = textLines[cursorLine].idxEnd;
			for (int i = textLines[cursorLine].idxStart + 1; i <= textLines[cursorLine].idxEnd; ++i)
			{
				float textWidth = m_editText.substringWidth(textLines[cursorLine].idxStart, i);
				if (textWidth > relPos.x)
				{
					cursorPos = (relPos.x - m_editText.substringWidth(0, i - 1) < textWidth - relPos.x) ? i - 1 : i;
					break;
				}
			}
			setCursorPos(cursorPos);
		}
		else
		{
			int cursorPos = editText.Length;
			for (int i = 1; i <= editText.Length; ++i)
			{
				float textWidth = m_editText.substringWidth(0, i);
				if (textWidth > relPos.x)
				{
					cursorPos = (relPos.x - m_editText.substringWidth(0, i - 1) < textWidth - relPos.x) ? i - 1 : i;
					break;
				}
			}
			setCursorPos(cursorPos);
		}
	}
	
	private string getCursorText()
	{
		if (editText == null)
			return "";
		return editText.Substring(0, m_cursorPos);
	}
	
	private bool checkRegexMatching(string text) 
	{
		if (regexString != null && regexString.Length > 0)
		{
			if (text != null && text.Length > 0 && !Regex.IsMatch(text, regexString))
			{
#if UNITY_EDITOR
#elif UNITY_IPHONE || UNITY_ANDROID
				if (tsKeyboard != null)
					tsKeyboard.text = text.Substring(0, m_cursorPos);
#endif
				if (onRegexCheckFail != null) {
					kScreen.instance.kTouchController.setFocusedObject(null);
					onRegexCheckFail();
				}
				return false;
			}
		}
		return true;
	}
	
	protected virtual void setCursorText(string text)
	{
		if (editType != kEditType.MULTILINE && text != null) {
			text = text.Replace("\n", "");
		}
		if (regexCheck == kRegexCheck.TYPING && !checkRegexMatching(text)) {
			return;
		}

		string rightText = editText.Substring(m_cursorPos);
		if (editType == kEditType.PASSWORD) {
			string pswdText = "";
			for (int i = 0; i < text.Length - 1; ++i)
				pswdText += '*';
			if (text.Length > 0)
				pswdText += (text.Length > m_cursorPos) ? text[text.Length - 1] : '*';
			for (int i = 0; i < rightText.Length; ++i)
				pswdText += '*';
			m_editText.setText(pswdText);
			m_passwordTime = PASSWORD_HIDE_TIME;
		} else {
			m_editText.setText(text + rightText);
		}
		editText = text + rightText;
		m_cursorPos = text.Length;
		m_hintText.gameObject.SetActive(editText == null || editText.Length == 0);
		if (m_btnDelete != null) {
			m_btnDelete.setState(editText == null || editText.Length == 0 ? kButtonState.DISABLED : kButtonState.RELEASED);
		}
		updateCursorPos();
	}
	
	public int getCursorPos()
	{
		return m_cursorPos;
	}
	
	public void setCursorPos(int pos)
	{
		hidePasswordLetter();
		m_cursorPos = Mathf.Clamp(pos, 0, editText.Length);
		if (m_cursorPos > 0 && System.Char.IsSurrogatePair(editText, m_cursorPos - 1))
			m_cursorPos = m_cursorPos - 1;
		updateCursorPos();
#if UNITY_EDITOR
#elif UNITY_IPHONE || UNITY_ANDROID
		if(tsKeyboard != null)
			tsKeyboard.text = editText.Substring(0, m_cursorPos);
#endif
	}
	
	public string getText()
	{
		return editText;
	}

	public void setText(string text)
	{
		if(text == null)
			return;
		if (!checkRegexMatching(text))
			return;
		editText = text;
		m_cursorPos = text.Length;
		setCursorText(text);
		if (!isFocused()) {
			if (textAlign == kEditAlign.LEFT) {
				m_cursorPos = 0;
				updateCursorPos();
			}
		}
	}
	
	public void setType(kEditType type)
	{
		editType = type;
		setText(editText);
	}

	private void hidePasswordLetter()
	{
		string pswdText = m_editText.getText();
		if (editType == kEditType.PASSWORD && m_cursorPos > 0 && pswdText[m_cursorPos - 1] != '*')
		{
			m_editText.setText(pswdText.Substring(0, m_cursorPos - 1) + '*' + pswdText.Substring(m_cursorPos));
			updateCursorPos();
		}
	}
	
	private void updateCursorPos()
	{
		List<kTextLine> textLines = m_editText.getTextLines();
		if (textLines == null)
			return;
		
		if (editType == kEditType.MULTILINE)
		{
			int cursorLine = 0;
			while (cursorLine < textLines.Count - 1 && textLines[cursorLine].idxEnd < m_cursorPos)
				++cursorLine;
			float lineHeight = (FONT_SIZE * charSize / 10f) * lineSpacing / BASE_LINE_SPACING;
			float dxCursor = m_editText.substringWidth(textLines[cursorLine].idxStart, m_cursorPos);
			float dyCursor = lineHeight * (cursorLine + 1) - 0.2f * lineHeight;
			if (textLines.Count != m_nbLines)
				autoResize(textLines.Count, lineHeight);
			
			if (lineHeight * textLines.Count < m_clipView.viewSize.y - 2 * EDGE_INSET)
			{
				m_editText.transform.localPosition = new Vector3(0, -EDGE_INSET, m_editText.transform.localPosition.z);
				m_cursor.transform.localPosition = new Vector3(dxCursor, m_editText.transform.localPosition.y - dyCursor, m_cursor.transform.localPosition.z);
			}
			else if (-m_editText.transform.localPosition.y + dyCursor >= m_clipView.viewSize.y - EDGE_INSET)
			{
				m_cursor.transform.localPosition = new Vector3(dxCursor, -m_clipView.viewSize.y + EDGE_INSET, m_cursor.transform.localPosition.z);
				m_editText.transform.localPosition = new Vector3(0, m_cursor.transform.localPosition.y + dyCursor, m_editText.transform.localPosition.z);
			}
			else
			{
				if (-m_editText.transform.localPosition.y + lineHeight * textLines.Count < m_clipView.viewSize.y - EDGE_INSET)
					m_editText.transform.localPosition = new Vector3(0, -m_clipView.viewSize.y + EDGE_INSET + lineHeight * textLines.Count, m_editText.transform.localPosition.z);
				if (-m_editText.transform.localPosition.y + dyCursor < EDGE_INSET + 0.8f * lineHeight)
				{
					m_cursor.transform.localPosition = new Vector3(dxCursor, -EDGE_INSET - 0.8f * lineHeight, m_cursor.transform.localPosition.z);
					m_editText.transform.localPosition = new Vector3(0, m_cursor.transform.localPosition.y + dyCursor, m_editText.transform.localPosition.z);
				}
				else
					m_cursor.transform.localPosition = new Vector3(dxCursor, m_editText.transform.localPosition.y - dyCursor, m_cursor.transform.localPosition.z);
			}
		}
		else
		{
			float dxCursor = m_editText.substringWidth(0, m_cursorPos);
			if (m_editText.getBounds().width + m_cursor.getBounds().width < m_clipView.viewSize.x)
			{
				float text_pos = textAlign == kEditAlign.RIGHT ? m_clipView.viewSize.x - m_cursor.getBounds().width - m_editText.substringWidth(0, editText.Length) : 0;
				m_editText.transform.localPosition = new Vector3(text_pos, m_editText.transform.localPosition.y, m_editText.transform.localPosition.z);
				m_cursor.transform.localPosition = new Vector3(text_pos + dxCursor, m_cursor.transform.localPosition.y, m_cursor.transform.localPosition.z);
			}
			else if (m_editText.transform.localPosition.x + dxCursor + m_cursor.getBounds().width >= m_clipView.viewSize.x)
			{	
				m_cursor.transform.localPosition = new Vector3(m_clipView.viewSize.x - m_cursor.getBounds().width, m_cursor.transform.localPosition.y, m_cursor.transform.localPosition.z);
				m_editText.transform.localPosition = new Vector3(m_cursor.transform.localPosition.x - dxCursor, m_editText.transform.localPosition.y, m_editText.transform.localPosition.z);
			}
			else
			{
				if (m_editText.transform.localPosition.x + m_editText.getBounds().width + m_cursor.getBounds().width < m_clipView.viewSize.x)
					m_editText.transform.localPosition = new Vector3(m_clipView.viewSize.x - m_editText.getBounds().width - m_cursor.getBounds().width, m_editText.transform.localPosition.y, m_editText.transform.localPosition.z);
				if (m_editText.transform.localPosition.x + dxCursor < 0)
				{
					m_cursor.transform.localPosition = new Vector3(0, m_cursor.transform.localPosition.y, m_cursor.transform.localPosition.z);
					m_editText.transform.localPosition = new Vector3(-dxCursor, m_editText.transform.localPosition.y, m_editText.transform.localPosition.z);
				}
				else
					m_cursor.transform.localPosition = new Vector3(m_editText.transform.localPosition.x + dxCursor, m_cursor.transform.localPosition.y, m_cursor.transform.localPosition.z);
			}
		}
		
		m_cursor.play(m_cursorAnim.id, PlaybackMode.ANIM_PLAY_LOOP, PlaybackDir.ANIM_PLAY_FW);
	}
	
	private void autoResize(int nbLines, float lineHeight)
	{
		if (nbLines <= resizeMaxLines || m_nbLines <= resizeMaxLines)
		{
			float targetHeight = Mathf.Max((Mathf.Min(nbLines, resizeMaxLines) + 0.7f) * lineHeight, m_baseRect.height);
			transform.localScale = new Vector3(transform.localScale.x, m_baseScale.y * targetHeight / m_baseRect.height, transform.localScale.z);
			m_clipView.transform.localScale = new Vector3(1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f);
			m_clipView.viewSize.y = targetHeight;
			m_clipView.transform.hasChanged = true;
			if (onAutoResize != null)
				onAutoResize(getBounds());
		}
		m_nbLines = nbLines;
	}

	public override Rect getBounds ()
	{
		if (m_bkg != null)
		{
			return m_bkg.getBounds ();
		}
		return base.getBounds();
	}

	public float getKeyboardHeight()
	{
		float scaleFactor = kScreen.instance.getKeyboardScaleFactor();
#if UNITY_EDITOR
		return (isFocused() ? 430 : 0) * scaleFactor;
#else
		return TouchScreenKeyboard.area.height * scaleFactor;
#endif
	}

	public void showKeyboard()
	{
#if UNITY_EDITOR
#elif UNITY_IPHONE
		TouchScreenKeyboardType kbType = (editType == kEditType.EMAIL) ? TouchScreenKeyboardType.EmailAddress : 
										(editType == kEditType.NUMBER) ? TouchScreenKeyboardType.NumbersAndPunctuation :
										(m_emojiiSupport ? TouchScreenKeyboardType.Default : TouchScreenKeyboardType.ASCIICapable);
		TouchScreenKeyboard.hideInput = true;
		if (tsKeyboard != null)
			tsKeyboard.active = false;
		tsKeyboard = TouchScreenKeyboard.Open(getCursorText(), kbType, false, false/*editType == kEditType.MULTILINE*/, editType == kEditType.PASSWORD);
#elif UNITY_ANDROID
		//TouchScreenKeyboard.hideInput = true; // backspace doesn't work with this on
		tsKeyboard = TouchScreenKeyboard.Open(getCursorText(), TouchScreenKeyboardType.Default, false, false, false);
#endif
	}
	
	public void hideKeyboard()
	{
#if UNITY_EDITOR
#elif UNITY_IPHONE || UNITY_ANDROID
			if (tsKeyboard != null)
				tsKeyboard.active = false;
			tsKeyboard = null;
#endif
	}

	protected override void onDestroy(){
		hideKeyboard();
		base.onDestroy();
	}

	protected override void onDisable(){
		//hideKeyboard();
		base.onDisable();
	}
}
