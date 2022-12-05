using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleInfo_", menuName = "New SO Puzzle Twelve Animal Info")]
public class TrSO_PuzzleTwelveAnimalInfo : ScriptableObject
{
	[SerializeField] string _nameAnimal;
	[SerializeField] Sprite _spriteAnimal;
	[SerializeField] Sprite[] _spriteFoods;

	public string xNameAnimal { get { return _nameAnimal; } }
	public Sprite xSpriteAnimal { get { return _spriteAnimal; } }
	public Sprite[] xSpFoods { get { return _spriteFoods; } }
}
