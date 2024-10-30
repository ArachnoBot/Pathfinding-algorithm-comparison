using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoManager : MonoBehaviour
{
    public Vector2Int start;
    public Vector2Int end;
    public Texture2D map;
    public GameObject gridQuadObj;
    public float algoDelay = 0;
    public int testIterations = 50;

    private int gridWidth;
    private int gridHeight;
    private Node[,] nodes;

    private TilemapManager tilemapManager;
    private LineRenderer lineRenderer;
    private Tester tester;

    private Dijkstra djikstra;
    private Astar astar;
    private JPS jps;

    void Start()
    {
        gridQuadObj.transform.localScale = new Vector3(map.width, map.height, 0);
        gridQuadObj.GetComponent<MeshRenderer>().material.SetVector("_GridTiling", new Vector2(map.width, map.height));

        Camera.main.orthographicSize = (map.height / 2f) + .5f; // Resize camera to fit the grid (only vertically)

        // Create grid of nodes based on image pixels
        gridWidth = map.width;
        gridHeight = map.height;
        nodes = new Node[gridWidth, gridHeight];

        // Debug end generation
        if (gridWidth == 19 && gridHeight == 13)
        {
            end = new Vector2Int(16, 6);
        }
        else
        {
            //end = new Vector2Int(gridWidth - 2, gridHeight - 2);
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                nodes[x, y] = new Node(x, y, true);
            }
        }

        tilemapManager = gameObject.GetComponent<TilemapManager>();
        tilemapManager.MoveTilemap(-(gridWidth / 2f), -(gridHeight / 2f));

        int count = 0;

        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                if (map.GetPixel(x, y) == Color.black)
                {
                    count++;
                    nodes[x, y].walkable = false;
                    tilemapManager.AddWallTile(x, y);
                }
            }
        }

        Debug.Log($"Map has {map.height * map.width} cells, {count} walls and {map.height * map.width - count} walkable, percentage of walls: {1.0f * count / (1.0f * map.height * map.width)}");

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = .2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        tilemapManager.AddStartTile(nodes[start.x, start.y]);
        tilemapManager.AddEndTile(nodes[end.x, end.y]);

        astar = new(nodes, tilemapManager);
        //StartCoroutine(astar.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y]));

        jps = new(nodes, tilemapManager);
        //StartCoroutine(jps.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y], .1f));

        djikstra = new(nodes, tilemapManager);
        //StartCoroutine(djikstra.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y], .1f));

        tester = new Tester(this, nodes);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClearVisuals();
            TestDjikstra(start, end, true, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ClearVisuals();
            TestAstar(start, end, true, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ClearVisuals();
            TestJPS(start, end, true, 0);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ClearVisuals();
            tester.TestAlgorithms(testIterations);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearVisuals();
        }
    }

    public (float, int) TestDjikstra(Vector2Int start, Vector2Int end, bool visual = false, float delay = 0f)
    {
        float time = Time.realtimeSinceStartup;
        List<Node> path = djikstra.FindPath(nodes[start.x, start.y], nodes[end.x, end.y]);
        time = Time.realtimeSinceStartup - time;

        if (path == null)
        {
            Debug.Log("No path found");
            return (0f, 0);
        }
        else if (visual)
        {
            ClearVisuals();
            StartCoroutine(djikstra.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y], delay));
            DrawPath(path);

            Debug.Log($"Total cost was: {path[0].gCost}");
            Debug.Log("Time taken: " + time);
        }

        return (time, path[0].gCost);
    }

    public (float, int) TestAstar(Vector2Int start, Vector2Int end, bool visual = false, float delay = 0f)
    {
        float time = Time.realtimeSinceStartup;
        List<Node> path = astar.FindPath(nodes[start.x, start.y], nodes[end.x, end.y]);
        time = Time.realtimeSinceStartup - time;

        if (path == null)
        {
            Debug.Log("No path found");
            return (0f, 0);
        }
        else if (visual)
        {
            ClearVisuals();
            StartCoroutine(astar.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y], delay));
            DrawPath(path);

            Debug.Log($"Total cost was: {path[0].gCost}");
            Debug.Log("Time taken: " + time);
        }

        return (time, path[0].gCost);
    }

    public (float, int) TestJPS(Vector2Int start, Vector2Int end, bool visual = false, float delay = 0f)
    {
        float time = Time.realtimeSinceStartup;
        List<Node> path = jps.FindPath(nodes[start.x, start.y], nodes[end.x, end.y]);
        time = Time.realtimeSinceStartup - time;

        if (path == null)
        {
            Debug.Log("No path found");
            return (0f, 0);
        }
        else if (visual)
        {
            ClearVisuals();
            StartCoroutine(jps.FindPathVisual(nodes[start.x, start.y], nodes[end.x, end.y], delay));
            DrawPath(path);

            Debug.Log($"Total cost was: {path[0].gCost}");
            Debug.Log("Time taken: " + time);
        }

        return (time, path[0].gCost);
    }

    private void DrawPath(List<Node> path)
    {
        lineRenderer.positionCount = path.Count;

        Vector3[] positions = new Vector3[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            positions[i] = new Vector3(path[i].x - (gridWidth / 2f) + .5f, path[i].y - (gridHeight / 2f) + .5f, -3);
        }
        lineRenderer.SetPositions(positions);
    }

    public void ClearVisuals()
    {
        tilemapManager.ClearTilemap();
        lineRenderer.positionCount = 0;
    }
}

public interface IAlgorithm
{
    List<Node> FindPath(Node start, Node end);
    IEnumerator FindPathVisual(Node start, Node end, float delay);
}
