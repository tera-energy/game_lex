using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    static StaminaManager _instance;

    public static StaminaManager xInstance { get { return _instance; } }



    public static int _maxStamina = 5;      // 최대 스테미나 개수
    //public static int DatabaseManager._myDatas.staminaTemp;
    static int _coolTimeMinute = 15;   // 총 쿨타임이 몇 분인지
    float _maxCoolTimeSecond;               // 총 쿨타임(초)
    float _currCoolTimeSecond;              // 현재 쿨타임

    // UI
    [SerializeField] GameObject _goStamina;
    [SerializeField] GameObject _goStaminaCount;
    [SerializeField] TextMeshProUGUI _txtCurrStamina;
    [SerializeField] TextMeshProUGUI _txtCountMinute;
    [SerializeField] TextMeshProUGUI _txtCountSecond;
    [SerializeField] TrUI_HoldButton _btnStartCustorm;
    [SerializeField] Button _btnStartNormal;

    public bool _isFull = true;
    bool _isAlreadyUpdate = false;
    public bool _isSetStamina;

    public void zSetStaminaOnUI()
    {
        if (DatabaseManager._myDatas == null) return;

        int stamina = DatabaseManager._myDatas.stamina;

        if (_txtCurrStamina)
        {
            _txtCurrStamina.text = stamina.ToString();
        }

        if (stamina > 0)
        {
            if (GameManager._state == TT.enumGameState.Lobby || GameManager._type == TT.enumGameType.Challenge)
            {
                _btnStartCustorm?.zInteractActivate();
                if (_btnStartNormal)
                    _btnStartNormal.interactable = true;
            }
        }
        else
        {
            if (GameManager._state == TT.enumGameState.Lobby || GameManager._type == TT.enumGameType.Challenge)
            {
                _btnStartCustorm?.zInteractDisable();
                if (_btnStartNormal)
                    _btnStartNormal.interactable = false;
            }
        }
    }

    public IEnumerator zCheckStamina(Action successMethod = null, Action failedMethod = null)
    {
        if (DatabaseManager._myDatas.stamina > 0)
        {
            GameManager._type = TT.enumGameType.Challenge;

            if (DatabaseManager._myDatas.stamina == _maxStamina)
            {
                DatabaseManager._myDatas.staminaDate = DateTime.UtcNow.ToString();
            }

            DatabaseManager._myDatas.stamina--;
            yield return StartCoroutine(DatabaseManager.xInstance.zSetStamina());
            zSetStaminaOnUI();
            successMethod?.Invoke();
        }
        else
        {
            GameManager._canBtnClick = true;
            failedMethod?.Invoke();
        }
    }

    IEnumerator yInitStamina()
    {
        _isFull = true;
        _isSetStamina = false;
        _maxCoolTimeSecond = 60 * _coolTimeMinute;

        yield return new WaitUntil(() => AuthManager._isCompleteSignIn);

        if (DatabaseManager._myDatas.stamina < _maxStamina)
        {
            float needFullSecond = (_maxStamina - DatabaseManager._myDatas.stamina) * _maxCoolTimeSecond;
            int diffSecond = TT.zGetDateDiffCurrToSeconds(ref DatabaseManager._myDatas.staminaDate);

            if (diffSecond >= needFullSecond)
            {
                zFullStamina();
            }
            else
            {
                _isFull = false;
                if (_goStaminaCount)
                    _goStaminaCount.SetActive(true);
            }
        }
        else
        {
            if (_goStaminaCount)
                _goStaminaCount.SetActive(false);
        }

        zSetStaminaOnUI();
        _isSetStamina = true;
    }

    public void zFullStamina()
    {
        _isFull = true;
        DatabaseManager._myDatas.staminaDate = "";
        if (DatabaseManager._myDatas.stamina < _maxStamina)
            DatabaseManager._myDatas.stamina = _maxStamina;

        if (_goStaminaCount)
            _goStaminaCount.SetActive(false);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        _instance = null;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (GameManager._state == TT.enumGameState.Lobby || GameManager._type == TT.enumGameType.Challenge)
            _goStamina.SetActive(true);
        else
            _goStamina.SetActive(false);

        StartCoroutine(yInitStamina());
    }

    void FixedUpdate()
    {
        if (_isFull) return;

        if (GameManager._state == TT.enumGameState.Lobby || (GameManager._state == TT.enumGameState.Result && GameManager._type == TT.enumGameType.Challenge))
        {
            int diffSecond = TT.zGetDateDiffCurrToSeconds(ref DatabaseManager._myDatas.staminaDate);
            _currCoolTimeSecond = (diffSecond % _maxCoolTimeSecond) + 1;
            float restCool = _maxCoolTimeSecond - _currCoolTimeSecond;
            int min = (int)restCool / 60;
            int sec = (int)restCool % 60;
            _txtCountMinute.text = min.ToString("00");
            _txtCountSecond.text = sec.ToString("00");

            if (diffSecond >= _maxCoolTimeSecond)
            {
                int numCharge = diffSecond / (int)_maxCoolTimeSecond;
                int addMin = _coolTimeMinute * numCharge;
                int currStamina = DatabaseManager._myDatas.stamina + numCharge;
                DateTime d = DateTime.Parse(DatabaseManager._myDatas.staminaDate);
                d = d.AddMinutes(addMin);
                DatabaseManager._myDatas.staminaDate = d.ToString("yyyy-MM-dd hh:mm:ss");

                if (!_isAlreadyUpdate && currStamina >= _maxStamina)
                {
                    _isAlreadyUpdate = true;
                    currStamina = _maxStamina;
                    zFullStamina();
                }

                DatabaseManager._myDatas.stamina = currStamina;
                StartCoroutine(DatabaseManager.xInstance.zSetStamina());
                zSetStaminaOnUI();
            }
        }
    }
}
