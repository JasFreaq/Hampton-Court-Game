using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSubstitute : MonoBehaviour
{
    [SerializeField] private Image m_itemImage;

    public void InitialiseSubstitute(Sprite image)
    {
        m_itemImage.sprite = image;
    }
}
