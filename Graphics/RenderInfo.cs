﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.Graphics
{
    public class RenderInfo
    {
        public float AspectRatio
        {
            get
            {
                return this.Width / this.Height;
            }
        }

        public int Pass;
        public float Width;
        public float Height;
        public SpriteBatch Batch;
        public GraphicsDevice GraphicsDevice;
        public Matrix Projection;

    }
}
