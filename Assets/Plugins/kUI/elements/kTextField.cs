using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;

public abstract class kTextField: kPickerItem
{
	static string[] MARKUP_TAG_REGEX = new string[]{"<color=[#0-9A-Za-z]+>", "<size=[0-9]+>", "<i>", "<b>", "<quad[ ]*[0-9a-z/=. ]*>"};
	static string[] MARKUP_TAG_END = new string[]{"</color>", "</size>", "</i>", "</b>"};
	
	protected class kMarkupTag
	{
		public string tagStart, tagEnd;
		public int idxStart, idxEnd;
		
		public kMarkupTag(string tagStart, int idxStart, string tagEnd = null, int idxEnd = 0)
		{
			this.tagStart = tagStart;
			this.idxStart = idxStart;
			this.tagEnd = tagEnd;
			this.idxEnd = idxEnd;
		}
		
		public bool isQuad()
		{
			return tagStart.StartsWith("<quad");
		}
	}
	protected const int MIN_WRAP_WIDTH = 50;

	[HideInInspector]
	public string 	m_text;
	public int 		m_textWarpWidth = 0;
	public int 		m_linesNumber = 0;
	
	protected List<kTextLine> 	m_textLines = null;
	protected List<kMarkupTag> 	m_markupTags = new List<kMarkupTag>();

	protected float m_spaceWidth = 0;

	public virtual string getText(){
		return m_text;
	}

	public virtual void setText(string txt){
		m_text = txt;
	}

	public virtual void setWarpWidth(int width){
		m_textWarpWidth = width;
	}

	public virtual int getWarpWidth(){
		return m_textWarpWidth;
	}

	public abstract TextAnchor getAnchor();
	public abstract void setAnchor(TextAnchor anchor);

	public abstract TextAlignment getAlignment();
	public abstract void setAlignment(TextAlignment align);

	//public abstract int getFontSize();
	//public abstract void setFontSize(int size);

	public abstract FontStyle getFontStyle();
	public abstract void setFontStyle(FontStyle style);

	public abstract void setColor(Color color);
	public abstract Color getColor();

//	public abstract void setAlpha(float alpha);

	public abstract float stringWidth(string s);

	private static Regex[] reg = null;

	public List<kTextLine> getTextLines()
	{
		return m_textLines;
	}

	private kMarkupTag getNextTagOpen(string txt, int idxStart)
	{
		if(reg == null){
			reg = new Regex[MARKUP_TAG_REGEX.Length];
			for (int i = 0; i < MARKUP_TAG_REGEX.Length; ++i)
				reg[i] = new Regex(MARKUP_TAG_REGEX[i]);
		}

		kMarkupTag result = new kMarkupTag(null, txt.Length);
		for (int i = 0; i < MARKUP_TAG_REGEX.Length; ++i)
		{
			Match match = reg[i].Match(txt, idxStart);
			if (match.Success && match.Index < result.idxStart)
			{
				result.tagStart = match.Value;
				result.idxStart = match.Index;
			}
		}
		return result.tagStart != null ? result : null;
	}
	
	private kMarkupTag getNextTagClose(string txt, int idxStart)
	{
		kMarkupTag result = new kMarkupTag(null, 0, null, txt.Length);
		for (int i = 0; i < MARKUP_TAG_END.Length; ++i)
		{
			int idx = txt.IndexOf(MARKUP_TAG_END[i], idxStart);
			if (idx >= 0 && idx < result.idxEnd)
			{
				result.tagEnd = MARKUP_TAG_END[i];
				result.idxEnd = idx;
			}
		}
		return result.tagEnd != null ? result : null;
	}
	
	private bool tagMatch(string tagStart, string tagEnd)
	{
		for (int i = 2; i < tagEnd.Length - 1; ++i)
			if (tagStart[i - 1] != tagEnd[i])
				return false;
		return true;
	}
	
