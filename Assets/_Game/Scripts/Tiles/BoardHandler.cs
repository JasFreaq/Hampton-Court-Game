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
    [SerializeField] private TileSubstitute m_tileSubstitute;
    [SerializeField] private Transform m_tileSpawnLocation;
    [SerializeField] private Transform m_tileFallLocation;
    
    [Header("Tile Transitions")]
    [SerializeField] private int m_substitutePoolCount = 12;
    [SerializeField] private float m_spawnFallTime = 0.8f;
    [SerializeField] private float m_spawnNextColumnTime = 0.25f;
    [SerializeField] private float m_tileSwapTime = 1f;
    [SerializeField] private float m_matchedFallTime = 0.6f;

    private TileObject[,] m_tileObjects;

    private List<TileSubstitute> m_tileSubstitutes = new List<TileSubstitute>();

    private TileObject m_currentSelectedTile;

    private void Start()
    {
        m_tileObjects = new TileObject[m_tileRows, m_tileColumns];

        for (int i = 0; i < m_tileRows; i++)
        {
            for (int j = 0; j < m_tileColumns; j++)
            {
                TileObject tileObject = Instantiate(m_tilePrefab, transform);

                tileObject.InitialiseTile(m_tileItems[Random.Range(0, m_tileItems.Length)]);
                tileObject.EnableSelection(false);
                tileObject.EnableImage(false);
                tileObject.TileIndex = new Vector2Int(i, j);
                tileObject.RegisterOnSelect(OnTileSelected);

                m_tileObjects[i, j] = tileObject;
            }
        }

        FixMatchingTiles();

        StartCoroutine(TilePlacementRoutine(true));
    }

    private void OnDisable()
    {
        foreach (TileObject tileObject in m_tileObjects)
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
                        if (m_tileObjects[i, j].TileItem.Name == m_tileObjects[i + 1, j].TileItem.Name &&
                            m_tileObjects[i, j].TileItem.Name == m_tileObjects[i + 2, j].TileItem.Name)
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
                                    swapTile = m_tileObjects[i, j + 1];
                                }
                                else if (j > 0 && j < m_tileColumns - 1)
                                {
                                    int swapIndex = 1 * Math.Sign(Random.value - 1f);
                                    swapTile = m_tileObjects[i, j + swapIndex];
                                }
                                else
                                {
                                    swapTile = m_tileObjects[i, j - 1];
                                }

                                TileObject.Swap(m_tileObjects[i, j], swapTile);

                                matchesFound = true;
                            }
                        }
                    }

                    if (j < m_tileColumns - 2) 
                    {
                        if (m_tileObjects[i, j].TileItem.Name == m_tileObjects[i, j + 1].TileItem.Name &&
                            m_tileObjects[i, j].TileItem.Name == m_tileObjects[i, j + 2].TileItem.Name)
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
                                    swapTile = m_tileObjects[i + 1, j];
                                }
                                else if (i > 0 && i < m_tileRows - 1)
                                {
                                    int swapIndex = 1 * Math.Sign(Random.value - 1f);
                                    swapTile = m_tileObjects[i + swapIndex, j];
                                }
                                else
                                {
                                    swapTile = m_tileObjects[i - 1, j];
                                }

                                TileObject.Swap(m_tileObjects[i, j], swapTile);

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
        while (m_tileObjects[i, j].TileItem.Name == replacementItem.Name)
            replacementItem = m_tileItems[Random.Range(0, m_tileItems.Length)];

        m_tileObjects[i, j].InitialiseTile(replacementItem);
    }
    
    private IEnumerator TilePlacementRoutine(bool setup)
    {
        yield return new WaitForEndOfFrame();

        WaitForSeconds waitForSeconds = new WaitForSeconds(m_spawnNextColumnTime);

        for (int j = 0; j < m_tileColumns; j++)
        {
            bool fallingInColumn = setup;

            for (int i = 0; i < m_tileRows; i++)
            {
                TileObject tileObject = m_tileObjects[i, j];
                
                if ((setup && tileObject.Validated) || (!setup && !tileObject.Validated)) 
                {
                    if (!setup)
                    {
                        TileItem newItem = m_tileItems[Random.Range(0, m_tileItems.Length)];
                        tileObject.InitialiseTile(newItem);

                        fallingInColumn = true;
                    }

                    Vector3 substitutePosition = tileObject.transform.position;
                    substitutePosition.y = m_tileSpawnLocation.position.y;

                    TileSubstitute substitute = Instantiate(m_tileSubstitute, substitutePosition, Quaternion.identity,
                        transform.parent);

                    bool addedToPool = false;
                    if (m_tileSubstitutes.Count < m_substitutePoolCount)
                    {
                        m_tileSubstitutes.Add(substitute);
                        addedToPool = true;
                    }

                    substitute.InitialiseSubstitute(tileObject.TileItem.Image);
                    
                    substitute.transform.DOMove(tileObject.transform.position, m_spawnFallTime)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            if (addedToPool)
                                substitute.gameObject.SetActive(false);
                            else
                                Destroy(substitute.gameObject);

                            if (!setup && CheckMatches(tileObject.TileIndex, out List<TileObject> matchingTiles))
                            {
                                HandleMatchedTiles(matchingTiles);
                                StartCoroutine(TilePlacementRoutine(false));
                            }

                            tileObject.EnableImage(true);
                        });
                }
            }

            if (fallingInColumn)
                yield return waitForSeconds;
        }
    }

    private void OnTileSelected(Vector2Int tileIndex)
    {
        TileObject newSelectedTile = m_tileObjects[tileIndex.x, tileIndex.y];
        
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
    }

    private void SwapTiles(TileObject tileA, TileObject tileB, bool swapback = false)
    {
        tileA.EnableImage(false);
        tileB.EnableImage(false);

        TileSubstitute substituteA = GetAvailableSubstitute();
        substituteA.InitialiseSubstitute(tileA.TileItem.Image);
        substituteA.transform.position = tileA.transform.position;
        substituteA.gameObject.SetActive(true);

        TileSubstitute substituteB = GetAvailableSubstitute();
        substituteB.InitialiseSubstitute(tileB.TileItem.Image);
        substituteB.transform.position = tileB.transform.position;
        substituteB.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        sequence.Join(substituteA.transform.DOMove(tileB.transform.position, m_tileSwapTime)
            .SetEase(Ease.OutBack));

        sequence.Join(substituteB.transform.DOMove(tileA.transform.position, m_tileSwapTime)
            .SetEase(Ease.OutBack));

        sequence.OnComplete(() =>
        {
            substituteA.gameObject.SetActive(false);
            substituteB.gameObject.SetActive(false);

            TileObject.Swap(tileA, tileB);

            tileA.EnableImage(true);
            tileB.EnableImage(true);

            if (!swapback)
            {
                bool matched = false;

                if (CheckMatches(tileA.TileIndex, out List<TileObject> matchingTilesA)) 
                {
                    HandleMatchedTiles(matchingTilesA);
                    matched = true;
                }
                
                if (CheckMatches(tileB.TileIndex, out List<TileObject> matchingTilesB))
                {
                    HandleMatchedTiles(matchingTilesB);
                    matched = true;
                }

                if (matched)
                {
                    StartCoroutine(TilePlacementRoutine(false));
                }
                else
                {
                    SwapTiles(tileB, tileA, true);
                }
            }
        });

        sequence.Play();
    }
    
    private TileSubstitute GetAvailableSubstitute()
    {
        foreach (TileSubstitute substitute in m_tileSubstitutes)
        {
            if (!substitute.gameObject.activeSelf)
            {
                return substitute;
            }
        }

        TileSubstitute newSubstitute = Instantiate(m_tileSubstitute, transform.parent);
        m_tileSubstitutes.Add(newSubstitute);

        return newSubstitute;
    }

    private bool CheckMatches(Vector2Int index, out List<TileObject> matchingTiles)
    {
        HashSet<TileObject> horizontalTiles = new HashSet<TileObject> { m_tileObjects[index.x, index.y] };
        horizontalTiles = CheckMatchesHorizontal(index, horizontalTiles);
        if (horizontalTiles.Count < 3)
        {
            horizontalTiles.Clear();
        }

        HashSet<TileObject> verticalTiles = new HashSet<TileObject> { m_tileObjects[index.x, index.y] };
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

            if (!matchingTiles.Contains(m_tileObjects[checkIndex.x, checkIndex.y])) 
            {
                if (m_tileObjects[index.x, index.y].TileItem?.Name ==
                    m_tileObjects[checkIndex.x, checkIndex.y].TileItem?.Name)
                {
                    matchingTiles.Add(m_tileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesHorizontal(checkIndex, matchingTiles);
                }
            }
        }

        if (index.x < m_tileRows - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(1, 0);

            if (!matchingTiles.Contains(m_tileObjects[checkIndex.x, checkIndex.y])) 
            {
                if (m_tileObjects[index.x, index.y].TileItem?.Name ==
                    m_tileObjects[checkIndex.x, checkIndex.y].TileItem?.Name)
                {
                    matchingTiles.Add(m_tileObjects[checkIndex.x, checkIndex.y]);
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
            
            if (!matchingTiles.Contains(m_tileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_tileObjects[index.x, index.y].TileItem?.Name ==
                    m_tileObjects[checkIndex.x, checkIndex.y].TileItem?.Name)
                {
                    matchingTiles.Add(m_tileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesVertical(checkIndex, matchingTiles);
                }
            }
        }

        if (index.y < m_tileColumns - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(0, 1);

            if (!matchingTiles.Contains(m_tileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_tileObjects[index.x, index.y].TileItem?.Name ==
                    m_tileObjects[checkIndex.x, checkIndex.y].TileItem?.Name)
                {
                    matchingTiles.Add(m_tileObjects[checkIndex.x, checkIndex.y]);
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
            Vector2Int tileIndex = tileObject.TileIndex;

            TileSubstitute fallSubstitute = GetAvailableSubstitute();

            fallSubstitute.InitialiseSubstitute(tileObject.TileItem.Image);
            Debug.Log($"{tileObject.TileItem.Image}");
            fallSubstitute.transform.position = tileObject.transform.position;
            fallSubstitute.gameObject.SetActive(true);

            Vector3 fallPosition = tileObject.transform.position;
            fallPosition.y = m_tileFallLocation.position.y - fallPosition.y;
            
            fallSubstitute.transform.DOMove(fallPosition, m_matchedFallTime)
                .SetEase(Ease.InBack)
                .OnComplete(() => fallSubstitute.gameObject.SetActive(false));

            tileObject.InvalidateTile();
            tileObject.EnableImage(false);
        }

        foreach (TileObject tileObject in tileObjects)
        {
            Vector2Int tileIndex = tileObject.TileIndex;

            for (int i = tileIndex.x - 1; i >= 0; i--)
            {
                TileObject fallingTile = m_tileObjects[i, tileIndex.y];
                if (fallingTile.Validated)
                {
                    TileSubstitute fallSubstitute = GetAvailableSubstitute();
                    fallSubstitute.InitialiseSubstitute(fallingTile.TileItem.Image);
                    fallSubstitute.transform.position = fallingTile.transform.position;
                    fallSubstitute.gameObject.SetActive(true);

                    TileObject destinationTile = m_tileObjects[i + 1, tileIndex.y];
                    for (int k = i + 2; k < m_tileRows; k++)
                    {
                        TileObject nextTile = m_tileObjects[k, tileIndex.y];
                        if (!nextTile.Validated)
                        {
                            destinationTile = nextTile;
                        }
                    }

                    fallSubstitute.transform.DOMove(destinationTile.transform.position, m_matchedFallTime)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            fallSubstitute.gameObject.SetActive(false);
                            destinationTile.EnableImage(true);
                        });

                    destinationTile.InitialiseTile(fallingTile.TileItem);

                    fallingTile.InvalidateTile();
                    fallingTile.EnableImage(false);
                }
            }
        }
    }
}
