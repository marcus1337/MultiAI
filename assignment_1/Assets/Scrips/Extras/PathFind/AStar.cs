using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class AStar : MonoBehaviour
{
    Point start, end;
    Grid grid;

    public static int getDistance(Point current, Point goal) {
        return Math.Max(Math.Abs(current.x - goal.x) , Math.Abs(current.y - goal.y));
    }

    public void setGrid(Grid grid)
    {
        this.grid = grid;
    }

    public AStar(Grid grid)
    {
        this.grid = grid;
    }

    int carStartDir;

    public void init(int startX, int startY, int endX, int endY, float startAngle)
    {
        start = new Point(startX, startY);
        end = new Point(endX, endY);
        if (startAngle >= 350 || startAngle <= 10)
            carStartDir = Node.DOWN;
        if (startAngle >= 80 && startAngle <= 110)
            carStartDir = Node.RIGHT;
        if (startAngle >= 170 && startAngle <= 190)
            carStartDir = Node.UP;
        if (startAngle >= 260 && startAngle <= 280)
            carStartDir = Node.LEFT;

        Debug.Log("Start angle test: " + carStartDir);

    }


    public Path<Node> solution;
    public List<Node> result;

    public void findPath()
    {
        Node startNode = new Node(start, 1, grid, carStartDir);
        Node endNode = new Node(end, 1, grid);

        solution = AStar.FindPath<Node>(startNode, endNode, (p1, p2) => { return AStar.getDistance(p1.location, p2.location); }, (p1) => { return AStar.getDistance(p1.location, end); });

        result = new List<Node>();
        if(solution != null)
        {
            IEnumerator<Node> enumerator = solution.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Node item = enumerator.Current;
                result.Add(item);
            }
        }

        
        //Console.WriteLine("RESULT: " + solution.TotalCost);
    }

    static public Path<Node> FindPath<Node>(
        Node start,
        Node destination,
        Func<Node, Node, int> distance,
        Func<Node, int> estimate)
        where Node : IHasNeighbours<Node>
    {
        var closed = new HashSet<Node>();
        var queue = new PriorityQueue<int, Path<Node>>();
        
        queue.Enqueue(0, new Path<Node>(start));
        while (!queue.IsEmpty)
        {
         
            var path = queue.Dequeue();
            if (closed.Contains(path.LastStep))
            {
                continue;
            }
            if (path.LastStep.sameSquare(destination))
                return path;
            closed.Add(path.LastStep);

            foreach (Node n in path.LastStep.Neighbours)
            {
                int d = distance(path.LastStep, n);
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + estimate(n) + n.GetNodeCost(), newPath);
            }
        }
        return null;
    }


}
