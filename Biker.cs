using UnityEngine;
using System.Collections;

public class Biker : MonoBehaviour
{
  public static Transform bTrans;
	public static bool live = true;
	public static float moveSpeed;
	public GameObject gcTestObj;
//	public GCTestObject _GCTestObject;
	public Vector3 bikerPosition;

	GameObject pGained,sandDirt, SMokes, lEObj, rEObj, breakPlate, finishLine;
	GameObject endMenu;
	ParticleAnimator lpAni, rpAni;
	Transform SteerPivot, WheelingPivot, ShadowPivot;
	
	
	void Awake()	
	{
		InitReference();
	}
	
	void InitReference()
	{
		live = true;
		bTrans = transform;
		breakPlate = transform.FindChild("LeaningPivot/WheelingPivot/backLight").gameObject;
		SteerPivot = transform.FindChild("LeaningPivot");
		WheelingPivot = transform.FindChild("LeaningPivot/WheelingPivot");
		ShadowPivot = transform.FindChild("ShadowPivot");
		sandDirt = transform.FindChild("LeaningPivot/SandDirt").gameObject;
		SMokes = GameObject.Find("Smokes");
		lEObj = SMokes.transform.FindChild("LSide").gameObject;
		rEObj = SMokes.transform.FindChild("RSide").gameObject;
		lpAni = lEObj.GetComponent<ParticleAnimator>();
		rpAni = rEObj.GetComponent<ParticleAnimator>();
		pGained = Resources.Load("Prefabs/Game/PointsGained") as GameObject;
		finishLine = Resources.Load("Prefabs/Game/FinishLine") as GameObject;
		endMenu = Resources.Load("Prefabs/Game/EndMenu") as GameObject;
		
		//enabled = false;
	}
	
	void Start()
	{
		WhiteSmoke();
		SendMessage("StartAudio");
		if(Application.loadedLevel == 2)
		{
			gcTestObj = GameObject.Find("GCTestObject");
//			_GCTestObject = gcTestObj.GetComponent<GCTestObject>();
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Points")	{
			GameObject temp = Instantiate(pGained) as GameObject;
			temp.transform.parent = other.transform;
			temp.SendMessage("ShowResult", other.name);
			Destroy(temp, 1.5f);
			other.tag = "Untagged";
			
		} else if(other.tag == "EnvInit")	{
			Vector3 newPos = new Vector3(0,0, other.transform.parent.position.z + 249.72f);
			Instantiate(Game.stEnvi, newPos, Quaternion.identity);
			if((Preference.DistanceTravelled / 20) > Preference.MaxDistance )	{//330.0f)	{//Init Finish Line Here....
				newPos.z -= 30f;
				Instantiate(finishLine, newPos, Quaternion.identity);
			}
		} else if(other.tag == "EnvClr")	{
			Destroy(other.transform.parent.gameObject);
			//Resources.UnloadUnusedAssets();
			
		} else if(other.tag == "FinishLine")	{
			if(Application.loadedLevel == 1)
			{
				live = false;
				Game.Obj.SendMessage("GameEnd");
				Invoke("ShowEndMenu", 4f);
			}
			if(Application.loadedLevel == 2)
			{
				if(!InitGCPlayer.gameOverbool)
				{
					InitGCPlayer.gameOverbool = true;
					GameObject.Find("SpawnPoint").SendMessage("Win");
				}
				StartCoroutine("RemoveBiker");
				Game.Obj.SendMessage("GameEnd");
			}
		}
	}
	
	IEnumerator RemoveBiker()
	{
		yield return new WaitForSeconds(2);
		Destroy(gameObject.GetComponent<Biker>());
	}
	
//	IEnumerator timedelay()
//	{
//		yield return new WaitForSeconds(13);
//		traffic.onfall=0;
//	}
	