	protected bool parseText(string txt)
	{
		m_markupTags = new List<kMarkupTag>();
		
		int idxStart = 0;
		kMarkupTag tagCurrent = null, tagNext = getNextTagOpen(txt, idxStart);
		while (tagCurrent != null || tagNext != null)
		{
			if (tagNext != null && tagNext.isQuad()) // don't search for close tag, close it now
			{
				tagNext.tagEnd = "";
				tagNext.idxEnd = tagNext.idxStart + tagNext.tagStart.Length;
				m_markupTags.Add(tagNext);
				idxStart = tagNext.idxEnd;
				tagNext = getNextTagOpen(txt, idxStart);
				continue;
			}
			if (tagCurrent != null)
			{
				kMarkupTag tagClose = getNextTagClose(txt, idxStart);
				if (tagClose == null || ((tagNext == null || tagClose.idxEnd < tagNext.idxStart) && !tagMatch(tagCurrent.tagStart, tagClose.tagEnd)))
				{
					// parse error
					m_markupTags.Clear();
					return false;
				}
				if (tagNext == null || tagClose.idxEnd < tagNext.idxStart)
				{
					// close current tag
					tagCurrent.tagEnd = tagClose.tagEnd;
					tagCurrent.idxEnd = tagClose.idxEnd;
					idxStart = tagCurrent.idxEnd + tagCurrent.tagEnd.Length;
					tagCurrent = null;
					for (int i = m_markupTags.Count - 1; i >= 0; --i)
						if (m_markupTags[i].tagEnd == null)
						{
							tagCurrent = m_markupTags[i];
							break;
						}
				}
				else
					tagCurrent = null;
			}
			else
			{
				// open new tag
				tagCurrent = tagNext;
				m_markupTags.Add(tagCurrent);
				idxStart = tagCurrent.idxStart + tagCurrent.tagStart.Length;
				tagNext = getNextTagOpen(txt, idxStart);
			}
		}
		return m_markupTags.Count > 0;
	}
	
	protected bool hasEmojiChars(string txt)
	{
		for (int i = 0; i < txt.Length; ++i)
			if (System.Char.IsSurrogatePair(txt, i))
				return true;
		return false;
	}
	
	private string buildWrapResult(string txt, List<kTextLine> lines, bool truncated = false)
	{
		string result = "";
		for (int i = 0; i < lines.Count; ++i)
		{
			if (truncated && i == lines.Count - 1)
			{
				result += txt.Substring(lines[i].idxStart, lines[i].idxEnd - lines[i].idxStart - 1) + "...";
				
				int tagIndex = 0;
				while (tagIndex < m_markupTags.Count && m_markupTags[tagIndex].idxStart < lines[i].idxEnd)
					++tagIndex;
				while (--tagIndex >= 0)
					if (m_markupTags[tagIndex].idxStart < lines[i].idxEnd && m_markupTags[tagIndex].idxEnd >= lines[i].idxEnd)
						result += m_markupTags[tagIndex].tagEnd;
			}
			else
			{
				result += txt.Substring(lines[i].idxStart, lines[i].idxEnd - lines[i].idxStart);
				if (i < lines.Count - 1)
					result += "\n";
			}
		}
		return result;
	}
	
	private int getNextSpaceIndex(string txt, int startIndex)
	{
		int idx = txt.IndexOf(' ', startIndex);
		int tagIdx = 0;
		while (idx >= 0)
		{
			while (tagIdx < m_markupTags.Count && m_markupTags[tagIdx].idxStart + m_markupTags[tagIdx].tagStart.Length <= idx)
				++tagIdx;
			if (tagIdx >= m_markupTags.Count || m_markupTags[tagIdx].idxStart > idx)
				return idx;
			idx = txt.IndexOf(' ', idx + 1);
		}
		return idx;
	}
	
	private int getNextCutIndex(string txt, int startIndex)
	{
		if (startIndex > 0 && startIndex < txt.Length && System.Char.IsSurrogatePair(txt, startIndex - 1))
			startIndex = startIndex + 1;
		for (int i = 0; i < m_markupTags.Count && m_markupTags[i].idxStart < startIndex; ++i)
			if (startIndex < m_markupTags[i].idxStart + m_markupTags[i].tagStart.Length)
				return m_markupTags[i].idxStart + m_markupTags[i].tagStart.Length;
		return startIndex;
	}
	
