﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Phantom.Misc;

namespace Phantom.Graphics
{
    public class Canvas
    {

        public float LineWidth;
        public Color StrokeColor;
        public Color FillColor;

        private RenderInfo info;
        private GraphicsDevice device;
        private BasicEffect effect;
        private List<CanvasAction> stack;

        // Buffers:
        private static VertexPositionColor[] pixel;
        private static Dictionary<int, CircleBuffer> circles;

        public Canvas(GraphicsDevice graphicsDevice)
        {
            this.device = graphicsDevice;
            this.effect = new BasicEffect(this.device);

            this.stack = new List<CanvasAction>();

            // Canvas Attributes Defaults:
            this.LineWidth = 1;
            this.StrokeColor = Color.Black;
            this.FillColor = Color.White;

            this.SetupGraphics();
        }

        internal void SetRenderInfo(RenderInfo info)
        {
            this.info = info;
        }

        private void SetupGraphics()
        {
            if (Canvas.pixel == null)
            {
                Canvas.pixel = new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(-.5f,-.5f,0),Color.White),
                    new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                    new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                    new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                    new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                    new VertexPositionColor(new Vector3(.5f,.5f,0),Color.White)
                };
            }

            // Build multiple cirlce buffers for multiple number of segments:
            if (Canvas.circles == null)
            {
                Canvas.circles = new Dictionary<int, CircleBuffer>();
                for (int i = 16; (i >> 1) < 1920; i <<= 1)
                    Canvas.circles[i] = new CircleBuffer(i);
            }

        }

        private void FillRect(Vector2 position, Vector2 halfSize, float angle, Color color)
        {
            Matrix scale = Matrix.CreateScale(new Vector3(halfSize*2, 0));
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));

            this.effect.World = scale * rotation * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, Canvas.pixel, 0, 2);
        }

        private void FillCircle(Vector2 position, float radius, Color color)
        {
            int segments;
            CircleBuffer circle;
            GetCircleBufferByRadius(radius, out segments, out circle);

            Matrix scale = Matrix.CreateScale(radius);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));
            this.effect.World = scale * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, circle.Vertices, 0, segments + 1, circle.Indices, 0, segments);
        }

        public void FillRect(Vector2 position, Vector2 halfSize, float angle)
        {
            this.FillRect(position, halfSize, angle, this.FillColor);
        }

        public void FillCircle(Vector2 position, float radius)
        {
            this.FillCircle(position, radius, this.FillColor);
        }

        public void StrokeLine(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            float sf = d.Length();
            float angle = (float)Math.Atan2(d.Y, d.X);

            Matrix scale = Matrix.CreateScale(new Vector3(sf, this.LineWidth, 0));
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix translation = Matrix.CreateTranslation(new Vector3(a + (d * .5f), 0));

            this.effect.World = scale * rotation * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = this.StrokeColor.ToVector3();
            this.effect.Alpha = this.StrokeColor.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, Canvas.pixel, 0, 2);

        }

        public void StrokeRect(Vector2 position, Vector2 halfSize, float angle)
        {
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix quarter = Matrix.CreateRotationZ(MathHelper.PiOver2);
            Matrix half = Matrix.CreateRotationZ(MathHelper.Pi);
            Matrix three = Matrix.CreateRotationZ(MathHelper.PiOver2+MathHelper.Pi);
            Vector2 a = position - Vector2.Transform(halfSize, rotation);
            Vector2 b = position - Vector2.Transform(halfSize, rotation * quarter);
            Vector2 c = position - Vector2.Transform(halfSize, rotation * half);
            Vector2 d = position - Vector2.Transform(halfSize, rotation * three);
            this.StrokeLine(a, b);
            this.StrokeLine(b, c);
            this.StrokeLine(c, d);
            this.StrokeLine(d, a);
            float halfWidth = this.LineWidth * .5f;
            this.FillCircle(a, halfWidth, this.StrokeColor);
            this.FillCircle(b, halfWidth, this.StrokeColor);
            this.FillCircle(c, halfWidth, this.StrokeColor);
            this.FillCircle(d, halfWidth, this.StrokeColor);
        }

        public void StrokeCircle(Vector2 position, float radius)
        {
            int segments;
            CircleBuffer circle;
            GetCircleBufferByRadius(radius, out segments, out circle);

            float strokeScale = .5f / radius * this.LineWidth;
            VertexPositionColor[] vertices = new VertexPositionColor[segments * 2];
            short[] indices = new short[segments * 2 + 2];
            for (int i = 0; i < segments; i++)
            {
                Vector3 pos = circle.Vertices[i + 1].Position;
                vertices[i * 2] = new VertexPositionColor(pos * (1f + strokeScale), Color.White);
                vertices[i * 2 + 1] = new VertexPositionColor(pos * (1f - strokeScale), Color.White);

                indices[i * 2] = (short)(i * 2);
                indices[i * 2 + 1] = (short)(i * 2 + 1);
            }
            indices[segments * 2] = 0;
            indices[segments * 2 + 1] = 1;

            Matrix scale = Matrix.CreateScale(radius);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));
            this.effect.World = scale * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = this.StrokeColor.ToVector3();
            this.effect.Alpha = this.StrokeColor.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, vertices, 0, segments * 2, indices, 0, segments * 2);

        }

        public void Stroke()
        {
            float halfWidth = this.LineWidth * .5f;

            this.stack.Insert(0, new CanvasAction(0, Vector2.Zero));

            for (int i = 1; i < this.stack.Count; i++)
            {
                CanvasAction prev = this.stack[i-1];
                CanvasAction curr = this.stack[i];
                switch (curr.Action)
                {
                    case 0:
                        if (prev.Action == 1)
                            this.FillCircle(prev.Position, halfWidth, this.StrokeColor);
                        break;
                    case 1:
                        this.StrokeLine(prev.Position, curr.Position);
                        //if (i > 1 && i < this.stack.Count) // TODO: not if 90 angle
                        this.FillCircle(prev.Position, halfWidth, this.StrokeColor);
                        break;
                }
            }
            this.stack.RemoveAt(0);

            Vector2 last = this.stack[this.stack.Count - 1].Position;
            if (this.stack[0].Position != last)
                this.FillCircle(last, halfWidth, this.StrokeColor);
        }

        public void Fill()
        {
            List<Vector2> poly = new List<Vector2>();

            this.stack.Insert(0, new CanvasAction(0, Vector2.Zero));

            for (int i = 1; i < this.stack.Count; i++)
            {
                CanvasAction prev = this.stack[i-1];
                CanvasAction curr = this.stack[i];
                switch (curr.Action)
                {
                    case 0:
                        if (poly.Count != 0)
                            FillPolygon(poly, this.FillColor);
                        poly.Clear();
                        break;
                    case 1:
                        if (prev.Action == 0)
                            poly.Add(prev.Position);
                        poly.Add(curr.Position);
                        break;
                }
            }
            if (poly.Count != 0)
                FillPolygon(poly, this.FillColor);

            this.stack.RemoveAt(0);
        }

        private void FillPolygon(List<Vector2> poly, Color color)
        {
            if (poly[0] != poly[poly.Count - 1])
                poly.Add(poly[0]);
            VertexPositionColor[] vertices = new VertexPositionColor[poly.Count];
            for (int i = 0; i < poly.Count; i++)
                vertices[i] = new VertexPositionColor(new Vector3(poly[i],0), Color.White);
            short[] indices = Triangulator.Triangulate(poly.ToArray());

            this.effect.World = this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);

        }

        public void Begin()
        {
            this.stack.Clear();
        }

        public void MoveTo(Vector2 position)
        {
            this.stack.Add(new CanvasAction(0, position));
        }
        public void MoveTo(float x, float y)
        {
            this.MoveTo(new Vector2(x, y));
        }
        public void LineTo(Vector2 position)
        {
            this.stack.Add(new CanvasAction(1, position));
        }
        public void LineTo(float x, float y)
        {
            this.LineTo(new Vector2(x, y));
        }


        private static void GetCircleBufferByRadius(float radius, out int segments, out CircleBuffer circle)
        {
            int guess = (int)Math.Max(12, Math.Log(radius) * (Math.Log(radius) * .75) * 12);
            segments = (int)Math.Pow(2, Math.Ceiling(Math.Log(guess) / Math.Log(2)));
            if (!Canvas.circles.ContainsKey(segments))
            {
                Canvas.circles[segments] = new CircleBuffer(segments);
                Debug.WriteLine("new CircleBuffer created for {0} segments.", segments);
            }
            circle = Canvas.circles[segments];
        }

        private class CircleBuffer
        {
            public readonly int Segments;
            public readonly VertexPositionColor[] Vertices;
            public readonly short[] Indices;

            public CircleBuffer(int segments)
            {
                this.Segments = segments;
                this.Vertices = new VertexPositionColor[this.Segments + 1];
                this.Indices = new short[this.Segments * 3];
                this.BuildFillTriangles();
            }

            private void BuildFillTriangles()
            {
                float step = MathHelper.TwoPi / this.Segments;
                this.Vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
                this.Vertices[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);
                int indexCount = 0;
                for (short i = 1; i < this.Segments; i++)
                {
                    float angle = i * step;
                    Vector3 v = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
                    this.Vertices[i+1] = new VertexPositionColor(v, Color.White);
                    this.Indices[indexCount++] = 0;
                    this.Indices[indexCount++] = i;
                    this.Indices[indexCount++] = (short)(i + 1);
                }
                this.Indices[indexCount++] = 0;
                this.Indices[indexCount++] = (short)this.Segments;
                this.Indices[indexCount++] = 1;
            }
        }

        private struct CanvasAction
        {
            public int Action;
            public Vector2 Position;
            public CanvasAction(int action, Vector2 position)
            {
                this.Action = action;
                this.Position = position;
            }
        }
    }
}