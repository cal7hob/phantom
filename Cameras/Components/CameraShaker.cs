﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Core;

namespace Phantom.Cameras.Components
{
	public class CameraShaker : CameraComponent
	{

		private float timer;
		private float delay;
		private float time;
        private float intensity;


        public override void HandleMessage(Message message)
        {
            if (message == Messages.CameraShake)
            {
                if (message.Data is float)
                {
                    float time = (float)message.Data;
                    this.Shake(time, 1);
                }
                else if (message.Data is Vector2)
                {
                    Vector2 v = (Vector2)message.Data;
                    this.Shake(v.X, v.Y);
                }
            }
        }

		public override void Update(float elapsed)
		{
			this.timer += elapsed;
			base.Update(elapsed);
			if (this.delay > 0)
			{
                this.delay -= elapsed;
                float noiseX = (float)(Math.Cos(timer * 20) * 0.5 + Math.Cos(timer * 60) * 0.3 + Math.Cos(timer * 90) * 0.1);
                float noiseY = (float)(Math.Sin(timer * 18) * 0.5 + Math.Cos(timer * 65) * 0.3 + Math.Sin(timer * 92) * 0.1);
                float x = (timer - (this.time / 2));
				float parabola = -(x * x) + this.time;
                this.Camera.Target.X += noiseX * intensity * delay/time;
                this.Camera.Target.Y += noiseY * intensity * delay/time;
				//this.Camera.Orientation = noise * MathHelper.PiOver4 * .1f;
			}
			else
				this.Camera.Orientation = 0;
		}

		private void Shake(float time, float intensity)
		{
            this.intensity = intensity;
			this.delay = this.time = time;
			this.timer = 0;
		}
	}
}
