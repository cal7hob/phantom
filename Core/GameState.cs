﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Cameras;

namespace Phantom.Core
{
    public class GameState : Component
    {
        public bool Transparent { get; protected set; }
        public bool Propagate { get; protected set; }
        public bool OnlyOntop { get; protected set; }

        public Input Input { get; protected set; }

        public Camera Camera { get; protected set; }


        public GameState()
        {
            this.AddComponent(new Input());
        }

        protected override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            if (component is Camera)
            {
                if (this.Camera != null)
                    this.RemoveComponent(this.Camera);
                this.Camera = component as Camera;
            }
			if (component is Input)
			{
				if (this.Input != null)
					this.RemoveComponent(this.Input);
				this.Input = component as Input;
			}
        }

        public virtual void BackOnTop()
        {
            this.Input.JustBack = true;
        }
    }
}
