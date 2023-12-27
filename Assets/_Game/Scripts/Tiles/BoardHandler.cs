using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class BoardHandler : MonoBehaviour
{
    private const int k_loopCheckBuffer = 100;

    [SerializeField] private int m_tileRows = 5;
    [SerializeField] private int m_tileColumns = 12;
    [SerializeField] private TileObject m_tilePrefab;
    [SerializeField] private TileItem[] m_tileItems;
    [SerializeField] private GameObject m_tilePlaceholder;
    [SerializeField] private Transform m_tileSpawnLocation;
    [SerializeField] private Transform m_tileFallLocation;
    
    [Header("Tile Transitions")]
    [SerializeField] private int m_tileObjectPoolCount = 12;
    [SerializeField] private float m_spawnFallTime = 0.6f;
    [SerializeField] private float m_spawnNextColumnTime = 0.25f;
    [SerializeField] private float m_tileSwapTime = 1f;
    [SerializeField] private float m_matchedFallTime = 0.4f;
    [SerializeField] private float m_newMatchBufferTime = 0.2f;

    private Transform[,] m_tilePlaceholders;
    private TileObject[,] m_boardTileObjects;

    private List<TileObject> m_tileObjectPool = new List<TileObject>();
    
    private TileObject m_currentSelectedTile;

    private void Start()
    {
        m_boardTileObjects = new TileObject[m_tileRows, m_tileColumns];
        m_tilePlaceholders = new Transform[m_tileRows, m_tileColumns];

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                GameObject tilePlaceholder = Instantiate(m_tilePlaceholder, transform);

                m_tilePlaceholders[i, j] = tilePlaceholder.transform;
            }
        }

        StartCoroutine(TileSpawnRoutine());
    }

    private void Update()
    {
        //string log = "";
        //for (int i = 0; i < m_tileRows; i++)
        //{
        //    for (int j = 0; j < m_tileColumns; j++)
        //    {
        //        log +=
        //            $"[{i}, {j}] : {(m_boardTileObjects[i, j] != null ? m_boardTileObjects[i, j].TileItem : null)}";
        //    }
        //}

        //Debug.Log(log);
    }

    private void OnDisable()
    {
        foreach (TileObject tileObject in m_boardTileObjects)
        {
            tileObject.DeregisterOnSelect(OnTileSelected);
        }
    }

    private void FixMatchingTiles()
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
                        if (m_boardTileObjects[i, j].TileItem?.Name == m_boardTileObjects[i + 1, j].TileItem?.Name &&
                            m_boardTileObjects[i, j].TileItem?.Name == m_boardTileObjects[i + 2, j].TileItem?.Name)
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
                                    swapTile = m_boardTileObjects[i, j + 1];
                                }
                                else if (j > 0 && j < m_tileColumns - 1)
                                {
                                    int swapIndex = Math.Sign(Random.value - 1f);
                                    swapTile = m_boardTileObjects[i, j + swapIndex];
                                }
                                else
                                {
                                    swapTile = m_boardTileObjects[i, j - 1];
                                }

                                SwapTileData(m_boardTileObjects[i, j], swapTile);

                                matchesFound = true;
                            }
                        }
                    }

                    if (j < m_tileColumns - 2) 
                    {
                        if (m_boardTileObjects[i, j].TileItem?.Name == m_boardTileObjects[i, j + 1].TileItem?.Name &&
                            m_boardTileObjects[i, j].TileItem?.Name == m_boardTileObjects[i, j + 2].TileItem?.Name)
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
                                    swapTile = m_boardTileObjects[i + 1, j];
                                }
                                else if (i > 0 && i < m_tileRows - 1)
                                {
                                    int swapIndex = Math.Sign(Random.value - 1f);
                                    swapTile = m_boardTileObjects[i + swapIndex, j];
                                }
                                else
                                {
                                    swapTile = m_boardTileObjects[i - 1, j];
                                }

                                SwapTileData(m_boardTileObjects[i, j], swapTile);

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
        TileItem replacementItem = m_tileItems[Random.Range(0, m_tileItems.Length)];
        while (m_boardTileObjects[i, j].TileItem.Name == replacementItem.Name)
            replacementItem = m_tileItems[Random.Range(0, m_tileItems.Length)];

        m_boardTileObjects[i, j].InitialiseTile(replacementItem);
    }

    private IEnumerator TileSpawnRoutine()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                Vector3 spawnPosition = m_tilePlaceholders[i, j].position;
                spawnPosition.y = m_tileSpawnLocation.position.y;

                TileObject tileObject = Instantiate(m_tilePrefab, spawnPosition,
                    Quaternion.identity, transform.parent);

                tileObject.InitialiseTile(m_tileItems[Random.Range(0, m_tileItems.Length)]);
                tileObject.EnableSelection(false);
                tileObject.TileIndex = new Vector2Int(i, j);
                tileObject.RegisterOnSelect(OnTileSelected);

                m_boardTileObjects[i, j] = tileObject;
            }
        }

        FixMatchingTiles();

        for (int i = 0; i < m_tileObjectPoolCount; i++)
        {
            TileObject tileObject = Instantiate(m_tilePrefab, transform.parent);
            tileObject.RegisterOnSelect(OnTileSelected);
            tileObject.gameObject.SetActive(false);

            m_tileObjectPool.Add(tileObject);
        }

        WaitForSeconds waitForSeconds = new WaitForSeconds(m_spawnNextColumnTime);

        for (int j = 0; j < m_tileColumns; j++)
        {
            for (int i = 0; i < m_tileRows; i++)
            {
                Vector3 finalPosition = m_tilePlaceholders[i, j].position;
                m_boardTileObjects[i, j].transform.DOMove(finalPosition, m_spawnFallTime).SetEase(Ease.OutBack);
            }

            yield return waitForSeconds;
        }
    }

    private void OnTileSelected(Vector2Int tileIndex)
    {
        TileObject newSelectedTile = m_boardTileObjects[tileIndex.x, tileIndex.y];

        if (m_currentSelectedTile == null)
        {
            m_currentSelectedTile = newSelectedTile;
            m_currentSelectedTile.EnableSelection(true);
        }
        else if (m_currentSelectedTile != newSelectedTile)
        {
            Vector2Int currentIndex = m_currentSelectedTile.TileIndex;
            Vector2Int newIndex = newSelectedTile.TileIndex;

            if ((newIndex.x == currentIndex.x - 1 || newIndex.x == currentIndex.x + 1) !=
                (newIndex.y == currentIndex.y - 1 || newIndex.y == currentIndex.y + 1) &&
                MathF.Abs(Vector2Int.Distance(currentIndex, newIndex)) - Mathf.Epsilon <= 1f)  
            {
                m_currentSelectedTile.EnableSelection(false);
                SwapTiles(m_currentSelectedTile, newSelectedTile);

                m_currentSelectedTile = null;
            }
            else
            {
                m_currentSelectedTile.EnableSelection(false);
                
                m_currentSelectedTile = newSelectedTile;
                m_currentSelectedTile.EnableSelection(true);
            }
        }
        else 
        {
            m_currentSelectedTile.EnableSelection(false);
            m_currentSelectedTile = null;
        }
    }

    private void SwapTiles(TileObject tileA, TileObject tileB, bool swapback = false)
    {
        Vector3 targetA = tileB.transform.position;
        Vector3 targetB = tileA.transform.position;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(tileA.transform.DOMove(targetA, m_tileSwapTime).SetEase(Ease.OutBack));

        sequence.Join(tileB.transform.DOMove(targetB, m_tileSwapTime).SetEase(Ease.OutBack));

        sequence.OnComplete(() =>
        {
            SwapTileData(tileA, tileB);

            if (!swapback)
            {
                bool matched = false;
                List<TileObject> matchingTiles = new List<TileObject>();

                if (CheckMatches(tileA.TileIndex, out List<TileObject> matchingTilesA)) 
                {
                    matchingTiles.AddRange(matchingTilesA);
                    matched = true;
                }
                
                if (CheckMatches(tileB.TileIndex, out List<TileObject> matchingTilesB))
                {
                    matchingTiles.AddRange(matchingTilesB);
                    matched = true;
                }

                if (matched)
                {
                    HandleMatchedTiles(matchingTiles);
                }
                else
                {
                    SwapTiles(tileB, tileA, true);
                }
            }
        });

        sequence.Play();
    }

    private void SwapTileData(TileObject tileA, TileObject tileB)
    {
        Vector2Int tempIndex = tileB.TileIndex;

        m_boardTileObjects[tileA.TileIndex.x, tileA.TileIndex.y] = tileB;
        tileB.TileIndex = tileA.TileIndex;

        m_boardTileObjects[tempIndex.x, tempIndex.y] = tileA;
        tileA.TileIndex = tempIndex;
    }

    private TileObject GetPooledTile()
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
        newTile.RegisterOnSelect(OnTileSelected);
        return newTile;
    }

    private bool CheckMatches(Vector2Int index, out List<TileObject> matchingTiles)
    {
        HashSet<TileObject> horizontalTiles = new HashSet<TileObject> { m_boardTileObjects[index.x, index.y] };
        horizontalTiles = CheckMatchesHorizontal(index, horizontalTiles);
        if (horizontalTiles.Count < 3)
        {
            horizontalTiles.Clear();
        }

        HashSet<TileObject> verticalTiles = new HashSet<TileObject> { m_boardTileObjects[index.x, index.y] };
        verticalTiles = CheckMatchesVertical(index, verticalTiles);
        if (verticalTiles.Count < 3)
        {
            verticalTiles.Clear();
        }

        HashSet<TileObject> matchingTilesSet = new HashSet<TileObject>();
        matchingTilesSet.AddRange(horizontalTiles);
        matchingTilesSet.AddRange(verticalTiles);

        matchingTiles = new List<TileObject>();
        matchingTiles.AddRange(matchingTilesSet);

        return matchingTiles.Count > 0;
    }

    private HashSet<TileObject> CheckMatchesHorizontal(Vector2Int index, HashSet<TileObject> matchingTiles)
    {
        if (index.x > 0)
        {
            Vector2Int checkIndex = index - new Vector2Int(1, 0);

            if (!matchingTiles.Contains(m_boardTileObjects[checkIndex.x, checkIndex.y])) 
            {
                if (m_boardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_boardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_boardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesHorizontal(checkIndex, matchingTiles);
                }
            }
        }

        if (index.x < m_tileRows - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(1, 0);

            if (!matchingTiles.Contains(m_boardTileObjects[checkIndex.x, checkIndex.y])) 
            {
                if (m_boardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_boardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_boardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesHorizontal(checkIndex, matchingTiles);
                }
            }
        }

        return matchingTiles;
    }
    
    private HashSet<TileObject> CheckMatchesVertical(Vector2Int index, HashSet<TileObject> matchingTiles)
    {
        if (index.y > 0)
        {
            Vector2Int checkIndex = index - new Vector2Int(0, 1);
            
            if (!matchingTiles.Contains(m_boardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_boardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_boardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_boardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesVertical(checkIndex, matchingTiles);
                }
            }
        }

        if (index.y < m_tileColumns - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(0, 1);

            if (!matchingTiles.Contains(m_boardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_boardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_boardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_boardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesVertical(checkIndex, matchingTiles);
                }
            }
        }

        return matchingTiles;
    }

    private void HandleMatchedTiles(List<TileObject> tileObjects)
    {
        foreach (TileObject tileObject in tileObjects)
        {
            Vector3 fallPosition = tileObject.transform.position;
            fallPosition.y = m_tileFallLocation.position.y - fallPosition.y;

            tileObject.transform.DOMove(fallPosition, m_matchedFallTime)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    tileObject.gameObject.SetActive(false);
                    m_tileObjectPool.Add(tileObject);
                });

            m_boardTileObjects[tileObject.TileIndex.x, tileObject.TileIndex.y] = null;
        }

        List<TileObject> changedTiles = new List<TileObject>();

        foreach (TileObject tileObject in tileObjects)
        {
            Vector2Int tileIndex = tileObject.TileIndex;

            for (int i = tileIndex.x - 1; i >= 0; i--)
            {
                if (m_boardTileObjects[i, tileIndex.y] != null)
                {
                    if (m_boardTileObjects[i + 1, tileIndex.y] != null)
                    {
                        continue;
                    }

                    Vector2Int destIndex = new Vector2Int(i + 1, tileIndex.y);
                    for (int k = i + 2; k < m_tileRows; k++)
                    {
                        if (m_boardTileObjects[k, tileIndex.y] == null)
                        {
                            destIndex = new Vector2Int(k, tileIndex.y);
                        }
                    }

                    TileObject fallingTile = m_boardTileObjects[i, tileIndex.y];
                    changedTiles.Add(fallingTile);
                    
                    fallingTile.TileIndex = destIndex;
                    m_boardTileObjects[destIndex.x, destIndex.y] = fallingTile;

                    m_boardTileObjects[i, tileIndex.y] = null;
                }
            }
        }

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                if (m_boardTileObjects[i, j] == null) 
                {
                    Vector3 spawnPosition = m_tilePlaceholders[i, j].position;
                    spawnPosition.y = m_tileSpawnLocation.position.y;

                    TileObject replacementTile = GetPooledTile();
                    changedTiles.Add(replacementTile);
                    replacementTile.transform.position = spawnPosition;

                    replacementTile.InitialiseTile(m_tileItems[Random.Range(0, m_tileItems.Length)]);
                    replacementTile.EnableSelection(false);
                    
                    replacementTile.TileIndex = new Vector2Int(i, j);
                    m_boardTileObjects[i, j] = replacementTile;
                }
            }
        }

        foreach (TileObject changedTile in changedTiles)
        {
            Vector3 finalPosition = m_tilePlaceholders[changedTile.TileIndex.x, changedTile.TileIndex.y].position;
            changedTile.transform.DOMove(finalPosition, m_spawnFallTime).SetEase(Ease.OutBack);
        }

        StartCoroutine(CheckNewMatchesRoutine(changedTiles));
    }

    private IEnumerator CheckNewMatchesRoutine(List<TileObject> tileObjects)
    {
        yield return new WaitForSeconds(m_spawnFallTime + m_newMatchBufferTime);

        foreach (TileObject tileObject in tileObjects)
        {
            if (CheckMatches(tileObject.TileIndex, out List<TileObject> matchingTiles))
            {
                HandleMatchedTiles(matchingTiles);
            }
        }
    }
}
