using System;
using UnityEngine;

public class Node
{
    public Node parent;
    public int x, y;
    public int heapIndex;
    public bool walkable;
    public int gCost;
    public int hCost;

    public int fCost { get { return gCost + hCost; } }

    public Node(int x, int y, bool walkable)
    {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }

    public bool IsSmallerThan(Node other)
    {
        if (fCost < other.fCost) return true;
        if (fCost > other.fCost) return false;
        return hCost < other.hCost; // Compare hCost if fCost is the same
    }
}


public class NodeHeap
{
    Node[] nodes;
    int nodeCount;

    public NodeHeap(int gridSize)
    {
        nodes = new Node[gridSize * gridSize];
    }

    public void Add(Node node)
    {
        node.heapIndex = nodeCount;
        nodes[nodeCount] = node;
        SortUp(node);
        nodeCount++;
    }

    public Node RemoveFirst()
    {
        Node smallestNode = nodes[0];
        nodeCount--;
        nodes[0] = nodes[nodeCount];
        nodes[0].heapIndex = 0;
        SortDown(nodes[0]);
        return smallestNode;
    }

    public void UpdateItem(Node node)
    {
        SortUp(node);
    }

    public int Count
    {
        get
        {
            return nodeCount;
        }
    }

    public bool Contains(Node node)
    {
        return node.heapIndex >= 0 && node.heapIndex < nodeCount && nodes[node.heapIndex] == node;
    }

    public void Clear()
    {
        Array.Clear(nodes, 0, nodeCount);
        nodeCount = 0;
    }

    void SortDown(Node node)
    {
        int count = 0;
        while (true)
        {
            if (count++ > 1000)
            {
                Debug.Log($"sortdown, node {node.x}, {node.y}");
                return;
            }

            int leftChildIndex = node.heapIndex * 2 + 1;
            int rightChildIndex = node.heapIndex * 2 + 2;
            int smallerChildIndex = node.heapIndex;

            if (leftChildIndex < nodeCount && nodes[leftChildIndex].IsSmallerThan(nodes[smallerChildIndex]))
            {
                smallerChildIndex = leftChildIndex;
            }

            if (rightChildIndex < nodeCount && nodes[rightChildIndex].IsSmallerThan(nodes[smallerChildIndex]))
            {
                smallerChildIndex = rightChildIndex;
            }

            if (smallerChildIndex == node.heapIndex) break; // Break if children arent smaller

            Swap(node, nodes[smallerChildIndex]);
            node = nodes[smallerChildIndex];
        }
    }

    void SortUp(Node node)
    {
        int count = 0;
        while (node.heapIndex > 0) // Do not sort if root
        {
            if (count++ > 1000)
            {
                Debug.Log($"sortup, node {node.x}, {node.y}");
                return;
            }
            int parentIndex = (node.heapIndex - 1) / 2;
            Node parentItem = nodes[parentIndex];

            if (node.IsSmallerThan(parentItem))
            {
                Swap(node, parentItem);
            }
            else
            {
                break;
            }
        }
    }

    void Swap(Node nodeA, Node nodeB)
    {
        nodes[nodeA.heapIndex] = nodeB;
        nodes[nodeB.heapIndex] = nodeA;

        (nodeB.heapIndex, nodeA.heapIndex) = (nodeA.heapIndex, nodeB.heapIndex);
    }
}