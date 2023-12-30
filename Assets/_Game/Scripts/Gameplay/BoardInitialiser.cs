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

    [SerializeField] private TileBehaviour m_tilePrefab;
    [SerializeField] private DishManager m_dishManager;
    [SerializeField] private TileItem[] m_tileItems;
    [SerializeField] private int m_tileItemsRosterSize = 5;
    [SerializeField] private GameObject m_tilePlaceholder;
    [SerializeField] private Transform m_tileSpawnLocation;
    [SerializeField] private int m_tileObjectPoolCount = 12;

    private BoardState m_board;

    private BoardHandler m_boardHandler;

    private List<TileItem> m_tileItemsRoster = new List<TileItem>();

    private List<TileBehaviour> m_tileObjectPool = new List<TileBehaviour>();

    private void Awake()
    {
        m_boardHandler = GetComponent<BoardHandler>();
    }

    private void Start()
    {
        m_board = new BoardState(m_tileRows, m_tileColumns);

        m_boardHandler.Initialise(m_board, m_dishManager);

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                GameObject tilePlaceholder = Instantiate(m_tilePlaceholder, transform);

                m_board.TilePlaceholders[i, j] = tilePlaceholder.transform;
            }
        }

        AssignTileItems();

        StartCoroutine(SpawnTileRoutine());
    }

    private void OnDisable()
    {
        foreach (TileBehaviour tileObject in m_board.BoardTileObjects)
        {
            tileObject.DeregisterOnSelect(m_boardHandler.OnTileSelected);
        }
    }

    public void AssignTileItems()
    {
        m_tileItemsRoster.Clear();

        List<TileItem> newIngredients = m_dishManager.GetNewDish().GetIngredients();
        foreach (TileItem tileItem in newIngredients)
        {
            if (!m_tileItemsRoster.Contains(tileItem))
            {
                m_tileItemsRoster.Add(tileItem);
            }
        }

        while (m_tileItemsRoster.Count < m_tileItemsRosterSize)
        {
            TileItem newTile = m_tileItems[Random.Range(0, m_tileItems.Length)];

            if (!m_tileItemsRoster.Contains(newTile))
            {
                m_tileItemsRoster.Add(newTile);
            }
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

                TileBehaviour tileBehaviour = Instantiate(m_tilePrefab, spawnPosition,
                    Quaternion.identity, transform.parent);

                tileBehaviour.InitialiseTile(GetRandomTileItem());
                tileBehaviour.EnableSelection(false);
                tileBehaviour.TileIndex = new Vector2Int(i, j);
                tileBehaviour.RegisterOnSelect(m_boardHandler.OnTileSelected);

                m_board.BoardTileObjects[i, j] = tileBehaviour;
            }
        }

        FixMatchingTiles();

        for (int i = 0; i < m_tileObjectPoolCount; i++)
        {
            TileBehaviour tileBehaviour = Instantiate(m_tilePrefab, transform.parent);
            tileBehaviour.RegisterOnSelect(m_boardHandler.OnTileSelected);
            tileBehaviour.gameObject.SetActive(false);

            m_tileObjectPool.Add(tileBehaviour);
        }

        m_boardHandler.PopulateBoard();
    }

    public TileItem GetRandomTileItem()
    {
        return m_tileItemsRoster[Random.Range(0, m_tileItemsRoster.Count)];
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
                                TileBehaviour swapTile;

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
                                TileBehaviour swapTile;

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

    public void AddToTilePool(TileBehaviour tile)
    {
        m_tileObjectPool.Add(tile);
    }

    public TileBehaviour GetPooledTile()
    {
        TileBehaviour poolTile = null;
        foreach (TileBehaviour tileObject in m_tileObjectPool)
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

        TileBehaviour newTile = Instantiate(m_tilePrefab, transform.parent);
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

    public TileBehaviour ReinitialiseTile(int row, int column)
    {
        Vector3 spawnPosition = m_board.TilePlaceholders[row, column].position;
        spawnPosition.y = m_tileSpawnLocation.position.y;

        TileBehaviour replacementTile = GetPooledTile();
        replacementTile.transform.position = spawnPosition;

        replacementTile.InitialiseTile(GetRandomTileItem());
        replacementTile.EnableSelection(false);

        replacementTile.TileIndex = new Vector2Int(row, column);
        m_board.BoardTileObjects[row, column] = replacementTile;

        return replacementTile;
    }
}
