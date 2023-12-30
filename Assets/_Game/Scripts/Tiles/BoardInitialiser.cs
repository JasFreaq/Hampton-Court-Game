using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardInitialiser : MonoBehaviour
{
    private const int k_loopCheckBuffer = 100;

    [SerializeField] private int m_tileRows = 5;
    [SerializeField] private int m_tileColumns = 12;

    [SerializeField] private TileObject m_tilePrefab;
    [SerializeField] private TileItem[] m_tileItems;
    [SerializeField] private GameObject m_tilePlaceholder;
    [SerializeField] private Transform m_tileSpawnLocation;
    [SerializeField] private int m_tileObjectPoolCount = 12;

    private BoardState m_board;

    private BoardHandler m_boardHandler;

    private List<TileObject> m_tileObjectPool = new List<TileObject>();

    private void Awake()
    {
        m_boardHandler = GetComponent<BoardHandler>();
    }

    private void Start()
    {
        m_board = new BoardState(m_tileRows, m_tileColumns);

        m_boardHandler.Initialise(m_board);

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                GameObject tilePlaceholder = Instantiate(m_tilePlaceholder, transform);

                m_board.TilePlaceholders[i, j] = tilePlaceholder.transform;
            }
        }

        StartCoroutine(SpawnTileRoutine());
    }

    private void OnDisable()
    {
        foreach (TileObject tileObject in m_board.BoardTileObjects)
        {
            tileObject.DeregisterOnSelect(m_boardHandler.OnTileSelected);
        }
    }

    private IEnumerator SpawnTileRoutine()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                Vector3 spawnPosition = m_board.TilePlaceholders[i, j].position;
                spawnPosition.y = m_tileSpawnLocation.position.y;

                TileObject tileObject = Instantiate(m_tilePrefab, spawnPosition,
                    Quaternion.identity, transform.parent);

                tileObject.InitialiseTile(GetRandomTileItem());
                tileObject.EnableSelection(false);
                tileObject.TileIndex = new Vector2Int(i, j);
                tileObject.RegisterOnSelect(m_boardHandler.OnTileSelected);

                m_board.BoardTileObjects[i, j] = tileObject;
            }
        }

        FixMatchingTiles();

        for (int i = 0; i < m_tileObjectPoolCount; i++)
        {
            TileObject tileObject = Instantiate(m_tilePrefab, transform.parent);
            tileObject.RegisterOnSelect(m_boardHandler.OnTileSelected);
            tileObject.gameObject.SetActive(false);

            m_tileObjectPool.Add(tileObject);
        }

        m_boardHandler.PopulateBoard();
    }

    public TileItem GetRandomTileItem()
    {
        return m_tileItems[Random.Range(0, m_tileItems.Length)];
    }

    public void FixMatchingTiles()
    {
        bool matchesFound;
        int loopCount = 0;

        do
        {
            matchesFound = false;
            loopCount++;

            bool adjustMatch = loopCount > k_loopCheckBuffer;

            for (int i = 0; i < m_tileRows; i++)
            {
                for (int j = 0; j < m_tileColumns; j++)
                {
                    if (i < m_tileRows - 2)
                    {
                        if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j].TileItem?.Name &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 2, j].TileItem?.Name)
                        {
                            if (adjustMatch)
                            {
                                ReplaceTileItem(i, j);
                                adjustMatch = false;
                            }
                            else
                            {
                                TileObject swapTile;

                                if (j == 0)
                                {
                                    swapTile = m_board.BoardTileObjects[i, j + 1];
                                }
                                else if (j > 0 && j < m_tileColumns - 1)
                                {
                                    int swapIndex = Math.Sign(Random.value - 1f);
                                    swapTile = m_board.BoardTileObjects[i, j + swapIndex];
                                }
                                else
                                {
                                    swapTile = m_board.BoardTileObjects[i, j - 1];
                                }

                                m_board.SwapTileData(m_board.BoardTileObjects[i, j], swapTile);

                                matchesFound = true;
                            }
                        }
                    }

                    if (j < m_tileColumns - 2)
                    {
                        if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j + 1].TileItem?.Name &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j + 2].TileItem?.Name)
                        {
                            if (adjustMatch)
                            {
                                ReplaceTileItem(i, j);
                                adjustMatch = false;
                            }
                            else
                            {
                                TileObject swapTile;

                                if (i == 0)
                                {
                                    swapTile = m_board.BoardTileObjects[i + 1, j];
                                }
                                else if (i > 0 && i < m_tileRows - 1)
                                {
                                    int swapIndex = Math.Sign(Random.value - 1f);
                                    swapTile = m_board.BoardTileObjects[i + swapIndex, j];
                                }
                                else
                                {
                                    swapTile = m_board.BoardTileObjects[i - 1, j];
                                }

                                m_board.SwapTileData(m_board.BoardTileObjects[i, j], swapTile);

                                matchesFound = true;
                            }
                        }
                    }
                }
            }

        } while (matchesFound);
    }

    private void ReplaceTileItem(int i, int j)
    {
        TileItem replacementItem = GetRandomTileItem();
        while (m_board.BoardTileObjects[i, j].TileItem.Name == replacementItem.Name)
            replacementItem = GetRandomTileItem();

        m_board.BoardTileObjects[i, j].InitialiseTile(replacementItem);
    }

    public void AddToTilePool(TileObject tile)
    {
        m_tileObjectPool.Add(tile);
    }

    public TileObject GetPooledTile()
    {
        TileObject poolTile = null;
        foreach (TileObject tileObject in m_tileObjectPool)
        {
            if (!tileObject.gameObject.activeSelf)
            {
                poolTile = tileObject;
            }
        }

        if (poolTile != null)
        {
            m_tileObjectPool.Remove(poolTile);
            poolTile.gameObject.SetActive(true);
            return poolTile;
        }

        TileObject newTile = Instantiate(m_tilePrefab, transform.parent);
        newTile.RegisterOnSelect(m_boardHandler.OnTileSelected);
        return newTile;
    }
    
    public void ReinitialiseBoard()
    {
        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                ReinitialiseTile(i, j);
            }
        }

        FixMatchingTiles();
    }

    public TileObject ReinitialiseTile(int row, int column)
    {
        Vector3 spawnPosition = m_board.TilePlaceholders[row, column].position;
        spawnPosition.y = m_tileSpawnLocation.position.y;

        TileObject replacementTile = GetPooledTile();
        replacementTile.transform.position = spawnPosition;

        replacementTile.InitialiseTile(GetRandomTileItem());
        replacementTile.EnableSelection(false);

        replacementTile.TileIndex = new Vector2Int(row, column);
        m_board.BoardTileObjects[row, column] = replacementTile;

        return replacementTile;
    }
}
