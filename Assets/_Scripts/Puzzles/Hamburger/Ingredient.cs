using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    

public class Ingredient : MonoBehaviour
{
    SpriteRenderer _sr;
    [SerializeField] Sprite[] _spIngredients;
    public int _indexId;
    Sprite[] _spCurrIngredient;
    

    public void zSetIngredient(int index, int sort)
    {
        _indexId = index;
        _sr.sprite = _spIngredients[index];
        _sr.sortingOrder = sort;

    }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
}
