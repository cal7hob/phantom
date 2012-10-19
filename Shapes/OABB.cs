﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Shapes
{
    public class OABB : Polygon
    {
        public override float RoughRadius
        {
            get
            {
                return this.HalfSize.Length();
            }
        }

        public Vector2 HalfSize { get; protected set; }

        public OABB( Vector2 halfSize )
            :base(new Vector2(-halfSize.X, -halfSize.Y), new Vector2(halfSize.X, -halfSize.Y), new Vector2(halfSize.X, halfSize.Y), new Vector2(-halfSize.X, halfSize.Y))
        {
            this.HalfSize = halfSize;
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }
    }
}