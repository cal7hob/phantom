﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Phantom.Cameras;

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

        public Renderer Renderer;
        public int Pass;
        public float Width;
        public float Height;
        public Canvas Canvas;
        public SpriteBatch Batch;
        public GraphicsDevice GraphicsDevice;
        public RenderTarget2D RenderTarget;
        public Camera Camera;
        public Matrix World;
        public Matrix Projection;
        public Vector2 Padding;
        public float Elapsed;

        public bool IsTopState;

    }
}
