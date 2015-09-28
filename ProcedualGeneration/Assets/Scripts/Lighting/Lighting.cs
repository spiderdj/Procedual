using UnityEngine;
using System.Collections.Generic;
using System;

public class Lighting : MonoBehaviour {


    class Edge : IComparable<Edge>
    {
        public Color color = Color.green;
        public Edge previous;
        public Edge next;
        public Vector2 start;
        public Vector2 end;
        public Vector2 normal { get { Vector2 v = end - start; Vector2 norm = new Vector2(-v.y, v.x); norm.Normalize();  return norm*normalMultiplier; } }
        public float distanceFromPlayer;
        public int normalMultiplier = 1;

        public Edge(Edge prev, Edge next, Vector2 start, Vector2 end, float distance, int normMultiplier)
        {
            this.previous = prev;
            this.next = next;
            this.start = start;
            this.end = end;
            this.normalMultiplier = normMultiplier;
            this.distanceFromPlayer = distance;
        }

        public Edge(Edge prev, Edge next, Vector2 start, Vector2 end, float distance, int normMultiplier, Color color)
        {
            this.previous = prev;
            this.next = next;
            this.start = start;
            this.end = end;
            this.color = color;
            this.distanceFromPlayer = distance;
            this.normalMultiplier = normMultiplier;
        }

        public int CompareTo(Edge other)
        {
            if (other.distanceFromPlayer < distanceFromPlayer)
                return 1;
            else if (other.distanceFromPlayer > distanceFromPlayer)
                return -1;
            return 0;
        }
    }



    public PlayerController player;
    public Texture2D lightingTexture;

    private List<Edge> edges;
    private List<Edge> debugEdges;
    public float viewDistance = 10.0f;

    [Range(0.0f, Mathf.PI * 2)]
    public float lookAngleMin = 1.09f;

    [Range(0.0f, Mathf.PI * 2)]
    public float lookAngleMax = Mathf.PI;

    [Range(0.0f, Mathf.PI * 2)]
    public float normalAngleMin = 0.39f;

    [Range(0.0f, Mathf.PI * 2)]
    public float normalAngleMax = Mathf.PI;

    public void Update()
    {

            if(Globals.map != null)
            {
                CalculateEdges();
            }
        
    }

    public void CalculateEdges()
    {
        debugEdges = new List<Edge>();
        edges = new List<Edge>();
        Vector2 playerPos = (Vector2)player.transform.position;
        Vector2 lookVector = player.transform.up.normalized;
        Vector2 norm = calculateNormal(playerPos,playerPos + lookVector);
        Vector2 oppositeNorm = norm * -1;

        Vector2 lookPoint = playerPos + (lookVector * viewDistance);
        Vector2 a = lookPoint + (norm * viewDistance);
        Vector2 b = lookPoint + (oppositeNorm * viewDistance);

        Edge pa = new Edge(null, null, playerPos, a, 0,1, Color.red);
        Edge pb = new Edge(null, null, playerPos, b, 0,1, Color.red);
        Edge ab = new Edge(pa, pb, a, b, viewDistance,1, Color.red);

        pa.previous = pb;
        pa.next = ab;

        pb.previous = ab;
        pb.next = pa;

        for (int x = 0; x < Globals.map.GetLength(0); x++)
        {
            for (int y = 0; y < Globals.map.GetLength(1); y++)
            {
                if (Globals.map[x, y] == 1)
                {

                    Edge[] tileEdges = getTileEdges(x, y, playerPos);

                        List<Edge> validEdges = new List<Edge>();

                        foreach (var edge in tileEdges)
                        {
                            Vector2 tilePos = new Vector2(x + edge.normal.x, y + edge.normal.y);
                            if(tilePos.x >= 0 && tilePos.x < Globals.map.GetLength(0) && tilePos.y >= 0 && tilePos.y < Globals.map.GetLength(1))
                            {
                                if (Globals.map[(int)tilePos.x, (int)tilePos.y] == 2)
                                    validEdges.Add(edge);
                            }
                        }

                        edges.AddRange(cullEdges(validEdges, lookVector, norm, playerPos));
                        edges.AddRange(cullEdges(validEdges, lookVector, oppositeNorm, playerPos));
                    }
                }
            }

        List<Edge> toRemove = new List<Edge>();
        foreach (var edge in edges)
        {
            foreach (var other in edges)
            {
                if(other != edge)
                {
                    if(other.start == edge.end)
                    {
                        if(edge.normal == other.normal)
                        {
                            edge.end = other.end;
                            toRemove.Add(other);
                        }
                        else
                        {
                            edge.next = other;
                            other.previous = edge;
                        }
                    }
                }
            }
        }

        foreach (var item in toRemove)
        {
            edges.Remove(item);
        }

        edges.Add(pa);
        edges.Add(pb);
        edges.Add(ab);

        edges.Sort();

        List<Edge> newEdges = new List<Edge>();

        foreach (Edge edge in edges)
        {
            if (edge.next == null)
            {
                Vector2 directionVector = (edge.end - playerPos).normalized;
                //debugEdges.Add(new Edge(null,null,edge.end,edge.end+directionVector * viewDistance * 2,0f));
                foreach (Edge other in edges)
                {
                    if (edge != other)
                    {
                        Vector2 intersect;
                        if (lineIntersetcs(edge.end, edge.end + directionVector * viewDistance * 2, other.start, other.end, out intersect))
                        {
                            Edge newEdge = new Edge(edge, other, edge.end, edge.end + directionVector * viewDistance * 2 * intersect.x, 0f, 1, Color.cyan);
                            newEdges.Add(newEdge);
                            other.start = other.start + ((other.end - other.start) * intersect.y);
                            break;
                        }
                    }
                }
            }



        }

        edges.AddRange(newEdges);



    }

