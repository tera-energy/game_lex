using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public static class TT
{
	static System.Random rng = null;

	[System.Serializable]
	public class SerialSaveData
	{
		public int _memory; // 기억력
		public int _concentration; // 집중력
		public int _thought; // 사고력
		public int _quickness; // 순발력
		public int _problemSolving; // 문제 해결 능력
	}
	[System.Serializable]
	public class SerialPlayerData
	{
		public string _name;
		public int _money;
	}

	public delegate void Callback();
	public delegate void CallbackBool(bool isYesClicked);
	public delegate void CallbackInt(int responseCode);
	public delegate void CallbackObject(object obj);
	public delegate void CallbackObjectList(List<object> objs);



	public interface IInteractable
	{
		void OnInteract();
		void OffInteract();
	}


	public enum enumGameState { Title, Lobby, Play, Paused, Result }
	public enum enumGameType { Train, Challenge }
	public enum enumAnimState { Idle = 0, Move = 10, Jump = 20, Attack = 30, Damaged = 40, Died = 50, Win = 60 }
	public enum enumButtonColor { Red, Yellow, Green, Blue }
	public enum enumButtonInput { Neutral, Up, Down, Left, Right }
	public enum enumDifficultyLevel { VeryEasy = 1, Easy, Normal, Hard, VeryHard }
	public enum enumTrRainbowColor { RED, ORANGE, YELLOW, GREEN, BLUE, NAVY, PURPLE }
	public enum enumPlayerSkills { Memory, Focus, Reflex, Solving, Thinking }
	public enum enumPlayGameType { Develop, Campaign, Exercise, Rank }
	public enum enumCollectibles { GoldCoin, Diamond }

	public static readonly string[] strPlayerSkills = new string[] { "기억력", "집중력", "순발력", "문제해결력", "사고력" };
	#region const string variables
	public const string
		strTagPlayer = "Player",
		strTagEnemy = "Enemy",
		strTagPlatform = "Platform",
		strDefaultName = "UnknownGuy",
		strConfigSFX = "is SFX on?",
		strConfigMusic = "is music on?",
		strConfigVibrate = "is Vibrate on?",
		strConfigAgreePrivacy = "is Agree Prviacy?";

	#endregion

	#region UserData
	public const string
		USERTABLE = "User",

		USERID = "userId",
		EMAIL = "email",
		NICKNAME = "nickName",
		MAXSCORE = "maxScore",
		STAMINA = "stamina",
		STAMINADATE = "staminaDate",

		AUTOLOGINPLATFORM = "AutoLoginPlatform",
		AUTOLOGINID = "AutoLoginUserId",

		FRIENDS = "Friends",
		REQUEST = "Request",
		LIST = "List";

	#endregion

	public const int
		intMaxLevel = 10,
		intMaxMoney = 9999;

	public const float
		floMaxMoveSpeed = 10f;

	public static Color zSetColor(enumTrRainbowColor col)
	{
		Color baseColor = Color.black;
		switch (col)
		{
			case enumTrRainbowColor.RED: baseColor = new Color(1, 0.25f, 0.25f); break;
			case enumTrRainbowColor.ORANGE: baseColor = new Color(1, 0.6f, 0.2f); break;
			case enumTrRainbowColor.YELLOW: baseColor = new Color(1, 1, 0.5f); break;
			case enumTrRainbowColor.GREEN: baseColor = new Color(0.5f, 1, 0.5f); break;
			case enumTrRainbowColor.BLUE: baseColor = new Color(0.5f, 0.8f, 1); break;
			case enumTrRainbowColor.NAVY: baseColor = new Color(0.2f, 0.2f, 0.75f); break;
			case enumTrRainbowColor.PURPLE: baseColor = new Color(1, 0.4f, 1); break;
		}
		return baseColor;
	}

	/// <summary>
	/// 현재 마우스커서(or 터치)가 UI오브젝트와 겹쳐있는지 확인.
	/// 주의: UI오브젝트 중 RayCast Target이 Uncheck되어 있으면 UI오브젝트가 아닌것으로 간주함.
	/// PC에서 클릭 시 예상대로 되는데 터치시엔 작동이 안되는건 현재 유니티 자체 버그일 확률이 있음. 
	/// 추후에 원래 유니티 함수인, EventSystem.current.IsPointerOverGameObject(...)를 사용하길 권장함.
	/// </summary>
	public static bool zIsPointerOverUIObject()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = Input.mousePosition;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);

		return results.Count > 0;
	}

	public static bool zIsPointerOverUIGraphic(Canvas topCanvas)
	{
		GraphicRaycaster raycaster = topCanvas.GetComponent<GraphicRaycaster>();
		//Debug.Log($"rayrayray: {raycaster.name}",raycaster.gameObject);
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = Input.mousePosition;
		List<RaycastResult> results = new List<RaycastResult>();
		raycaster.Raycast(eventData, results);

		return results.Count > 0;
	}

	public static void zCreateCollectEffect(GameObject reference, int howMany, Vector3 fromPos, Vector3 toPos, Vector3 scale, bool useRandomCurve = true, Ease tweenEase = Ease.InCubic)
	{
		float doDelay = 0;
		for (int i = 0; i < howMany; i++)
		{
			Transform c = GameObject.Instantiate(reference, reference.transform.parent).transform;
			c.localScale = scale;
			c.position = fromPos;

			if (useRandomCurve)
			{
				//생성지점과 도착지점의 중간을 지나쳐갈 지점을 만듬.
				Vector3 midCurvePath = Vector3.Lerp(fromPos, toPos, UnityEngine.Random.Range(0.3f, 0.6f)) +
					new Vector3(UnityEngine.Random.Range(-500f, 500f), UnityEngine.Random.Range(-100f, 100f));
				Vector3[] paths = new Vector3[] { fromPos, midCurvePath, toPos };
				c.DOPath(paths, 1f, PathType.CatmullRom, PathMode.Ignore).SetDelay(doDelay).SetEase(tweenEase).OnComplete(() => GameObject.Destroy(c.gameObject));
			}
			else
			{
				c.DOMove(toPos, 1f).SetDelay(doDelay).SetEase(tweenEase).OnComplete(() => GameObject.Destroy(c.gameObject));
			}
			doDelay += 1.5f / howMany;
		}
	}

	/// <summary>
	/// 새로운 백터값을 리턴. <br/><br/>
	/// 사용예: transform.position = transform.position.zNew(y:10);<br/>
	/// 설명: 현재 게임 오브젝트의 y축 위치를 10으로 바꿈.
	/// </summary>
	public static Vector3 zNew(this Vector3 original, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(x ?? original.x, y ?? original.y, z ?? original.z);
	}

	/// <summary>
	/// 새로운 백터값을 리턴. <br/><br/>
	/// 사용예: transform.position = transform.position.zMod(y:-10);<br/>
	/// 설명: 현재 게임 오브젝트의 y축 위치를 10만큼 내림.
	/// </summary>
	public static Vector3 zMod(this Vector3 original, float? x = null, float? y = null, float? z = null)
	{
		float newX = (float)(x != null ? original.x + x : original.x);
		float newY = (float)(y != null ? original.y + y : original.y);
		float newZ = (float)(z != null ? original.z + z : original.z);
		return new Vector3(newX, newY, newZ);
	}

	/// <summary>
	/// 배열을 랜덤으로 섞음.<br/><br/>
	/// 사용예: string[] names = new string[] { "A", "B", "C", "D", "E" };<br/>
	///			names.zShuffle();
	/// </summary>
	public static void zShuffle<T>(this T[] array)
	{
		if (rng == null) rng = new System.Random(UnityEngine.Random.Range(0, 1000000));

		int n = array.Length;
		while (n > 1)
		{
			int k = rng.Next(n);
			n--;
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}
	}

	/// <summary>
	/// 리스트를 랜덤으로 섞음.<br/><br/>
	/// 사용예: List<int> numbers = new List<int>(); <br/>
	///			//then, for i, numbers.Add(i).<br/>
	///			numbers.zShuffle();
	/// </summary>
	public static void zShuffle<T>(this IList<T> list)
	{
		if (rng == null) rng = new System.Random(UnityEngine.Random.Range(0, 1000000));

		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	/// <summary>
	/// 특정 (람다)메소드를 일정 타이머 후에 실행되도록 예약. 유니티 Invoke의 업그레이드 버젼.<br/><br/>
	/// 사용예: TT.UtilTimerFunc.zCreate(myMethod, 3f, "Shoot after 3");<br/>
	/// 예약취소: TT.UtilTimerFunc.zCancelTimer("Shoot after 3");
	/// </summary>

	/* <테스팅 코드 블럭> 하단 코드를 아무 스크립트에 복사 붙여넣기하여 테스트 (유니티함수 Start에서 테스트하기 추천)
	--------------------------------------------------------------------------------------------------------------
	TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {1111}"));
    TT.UtilDelayedFunc.zCreateAtLate(() => Debug.Log($"debug: {2222}"));
    TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {3333}"));


    TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {4444}"), 0f,"Hahaha");
    TT.UtilDelayedFunc.zCancel("Hahaha");
    TT.UtilDelayedFunc udf = TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {5555}"), 0f, "Hoyohoyo");
    udf.zKillSelf();

    int timer = 3;
    TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {"Get"}",gameObject), 3.5f);
    TT.UtilDelayedFunc.zCreate(() => Debug.Log($"debug: {"Ready"}",gameObject), 4.5f);
    TT.UtilDelayedFunc.zCreateRepeat(() => { 
		Debug.Log($"Countdown [{timer}]");
		timer--; 
	},5.5f, 3, 1f);

    //TT.UtilDelayedFunc.zCancelAll();
	--------------------------------------------------------------------------------------------------------------
	*/

	public class UtilDelayedFunc
	{
		#region 구현코드
		// MonoBehaviour 함수에 접근하기 위한 더미클래스.
		class MonoBehaviourUpdate : MonoBehaviour
		{
			public Action _onUpdate;
			private void Update()
			{
				_onUpdate?.Invoke();
			}
		}
		class MonoBehaviourLateUpdate : MonoBehaviour
		{
			public Action _onLateUpdate;
			private void LateUpdate()
			{
				_onLateUpdate?.Invoke();
			}
		}

		static List<UtilDelayedFunc> _activeDFList;
		static GameObject _initGameObject;
		GameObject _gameObject;
		Action _action;
		float _delay;
		int _numRepeats;
		float _repeatTimer;
		string _functionTag;
		bool _useUnscaledTimer;

		UtilDelayedFunc(GameObject gameObj, Action action, float delay, int repeatXMore, float repeatTimer, string functionTag, bool useUnscaledTimer)
		{
			_gameObject = gameObj;
			_action = action;
			_delay = delay;
			_numRepeats = repeatXMore;
			_repeatTimer = repeatTimer;
			_functionTag = functionTag;
			_useUnscaledTimer = useUnscaledTimer;
		}

		//////////////////////////////////////////////////

		static void yInitIfNeeded()
		{
			if (_initGameObject == null)
			{
				_initGameObject = new GameObject("UDF_Init");
				_activeDFList = new List<UtilDelayedFunc>();
			}
		}

		/// <summary> functionTag는 나중에 해당 DelayedFunc를 취소할 때 쓰임. </summary>
		public static UtilDelayedFunc zCreate(Action act, float delay = -1, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreate", typeof(MonoBehaviourUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, 0, 0, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourUpdate>()._onUpdate = delayedFunc.yUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary>"repeatXMore" 패러미터 값으로 -1을 보내면 무한실행. <br/>
		/// 예시: 카운트다운 <br/>
		/// int timer = 3;	<br/>
		/// TT.UtilDelayedFunc.zCreateRepeat(()=>{ Debug.Log($"Countdown [{timer}]"); timer--; },5f,3,1f);
		/// </summary>
		public static UtilDelayedFunc zCreateRepeat(Action act, float delay, int repeatXMore, float repeatTimer, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreateRepeat", typeof(MonoBehaviourUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, repeatXMore, repeatTimer, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourUpdate>()._onUpdate = delayedFunc.yUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary> 해당 함수를 Unity의 LateUpdate 때 실행함. </summary> 
		public static UtilDelayedFunc zCreateAtLate(Action act, float delay = -1, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreateAtLate", typeof(MonoBehaviourLateUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, 0, 0, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourLateUpdate>()._onLateUpdate = delayedFunc.yLateUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary>"repeatXMore" 패러미터 값으로 -1을 보내면 무한실행. <br/>
		/// 예시: 카운트다운 <br/>
		/// int timer = 3;	<br/>
		/// TT.UtilDelayedFunc.zCreateRepeatAtLate(()=>{ Debug.Log($"Countdown [{timer}]"); timer--; },5f,3,1f);
		public static UtilDelayedFunc zCreateRepeatAtLate(Action act, float delay, int repeatXMore, float repeatTimer, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreateRepeatAtLate", typeof(MonoBehaviourLateUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, repeatXMore, repeatTimer, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourLateUpdate>()._onLateUpdate = delayedFunc.yLateUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary> 해당 functionTag를 가지고 있는 DelayedFunc가 하나라도 있는지 확인. </summary>
		public static bool zIsAlive(string functionTag)
		{
			for (int i = _activeDFList.Count - 1; i >= 0; i--)
			{
				if (_activeDFList[i]._functionTag == functionTag)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary> 해당 functionTag를 가지고 있는 모든 DelayedFunc를 취소함. </summary>
		public static void zCancel(string functionTag)
		{
			for (int i = _activeDFList.Count - 1; i >= 0; i--)
			{
				if (_activeDFList[i]._functionTag == functionTag)
				{
					_activeDFList[i].zKillSelf();
				}
			}
		}

		/// <summary> 다른 곳(오브젝트)에도 DelayedFunc를 만들어 사용하고 있을 수 있으므로 이 함수는 매우 신중히 써야함. </summary>
		public static void zCancelAll()
		{
			for (int i = _activeDFList.Count - 1; i >= 0; i--)
			{
				//Debug.Log($"UDF CANCELING >> {_activeDFList[i]._functionTag}");
				_activeDFList[i].zKillSelf();
			}
		}

		#region zCreate()에 의해 만들어진 UtilDelayedFunc 오브젝트의 레퍼런스를 가지고 있다면 사용할 수 있는 함수들.

		public void zKillSelf()
		{
			MonoBehaviourUpdate monoUpdate = _gameObject.GetComponent<MonoBehaviourUpdate>();
			if (monoUpdate != null) monoUpdate._onUpdate -= yUpdate;
			MonoBehaviourLateUpdate monoLateUpdate = _gameObject.GetComponent<MonoBehaviourLateUpdate>();
			if (monoLateUpdate != null) monoLateUpdate._onLateUpdate -= yLateUpdate;

			_activeDFList?.Remove(this);
			if (_gameObject != null) UnityEngine.Object.Destroy(_gameObject);
		}

		public void zChangeDelay(float newDelay)
		{
			_delay = newDelay;
		}


		public void zSetRepeat(int repeatXMore, float repeatTimer)
		{
			_numRepeats = repeatXMore;
			_repeatTimer = repeatTimer;
		}

		/// <summary> repeatNum = -1 >> 무한번 <br/>
		/// [주의] 이미 타이머는 돌아가고 있을 것이므로 0을 넘겨준다해도 바로 멈추지 않고 한번은 Action을 실행하고 멈춤. <br/>
		/// 바로 멈추려면 zKillSelf()를 사용.
		/// </summary>
		public void zSetRepeatNumber(int repeatNum) { _numRepeats = repeatNum; }
		public void zSetRepeatTimer(float repeatTimer) { _repeatTimer = repeatTimer; }
		public void zAddMoreRepeats(int howManyMore) { _numRepeats += howManyMore; if (_numRepeats < 0) _numRepeats = 0; }
		#endregion

		void yUpdate()
		{
			if (_delay > 0)
			{
				if (_useUnscaledTimer)
					_delay -= Time.unscaledDeltaTime;
				else
					_delay -= Time.deltaTime;
			}

			if (_delay <= 0)
			{
				_action();
				if (_numRepeats == 0)
					zKillSelf();

				if (_numRepeats > 0)
				{
					_numRepeats--;
					_delay = _repeatTimer;
				}

				if (_numRepeats < 0)
				{
					_delay = _repeatTimer;
				}
			}
		}

		void yLateUpdate()
		{
			if (_delay > 0)
			{
				if (_useUnscaledTimer)
					_delay -= Time.unscaledDeltaTime;
				else
					_delay -= Time.deltaTime;
			}

			if (_delay <= 0)
			{
				_action();
				if (_numRepeats == 0)
					zKillSelf();

				if (_numRepeats > 0)
				{
					_numRepeats--;
					_delay = _repeatTimer;
				}

				if (_numRepeats < 0)
				{
					_delay = _repeatTimer;
				}
			}
		}

		#endregion
	}

	#region Parse Json

	public static int zSortRankScoreList(TrRankUsersData a, TrRankUsersData b)
	{
		return a._userScore.CompareTo(b._userScore);
	}

	public static int zReverseSorRankScoretList(TrRankUsersData a, TrRankUsersData b)
	{
		return b._userScore.CompareTo(a._userScore);
	}

	public static Vector3 zBezierCurve(Vector3 P_1, Vector3 P_2, Vector3 P_3, Vector3 P_4, float value)
	{
		Vector3 A = Vector3.Lerp(P_1, P_2, value);
		Vector3 B = Vector3.Lerp(P_2, P_3, value);
		Vector3 C = Vector3.Lerp(P_3, P_4, value);

		Vector3 D = Vector3.Lerp(A, B, value);
		Vector3 E = Vector3.Lerp(B, C, value);

		Vector3 F = Vector3.Lerp(D, E, value);

		return F;
	}

	private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();
	public static WaitForSeconds WaitForSeconds(float seconds)
	{
		WaitForSeconds wfs;
		if (!waitForSeconds.TryGetValue(seconds, out wfs))
			waitForSeconds.Add(seconds, wfs = new WaitForSeconds(seconds));
		return wfs;
	}


	public static string zToJson<T>(T[] array)
	{
		TrWrapper<T> wrapper = new TrWrapper<T>();
		wrapper._items = array;
		return JsonUtility.ToJson(wrapper);
	}

	public static void zSaveToJson<T>(List<T> toSave, string fileName)
	{
		string content = TT.zToJson<T>(toSave.ToArray());
		string path = zGetPath(fileName);
		FileStream fileStream = new FileStream(path, FileMode.Create);

		using (StreamWriter writer = new StreamWriter(fileStream))
		{
			writer.Write(content);
		}
	}
	public static T[] zFromJson<T>(string json)
	{
		TrWrapper<T> wrapper = JsonUtility.FromJson<TrWrapper<T>>(json);
		return wrapper._items;
	}

	public static List<T> ReadFromJSON<T>(string fileName)
	{
		string content = ReadFile(fileName);
		if (string.IsNullOrEmpty(content) || content == "{}")
		{
			return new List<T>();
		}

		List<T> res = zFromJson<T>(content).ToList();
		return res;
	}

	public static List<T> ReadFromJSONContent<T>(string content)
	{
		List<T> res = zFromJson<T>(content).ToList();
		return res;
	}


	public static string ReadFile(string fileName)
	{
		string path = zGetPath(fileName);
		if (File.Exists(path))
		{
			using (StreamReader reader = new StreamReader(path))
			{
				string content = reader.ReadToEnd();
				return content;
			}
		}
		return "";
	}

	public static string zGetPath(string fileName)
	{
		/*return Application.dataPath + "/Resources/" + fileName;*/
		return Application.persistentDataPath + "/" + fileName;
	}
	#endregion

	public static int zGetDateDiffCurrToSeconds(ref string stLastDate)
	{
		DateTime lastDate = DateTime.Parse(stLastDate);
		DateTime currDate = DateTime.UtcNow;

		TimeSpan diffDate = currDate - lastDate;

		int diffSeconds = (int)diffDate.TotalSeconds;

		return diffSeconds;
	}

	public static Queue<string> zFormatQueueByString(string s1 = null, string s2 = null, string s3 = null, string s4 = null, string s5 = null)
	{
		Queue<string> result = new Queue<string>();
		if (s1 != null) result.Enqueue(s1);
		if (s2 != null) result.Enqueue(s2);
		if (s3 != null) result.Enqueue(s3);
		if (s4 != null) result.Enqueue(s4);
		if (s5 != null) result.Enqueue(s5);

		return result;
	}


	// 1등은 0부터
	public static int zGetRank(int rankNum, int[] array, int score)
	{
		int num = array.Length;
		int rank = -1;
		if (num > 0)
		{
			if (num < rankNum)
			{
				for (int i = 0; i < num; i++)
				{
					if (array[i] <= score)
					{
						rank = i;
						break;
					}
				}
				if (rank != -1)
				{
					rank = num;
				}
			}
			else
			{
				for (int i = 0; i < rankNum; i++)
				{
					if (array[i] <= score)
					{
						rank = i;
						break;
					}
				}
			}
		}
		else
		{
			rank = 0;
		}
		return rank;
	}

	public static void zSetInteractButtons(ref Button[] buttons, bool isActivate)
	{
		int num = buttons.Length;
		for (int i = 0; i < num; i++)
		{
			buttons[i].interactable = isActivate;
		}
	}
}

[Serializable]
public class TrWrapper<T>
{
	public T[] _items;
}



