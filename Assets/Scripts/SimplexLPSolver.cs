using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplexLPSolver
{
    public class Edge
    {
        public int From { get; set; }
        public int To { get; set; }
        public float Cost { get; set; }
        
        public Edge(int from, int to, float cost)
        {
            From = from;
            To = to;
            Cost = cost;
        }
    }
    
    public delegate void NodeVisitedCallback(int nodeIndex, float distance);
    
    public static List<int> SolveShortestPath(int numNodes, List<Edge> edges, int startNode, int goalNode, NodeVisitedCallback onNodeVisited = null)
    {
        Dictionary<int, float> distance = new Dictionary<int, float>();
        Dictionary<int, int> previous = new Dictionary<int, int>();
        
        for (int i = 0; i < numNodes; i++)
        {
            distance[i] = float.MaxValue;
            previous[i] = -1;
        }
        distance[startNode] = 0;
        
        HashSet<int> visited = new HashSet<int>();
        
        for (int i = 0; i < numNodes; i++)
        {
            int u = -1;
            float minDist = float.MaxValue;
            
            for (int node = 0; node < numNodes; node++)
            {
                if (!visited.Contains(node) && distance[node] < minDist)
                {
                    minDist = distance[node];
                    u = node;
                }
            }
            
            if (u == -1 || u == goalNode) break;
            
            visited.Add(u);
            
            onNodeVisited?.Invoke(u, distance[u]);
            
            foreach (Edge edge in edges)
            {
                if (edge.From == u)
                {
                    float alt = distance[u] + edge.Cost;
                    if (alt < distance[edge.To])
                    {
                        distance[edge.To] = alt;
                        previous[edge.To] = u;
                    }
                }
            }
        }
        
        List<int> path = new List<int>();
        int current = goalNode;
        
        if (previous[current] == -1 && current != startNode)
        {
            return path;
        }
        
        while (current != -1)
        {
            path.Insert(0, current);
            current = previous[current];
        }
        
        return path;
    }
}