	protected string wrapText(string txt, int wrapWidth)
	{
		m_textLines = new List<kTextLine>();
		kTextLine line = new kTextLine();
		m_textLines.Add(line);
		int wordStartIndex = 0;
		while (wordStartIndex < txt.Length)
		{
			int wordEndIndex = getNextSpaceIndex(txt, wordStartIndex);
			if (wordEndIndex < 0)
				wordEndIndex = txt.Length;
			int lineEndIndex = txt.IndexOf("\n", wordStartIndex, wordEndIndex - wordStartIndex);
			if (lineEndIndex >= 0)
				wordEndIndex = lineEndIndex;
			float wordWidth = substringWidth(txt, wordStartIndex, wordEndIndex);
			if (wrapWidth > 0 && line.width + wordWidth > wrapWidth)
			{
				if (wordWidth > wrapWidth || m_textLines.Count == m_linesNumber)
				{
					while (wordWidth > wrapWidth || m_textLines.Count == m_linesNumber)
					{
						int cutIndex = wordStartIndex;
						int nextCutIndex = getNextCutIndex(txt, cutIndex + 1);
						while (cutIndex < txt.Length && line.width + substringWidth(txt, wordStartIndex, nextCutIndex) <= wrapWidth)
						{
							cutIndex = nextCutIndex;
							nextCutIndex = getNextCutIndex(txt, cutIndex + 1);
						}
						if (cutIndex == wordStartIndex && txt[line.idxEnd] == ' ') // remove last space
							line.width -= m_spaceWidth;
						line.idxEnd = cutIndex;
						line.width += substringWidth(txt, wordStartIndex, cutIndex);
						if (m_textLines.Count == m_linesNumber)
							return buildWrapResult(txt, m_textLines, true);
						line = new kTextLine();
						line.idxStart = cutIndex;
						wordStartIndex = cutIndex;
						wordWidth = substringWidth(txt, wordStartIndex, wordEndIndex);
						m_textLines.Add(line);
					}
				}
				else
				{
					if (txt[line.idxEnd] == ' ') // remove last space
						line.width -= m_spaceWidth;
					line = new kTextLine();
					line.idxStart = wordStartIndex;
					m_textLines.Add(line);
				}
			}
			line.idxEnd = wordEndIndex;
			line.width += wordWidth;
			if (wordEndIndex < txt.Length && txt[wordEndIndex] == ' ') // add space
				line.width += substringWidth(txt, wordEndIndex, wordEndIndex + 1);
			wordStartIndex = wordEndIndex + 1;
			if (wordEndIndex == lineEndIndex)
			{
				if (m_textLines.Count == m_linesNumber)
					return buildWrapResult(txt, m_textLines);
				line = new kTextLine();
				line.idxStart = wordStartIndex;
				line.idxEnd = wordStartIndex;
				m_textLines.Add(line);
			}
		}
		return buildWrapResult(txt, m_textLines);
	}

	public float substringWidth(int idxStart, int idxEnd)
	{
		return substringWidth(m_text, idxStart, idxEnd);
	}

	public float substringWidth(string txt, int idxStart, int idxEnd)
	{
		int tagIndex = 0;
		StringBuilder builder = new StringBuilder();
		while (tagIndex < m_markupTags.Count && m_markupTags[tagIndex].idxStart < idxEnd)
		{
			kMarkupTag tag = m_markupTags[tagIndex];
			if (tag.idxStart < idxStart)
			{
				if (idxStart < tag.idxStart + tag.tagStart.Length)
					idxStart = tag.idxStart + tag.tagStart.Length;
				else if (tag.idxEnd < idxStart && idxStart < tag.idxEnd + tag.tagEnd.Length)
					idxStart = tag.idxEnd + tag.tagEnd.Length;
				if (tag.idxEnd >= idxStart)
					builder.Append(tag.tagStart);
			}
			else
			{
				if (idxEnd < tag.idxStart + tag.tagStart.Length)
					idxEnd = tag.idxStart;
				else if (tag.idxEnd < idxEnd && idxEnd < tag.idxEnd + tag.tagEnd.Length)
					idxEnd = tag.idxEnd;
			}
			++tagIndex;
		}
		if (idxStart >= idxEnd)
			return 0;
		builder.Append(txt, idxStart, idxEnd - idxStart);
		while (--tagIndex >= 0)
		{
			kMarkupTag tag = m_markupTags[tagIndex];
			if (tag.idxStart < idxEnd && tag.idxEnd >= idxEnd)
				builder.Append(tag.tagEnd);
		}
		
		// stupid TextMesh ignores end spaces
		/*float width = stringWidth(builder.ToString());
		while (idxEnd > idxStart)
		{
			if (txt[idxEnd - 1] == ' ')
			{
				width += m_spaceWidth;
				--idxEnd;
			}
			else
			{
				int endTagLength = trimEndTag(idxEnd);
				if (endTagLength > 0)
					idxEnd -= endTagLength;
				else
					break;
			}
		}
		return width;*/

		// surprise: TextMesh fixed in Unity 5
		return stringWidth(builder.ToString());
	}

	private int trimEndTag(int pos)
	{
		for (int i = 0; i < m_markupTags.Count && m_markupTags[i].idxStart < pos; ++i)
		{
			if (m_markupTags[i].idxStart + m_markupTags[i].tagStart.Length == pos)
				return m_markupTags[i].tagStart.Length;
			if (m_markupTags[i].idxEnd + m_markupTags[i].tagEnd.Length == pos)
				return m_markupTags[i].tagEnd.Length;
		}
		return 0;
	}
}