	void OnCollisionEnter (Collision other)
	{
		if(live)	{
//			if(Application.loadedLevel == 2)
//			{
//				StartCoroutine("timedelay");
//				traffic.onfall=1;
//			}
			float hitDiff = (other.transform.position.x - transform.position.x);
			string resAni = (hitDiff > 0 ) ? "FallLeft" : "Fall";
			Fall(resAni);
			
			if(SystemInfo.deviceModel == "iPhone")	{
//				iPhoneUtils.Vibrate();
			}
			Invoke("ReSpwan", 3f);
		}
	}
	void Fall(string resAni)	
	{
		animation.Play(resAni);
		Game.CameraObj.animation.CrossFade(resAni);

		live = false;
		moveFinalSpeed = 0;
		moveSmoothSync = 0.059f;
		transform.localEulerAngles = new Vector3 (bTrans.localEulerAngles.x, 0, bTrans.localEulerAngles.z);	//Steering
		WheelingPivot.localEulerAngles = new Vector3 (0, WheelingPivot.localEulerAngles.y, WheelingPivot.localEulerAngles.z);
		SteerPivot.localEulerAngles = new Vector3 (SteerPivot.localEulerAngles.x, SteerPivot.localEulerAngles.y, 0);	//Leaning...
		
		rEObj.particleEmitter.emit = lEObj.particleEmitter.emit = false;
		sandDirt.particleEmitter.emit = false;
		Utility.SetVisibility(breakPlate, false);
		Utility.SetVisibility(ShadowPivot.gameObject, false);
		SendMessage("StopAudio");
		Preference.KilometerPerHour = 0;
	}
	
	void ReSpwan()	{
		//traffic.onfall=0;
		animation.Play("Start");
		SendMessage("StartAudio");
		WhiteSmoke();

		float xPos =  bTrans.position.x > 0 ? 2f : -2f;
		bTrans.position = new Vector3(xPos, 0, bTrans.position.z - 2f);
		Game.CameraObj.transform.localPosition= Vector3.zero;
		Game.CameraObj.transform.localEulerAngles = Vector3.zero;
		rEObj.particleEmitter.emit = lEObj.particleEmitter.emit = true;
		Utility.SetVisibility(breakPlate, true);
		Utility.SetVisibility(ShadowPivot.gameObject, true);

		moveFinalSpeed = 30f;
		moveSmoothSync = 2f;
		live = true;
		return;
	}
	
	void ShowEndMenu()
	{
		Preference.GameState = GameStates.Won;
		SendMessage("StopAudio");
		Instantiate(endMenu);
	}
	float hori = 0, vert = 0;
	void GetInput ()
	{
		float deadZone = 0.15f;
		//#if UNITY_EDITOR
		hori = Input.GetAxis ("Horizontal");
		//For Leaning...
		vert = Input.GetAxis ("Vertical");
		//For rSkiding...
		///#elif UNITY_IPHONE
//		Vector3 acceleration = Vector3.zero;
//		foreach (AccelerationEvent accEvent in Input.accelerationEvents) {
//			acceleration += accEvent.acceleration * accEvent.deltaTime;
//		}

		float xAccLimit = 0.4f, xDesLimit = 1.0f;
		float xAcc = Mathf.Floor (Input.acceleration.x * 10f) / 10f;
		hori = Preference.MapK (-xAccLimit, xAccLimit, xAcc, -xDesLimit, xDesLimit);
		
		float yAcc = Mathf.Floor (Input.acceleration.y * 10f) / 10f;
		yAcc = Mathf.Clamp (yAcc, -0.9f, -0.1f);
		vert = Preference.MapK (-0.9f, -0.1f, yAcc, 0.0f, 1.0f);
	//	#endif
		hori = Mathf.Clamp ((Mathf.Abs (hori) > deadZone ? hori : 0.0f), -1.0f, 1.0f);
		vert = Mathf.Clamp01 (Mathf.Abs (vert) > deadZone ? vert : 0.0f);
		
	}
	void Update ()
	{
				
		if(Preference.GameState == GameStates.Running )	
		{
			GetInput ();
			Accelerate ();
		}
	}

	void Accelerate ()
	{
		MoveBike();
	}
		
