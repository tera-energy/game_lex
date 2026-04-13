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
		public int _memory; // ����
		public int _concentration; // ���߷�
		public int _thought; // �����
		public int _quickness; // ���߷�
		public int _problemSolving; // ���� �ذ� �ɷ�
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
	public enum enumGameType { Train, Challenge, Battle }
	public enum enumAnimState { Idle = 0, Move = 10, Jump = 20, Attack = 30, Damaged = 40, Died = 50, Win = 60 }
	public enum enumButtonColor { Red, Yellow, Green, Blue }
	public enum enumButtonInput { Neutral, Up, Down, Left, Right }
	public enum enumDifficultyLevel { VeryEasy = 1, Easy, Normal, Hard, VeryHard }
	public enum enumTrRainbowColor { RED, ORANGE, YELLOW, GREEN, BLUE, NAVY, PURPLE }
	public enum enumPlayerSkills { Memory, Focus, Reflex, Solving, Thinking }
	public enum enumPlayGameType { Develop, Campaign, Exercise, Rank }
	public enum enumCollectibles { GoldCoin, Diamond }

	public static readonly string[] strPlayerSkills = new string[] { "����", "���߷�", "���߷�", "�����ذ��", "�����" };
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
	/// ���� ���콺Ŀ��(or ��ġ)�� UI������Ʈ�� �����ִ��� Ȯ��.
	/// ����: UI������Ʈ �� RayCast Target�� Uncheck�Ǿ� ������ UI������Ʈ�� �ƴѰ����� ������.
	/// PC���� Ŭ�� �� ������ �Ǵµ� ��ġ�ÿ� �۵��� �ȵǴ°� ���� ����Ƽ ��ü ������ Ȯ���� ����. 
	/// ���Ŀ� ���� ����Ƽ �Լ���, EventSystem.current.IsPointerOverGameObject(...)�� ����ϱ� ������.
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
				//���������� ���������� �߰��� �����İ� ������ ����.
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
	/// ���ο� ���Ͱ��� ����. <br/><br/>
	/// ��뿹: transform.position = transform.position.zNew(y:10);<br/>
	/// ����: ���� ���� ������Ʈ�� y�� ��ġ�� 10���� �ٲ�.
	/// </summary>
	public static Vector3 zNew(this Vector3 original, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(x ?? original.x, y ?? original.y, z ?? original.z);
	}

	/// <summary>
	/// ���ο� ���Ͱ��� ����. <br/><br/>
	/// ��뿹: transform.position = transform.position.zMod(y:-10);<br/>
	/// ����: ���� ���� ������Ʈ�� y�� ��ġ�� 10��ŭ ����.
	/// </summary>
	public static Vector3 zMod(this Vector3 original, float? x = null, float? y = null, float? z = null)
	{
		float newX = (float)(x != null ? original.x + x : original.x);
		float newY = (float)(y != null ? original.y + y : original.y);
		float newZ = (float)(z != null ? original.z + z : original.z);
		return new Vector3(newX, newY, newZ);
	}

	/// <summary>
	/// �迭�� �������� ����.<br/><br/>
	/// ��뿹: string[] names = new string[] { "A", "B", "C", "D", "E" };<br/>
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
	/// ����Ʈ�� �������� ����.<br/><br/>
	/// ��뿹: List<int> numbers = new List<int>(); <br/>
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
	/// Ư�� (����)�޼ҵ带 ���� Ÿ�̸� �Ŀ� ����ǵ��� ����. ����Ƽ Invoke�� ���׷��̵� ����.<br/><br/>
	/// ��뿹: TT.UtilTimerFunc.zCreate(myMethod, 3f, "Shoot after 3");<br/>
	/// �������: TT.UtilTimerFunc.zCancelTimer("Shoot after 3");
	/// </summary>

	/* <�׽��� �ڵ� ����> �ϴ� �ڵ带 �ƹ� ��ũ��Ʈ�� ���� �ٿ��ֱ��Ͽ� �׽�Ʈ (����Ƽ�Լ� Start���� �׽�Ʈ�ϱ� ��õ)
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
		#region �����ڵ�
		// MonoBehaviour �Լ��� �����ϱ� ���� ����Ŭ����.
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

		/// <summary> functionTag�� ���߿� �ش� DelayedFunc�� ����� �� ����. </summary>
		public static UtilDelayedFunc zCreate(Action act, float delay = -1, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreate", typeof(MonoBehaviourUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, 0, 0, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourUpdate>()._onUpdate = delayedFunc.yUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary>"repeatXMore" �з����� ������ -1�� ������ ���ѽ���. <br/>
		/// ����: ī��Ʈ�ٿ� <br/>
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

		/// <summary> �ش� �Լ��� Unity�� LateUpdate �� ������. </summary> 
		public static UtilDelayedFunc zCreateAtLate(Action act, float delay = -1, string functionTag = null, bool useUnscaledTime = false)
		{
			yInitIfNeeded();
			GameObject go = new GameObject("UDFzCreateAtLate", typeof(MonoBehaviourLateUpdate));
			UtilDelayedFunc delayedFunc = new UtilDelayedFunc(go, act, delay, 0, 0, functionTag, useUnscaledTime);
			go.GetComponent<MonoBehaviourLateUpdate>()._onLateUpdate = delayedFunc.yLateUpdate;
			_activeDFList.Add(delayedFunc);

			return delayedFunc;
		}

		/// <summary>"repeatXMore" �з����� ������ -1�� ������ ���ѽ���. <br/>
		/// ����: ī��Ʈ�ٿ� <br/>
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

		/// <summary> �ش� functionTag�� ������ �ִ� DelayedFunc�� �ϳ��� �ִ��� Ȯ��. </summary>
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

		/// <summary> �ش� functionTag�� ������ �ִ� ��� DelayedFunc�� �����. </summary>
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

		/// <summary> �ٸ� ��(������Ʈ)���� DelayedFunc�� ����� ����ϰ� ���� �� �����Ƿ� �� �Լ��� �ſ� ������ �����. </summary>
		public static void zCancelAll()
		{
			for (int i = _activeDFList.Count - 1; i >= 0; i--)
			{
				//Debug.Log($"UDF CANCELING >> {_activeDFList[i]._functionTag}");
				_activeDFList[i].zKillSelf();
			}
		}

		#region zCreate()�� ���� ������� UtilDelayedFunc ������Ʈ�� ���۷����� ������ �ִٸ� ����� �� �ִ� �Լ���.

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

		/// <summary> repeatNum = -1 >> ���ѹ� <br/>
		/// [����] �̹� Ÿ�̸Ӵ� ���ư��� ���� ���̹Ƿ� 0�� �Ѱ��ش��ص� �ٷ� ������ �ʰ� �ѹ��� Action�� �����ϰ� ����. <br/>
		/// �ٷ� ���߷��� zKillSelf()�� ���.
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


	// 1���� 0����
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



