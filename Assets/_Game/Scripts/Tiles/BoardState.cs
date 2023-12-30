using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    private int m_rows;
    private int m_columns;

    private Transform[,] m_tilePlaceholders;
    private TileObject[,] m_boardTileObjects;

    public BoardState(int rows, int columns)
    {
        m_rows = rows;
        m_columns = columns;

        m_boardTileObjects = new TileObject[rows, columns];
        m_tilePlaceholders = new Transform[rows, columns];
    }

    public int Rows => m_rows;

    public int Columns => m_columns;

    public Transform[,] TilePlaceholders => m_tilePlaceholders;

    public TileObject[,] BoardTileObjects => m_boardTileObjects;

    public void SwapTileData(TileObject tileA, TileObject tileB)
    {
        Vector2Int tempIndex = tileB.TileIndex;

        m_boardTileObjects[tileA.TileIndex.x, tileA.TileIndex.y] = tileB;
        tileB.TileIndex = tileA.TileIndex;

        m_boardTileObjects[tempIndex.x, tempIndex.y] = tileA;
        tileA.TileIndex = tempIndex;
    }
}
