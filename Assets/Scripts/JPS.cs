using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPS
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
        tilemapManager.AddStartTile(start);
        tilemapManager.AddEndTile(end);

        NodeHeap openSet = new(gridWidth * gridHeight);
        HashSet<Node> closedSet = new();

        start.gCost = 0;
        start.hCost = GetDistance(start, end);

        foreach (var direction in directions) // Do a jump in every direction from start
        {
            Node jumpPoint = Jump(start, direction, end);
            if (jumpPoint != null && jumpPoint.walkable)
            {
                jumpPoint.parent = start;
                jumpPoint.gCost = GetDistance(jumpPoint, start);
                jumpPoint.hCost = GetDistance(jumpPoint, end);
                openSet.Add(jumpPoint);
                tilemapManager.AddOpenTile(jumpPoint);
            }
        }

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);
            tilemapManager.AddClosedTile(currentNode, false);

            foreach (Node node in GetJumpPoints(currentNode, end))
            {
                if (node == end)
                {
                    Debug.Log(closedSet.Count + openSet.Count + " nodes visited");
                    node.parent = currentNode;
                    node.gCost = node.parent.gCost + GetDistance(node, node.parent);
                    return RetracePath(start, end);
                }

                if (!closedSet.Contains(node) && node.walkable)
                {
                    node.parent = currentNode;
                    node.gCost = node.parent.gCost + GetDistance(node, node.parent);
                    node.hCost = GetDistance(node, end);
                    openSet.Add(node);
                    tilemapManager.AddOpenTile(node);
                }
            }
        }

        Debug.LogWarning("No path found");
        return null;
    }

    public IEnumerator FindPathVisual(Node start, Node end)
    {
        tilemapManager.AddStartTile(start);
        tilemapManager.AddEndTile(end);

        NodeHeap openSet = new(gridWidth * gridHeight);
        HashSet<Node> closedSet = new();

        start.gCost = 0;
        start.hCost = GetDistance(start, end);

        foreach (var direction in directions) // Do a jump in every direction from start
        {
            Node jumpPoint = Jump(start, direction, end);
            if (jumpPoint != null && jumpPoint.walkable)
            {
                jumpPoint.parent = start;
                jumpPoint.gCost = GetDistance(jumpPoint, start);
                jumpPoint.hCost = GetDistance(jumpPoint, end);
                openSet.Add(jumpPoint);
                tilemapManager.AddOpenTile(jumpPoint);
            }
        }

        yield return new WaitForSecondsRealtime(.5f);
        Debug.Log("Initial positions gotten");

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);
            tilemapManager.AddClosedTile(currentNode, false);

            foreach (Node node in GetJumpPoints(currentNode, end))
            {
                if (node == end)
                {
                    Debug.Log(closedSet.Count + openSet.Count + " nodes visited");
                    yield break;
                }

                if (!closedSet.Contains(node))
                {
                    node.parent = currentNode;
                    node.gCost = node.parent.gCost + GetDistance(node, node.parent);
                    node.hCost = GetDistance(node, end);
                    openSet.Add(node);
                    tilemapManager.AddOpenTile(node);
                }
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }

        Debug.LogWarning("No path found");
        yield break;
    }

    private List<Node> GetJumpPoints(Node node, Node end)
    {
        List<Node> neighbors = GetNeighbors(node);

        List<Node> jumpPoints = new();
        Node jumpNode;

        foreach (Node neighbor in neighbors)
        {
            (int x, int y) dir = (Math.Clamp(neighbor.x - neighbor.parent.x, -1, 1), Math.Clamp(neighbor.y - neighbor.parent.y, -1, 1));
            Debug.Log($"Node {neighbor.x}, {neighbor.y} jumping in dir " + dir);
            if ((jumpNode = Jump(neighbor, dir, end)) != null)
            {
                Debug.Log($"Jump node found at {jumpNode.x}, {jumpNode.y}");
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

        (int x, int y) direction = (Math.Clamp(node.x - node.parent.x, -1, 1), Math.Clamp(node.y - node.parent.y, -1, 1));

        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            if (IsWalkable(node.x, node.y + direction.y)) neighbors.Add(nodes[node.x + direction.x, node.y + direction.y]);
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

        //neighbors.AddRange(GetForcedNeighbors(node.x, node.y, direction));

        foreach (Node neighbor in neighbors)
        {
            neighbor.parent = node;
        }

        return neighbors;
    }

    private Node Jump(Node node, (int x, int y) direction, Node end)
    {
        int x = node.x + direction.x;
        int y = node.y + direction.y;

        if (!IsWalkable(x, y)) return null;

        if (nodes[x, y] == end)
        {
            return nodes[x, y];
        }

        if (HasForcedNeighbor(x, y, direction))
        {
            Debug.Log($"Node at {x}, {y} had forced neighbor");
            return nodes[x, y];
        }

        if (direction.x != 0 && direction.y != 0) // Moving diagonally
        {
            if (Jump(nodes[x, y], directions[(Array.IndexOf(directions, direction) - 1 + directions.Length) % directions.Length], end) != null)
            {
                return nodes[x, y];
            }

            if (Jump(nodes[x, y], directions[(Array.IndexOf(directions, direction) + 1) % directions.Length], end) != null)
            {
                return nodes[x, y];
            }
        }

        return Jump(nodes[x, y], direction, end);
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
