using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueInfo_", menuName = "New SO Dialogue Animal Info")]
public class TrSO_DialogueAnimalData : ScriptableObject
{
	[SerializeField] string _animalName;
	[SerializeField] Sprite[] _animalSprits;
	[SerializeField] Color _animalNameImageBackgroundColor;
	
	public string xAnimalName { get { return _animalName; } }
	public Sprite[] xAnimalSprite { get { return _animalSprits; } }
	public Color xAnimalNameImageBackgroundColor { get { return _animalNameImageBackgroundColor; } }
}
