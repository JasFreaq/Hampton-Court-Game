using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileObject : MonoBehaviour
{
    [SerializeField] private Image m_selectionBorderImage;
    [SerializeField] private Image m_itemImage;
    [SerializeField] private GameObject m_itemImageMask;

    private TileItem m_tileItem;

    private Vector2Int m_tileIndex;

    private Action<Vector2Int> m_onSelect;

    public TileItem TileItem => m_tileItem;

    public Vector2Int TileIndex
    {
        get => m_tileIndex;
        set { m_tileIndex = value; }
    }

    public static void Swap(TileObject tileA, TileObject tileB)
    {
        TileItem tileItem = tileA.TileItem;
        tileA.SetTileItem(tileB.TileItem);
        tileB.SetTileItem(tileItem);
    }

    public void SetTileItem(TileItem tileItem)
    {
        m_tileItem = tileItem;

        m_itemImage.sprite = tileItem.Image;
    }

    public void EnableSelection(bool enable)
    {
        m_selectionBorderImage.enabled = enable;
    }
    
    public void EnableImage(bool enable)
    {
        m_itemImageMask.SetActive(enable);
    }

    public void OnSelect()
    {
        m_onSelect?.Invoke(m_tileIndex);
    }

    public void RegisterOnSelect(Action<Vector2Int> action)
    {
        m_onSelect += action;
    }
    
    public void DeregisterOnSelect(Action<Vector2Int> action)
    {
        m_onSelect -= action;
    }
}
