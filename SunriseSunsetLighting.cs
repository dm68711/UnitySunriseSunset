using UnityEngine;
using System.Collections;

public class SunriseSunsetLighting : MonoBehaviour {

	private Light m_light        = null;
	private float m_sunriseStart = 7.0f;  //  7:00
	private float m_sunriseEnd   = 8.0f;  //  7:30
	private float m_sunsetStart  = 20.0f; // 20:00
	private float m_sunsetEnd    = 21.0f; // 20:30

	private float m_dayIntensity   = 1.0f;
	private float m_nightIntensity = 0.0f; // ambient light can be moonshine

	public void Awake() {
		Debug.Log("SunriseSunsetLighting::Awake - script loading");
	}

	//
	// Use this for initialization
	//

	public void Start () {
		Debug.Log ("starting SunriseSunsetLighting MonoBehavior");
		m_light = GetLight ();
		if(m_light) {
			Debug.Log ("SunriseSunsetLighting::Start - launching SunriseSunsetLoop coroutine");
			StartCoroutine( SunriseSunsetLoop() ); 
		}
		else {
			Debug.Log ("SunriseSunsetLighting::Start - failed to find light");
		}
	}

	//
	// Update is called once per frame
	//

	public void Update () {

		// For simplicity, we could simply set the light intensity according to the time of day in
		// this Update() routine. However, that unnecessarily adjusts the light's intensity when
		// that intensity is unchanging (during mid-day and mid-night). Using a coroutine solves
		// that by wait for hours at a time until the next major lighting event ... that is, 
		// the start of sunrise or sunset.

		//if(m_light) {
		//	float tod = TimeOfDay ();
		//	m_light.intensity = GetLightIntensity(tod);
		//}
	}

	//
	// calculate light intensity as a function of time of day
	//

	private float GetLightIntensity(float tod) {
		if(tod < m_sunriseStart) {
			return m_nightIntensity;
		}
		else if(tod >= m_sunriseStart && tod <= m_sunriseEnd ) {
			return Interpolate(m_nightIntensity, m_dayIntensity, m_sunriseStart, m_sunriseEnd, tod);
		}
		else if(tod > m_sunriseEnd && tod < m_sunsetStart) {
			return m_dayIntensity;
		}
		else if(tod >= m_sunsetStart && tod <= m_sunsetEnd ) {
			return Interpolate(m_dayIntensity, m_nightIntensity, m_sunsetStart, m_sunsetEnd, tod);
		}
		else if(tod > m_sunsetStart) {
			return m_nightIntensity;
		}

		return m_dayIntensity; // shouldn't get here
	}

	//
	// interpolate a value between start and end values based on elapsed time
	//

	private float Interpolate(float startValue, float endValue, float startTime, float endTime, float t) {
		if(t < startTime) return startValue;
		if(t > endTime)   return endTime;
		float fractionOfTimeInterval = (t-startTime)/(endTime-startTime);
		float valueRange = endValue - startValue;
		return startValue + valueRange*fractionOfTimeInterval;
	}

	//
	// This function scales game time to time of day. Sunrise and sunset times assume a 24 hour day.
	//

	private float GameTimeToTimeOfDay() {
		return Time.time % 24.0f; // 1 second equivalent to 1 hour
	}

	//
	// convert a time of day to game time
	//

	private float TimeOfDayToGameTime(float tod) {
		float numDaysElapsed = Mathf.Floor(Time.time / 24.0f);
		float dayStartGameTime = numDaysElapsed * 24.0f;
		Debug.Log ("TimeOfDayToGameTime - day start time is " + dayStartGameTime + " for game time " + Time.time);
		float gameTime = dayStartGameTime + tod;
		Debug.Log ("TimeOfDayToGameTime(" + tod + ") returns " + gameTime);
		return gameTime;
	}

	//
	// get the light that we want to adjust from the scene
	//

	private Light GetLight() {
		// TODO: retrieve light by tag using GameObject.FindWithTag
		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		if(lights.Length < 1) {
			return null;
		}
		return lights[0];
	}

	//
	// coroutine that loops over daily sunrise and sunset events and sleeps/waits in-between
	//

	private IEnumerator SunriseSunsetLoop() {
		// assume we are called at game time 0, which is midnight

		while(true) {
			// during the night, wait for sunrise to start
			m_light.intensity = m_nightIntensity;
			float waitTime = TimeOfDayToGameTime(m_sunriseStart) - Time.time;
			if(waitTime > 0.0f) {
				yield return new WaitForSeconds(waitTime);
			}

			// adjust the light intensity during sunrise
			while(GameTimeToTimeOfDay() < m_sunriseEnd) {
				m_light.intensity = GetLightIntensity (GameTimeToTimeOfDay ());
				yield return new WaitForSeconds( 0.1f );
			}

			// during the day, wait until sunset
			m_light.intensity = m_dayIntensity;
			waitTime = TimeOfDayToGameTime (m_sunsetStart) - Time.time;
			if(waitTime > 0.0f) {
				yield return new WaitForSeconds(waitTime);
			}

			// adjust the light intensity during sunset
			while(GameTimeToTimeOfDay() < m_sunsetEnd) {
				m_light.intensity = GetLightIntensity (GameTimeToTimeOfDay ());
				yield return new WaitForSeconds( 0.1f );
			}

			// during the night, wait until midnight, then return to the top of the loop
			m_light.intensity = m_nightIntensity;
			waitTime = TimeOfDayToGameTime (24.0f) - Time.time;
			if(waitTime > 0.0f) {
				yield return new WaitForSeconds(waitTime);
			}
		}
	}
}
