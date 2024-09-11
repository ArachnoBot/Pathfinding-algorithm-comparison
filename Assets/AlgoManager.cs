using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoManager : MonoBehaviour
{
    public Vector2Int start;
    public Vector2Int end;
    public Texture2D map;

    private int gridSize = 10;
    private Node[,] nodeGrid;
    private Astar astar;
    private TilemapManager tilemapManager;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.05f; // Adjust width as needed
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        lineRenderer.startColor = Color.red; // Start color
        lineRenderer.endColor = Color.red;   // End color

        nodeGrid = new Node[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                nodeGrid[x, y] = new Node(x, y, true);
            }
        }

        tilemapManager = gameObject.GetComponent<TilemapManager>();
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                if (map.GetPixel(x, y) == Color.black)
                {
                    nodeGrid[x, y].walkable = false;
                    tilemapManager.AddWallTile(x, y);
                }
            }
        }

        astar = new(nodeGrid, tilemapManager);
        //StartCoroutine(astar.FindPathVisual(nodeGrid[start.x, start.y], nodeGrid[end.x, end.y]));

        float startTime = Time.realtimeSinceStartup;
        List<Node> path = astar.FindPath(nodeGrid[start.x, start.y], nodeGrid[end.x, end.y]);
        Debug.Log("Time taken: " + (Time.realtimeSinceStartup - startTime));
        if (path == null)
        {
            Debug.Log("No path found");
        }
        else
        {
            DrawPath(path);
            Debug.Log($"Total cost was: {path[0].gCost}");
        }
    }

    public void DrawPath(List<Node> path)
    {
        lineRenderer.positionCount = path.Count;

        Vector3[] positions = new Vector3[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            positions[i] = new Vector3(path[i].x + .5f, path[i].y + .5f, 0);
        }
        lineRenderer.SetPositions(positions);
    }
}