    private Edge[] getTileEdges(int x, int y, Vector2 playerPos)
    {
        Vector2 delta = new Vector2(x, y) - playerPos;

        int xMod = 1;
        int yMod = 1;

        if (delta.x < 0)
            yMod = -1;

        if (delta.y > 0)
            xMod = -1;

        Edge[] tileEdges =  new Edge[]{ new Edge(null, null, new Vector2(x + 0.5f*xMod, y - 0.5f), new Vector2(x - 0.5f*xMod, y - 0.5f), (playerPos - new Vector2(x + 0.5f*xMod, y - 0.5f)).magnitude,xMod),
                                                       new Edge(null, null, new Vector2(x - 0.5f, y - 0.5f*yMod), new Vector2(x - 0.5f, y + 0.5f*yMod), (playerPos - new Vector2(x - 0.5f, y - 0.5f*yMod)).magnitude,yMod),
                                                       new Edge(null, null, new Vector2(x - 0.5f*xMod, y + 0.5f), new Vector2(x + 0.5f*xMod, y + 0.5f), (playerPos - new Vector2(x - 0.5f*xMod, y + 0.5f)).magnitude,xMod),
                                                       new Edge(null, null, new Vector2(x + 0.5f, y + 0.5f*yMod), new Vector2(x + 0.5f, y - 0.5f*yMod), (playerPos - new Vector2(x + 0.5f, y + 0.5f*yMod)).magnitude,yMod) };


        return tileEdges;
    }


