using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The box used for displaying the content.
// Must be inherited from the class ListBox
public class TrListBoxMainMenu : ListBox {
    [SerializeField] Image _btnFrontImage;
    [SerializeField] TextMeshProUGUI _btnTitle;

	public Image xBtnFrontImage { get { return _btnFrontImage; } }
	public TextMeshProUGUI xButtonTitle { get { return _btnTitle; } }

	/// This function is invoked by the `CircularScrollingList` for updating the list content.
	/// The type of the content will be converted to `object` in the `IntListBank` (Defined later)
	/// So it should be converted back to its own type for being used.
	/// The original type of the content is `int`.
	/// 즉, 박스에다 컨텐츠를 담는 작업. 테라(Tr)버전은 컨텐트를 시리얼클래스로 정함.
	protected override void UpdateDisplayContent(object content) {
		trScrollContentMainMenu scmm = (trScrollContentMainMenu)content;

		_btnFrontImage.sprite = scmm.xSprite;
		_btnTitle.text = scmm.xBtnTitle;
	}

	public void zSetButtonTitle(string title) {
		_btnTitle.text = title;
	}
}