using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DishManager : MonoBehaviour
{
    [SerializeField] private DishData[] m_dishes;
    [SerializeField] private Vector2Int m_requiredIngredientsRange;
    [SerializeField] private float m_fadeDuration = 1f;
    [SerializeField] private TextMeshProUGUI m_ingredient1RequiredText;
    [SerializeField] private TextMeshProUGUI m_ingredient2RequiredText;
    [SerializeField] private TextMeshProUGUI m_ingredient3RequiredText;
    [SerializeField] private Image m_dishImage;
    [SerializeField] private Image m_ingredient1Image;
    [SerializeField] private Image m_ingredient2Image;
    [SerializeField] private Image m_ingredient3Image;

    private DishData m_lastDish = null;

    private int m_ingredient1Required;
    private int m_ingredient2Required;
    private int m_ingredient3Required;
    
    public DishData GetNewDish()
    {
        DishData newDish = null;

        do
        {
            newDish = m_dishes[Random.Range(0, m_dishes.Length)];

        } while (newDish == m_lastDish);

        UpdateImage(m_dishImage, newDish.Image);
        UpdateImage(m_ingredient1Image, newDish.Ingredient1.Image);
        UpdateImage(m_ingredient2Image, newDish.Ingredient2.Image);
        UpdateImage(m_ingredient3Image, newDish.Ingredient3.Image);

        m_ingredient1Required = Random.Range(m_requiredIngredientsRange.x, m_requiredIngredientsRange.y);
        m_ingredient2Required = Random.Range(m_requiredIngredientsRange.x, m_requiredIngredientsRange.y);
        m_ingredient3Required = Random.Range(m_requiredIngredientsRange.x, m_requiredIngredientsRange.y);
        UpdateIngredientsRequiredText();

        m_lastDish = newDish;

        return newDish;
    }

    public bool IsDishCleared(List<TileBehaviour> matchedTiles)
    {
        UpdateDishState(matchedTiles);

        return m_ingredient1Required == 0 && m_ingredient2Required == 0 && m_ingredient3Required == 0;
    }

    private void UpdateDishState(List<TileBehaviour> matchedTiles)
    {
        foreach (TileBehaviour matchedTile in matchedTiles)
        {
            if (matchedTile.TileItem.Name == m_lastDish.Ingredient1.Name)
            {
                m_ingredient1Required = Mathf.Max(0, m_ingredient1Required - 1);
            }
            else if (matchedTile.TileItem.Name == m_lastDish.Ingredient2.Name)
            {
                m_ingredient2Required = Mathf.Max(0, m_ingredient2Required - 1);
            }
            else if (matchedTile.TileItem.Name == m_lastDish.Ingredient3.Name)
            {
                m_ingredient3Required = Mathf.Max(0, m_ingredient3Required - 1);
            }
        }

        UpdateIngredientsRequiredText();
    }

    private void UpdateIngredientsRequiredText()
    {
        m_ingredient1RequiredText.text = m_ingredient1Required.ToString();
        m_ingredient2RequiredText.text = m_ingredient2Required.ToString();
        m_ingredient3RequiredText.text = m_ingredient3Required.ToString();
    }

    private void UpdateImage(Image imageSource, Sprite newImage)
    {
        Color color = imageSource.color;
        color.a = 0f;
        imageSource.color = color;

        imageSource.sprite = newImage;
        imageSource.DOFade(1f, m_fadeDuration);
    }
}
