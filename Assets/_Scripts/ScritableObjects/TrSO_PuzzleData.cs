using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="PuzzleInfo_", menuName = "New SO Puzzle Info")]
public class TrSO_PuzzleData : ScriptableObject
{
	[SerializeField] string _gameSceneName;
    [SerializeField] Sprite _gameScreenshot;
    [SerializeField] string _gameTitle;
	[SerializeField] string _gameDescription;
	[SerializeField] int[] _gameTimesByNanido = new int[5];
	[SerializeField] TT.enumPlayerSkills[] _gameSkills;
	[SerializeField] string[] _challenges;

	public string xGameSceneName { get { return _gameSceneName; } }
	public Sprite xGameScreenshot { get { return _gameScreenshot; } }
	public string xGameTitle { get { return _gameTitle; } }
	public string xGameDescription { get { return _gameDescription; } }
	public int[] xGameTimesByNanido { get { return _gameTimesByNanido; } }
	public TT.enumPlayerSkills[] xGameSkills { get { return _gameSkills; } }
	public string[] xChallenges { get { return _challenges; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	//============================================================================================================

}
