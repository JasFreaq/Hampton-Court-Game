using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Item", menuName = "Tile Item", order = 0)]
public class TileItem : ScriptableObject
{
    [SerializeField] private string m_name;
    [SerializeField] private Sprite m_image;

    public string Name => m_name;

    public Sprite Image => m_image;
}