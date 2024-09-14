using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoManager : MonoBehaviour
{
    public Vector2Int start;
    public Vector2Int end;
    public Texture2D map;
    public GameObject gridQuadObj;

    private int gridWidth;
    private int gridHeight;
    private Node[,] nodeGrid;

    private TilemapManager tilemapManager;
    private LineRenderer lineRenderer;

    private Astar astar;
    private JPS jps;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.05f; // Adjust width as needed
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        lineRenderer.startColor = Color.red; // Start color
        lineRenderer.endColor = Color.red;   // End color

        gridQuadObj.transform.localScale = new Vector3(map.width, map.height, 0);
        gridQuadObj.GetComponent<MeshRenderer>().material.SetVector("_GridTiling", new Vector2(map.width, map.height));

        Camera.main.orthographicSize = (map.height / 2f) + .5f; // Resize camera to fit the grid (only vertically)

        // Create grid of nodes based on image pixels
        gridWidth = map.width;
        gridHeight = map.height;
        nodeGrid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                nodeGrid[x, y] = new Node(x, y, true);
            }
        }

        tilemapManager = gameObject.GetComponent<TilemapManager>();
        tilemapManager.MoveTilemap(-(gridWidth / 2f), -(gridHeight / 2f));
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

        jps = new(nodeGrid, tilemapManager);
        StartCoroutine(jps.FindPathVisual(nodeGrid[start.x, start.y], nodeGrid[end.x, end.y]));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float startTime = Time.realtimeSinceStartup;
            List<Node> path = jps.FindPath(nodeGrid[start.x, start.y], nodeGrid[end.x, end.y]);
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
