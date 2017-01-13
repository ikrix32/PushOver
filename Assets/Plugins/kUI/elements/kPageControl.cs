using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kPageControl : kObject 
{
	public kSpriteItem m_defaultIcon = new kSpriteItem();
	public kSpriteItem m_selectedIcon = new kSpriteItem();
	public float xOffset = 5f;
	
	public  int m_noPages = 0;
	private int m_selectedPage = 0;
	
	private List<kSpriteObject> m_pages = new List<kSpriteObject>();
	
	protected override void onInit(){
		base.onInit();

		#if UPDATE_SPRITE_FRAME_IDS
		kSpriteObject.validateSpriteItem(m_defaultIcon,sourceSprite,gameObject);
		kSpriteObject.validateSpriteItem(m_selectedIcon,sourceSprite,gameObject);
		#endif
		
		if(Application.isPlaying){
			setNoPages(m_noPages);
		}
	}
	
	protected override void onDestroy(){
		for(int i = 0; i < m_pages.Count;i++){
			if(m_pages[i] != null)
				Destroy(m_pages[i].gameObject);
		}
		base.onDestroy();
	}
	
	private bool recomputePositions = false;
	protected override void onUpdate(){
		if(recomputePositions && m_noPages > 0){
			float pageWidth = m_pages[0].getBounds().width + xOffset;
			float startX = -1 * (m_noPages * pageWidth)/2;
					
			for(int i = 0; i < m_noPages;i++)
				m_pages[i].transform.localPosition = Vector3.right * (startX + i * pageWidth);
			recomputePositions = false;
		}
	}
	
	public void setNoPages(int noPages){
		m_noPages = noPages;
		
		kSpriteObject page = null;
		for(int i = m_noPages;i < m_pages.Count;i++){
			m_pages[i].gameObject.SetActive(false);//just hide
		}
		
		while(m_pages.Count < m_noPages){
			GameObject holder = new GameObject();
			holder.transform.parent = transform;
			page = holder.AddComponent<kSpriteObject>();
			page.name = "dot";
			page.setSourceSprite(this.sprite);
			page.m_defaultAnim= m_defaultIcon;
			m_pages.Add(page);
		}
		
		if(m_noPages > 0){
			for(int i = 0; i < m_noPages;i++){
				page = m_pages[i];
				page.gameObject.SetActive(true);
				page.play(page.m_defaultAnim.id,PlaybackMode.ANIM_PLAY_LOOP,PlaybackDir.ANIM_PLAY_FW);
			}
		}
		setSelectedPage(0);
		recomputePositions = true;
	}
	
	public void setSelectedPage(int index){
		if(m_selectedPage >= 0 && m_selectedPage < m_noPages)
			m_pages[m_selectedPage].play(m_defaultIcon.id,PlaybackMode.ANIM_PLAY_LOOP,PlaybackDir.ANIM_PLAY_FW);
	
		if(index >= 0 && index < m_noPages){
			m_selectedPage = index;
			m_pages[index].play(m_selectedIcon.id,PlaybackMode.ANIM_PLAY_LOOP,PlaybackDir.ANIM_PLAY_FW);
		}
		recomputePositions = true;
	}
}
