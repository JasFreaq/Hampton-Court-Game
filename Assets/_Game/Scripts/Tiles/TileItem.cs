using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Item", menuName = "Tile Item", order = 0)]
public class TileItem : ScriptableObject
{
    [SerializeField] private string m_name;
    [SerializeField] private Sprite m_image;
    [SerializeField] private ItemType m_itemType;

    public string Name => m_name;

    public Sprite Image => m_image;

    public ItemType ItemType => m_itemType;
}