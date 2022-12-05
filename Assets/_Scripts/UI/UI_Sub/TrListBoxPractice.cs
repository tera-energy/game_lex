using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// The box used for displaying the content in the client's UI.
// Must be inherited from the class ListBox
public class TrListBoxPractice : ListBox {
    [SerializeField] Image _gameScreenshot;
    [SerializeField] TextMeshProUGUI _gameTitle;
	[SerializeField] TextMeshProUGUI _gameDescription;
	[SerializeField] TextMeshProUGUI[] _gameSkills = new TextMeshProUGUI[3];
	[SerializeField] Image[] _gameSkillIcons = new Image[3];
	[SerializeField] TextMeshProUGUI _gameTime;
	[SerializeField] GameObject _gameInfoPanel;
	public Transform _nanidoArrow;
	public Sprite[] _skillIcons;

	public Image xGameScreenshot { get { return _gameScreenshot; } }
	public TextMeshProUGUI xGameTitle { get { return _gameTitle; } }
	public TextMeshProUGUI xGameDescription { get { return _gameDescription; } }
	public TextMeshProUGUI[] xGameSkills { get { return _gameSkills; } }
	public TextMeshProUGUI xGameTime { get { return _gameTime; } }
	public GameObject xGameDetailWindow { get { return _gameInfoPanel; } }




	// 박스에다 컨텐츠를 담는 작업.
	protected override void UpdateDisplayContent(object content) {
		trScrollContentPractice scp = (trScrollContentPractice)content;

		_gameScreenshot.sprite = scp.xPuzzleData.xGameScreenshot;
		_gameTitle.text = scp.xPuzzleData.xGameTitle;
		_gameDescription.text = scp.xPuzzleData.xGameDescription;
		for(int i = 0;i<3;i++) {
			if(i < scp.xPuzzleData.xGameSkills.Length) {
				_gameSkills[i].text = TT.strPlayerSkills[(int)scp.xPuzzleData.xGameSkills[i]];
				if(scp.xPuzzleData.xGameSkills[i] == TT.enumPlayerSkills.Solving) {
					_gameSkills[i].text = "문해력";    //문제해결력은 길어서 문해력으로 줄임.
				}
				_gameSkillIcons[i].gameObject.SetActive(true);
				_gameSkillIcons[i].sprite = _skillIcons[(int)scp.xPuzzleData.xGameSkills[i]];
			} else {
				_gameSkills[i].text = "";
				_gameSkillIcons[i].gameObject.SetActive(false);
			}
		}
		_gameTime.text = scp.xPuzzleData.xGameTimesByNanido[(int)TrUI_Window_Nanido.xInstance.xxSelectedDifficulty-1] + "초 내외";    //디폴트시간 = 노멀난이도
		zHideGameInfoPanel();
	}

	public void zShowGameInfoPanel() {
		_gameInfoPanel.SetActive(true);
		_gameScreenshot.color = Color.white;
	}

	public void zHideGameInfoPanel() {
		_nanidoArrow.eulerAngles = _nanidoArrow.eulerAngles.zNew(z: 180);
		_gameInfoPanel.SetActive(false);
		_gameScreenshot.color = Color.grey;
	}

	public void zToggleNanidoWindow() {
		if(TrUI_Window_Nanido.xInstance.xIsShowing) {
			TrUI_Window_Nanido.xInstance.zHide();
			TrAudio_UI.xInstance.zzPlay_PopupClose();

			_nanidoArrow.DOKill();
			_nanidoArrow.DORotate(new Vector3(0,0,180),0.5f);
		} else {
			TrUI_Window_Nanido.xInstance.zShow();
			TrAudio_UI.xInstance.zzPlay_Popup();

			_nanidoArrow.DOKill();
			_nanidoArrow.DORotate(Vector3.zero,0.5f);
		}
	}
}