using System.Collections.Generic;
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


public class Heap
{
    Node[] nodes;
    int nodeCount;

    public Heap(int gridSize)
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
        Node firstItem = nodes[0];
        nodeCount--;
        nodes[0] = nodes[nodeCount];
        nodes[0].heapIndex = 0;
        SortDown(nodes[0]);
        return firstItem;
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
        return Equals(nodes[node.heapIndex], node);
    }

    void SortDown(Node node)
    {
        while (true)
        {
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
        while (node.heapIndex > 0) // Do not sort if root
        {
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