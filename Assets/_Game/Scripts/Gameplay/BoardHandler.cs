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
    [SerializeField] private float m_spawnFallTime = 0.6f;
    [SerializeField] private float m_spawnNextColumnTime = 0.25f;
    [SerializeField] private float m_tileSwapTime = 1f;
    [SerializeField] private float m_matchedFallTime = 0.4f;
    [SerializeField] private float m_newMatchBufferTime = 0.2f;
    [SerializeField] private float m_repopulateBufferTime = 0.1f;
    [SerializeField] private Transform m_tileFallLocation;

    private BoardState m_board;

    private BoardInitialiser m_boardInitialiser;
    private MatchHandler m_matchHandler;

    private TileBehaviour m_currentSelectedTile;

    private DishManager m_dishManager;

    private int m_newMatchesCheckRoutines = 0;

    private void Awake()
    {
        m_boardInitialiser = GetComponent<BoardInitialiser>();
        m_matchHandler = GetComponent<MatchHandler>();
    }

    public void Initialise(BoardState board, DishManager dishManager)
    {
        m_board = board;
        m_dishManager = dishManager;

        m_matchHandler.Initialise(board);
    }

    public void PopulateBoard()
    {
        StartCoroutine(PopulateBoardRoutine());
    }

    private IEnumerator PopulateBoardRoutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(m_spawnNextColumnTime);

        for (int j = 0; j < m_board.Columns; j++)
        {
            for (int i = 0; i < m_board.Rows; i++)
            {
                Vector3 finalPosition = m_board.TilePlaceholders[i, j].position;
                m_board.BoardTileObjects[i, j].transform.DOMove(finalPosition, m_spawnFallTime).SetEase(Ease.OutBack);
            }

            yield return waitForSeconds;
        }
    }

    public void OnTileSelected(Vector2Int tileIndex)
    {
        TileBehaviour newSelectedTile = m_board.BoardTileObjects[tileIndex.x, tileIndex.y];

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

    private void SwapTiles(TileBehaviour tileA, TileBehaviour tileB, bool swapback = false)
    {
        Vector3 targetA = tileB.transform.position;
        Vector3 targetB = tileA.transform.position;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(tileA.transform.DOMove(targetA, m_tileSwapTime).SetEase(Ease.OutBack));

        sequence.Join(tileB.transform.DOMove(targetB, m_tileSwapTime).SetEase(Ease.OutBack));

        sequence.OnComplete(() =>
        {
            m_board.SwapTileData(tileA, tileB);

            if (!swapback)
            {
                bool matched = false;
                List<TileBehaviour> matchingTiles = new List<TileBehaviour>();

                if (m_matchHandler.HasMatches(tileA.TileIndex, out List<TileBehaviour> matchingTilesA)) 
                {
                    matchingTiles.AddRange(matchingTilesA);
                    matched = true;
                }
                
                if (m_matchHandler.HasMatches(tileB.TileIndex, out List<TileBehaviour> matchingTilesB))
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

    private void HandleMatchedTiles(List<TileBehaviour> matchedTiles)
    {
        foreach (TileBehaviour matchedTile in matchedTiles)
        {
            if (m_board.BoardTileObjects[matchedTile.TileIndex.x, matchedTile.TileIndex.y] != null &&
                m_board.BoardTileObjects[matchedTile.TileIndex.x, matchedTile.TileIndex.y] == matchedTile)
                HandleFallingTile(matchedTile);
        }
        
        List<TileBehaviour> changedTiles = new List<TileBehaviour>();

        foreach (TileBehaviour tileObject in matchedTiles)
        {
            Vector2Int tileIndex = tileObject.TileIndex;

            for (int i = tileIndex.x - 1; i >= 0; i--)
            {
                if (m_board.BoardTileObjects[i, tileIndex.y] != null)
                {
                    if (m_board.BoardTileObjects[i + 1, tileIndex.y] != null)
                    {
                        continue;
                    }

                    Vector2Int destIndex = new Vector2Int(i + 1, tileIndex.y);
                    for (int k = i + 2; k < m_board.Rows; k++)
                    {
                        if (m_board.BoardTileObjects[k, tileIndex.y] == null)
                        {
                            destIndex = new Vector2Int(k, tileIndex.y);
                        }
                    }

                    TileBehaviour fallingTile = m_board.BoardTileObjects[i, tileIndex.y];
                    changedTiles.Add(fallingTile);
                    
                    fallingTile.TileIndex = destIndex;
                    m_board.BoardTileObjects[destIndex.x, destIndex.y] = fallingTile;

                    m_board.BoardTileObjects[i, tileIndex.y] = null;
                }
            }
        }

        for (int i = 0; i < m_board.Rows; i++)
        {
            for (int j = 0; j < m_board.Columns; j++)
            {
                if (m_board.BoardTileObjects[i, j] == null)
                {
                    TileBehaviour replacementTile = m_boardInitialiser.ReinitialiseTile(i, j);
                    changedTiles.Add(replacementTile);
                }
            }
        }

        foreach (TileBehaviour changedTile in changedTiles)
        {
            Vector3 finalPosition = m_board.TilePlaceholders[changedTile.TileIndex.x, changedTile.TileIndex.y].position;
            changedTile.transform.DOMove(finalPosition, m_spawnFallTime).SetEase(Ease.OutBack);
        }

        if (m_dishManager.IsDishCleared(matchedTiles))
        {
            m_newMatchesCheckRoutines = 0;
            StopAllCoroutines();

            StartCoroutine(ProcessNewDish());
        }
        else
        {
            m_newMatchesCheckRoutines++;
            StartCoroutine(CheckNewMatchesRoutine(changedTiles));
        }
    }

    public void HandleFallingTile(TileBehaviour fallingTile)
    {
        Vector3 fallPosition = fallingTile.transform.position;
        fallPosition.y = m_tileFallLocation.position.y - fallPosition.y;

        fallingTile.transform.DOMove(fallPosition, m_matchedFallTime)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                fallingTile.gameObject.SetActive(false);
                m_boardInitialiser.AddToTilePool(fallingTile);
            });

        m_board.BoardTileObjects[fallingTile.TileIndex.x, fallingTile.TileIndex.y] = null;
    }

    private IEnumerator CheckNewMatchesRoutine(List<TileBehaviour> changedTiles)
    {
        yield return new WaitForSeconds(m_spawnFallTime + m_newMatchBufferTime);

        HashSet<TileBehaviour> allMatchingTiles = new HashSet<TileBehaviour>();

        foreach (TileBehaviour changedTile in changedTiles)
        {
            if (m_matchHandler.HasMatches(changedTile.TileIndex, out List<TileBehaviour> matchingTiles))
            {
                allMatchingTiles.AddRange(matchingTiles);

                //string debug = $"{Time.time}:\n";
                //foreach (TileBehaviour tile in matchingTiles)
                //{
                //    debug += $"{tile.TileIndex} : {tile.TileItem.Name}\t";
                //}
                //Debug.Log(debug);
            }
        }

        List<TileBehaviour> allMatchingTilesList = new List<TileBehaviour>();
        allMatchingTilesList.AddRange(allMatchingTiles);
        HandleMatchedTiles(allMatchingTilesList);

        yield return new WaitForSeconds(m_repopulateBufferTime);

        m_newMatchesCheckRoutines = Mathf.Max(0, m_newMatchesCheckRoutines - 1);
        if (m_newMatchesCheckRoutines == 0 && !m_matchHandler.HasPotentialMatches())
        {
            StartCoroutine(RepopulateBoardRoutine());
        }
    }

    private IEnumerator ProcessNewDish()
    {
        m_boardInitialiser.AssignTileItems();

        yield return new WaitForSeconds(m_spawnFallTime + m_newMatchBufferTime);

        yield return RepopulateBoardRoutine();
    }

    private IEnumerator RepopulateBoardRoutine()
    {
        for (int i = 0; i < m_board.Rows; i++)
        {
            for (int j = 0; j < m_board.Columns; j++)
            {
                TileBehaviour tile = m_board.BoardTileObjects[i, j];
                HandleFallingTile(tile);
            }
        }

        yield return new WaitForSeconds(m_matchedFallTime);

        m_boardInitialiser.ReinitialiseBoard();

        PopulateBoard();
    }
}
