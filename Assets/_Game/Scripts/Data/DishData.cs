using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dish", menuName = "Dish", order = 1)]
public class DishData : ScriptableObject
{
    [SerializeField] private string m_name;
    [SerializeField] private Sprite m_image;

    [SerializeField] private TileItem m_ingredient1;
    [SerializeField] private TileItem m_ingredient2;
    [SerializeField] private TileItem m_ingredient3;

    public Sprite Image => m_image;

    public TileItem Ingredient1 => m_ingredient1;

    public TileItem Ingredient2 => m_ingredient2;

    public TileItem Ingredient3 => m_ingredient3;

    public List<TileItem> GetIngredients()
    {
        return new List<TileItem> { m_ingredient1, m_ingredient2, m_ingredient3 };
    }
}
