using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPS : IAlgorithm
{
    private Node[,] nodes;
    private int gridWidth;
    private int gridHeight;
    private TilemapManager tilemapManager;
    private readonly (int x, int y)[] directions = {
        (1, 0), // Right
        (1, -1), // Bottom right
        (0, -1), // Bottom
        (-1, -1), // Bottom Left
        (-1, 0), // Left
        (-1, 1), // Top left
        (0, 1), // Top
        (1, 1), // Top right
    };

    public JPS(Node[,] nodes, TilemapManager tilemapManager)
    {
        this.nodes = nodes;
        gridWidth = nodes.GetLength(0);
        gridHeight = nodes.GetLength(1);

        this.tilemapManager = tilemapManager;
    }

    public List<Node> FindPath(Node start, Node end)
    {
        NodeHeap openSet = new(gridWidth * gridHeight);
        HashSet<Node> closedSet = new();

        start.gCost = 0;
        start.hCost = GetDistance(start, end);

        foreach (var direction in directions) // Do a jump in every direction from start
        {
            Node jumpPoint = Jump(start.x + direction.x, start.y + direction.y, direction, end);
            if (jumpPoint == end)
            {
                jumpPoint.parent = start;
                return RetracePath(start, end);
            }
            else if (jumpPoint != null && jumpPoint.walkable)
            {
                jumpPoint.parent = start;
                jumpPoint.gCost = GetDistance(jumpPoint, start);
                jumpPoint.hCost = GetDistance(jumpPoint, end);
                openSet.Add(jumpPoint);
            }
        }

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);

            foreach (Node jumpNode in GetJumpPoints(currentNode, end))
            {
                if (closedSet.Contains(jumpNode)) continue;

                int newGCost = currentNode.gCost + GetDistance(currentNode, jumpNode);
                if (newGCost < jumpNode.gCost || !openSet.Contains(jumpNode))
                {
                    jumpNode.gCost = newGCost;
                    jumpNode.hCost = GetDistance(jumpNode, end);
                    jumpNode.parent = currentNode;

                    if (jumpNode == end)
                    {
                        return RetracePath(start, end);
                    }

                    if (!openSet.Contains(jumpNode))
                    {
                        openSet.Add(jumpNode);
                    }
                    else
                    {
                        openSet.UpdateItem(jumpNode);
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
        start.hCost = GetDistance(start, end);

        foreach (var direction in directions) // Do a jump in every direction from start
        {
            Node jumpPoint = Jump(start.x + direction.x, start.y + direction.y, direction, end);
            if (jumpPoint != null && jumpPoint.walkable)
            {
                jumpPoint.parent = start;
                jumpPoint.gCost = GetDistance(jumpPoint, start);
                jumpPoint.hCost = GetDistance(jumpPoint, end);
                openSet.Add(jumpPoint);
                tilemapManager.AddOpenTile(jumpPoint, delay > 0);
            }
        }

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);
            tilemapManager.AddClosedTile(currentNode, delay > 0);

            foreach (Node jumpNode in GetJumpPoints(currentNode, end))
            {
                if (jumpNode == end)
                {
                    Debug.Log(closedSet.Count + openSet.Count + " nodes visited");
                    yield break;
                }

                if (closedSet.Contains(jumpNode)) continue;

                int newGCost = currentNode.gCost + GetDistance(currentNode, jumpNode);
                if (newGCost < jumpNode.gCost || !openSet.Contains(jumpNode))
                {
                    jumpNode.gCost = newGCost;
                    jumpNode.hCost = GetDistance(jumpNode, end);
                    jumpNode.parent = currentNode;

                    //Debug.Log($"Node {jumpNode.x}, {jumpNode.y} (g: {jumpNode.gCost}) now has parent {currentNode.x}, {currentNode.y} (g: {currentNode.gCost})");

                    if (!openSet.Contains(jumpNode))
                    {
                        openSet.Add(jumpNode);
                        tilemapManager.AddOpenTile(jumpNode, delay > 0);
                    }
                    else
                    {
                        openSet.UpdateItem(jumpNode);
                    }
                }
            }
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);
        }

        yield break;
    }

    private List<Node> GetJumpPoints(Node node, Node end)
    {
        List<Node> neighbors = GetNeighbors(node);

        List<Node> jumpPoints = new();
        Node jumpNode;

        foreach (Node neighbor in neighbors)
        {
            if ((jumpNode = Jump(neighbor.x, neighbor.y, GetDirection(node, neighbor), end)) != null)
            {
                jumpPoints.Add(jumpNode);
            }
        }

        return jumpPoints;
    }

    private List<Node> GetNeighbors(Node node)
    {
        if (node.parent == null)
        {
            Debug.LogWarning($"No parent found for node {node.x}, {node.y}");
            return null;
        }

        List<Node> neighbors = new();

        (int x, int y) direction = GetDirection(node.parent, node);

        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            if (IsWalkable(node.x + direction.x, node.y + direction.y)) neighbors.Add(nodes[node.x + direction.x, node.y + direction.y]);
            if (IsWalkable(node.x + direction.x, node.y)) neighbors.Add(nodes[node.x + direction.x, node.y]);
            if (IsWalkable(node.x, node.y + direction.y)) neighbors.Add(nodes[node.x, node.y + direction.y]);
        }
        else if (direction.y == 0) // Moving horizontally
        {
            if (IsWalkable(node.x + direction.x, node.y)) neighbors.Add(nodes[node.x + direction.x, node.y]);
        }
        else if (direction.x == 0) // Moving vertically
        {
            if (IsWalkable(node.x, node.y + direction.y)) neighbors.Add(nodes[node.x, node.y + direction.y]);
        }

        neighbors.AddRange(GetForcedNeighbors(node.x, node.y, direction));

        return neighbors;
    }

    private List<Node> GetForcedNeighbors(int x, int y, (int x, int y) direction)
    {
        List<Node> forcedNeighbors = new();

        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            if (!IsWalkable(x - direction.x, y) && IsWalkable(x - direction.x, y + direction.y)) forcedNeighbors.Add(nodes[x - direction.x, y + direction.y]);
            if (!IsWalkable(x, y - direction.y) && IsWalkable(x + direction.x, y - direction.y)) forcedNeighbors.Add(nodes[x + direction.x, y - direction.y]);
        }
        else if (direction.y == 0) // Moving horizontally
        {
            if (!IsWalkable(x, y - 1) && IsWalkable(x + direction.x, y - 1)) forcedNeighbors.Add(nodes[x + direction.x, y - 1]);
            if (!IsWalkable(x, y + 1) && IsWalkable(x + direction.x, y + 1)) forcedNeighbors.Add(nodes[x + direction.x, y + 1]);
        }
        else if (direction.x == 0) // Moving vertically
        {
            if (!IsWalkable(x - 1, y) && IsWalkable(x - 1, y + direction.y)) forcedNeighbors.Add(nodes[x - 1, y + direction.y]);
            if (!IsWalkable(x + 1, y) && IsWalkable(x + 1, y + direction.y)) forcedNeighbors.Add(nodes[x + 1, y + direction.y]);
        }

        return forcedNeighbors;
    }

    private Node Jump(int x, int y, (int x, int y) direction, Node end)
    {
        if (!IsWalkable(x, y)) return null;

        //Debug.Log($"Jumping at {x}, {y} in direction {direction}");

        if (direction == (0, 0))
        {
            Debug.LogWarning($"Jump at {x}, {y} had no direction");
            return null;
        }

        if (nodes[x, y] == end)
        {
            return nodes[x, y];
        }

        if (HasForcedNeighbor(x, y, direction))
        {
            //Debug.Log($"Node {x}, {y} ({direction}) had forced neighbor");
            return nodes[x, y];
        }

        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            if (Jump(x, y + direction.y, (0, direction.y), end) != null || Jump(x + direction.x, y, (direction.x, 0), end) != null)
            {
                //Debug.Log($"Node {x}, {y} was moving diagonally ({direction}) and had a straight line with forced neighbor");
                return nodes[x, y];
            }
        }

        return Jump(x + direction.x, y + direction.y, direction, end);
    }

    private bool HasForcedNeighbor(int x, int y, (int x, int y) direction)
    {
        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            return (!IsWalkable(x - direction.x, y) && IsWalkable(x - direction.x, y + direction.y)) || (!IsWalkable(x, y - direction.y) && IsWalkable(x + direction.x, y - direction.y));
        }
        else if (direction.y == 0) // Moving horizontally
        {
            return (!IsWalkable(x, y - 1) && IsWalkable(x + direction.x, y - 1)) || (!IsWalkable(x, y + 1) && IsWalkable(x + direction.x, y + 1));
        }
        else if (direction.x == 0) // Moving vertically
        {
            return (!IsWalkable(x - 1, y) && IsWalkable(x - 1, y + direction.y)) || (!IsWalkable(x + 1, y) && IsWalkable(x + 1, y + direction.y));
        }

        return false;
    }

    private (int, int) GetDirection(Node from, Node to)
    {
        return (Math.Clamp(to.x - from.x, -1, 1), Math.Clamp(to.y - from.y, -1, 1));
    }

    private bool IsWalkable(int x, int y)
    {
        return IsWithinGrid(x, y) && nodes[x, y].walkable;
    }

    private bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
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

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Math.Abs(nodeA.x - nodeB.x);
        int distY = Math.Abs(nodeA.y - nodeB.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);

        return 14 * distX + 10 * (distY - distX);
    }

    /*

    Manhattan distance:
    return Math.Abs(nodeA.x - nodeB.x) + Mathf.Abs(nodeA.y - nodeB.y);

    Octile distance:
    int distX = Math.Abs(nodeA.x - nodeB.x);
    int distY = Math.Abs(nodeA.y - nodeB.y);

    if (distX > distY)
        return 14 * distY + 10 * (distX - distY);

    return 14 * distX + 10 * (distY - distX);
    
    */
}
