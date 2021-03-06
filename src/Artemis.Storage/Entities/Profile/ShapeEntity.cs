﻿using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile
{
    public class ShapeEntity
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public ShapeEntityType Type { get; set; }
        public List<ShapePointEntity> Points { get; set; }
    }

    public class ShapePointEntity
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public enum ShapeEntityType
    {
        Ellipse,
        Fill,
        Polygon,
        Rectangle
    }
}