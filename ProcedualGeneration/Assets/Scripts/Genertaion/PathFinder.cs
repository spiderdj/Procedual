using UnityEngine;
using System.Collections.Generic;


class Node
{
    public int F{get{return G+H;}}
    public int G=0;
    public int H=0;

    public int x;
    public int y;
    public Node parent;

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class PathFinder
{
    Node[,] map;
    int[,] tiles;

    List<Node>
 ClosedList = new List<Node>();
    List<Node> OpenList = new List<Node>();
    public PathFinder(int[,] tiles)
    {
        this.tiles = tiles;
        map = new Node[tiles.GetLength(0), tiles.GetLength(1)];
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                map[x, y] = new Node(x,y);      
            }
        }
    }

    public List<Point> findPath(Point start,Point destination)
    {

        //Calculate H
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y].H = Mathf.Abs(destination.x - start.x) + Mathf.Abs(destination.y - start.y);
            }
        }

        OpenList = new List<Node>();
        ClosedList = new List<Node>();

        List<Point> path = new List<Point>();

        OpenList.Add(map[start.x, start.y]);

        while(OpenList.Count > 0 && !ClosedList.Contains(map[destination.x,destination.y]))
        {
            Node currentNode = getLowestF(OpenList);
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);
            
            if(currentNode.x > 0)
            {
                Node leftNode = map[currentNode.x - 1, currentNode.y];
                if(!ClosedList.Contains(leftNode))
                    calculate(currentNode,leftNode);   
            }
            
            if(currentNode.x < map.GetLength(0)-1)
            {
                Node rightNode = map[currentNode.x + 1, currentNode.y];
                if (!ClosedList.Contains(rightNode))
                    calculate(currentNode, rightNode);   
            }

            if(currentNode.y > 0)
            {
                Node upNode = map[currentNode.x, currentNode.y-1];
                if(!ClosedList.Contains(upNode))
                    calculate(currentNode, upNode);   
            }

            if(currentNode.y < map.GetLength(1)-1)
            {
                Node downNode = map[currentNode.x, currentNode.y+1];
                if (!ClosedList.Contains(downNode))
                    calculate(currentNode, downNode);   
            }

        }

        Node cNode = map[destination.x, destination.y];
    
        while(cNode != map[start.x,start.y])
        {
            path.Add(new Point(cNode.x, cNode.y));
            cNode = cNode.parent;
        }
        path.Reverse();
        return path;
    }

    private void calculate(Node baseNode,Node node)
    {
        if(OpenList.Contains(node))
        {
            int newG = baseNode.G + (3 - tiles[node.x, node.y]);
            if(newG < node.G)
            {
                node.G = baseNode.G + (3 - tiles[node.x, node.y]);
                node.parent = baseNode;
            }
        }
        else
        {
            node.G = baseNode.G + (3 - tiles[node.x, node.y]);
            node.parent = baseNode;
            OpenList.Add(node);
        }
    }

    private Node getLowestF(List<Node> list)
    {
        Node lowest = list[0];

        foreach (Node node in list)
        {
            if(node.F < lowest.F)
            {
                lowest = node;
            }
        }

        return lowest;
    }

	
}
