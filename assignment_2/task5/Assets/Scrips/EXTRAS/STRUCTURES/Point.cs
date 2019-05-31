using Assets.Scrips.EXTRAS.STRUCTURES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.HELPERS
{
    public class Point
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            var item = obj as Point;
            if (item == null)
            {
                return false;
            }
            return x == item.x && y == item.y;
        }
        public override int GetHashCode()
        {
            return (x.GetHashCode() * 7) ^ (y.GetHashCode() * 11);
        }
    }

    public class Node
    {
        public int ID;
        public Vector3 position; //"Real position" value
        public int mapX, mapY; //2D array index
        int x, y;

        public Node(Node node)
        {
            ID = node.ID;
            position = new Vector3(node.position.x, node.position.y, node.position.z);
            mapX = node.mapX;
            mapY = node.mapY;
            x = node.x;
            y = node.y;
        }

        public void setMapXY(int mapX, int mapY)
        {
            this.mapX = mapX;
            this.mapY = mapY;
        }

        public Node(float x, float y, int ID, int mapX, int mapY)
        {
            this.position = new Vector3(x, 0.1f, y);
            this.ID = ID;
            this.mapX = mapX;
            this.mapY = mapY;
        }

        public Node(float x, float y, int ID)
        {
            this.position = new Vector3(x, 0.1f, y);
            this.ID = ID;
        }


        public override bool Equals(object obj)
        {
            var item = obj as Node;
            if (item == null)
            {
                return false;
            }
            return position.x == item.position.x && position.y == item.position.y && ID == item.ID;
        }
        public override int GetHashCode()
        {
            return (position.x.GetHashCode() * 7) ^ (position.y.GetHashCode() * 3) ^ ID;
        }
    }

}
