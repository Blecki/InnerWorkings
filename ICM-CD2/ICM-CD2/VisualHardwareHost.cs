using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ICM_CD2
{
    public class VisualHardwareHost : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        IVisualHardware screenDevice;
        Assemblage.IN8 CPU;
        byte[] ports;

        public VisualHardwareHost(System.Type hostedHardwareType, Assemblage.IN8 CPU, params byte[] ports)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.CPU = CPU;
            this.ports = ports;

            screenDevice = Activator.CreateInstance(hostedHardwareType) as IVisualHardware;
            if (screenDevice == null) throw new InvalidProgramException("Cannot host devices that are not visual hardware.");

            screenDevice.Connect(CPU, ports);
            var size = screenDevice.PreferredWindowSize;

            graphics.PreferredBackBufferHeight = (int)size.X;
            graphics.PreferredBackBufferWidth = (int)size.Y;
        }

        protected override void Initialize()
        {
            screenDevice.Initialize(GraphicsDevice, Content);
            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            screenDevice.Draw();

            base.Draw(gameTime);
        }
    }
}