    private List<Edge> cullEdges(List<Edge> tileEdges, Vector2 lookVector, Vector2 normal, Vector2 playerPos)
    {
        Vector2 lookPoint = playerPos + (lookVector * viewDistance);
        Vector2 a = lookPoint + (normal * viewDistance);
        List<Edge> results = new List<Edge>();
        foreach (var edge in tileEdges)
        {
            float lookAngle = getAngle(lookVector, edge.normal);
            float normalAngle = getAngle(normal, edge.normal); //Note not working properly, different sides of view interfereing with edge detection

            bool lookAngleVisible = (lookAngle > lookAngleMin && lookAngle < lookAngleMax);
            bool normalAngleVisible = (normalAngle >= normalAngleMin && normalAngle <=  normalAngleMax);

            if (lookAngleVisible&&normalAngleVisible)
            {
                //Debug.Log(edge.normal + " : " + normalAngle + " : " + lookAngle);
                bool start = PointInTriangle(playerPos, lookPoint, a, edge.start);
                bool end = PointInTriangle(playerPos, lookPoint, a, edge.end);

                Vector2 intersectA;
                bool intersectsA = lineIntersetcs(edge.start, edge.end, playerPos, a, out intersectA);
                Vector2 intersectB;
                bool intersectsB = lineIntersetcs(edge.start, edge.end, playerPos, lookPoint, out intersectB);
                Vector2 intersectC;
                bool intersectsC = lineIntersetcs(edge.start, edge.end, a, lookPoint, out intersectC); // - (a-lookPoint).normalized
                if (start && end)
                {
                    edges.Add(edge);
                }
                else if (intersectsA)
                {
                    if (start)
                    {
                        edge.end = edge.start + ((edge.end - edge.start) * intersectA.x);
                    }
                    else if (end)
                    {
                        edge.start = edge.start + ((edge.end - edge.start) * intersectA.x);
                    }
                    results.Add(edge);
                }
                else if (intersectsC)
                {
                    if (start)
                    {
                        edge.end = edge.start + ((edge.end - edge.start) * intersectC.x);
                    }
                    else if (end)
                    {
                        edge.start = edge.start + ((edge.end - edge.start) * intersectC.x);
                    }
                    results.Add(edge);
                }
                else if (intersectsB)
                {
                    results.Add(edge);
                }
            }
        }
        return results;
    }

    private float getAngle(Vector2 a, Vector2 b)
    {
        float result = 0;
        float cosAngle = Vector2.Dot(a, b)/(a.magnitude * b.magnitude);
        result = Mathf.Acos(cosAngle);
        return result;
    }

    private bool lineIntersetcs(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End,out Vector2 result)
    {
        Vector2 p = (line1End - line1Start);
        Vector2 q = (line2End - line2Start);
       
        var qps = det(q,(line2Start - line1Start));
        var rs = det(q,p);

        if(qps == 0 && rs==0)
        {
            //Co-linear
            //Debug.Log("Co-linear");
            result = Vector2.zero;
            return false;
        }
        else if(rs == 0 && qps != 0)
        {
            //Parrallel
            result = Vector2.zero;
            return false;
        }
        else if(rs != 0)
        {
            float x = qps / rs;
            float y = det(p, line2Start - line1Start) / det(q, p);
            if(x >= 0 && x <= 1 && y >=0 && y<=1)
            {
                result = new Vector2(x, y);
                return true;
            }
            result = Vector2.zero;
            return false;
        }
        result = Vector2.zero;
        return false;
    }

    private float det(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private Vector2 cross(Vector2 a, Vector2 b)
    {
        return (Vector2)Vector3.Cross(a, b);
    }

    private bool PointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        float alpha = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) /
        ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
        float beta = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) /
               ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
        float gamma = 1.0f - alpha - beta;


        if (alpha <= 0 || beta <= 0 || gamma <= 0)
            return false;

        return true;
    }


    private Vector2 calculateNormal(Vector2 start, Vector2 end)
    {
        Vector2 v = end - start;
        Vector2 norm = new Vector2(-v.y, v.x);
        norm.Normalize();
        return norm;
    }


    void OnDrawGizmos()
    {
        if (edges != null)
        {
            foreach (var edge in edges)
            {
                Gizmos.color = edge.color;
                Gizmos.DrawLine(edge.start, edge.end);
                Gizmos.color = Color.magenta;

                Vector2 midPoint = edge.start + ((edge.end - edge.start) / 2);
                Gizmos.DrawLine(midPoint, midPoint + edge.normal / 2);
            }
        }

        if(debugEdges!=null)
        {
            foreach (var edge in debugEdges)
            {
                Gizmos.color = edge.color;
                Gizmos.DrawLine(edge.start, edge.end);
                Gizmos.color = Color.magenta;

                Vector2 midPoint = edge.start + ((edge.end - edge.start) / 2);
                Gizmos.DrawLine(midPoint, midPoint + edge.normal / 2);
            }
        }
    }
	
}
