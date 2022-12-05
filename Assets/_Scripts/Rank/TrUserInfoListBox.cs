using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrUserInfoListBox : MonoBehaviour
{
    // ¾Õ¸é
    [SerializeField] GameObject _goUserInfoFront;
    [SerializeField] GameObject _goUserInfoBack;
    [SerializeField] TextMeshProUGUI _userName;
    [SerializeField] TextMeshProUGUI _userTitle;
    [SerializeField] Image _userTitleField;
    [SerializeField] Image _userTier;
    [SerializeField] Image _userTierBorder;

    // µÞ¸é
    [SerializeField] TextMeshProUGUI _winRateNum;
    [SerializeField] TextMeshProUGUI _betterThanYou;
    [SerializeField] TextMeshProUGUI _escapeNum;

    public TextMeshProUGUI xWinRateNum { get { return _winRateNum; } }
    public TextMeshProUGUI xBetterThanYou { get { return _betterThanYou; } }
    public TextMeshProUGUI xEscapeNum { get { return _escapeNum; } }

    public TextMeshProUGUI xUserName { get { return _userName; } }
    public TextMeshProUGUI xUserTitle { get { return _userTitle; } }
    public Image xUserTitleField { get { return _userTitleField; } }
    public Image xUserTier { get { return _userTier; } }
    public Image xUserTierBorder { get { return _userTierBorder; } }
    public GameObject xGoUserInfoFront { get { return _goUserInfoFront; } }
    public GameObject xGoUserInfoBack { get { return _goUserInfoBack; } }

}