		if( live)	
		{
			SteerBike ();
			SlideBike ();
			WheelBike ();
			BreakBike();
			PropelBike();
		}
	}

	float preKPH = 0, holdBreakCnt = 0, holdBreak = 1f;
	float colorTarget = .3f, colorSmoothSync = 0.25f, colorVelocity = 0.0f;
	void BreakBike()	{
		float curKPH = Mathf.Clamp(Preference.KilometerPerHour, 0, 1000);
		float diff = preKPH - curKPH;

		if(diff > 0.1f && holdBreakCnt < holdBreak)		{
			colorTarget = 1.0f;
			holdBreakCnt += holdBreak;
		} else if( diff < 0.1f && holdBreakCnt > 0)		{
			colorTarget = .3f;
			holdBreakCnt = 0;
		}
		
		float col = breakPlate.renderer.material.GetColor("_TintColor").r;
		col = Mathf.SmoothDamp(col, colorTarget, ref colorVelocity, colorSmoothSync);
		breakPlate.renderer.material.SetColor("_TintColor", new Color(col, 0, 0));
		holdBreakCnt =  Mathf.Clamp01(holdBreakCnt - Time.deltaTime);
		preKPH = curKPH;
	}
	bool colorS = false, whiteS= true;
	void ColorSmoke()	{
		//traffic.onfall=0;
		Color[] modifiedColors = rpAni.colorAnimation;
		modifiedColors[0] = new Color(156f/256f, 19f/256f, 1, 173f/256f);
		modifiedColors[1] = new Color(153f/256f, 0, 228f/256f, 1f);
		modifiedColors[2] = new Color(234f/256f, 213f/256f, 9f/256f, 110f/256f);
		modifiedColors[3]= new Color(216f/256f, 0, 1f, 166f/256f);
		modifiedColors[4] =  new Color(8f/256f, 6f/256f, 1f, 64f/256f);
		lpAni.colorAnimation = modifiedColors;
		rpAni.colorAnimation = modifiedColors;
		Color mCol = new Color(140f/256f, 129f/256f, 251f/256f, 88f/256f);
		lpAni.renderer.material.color = mCol;
		rpAni.renderer.material.color = mCol;
		colorS = true;
		whiteS = false;
	}
	void WhiteSmoke()	{
		Color[] modifiedColors = rpAni.colorAnimation;
		float cVal = 131f/256f;		modifiedColors[0] = new Color(cVal, cVal, cVal, 53f/256f);
		cVal = 124f/256f;		modifiedColors[1] = new Color(cVal, cVal, cVal, 1f);
		cVal = 82f/256f;		modifiedColors[2] = new Color(cVal, cVal, cVal, 110f/256f);
		cVal = 1f;		modifiedColors[3] = new Color(cVal, cVal, cVal, 125f/256f);
		cVal = 179f/256f;		modifiedColors[4] = new Color(cVal, cVal, cVal, 64f/256f);
		lpAni.colorAnimation = modifiedColors;
		rpAni.colorAnimation = modifiedColors;
		cVal = 82f/256f;
		Color mCol = new Color(cVal, cVal, cVal, cVal);
		lpAni.renderer.material.color = mCol;
		rpAni.renderer.material.color = mCol;
		whiteS= true;
		colorS = false;
	}
	void PropelBike()	
	{
		if(colorS && !Input.GetMouseButton(0))
			WhiteSmoke();
		
		if(whiteS && Input.GetMouseButton(0))
			ColorSmoke();

		sandDirt.particleEmitter.emit = Mathf.Abs(bTrans.position.x) > 2.25f ? true : false;
	}
	
	
	
	public int count = 301;
	
	
	public float /*moveSpeed = 0,*/ moveNormalSpeed = 5.3f, moveTargetSpeed = 0, moveFinalSpeed = 30.0f, moveSmoothSync = 1.0f, moveVelocity = 0.0f;
	
