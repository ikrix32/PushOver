using UnityEngine;
using System.Collections;

public class kTable : kSpriteObject
{
	public kTableItem[] items;

	public System.Action<kTableItem> onSelectionChanged = null;

	private kTableItem _selectedItem = null;

	protected override void onInit()
	{
		base.onInit();
		if (Application.isPlaying)
		{
			for (int i = 0; i < items.Length; ++i)
				items[i].onSelect = onItemSelected;
		}
	}
	
	private void onItemSelected(kTableItem item)
	{
		if (_selectedItem == item)
			return;
		if (_selectedItem != null)
			_selectedItem.setState(kTableItemState.IDLE);
		if (item != null)
			item.setState(kTableItemState.SELECTED);
		_selectedItem = item;
		if (onSelectionChanged != null)
			onSelectionChanged(_selectedItem);
	}

	public int getSelection()
	{
		for (int i = 0; i < items.Length; ++i)
			if (_selectedItem == items[i])
				return i;
		return -1;
	}
	
	public void setSelection(int index)
	{
		onItemSelected(index >= 0 ? items[index] : null);
	}
}
