using System.Collections;
using System.Numerics;
using UnityEngine;

public class Tester
{
    private AlgoManager algoManager;
    private Node[,] nodes;
    private int gridWidth;
    private int gridHeight;

    private Vector2Int start;
    private Vector2Int end;

    public Tester(AlgoManager algoManager, Node[,] nodes)
    {
        this.algoManager = algoManager;
        this.nodes = nodes;
        gridWidth = nodes.GetLength(0);
        gridHeight = nodes.GetLength(1);
    }

    public void TestAlgorithms(int iterations)
    {
        float djikstraTime = 0f;
        float astarTime = 0f;
        float jpsTime = 0f;

        for (int i = 0; i < iterations; i++)
        {
            SetStartAndEnd();

            (float, int) result;
            int optimalPathCost;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    nodes[x, y].hCost = 0;
                    nodes[x, y].gCost = 0;
                    nodes[x, y].parent = null;
                }
            }

            result = algoManager.TestDjikstra(start, end);
            optimalPathCost = result.Item2;
            djikstraTime += result.Item1;

            result = algoManager.TestAstar(start, end);
            astarTime += result.Item1;

            if (optimalPathCost != result.Item2)
            {
                Debug.LogError($"A* found path with cost {result.Item2} but the optimal is {optimalPathCost}");
                Debug.LogError($"start: {start.x}, {start.y}, end: {end.x}, {end.y}");
                return;
            }

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    nodes[x, y].hCost = 0;
                    nodes[x, y].gCost = 0;
                    nodes[x, y].parent = null;
                }
            }

            result = algoManager.TestJPS(start, end);
            jpsTime += result.Item1;

            if (optimalPathCost != result.Item2)
            {
                Debug.LogError($"JPS found path with cost {result.Item2} but the optimal is {optimalPathCost}");
                Debug.LogError($"start: {start.x}, {start.y}, end: {end.x}, {end.y}");
                return;
            }
        }

        djikstraTime /= iterations;
        astarTime /= iterations;
        jpsTime /= iterations;

        Debug.Log($"Djikstra took {djikstraTime}, A* took {astarTime} and JPS took {jpsTime} seconds on average ({iterations} iterations)");
    }

    private void SetStartAndEnd()
    {
        start = GetRandomPoint();
        end = GetRandomPoint();

        while (!IsWalkable(start.x, start.y))
        {
            start = GetRandomPoint();
        }

        while (!IsWalkable(end.x, end.y) || start.x == end.x || start.y == end.y)
        {
            end = GetRandomPoint();
        }
    }

    private Vector2Int GetRandomPoint()
    {
        return new Vector2Int((int)(Random.value * gridWidth - 1), (int)(Random.value * gridHeight - 1));
    }

    private bool IsWalkable(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && nodes[x, y].walkable;
    }
}
