using UnityEngine;
using System.Collections.Generic;
using System;


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

public enum Direction { Straight, Diagonal}

public class PathFinder
{
    Node[,] map;
    int[,] tiles;

    List<Node> ClosedList = new List<Node>();
    List<Node> OpenList = new List<Node>();

    public bool pathDiagonal = true;

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

    /// <summary>
    /// Find a path from start to destination
    /// </summary>
    /// <param name="start">The co-ordinates of the start point in the grid</param>
    /// <param name="destination">The co-ordinates of the end point in the grid</param>
    /// <param name="GetPathingValue"> A function which should return the G value of the tile, should be negative if tile is not pathable</param>
    /// <returns>List of co-ordinates which is the path from start to destination</returns>
    public List<Point> findPath(Point start,Point destination, Func<int,Direction,int> GetPathingValue)
    {

        //Calculate H
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y].H = Mathf.Abs(destination.x - start.x) + Mathf.Abs(destination.y - start.y);
                map[x, y].parent = null;
            }
        }

        OpenList = new List<Node>();
        ClosedList = new List<Node>();

        List<Point> path = new List<Point>();

        OpenList.Add(map[start.x, start.y]);

        if (start.x < 0 || start.x > map.GetLength(0) - 1 || start.y < 0 || start.y > map.GetLength(1) - 1)
            return path;

        if (GetPathingValue(tiles[destination.x, destination.y], Direction.Straight) < 0)
            return path;

        while(OpenList.Count > 0 && !ClosedList.Contains(map[destination.x,destination.y]))
        {
            Node currentNode = getLowestF(OpenList);
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (currentNode.x + x >= 0 && currentNode.x + x < tiles.GetLength(0) && currentNode.y + y >= 0 && currentNode.y+y < tiles.GetLength(1))
                    {
                            Node node = map[currentNode.x + x, currentNode.y + y];
                            if (node != currentNode && !ClosedList.Contains(node))
                            {
                                if (x != 0 && y != 0)
                                {
                                    //If both x and y aren't 0 then we're on a diagonal
                                    if (pathDiagonal)
                                    {
                                        calculate(currentNode, node, GetPathingValue, Direction.Diagonal);
                                    }
                                }
                                else
                                {
                                    calculate(currentNode, node, GetPathingValue, Direction.Straight);
                                }
                            }
                    }
                }
            }
        }

        Node cNode = map[destination.x, destination.y];
    
        while(cNode != map[start.x,start.y] && cNode != null)
        {
            path.Add(new Point(cNode.x, cNode.y));
            cNode = cNode.parent;
        }
        path.Reverse();
        return path;
    }

    private void calculate(Node baseNode,Node node,Func<int,Direction,int> getPathingValue,Direction direction)
    {
        int G = getPathingValue(tiles[node.x, node.y], direction);
        if (G > 0)
        {
            if (OpenList.Contains(node))
            {
                int newG = baseNode.G + G;
                if (newG < node.G)
                {
                    node.G = baseNode.G + (3 - tiles[node.x, node.y]);
                    node.parent = baseNode;
                }
            }
            else
            {
                node.G = baseNode.G + G;
                node.parent = baseNode;
                OpenList.Add(node);
            }
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
