using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    private Tilemap tilemap;
    private Tile wallTile;
    private Tile openTile;
    private Tile closedTile;

    void Start()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        wallTile = CreateTile(Color.black);
        openTile = CreateTile(Color.green);
        closedTile = CreateTile(Color.cyan);
    }

    private Tile CreateTile(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;

        return tile;
    }

    public void AddClosedTile(Node node, bool addCostTexts = true)
    {
        tilemap.SetTile(new(node.x, node.y, 0), closedTile);

        if (addCostTexts) AddCostTexts(node);
    }

    public void AddOpenTile(Node node, bool addCostTexts = true)
    {
        tilemap.SetTile(new Vector3Int(node.x, node.y, 0), openTile);

        if (addCostTexts) AddCostTexts(node);
    }

    public void AddWallTile(int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
    }

    private void AddCostTexts(Node node) // Top left is G, middle is F and bottom right is H
    {
        GameObject textObj;
        string textName = $"Text_{node.x}_{node.y}";
        if ((textObj = GameObject.Find(textName)) != null)
        {
            TextMeshPro[] costTexts = textObj.GetComponentsInChildren<TextMeshPro>();
            costTexts[0].text = node.fCost.ToString();
            costTexts[1].text = node.gCost.ToString();
            costTexts[2].text = node.hCost.ToString();
        }
        else
        {
            textObj = new(textName);
            textObj.transform.position = new(node.x, node.y, -1);

            GameObject fCostObj = new("fCostObj");
            fCostObj.transform.SetParent(textObj.transform);
            fCostObj.transform.localPosition = new Vector3(.5f, .5f, 0);

            TextMeshPro fCostText = fCostObj.AddComponent<TextMeshPro>();
            fCostText.fontSize = 3;
            fCostText.color = Color.black;
            fCostText.alignment = TextAlignmentOptions.Center;
            fCostText.text = node.fCost.ToString();


            GameObject gCostObj = new("gCostObj");
            gCostObj.transform.SetParent(textObj.transform);
            gCostObj.transform.localPosition = new Vector3(.2f, .8f, 0);

            TextMeshPro gCostText = gCostObj.AddComponent<TextMeshPro>();
            gCostText.fontSize = 2;
            gCostText.color = Color.black;
            gCostText.alignment = TextAlignmentOptions.Center;
            gCostText.text = node.gCost.ToString();


            GameObject hCostObj = new("hCostObj");
            hCostObj.transform.SetParent(textObj.transform);
            hCostObj.transform.localPosition = new Vector3(.8f, .2f, 0);

            TextMeshPro hCostText = hCostObj.AddComponent<TextMeshPro>();
            hCostText.fontSize = 2;
            hCostText.color = Color.black;
            hCostText.alignment = TextAlignmentOptions.Center;
            hCostText.text = node.hCost.ToString();
        }
    }
}
