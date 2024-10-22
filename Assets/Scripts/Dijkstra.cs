using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : IAlgorithm
{
    private Node[,] nodes;
    private int gridWidth;
    private int gridHeight;
    private TilemapManager tilemapManager;

    public Dijkstra(Node[,] nodes, TilemapManager tilemapManager)
    {
        this.nodes = nodes;
        gridWidth = nodes.GetLength(0);
        gridHeight = nodes.GetLength(1);

        this.tilemapManager = tilemapManager;
    }

    public List<Node> FindPath(Node start, Node end)
    {
        NodeHeap openSet = new(gridWidth * gridHeight);

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                nodes[i, j].gCost = int.MaxValue;
            }
        }

        start.gCost = 0;
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            if (currentNode == end)
            {
                openSet.Clear();
                return RetracePath(start, currentNode);
            }

            for (int x = currentNode.x - 1; x <= currentNode.x + 1; x++)
            {
                for (int y = currentNode.y - 1; y <= currentNode.y + 1; y++)
                {
                    if (x == currentNode.x && y == currentNode.y) continue; // Not a neighbor
                    if (!IsWithinGrid(x, y)) continue; // Outside grid bounds

                    Node neighbor = nodes[x, y];

                    if (!neighbor.walkable) continue; // Exclude walls

                    int newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newGCost < neighbor.gCost)
                    {
                        neighbor.gCost = newGCost;
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
        }

        return null;
    }

    public IEnumerator FindPathVisual(Node start, Node end, float delay)
    {
        NodeHeap openSet = new(gridWidth * gridHeight);
        HashSet<Node> closedSet = new();

        start.gCost = 0;
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode != start && currentNode != end) tilemapManager.AddClosedTile(currentNode, delay > 0);

            if (currentNode == end)
            {
                Debug.Log(closedSet.Count + openSet.Count + " nodes visited");
                yield break;
            }

            for (int x = currentNode.x - 1; x <= currentNode.x + 1; x++)
            {
                for (int y = currentNode.y - 1; y <= currentNode.y + 1; y++)
                {
                    if (x == currentNode.x && y == currentNode.y) continue; // Not a neighbor
                    if (!IsWithinGrid(x, y)) continue; // Outside grid bounds

                    Node neighbor = nodes[x, y];

                    if (!neighbor.walkable || closedSet.Contains(neighbor)) continue; // Exclude walls and visited nodes

                    int newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newGCost < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newGCost;
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                            if (neighbor != start && neighbor != end) tilemapManager.AddOpenTile(neighbor, delay > 0);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);
        }

        yield break;
    }

    private bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        if ((nodeA.x - nodeB.x) != 0 && (nodeA.y - nodeB.y) != 0)
        {
            return 14;
        }
        return 10;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;

            if (path.Count > 10000)
            {
                Debug.LogWarning("Path was over 10 000 nodes (possible loop), returning null");
                return null;
            }
        }

        path.Add(startNode);

        return path;
    }
}
