using UnityEngine;
using System.Collections;

public class kSmoothCounter 
{
	public System.Action<float> m_onChange;

	//Percent should be between 0 and 1
	float currentPercent = 0;
	public void SetStartPercent(float percent){
		currentPercent = percent;
	}
	private bool  m_running = false;
	private float m_target = 0;

	public IEnumerator CountTo(float toPercent,float incrementStep,float incrementPeriod = 0,System.Action onComplete = null)
	{
		m_target = toPercent;
		if(!m_running)
		{
			m_running = true;
			while (m_running && currentPercent + incrementStep <=  m_target) 
			{
				currentPercent += incrementStep;
				m_onChange(currentPercent);

				if(incrementPeriod == 0)
					yield return null;
				else if(currentPercent < m_target)
					yield return new WaitForSeconds(incrementPeriod);
			}
			if(currentPercent != m_target)
				m_onChange(m_target);

			m_running = false;
		}
		if ( onComplete != null )
			onComplete();
	}

	public bool IsCounting(){
		return m_running;
	}
}
