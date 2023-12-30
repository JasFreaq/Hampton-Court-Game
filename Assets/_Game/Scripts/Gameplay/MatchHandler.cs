using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchHandler : MonoBehaviour
{
    private BoardState m_board;

    public void Initialise(BoardState board)
    {
        m_board = board;
    }

    public bool HasMatches(Vector2Int index, out List<TileBehaviour> matchingTiles)
    {
        HashSet<TileBehaviour> horizontalTiles = new HashSet<TileBehaviour> { m_board.BoardTileObjects[index.x, index.y] };
        horizontalTiles = CheckMatchesHorizontal(index, horizontalTiles);
        if (horizontalTiles.Count < 3)
        {
            horizontalTiles.Clear();
        }

        HashSet<TileBehaviour> verticalTiles = new HashSet<TileBehaviour> { m_board.BoardTileObjects[index.x, index.y] };
        verticalTiles = CheckMatchesVertical(index, verticalTiles);
        if (verticalTiles.Count < 3)
        {
            verticalTiles.Clear();
        }

        HashSet<TileBehaviour> matchingTilesSet = new HashSet<TileBehaviour>();
        matchingTilesSet.AddRange(horizontalTiles);
        matchingTilesSet.AddRange(verticalTiles);

        matchingTiles = new List<TileBehaviour>();
        matchingTiles.AddRange(matchingTilesSet);

        return matchingTiles.Count > 0;
    }

    public HashSet<TileBehaviour> CheckMatchesHorizontal(Vector2Int index, HashSet<TileBehaviour> matchingTiles)
    {
        if (index.x > 0)
        {
            Vector2Int checkIndex = index - new Vector2Int(1, 0);

            if (!matchingTiles.Contains(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_board.BoardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_board.BoardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesHorizontal(checkIndex, matchingTiles);
                }
            }
        }

        if (index.x < m_board.Rows - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(1, 0);

            if (!matchingTiles.Contains(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_board.BoardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_board.BoardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesHorizontal(checkIndex, matchingTiles);
                }
            }
        }

        return matchingTiles;
    }

    public HashSet<TileBehaviour> CheckMatchesVertical(Vector2Int index, HashSet<TileBehaviour> matchingTiles)
    {
        if (index.y > 0)
        {
            Vector2Int checkIndex = index - new Vector2Int(0, 1);

            if (!matchingTiles.Contains(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_board.BoardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_board.BoardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesVertical(checkIndex, matchingTiles);
                }
            }
        }

        if (index.y < m_board.Columns - 1)
        {
            Vector2Int checkIndex = index + new Vector2Int(0, 1);

            if (!matchingTiles.Contains(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]))
            {
                if (m_board.BoardTileObjects[index.x, index.y]?.TileItem.Name ==
                    m_board.BoardTileObjects[checkIndex.x, checkIndex.y]?.TileItem.Name)
                {
                    matchingTiles.Add(m_board.BoardTileObjects[checkIndex.x, checkIndex.y]);
                    matchingTiles = CheckMatchesVertical(checkIndex, matchingTiles);
                }
            }
        }

        return matchingTiles;
    }

    public bool HasPotentialMatches()
    {
        for (int i = 0; i < m_board.Rows; i++)
        {
            for (int j = 0; j < m_board.Columns; j++)
            {
                if (i < m_board.Rows - 1)
                {
                    if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j].TileItem?.Name)
                    {
                        if (i > 1 &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 2, j].TileItem?.Name)
                        {
                            return true;
                        }

                        if (i < m_board.Rows - 3 &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 3, j].TileItem?.Name)
                        {
                            return true;
                        }

                        if (j > 0)
                        {
                            if (i > 0 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 1, j - 1].TileItem?.Name)
                            {
                                return true;
                            }

                            if (i < m_board.Rows - 2 &&
                                     m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 2, j - 1].TileItem?.Name)
                            {
                                return true;
                            }
                        }

                        if (j < m_board.Columns - 1)
                        {
                            if (i > 0 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 1, j + 1].TileItem?.Name)
                            {
                                return true;
                            }

                            if (i < m_board.Rows - 2 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 2, j + 1].TileItem?.Name)
                            {
                                return true;
                            }
                        }
                    }

                    if (i < m_board.Rows - 2)
                    {
                        if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 2, j].TileItem?.Name)
                        {
                            if (j > 0)
                            {
                                if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j - 1].TileItem?.Name)
                                {
                                    return true;
                                }
                            }

                            if (j < m_board.Columns - 1)
                            {
                                if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j + 1].TileItem?.Name)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                if (j < m_board.Columns - 1)
                {
                    if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j + 1].TileItem?.Name)
                    {
                        if (j > 1 &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j - 2].TileItem?.Name)
                        {
                            return true;
                        }

                        if (j < m_board.Columns - 3 &&
                            m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j + 3].TileItem?.Name)
                        {
                            return true;
                        }   

                        if (i > 0)
                        {
                            if (j > 0 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 1, j - 1].TileItem?.Name)
                            {
                                return true;
                            }

                            if (j < m_board.Columns - 2 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 1, j + 2].TileItem?.Name)
                            {
                                return true;
                            }
                        }

                        if (i < m_board.Rows - 1)
                        {
                            if (j > 0 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j - 1].TileItem?.Name)
                            {
                                return true;
                            }

                            if (j < m_board.Columns - 2 &&
                                m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j + 2].TileItem?.Name)
                            {
                                return true;
                            }
                        }
                    }

                    if (j < m_board.Columns - 2)
                    {
                        if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i, j + 2].TileItem?.Name)
                        {
                            if (i > 0)
                            {
                                if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i - 1, j + 1].TileItem?.Name)
                                {
                                    return true;
                                }
                            }

                            if (i < m_board.Rows - 1)
                            {
                                if (m_board.BoardTileObjects[i, j].TileItem?.Name == m_board.BoardTileObjects[i + 1, j + 1].TileItem?.Name)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
