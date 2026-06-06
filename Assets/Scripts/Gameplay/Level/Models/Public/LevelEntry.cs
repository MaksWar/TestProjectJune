using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Level.Models.Public
{
    [Serializable]
    public class LevelEntry
    {
        public string LevelID;
        public string FigureId;
        public FigureType FigureType;
        public List<PathEntry> PathEntries = new();
    }

    [Serializable]
    public class PathEntry
    {
        public int Order;
        public PathEntryType Type;
        public bool Closed;
        public List<Vector2> Path = new();
        public List<PathPointEntry> PointEntries = new();
    }

    [Serializable]
    public class PathPointEntry
    {
        public Vector2 Position;
        public float Angle;
        public float HandleLength = 0.5f;
    }
}
