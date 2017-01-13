using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[ExecuteInEditMode]
public class kTextMeshOutline : kBehaviourScript{

	public Color 		m_cTextColor;
	public Color		m_cShadowColor;
	public string 		m_sTextField;
	public float		m_fOutlineStrength;
	public float		m_fOutlineBotStrength;
	public int 			m_nCharacterSize;
	public int 			m_nFontSize;
	public float 		m_fLineSpacing;
	public Font			m_Font;

	public kTextMesh m_txtMain;
	public kTextMesh m_txtTopLeft;
	public kTextMesh m_txtTop;
	public kTextMesh m_txtTopRight;
	public kTextMesh m_txtLeft;
	public kTextMesh m_txtRight;
	public kTextMesh m_txtBotLeft;
	public kTextMesh m_txtBot;
	public kTextMesh m_txtBotRight;

	protected override void onInit(){
		base.onInit();
		InitTextFields ();
	}

	#if UNITY_EDITOR
	//just to make sure the properties are properly set
	protected override void onUpdate()
	{
		VerifyConsistency ();
	}

	private void VerifyConsistency()
	{
		if (m_txtMain == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtMain property");

		if (m_txtTopLeft == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtTopLeft property");

		if (m_txtTop == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtTop property");
		
		if (m_txtTopRight == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtTopRight property");

		if (m_txtLeft == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtLeft property");

		if (m_txtRight == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtRight property");

		if (m_txtBotLeft == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtBotLeft property");

		if (m_txtBot == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtBot property");

		if (m_txtBotRight == null)
			Debug.LogError ("[kTextMeshOutline] Error, you must set the txtBotRight property");
	}
	#endif

	private void InitTextFields()
	{
		InitTextField (ref m_txtMain, m_cTextColor, 	new Vector3(0f,0f,-1f));
		InitTextField (ref m_txtTopLeft, m_cShadowColor, 	new Vector3( -m_fOutlineStrength, m_fOutlineStrength, 0f));
		InitTextField (ref m_txtTop, m_cShadowColor, 		new Vector3( 0f, m_fOutlineStrength, 0f));
		InitTextField (ref m_txtTopRight, m_cShadowColor, 	new Vector3( m_fOutlineStrength, m_fOutlineStrength, 0f));
		InitTextField (ref m_txtLeft, m_cShadowColor, 		new Vector3( -m_fOutlineStrength, 0f, 0f));
		InitTextField (ref m_txtRight, m_cShadowColor, 	new Vector3( m_fOutlineStrength, 0f, 0f));
		InitTextField (ref m_txtBotLeft, m_cShadowColor, 	new Vector3( - m_fOutlineStrength, -( m_fOutlineStrength + m_fOutlineBotStrength), 0f));
		InitTextField (ref m_txtBot, m_cShadowColor, 		new Vector3( 0f, -( m_fOutlineStrength + m_fOutlineBotStrength), 0f));
		InitTextField (ref m_txtBotRight, m_cShadowColor,  new Vector3( m_fOutlineStrength, -( m_fOutlineStrength + m_fOutlineBotStrength), 0f));
	}

	//Initialize the text field
	//usage: for the main text pass the text color property
	//		 for the shadow texts pass black 
	private void InitTextField(ref kTextMesh textMesh, Color color, Vector3 pos )
	{
		textMesh.gameObject.SetActive (true);	
		textMesh.setText (m_sTextField);
		textMesh.setup (m_Font, m_nFontSize, m_nCharacterSize, m_fLineSpacing); 
		textMesh.setColor (color);
		textMesh.transform.localPosition = pos;
	}

	//set the color for the main text
	public void setColor(Color color)
	{
		m_cTextColor = color;
		m_txtMain.setColor (color);
	}

	public void setShadowColor(Color color)
	{
		m_cShadowColor = color;
		m_txtTopLeft.setColor( m_cShadowColor );
		m_txtTop.setColor( m_cShadowColor );
		m_txtTopRight.setColor( m_cShadowColor );
		m_txtLeft.setColor( m_cShadowColor );
		m_txtRight.setColor( m_cShadowColor );
		m_txtBotLeft.setColor( m_cShadowColor );
		m_txtBot.setColor( m_cShadowColor );
		m_txtBotRight.setColor( m_cShadowColor );
	}

	public void setText(string text)
	{
		m_sTextField = text;
		m_txtMain.setText (m_sTextField);
		m_txtTopLeft.setText (m_sTextField);
		m_txtTop.setText (m_sTextField);
		m_txtTopRight.setText (m_sTextField);
		m_txtLeft.setText (m_sTextField);
		m_txtRight.setText (m_sTextField);
		m_txtBotLeft.setText (m_sTextField);
		m_txtBot.setText (m_sTextField);
		m_txtBotRight.setText (m_sTextField);
	}
	
	public void playSound(AudioClip clip, float volume = -1f, bool loop = false)
	{
		AudioSource audioSource = GetComponent<AudioSource>();
		if ( audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		if (loop) {
			audioSource.loop = true;
			audioSource.volume = volume >= 0 ? volume : kScreen.FX_VOLUME;
			audioSource.clip = clip;
			audioSource.Play();
		} else {
			audioSource.PlayOneShot(clip, volume >= 0 ? volume : kScreen.FX_VOLUME);
		}
	}
	
	public void stopSound()
	{
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource != null)
			audioSource.Stop();
	}

}