//	IEnumerator MoveBike ()
	void MoveBike ()
	{
		moveTargetSpeed = live? ( Mathf.Abs(bTrans.position.x) < 2.2f ? (Input.GetMouseButton (0) ? 45.0f : (moveNormalSpeed + (vert * moveFinalSpeed)) ) : 5.3f) : 0;	// Raise Speed Too...
		moveSpeed = Mathf.SmoothDamp (moveSpeed, moveTargetSpeed, ref moveVelocity, moveSmoothSync);	// Damping Target...
		moveSpeed = (float)Mathf.FloorToInt(moveSpeed * 100) / 100f;
		float offset = moveSpeed * Time.deltaTime;
		transform.Translate (0, 0, offset);	// Translation...
		
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp (pos.x, -4.0f, 4.0f);	
		transform.position = pos;			// Bounding 
		
		Preference.DistanceTravelled += offset;
		Preference.KilometerPerHour = Mathf.Clamp(Preference.MapK(0, 45,moveSpeed , 0, 240), 0, 240);	// Global Update
		

	}

	float steerMin = 8.0f, steerMax = 18.0f, steerWheeling = 18.0f;
	float steerRotation = 0, steerTargetRotation = 0, steerFinalRotation = 11.0f, steerSmoothSync = 0.3f /*.8f*/, steerVelocity = 0.0f;
	void SteerBike ()
	{
		steerFinalRotation = Preference.MapK(5.3f, 45, moveSpeed, steerMin, steerMax);
		steerFinalRotation = moveSpeed < 40.0f ? (steerMax - steerFinalRotation) + steerMin : steerWheeling;
		steerSmoothSync = moveSpeed < 40.0f ? .45f : .35f;
		steerTargetRotation = ((bTrans.position.x == 4.0f && hori > 0) || (bTrans.position.x == -4.0f && hori < 0)) ? 0 : hori * steerFinalRotation;
		steerTargetRotation = Mathf.FloorToInt(steerTargetRotation * 100) / 100f;
		
		steerRotation = Mathf.SmoothDamp (steerRotation, steerTargetRotation, ref steerVelocity, steerSmoothSync);
		//transform.localEulerAngles = new Vector3 (bTrans.localEulerAngles.x, steerRotation, bTrans.localEulerAngles.z);	//Steering
		steerRotation = Mathf.FloorToInt(steerRotation * 10f) / 10f;
		transform.localEulerAngles = new Vector3 (bTrans.localEulerAngles.x, steerRotation, bTrans.localEulerAngles.z);	//Steering

	}

	float slideAngle = 0, slideTargetAngle = 0, slideFinalAngle = 15.0f, slideSmoothSync = 0.4f, slideVelocity = 0.0f;
	void SlideBike ()
	{
		float raiseLean = vert * slideFinalAngle;
		
		slideTargetAngle = Mathf.Abs (transform.position.x) < 3.0f && live ? (hori * -(slideFinalAngle + raiseLean)) : 0;
		slideAngle = Mathf.SmoothDamp (slideAngle, slideTargetAngle, ref slideVelocity, slideSmoothSync);
		slideAngle = Mathf.FloorToInt(slideAngle * 100) / 100f;

		SteerPivot.localEulerAngles = new Vector3 (SteerPivot.localEulerAngles.x, SteerPivot.localEulerAngles.y, slideAngle);	//Leaning...
	}

	float wheelAngle = 0, wheelTargetAngle = 0, wheelFinalAngle = -30.0f, wheelSmoothSync = 0.2f, wheelVelocity = 0.0f;
	void WheelBike ()
	{
		
		bool leftButton = Input.GetMouseButton (0);
		
//		wheelTargetAngle = leftButton && moveSpeed > 15.0f && live ? wheelFinalAngle : 0.0f;	//For Wheeling...
		
		if((leftButton) && (moveSpeed > 15.0f) && (live))
		{
			wheelTargetAngle = wheelFinalAngle;
		}
		else
		{
			wheelTargetAngle = 0.0f;
		}
		
		wheelAngle = Mathf.SmoothDamp (wheelAngle, wheelTargetAngle, ref wheelVelocity, wheelSmoothSync);
		wheelAngle = Mathf.FloorToInt(wheelAngle * 100) / 100f;

		WheelingPivot.localEulerAngles = new Vector3 (-wheelAngle, WheelingPivot.localEulerAngles.y, WheelingPivot.localEulerAngles.z);
	}
	
	
	void LostOnNetWork()
	{
		Debug.Log("LostOnNetWork");
		live = false;
		Game.Obj.SendMessage("GameEnd");
		Invoke("ShowEndMenu", 4f);	
	}
}
