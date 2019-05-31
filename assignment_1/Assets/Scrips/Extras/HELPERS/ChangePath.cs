using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.Extras.Structures
{
    class ChangePath
    {
        List<Node> firstPath;
        List<Node> firstReversed;

        List<Node> oldStartToNewStart;
        List<Node> newEndToOldEnd;
        Point newStart, newEnd;

        Grid modGrid;
        Grid realGrid;

        List<Node> result;

        public List<Node> getResult()
        {
            return result.ToList();
        }

        private TerrainManager terrainManager;

        public Point getStartPoint()
        {
            return new Point(newStart);
        }

        public Point getEndPoint()
        {
            return new Point(newEnd);
        }

        public ChangePath(TerrainManager terrainManager, List<Node> firstPath, Grid realGrid)
        {
            this.terrainManager = terrainManager;
            this.firstPath = firstPath.ToList();
            firstReversed = firstPath.ToList();
            firstReversed.Reverse();
            this.realGrid = realGrid;
            result = new List<Node>();
        }

        public void generatePath()
        {
            int stepsStart = NewStartIndex();
            int stepsEnd = NewEndIndex();

            int counter = 0;
            foreach (Node node in firstPath)
            {
                if(counter == stepsStart)
                {
                    newStart = new Point(node.location);
                }

                if (counter >= stepsStart && counter <= stepsEnd)
                {
                    result.Add(new Node(node));
                    Debug.Log("Counter: " + counter + " stepstart: " + stepsStart + " x: "
                    + node.location.x + " y: " + node.location.y);
                }
                   
                counter++;
                Debug.Log("Counter: " + counter + " stepstart: " + stepsStart + " x: "
                    + node.location.x + " y: " + node.location.y);
            }

            Debug.Log("START: Steps: " + stepsStart + " END: Steps: " + stepsEnd);
        }

        private int NewStartIndex()
        {
            int steps = 0;
            foreach (Node node in firstPath)
            {
                if (hasPointOffset(node.location, 3))
                    break;
                steps++;
            }
            return steps;
        }

        private int NewEndIndex()
        {
            int steps = 0;

            foreach (Node node in firstReversed)
            {
                if (hasPointOffset(node.location, 3))
                    break;
                steps++;
            }
            return firstPath.Count - steps;
        }

        private bool hasPointOffset(Point p, int offset)
        {
            for (int i = 0; i <= offset; i++)
            {
                for (int j = 0; j <= offset; j++)
                {
                    if (!realGrid.IsOnGrid(p.x + i / 2, p.y + j / 2) || !realGrid.IsOnGrid(p.x - i / 2, p.y - j / 2)
                        || !realGrid.IsOnGrid(p.x + i, p.y) || !realGrid.IsOnGrid(p.x - i, p.y) ||
                        !realGrid.IsOnGrid(p.x, p.y + j) || !realGrid.IsOnGrid(p.x, p.y - j))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }
}
