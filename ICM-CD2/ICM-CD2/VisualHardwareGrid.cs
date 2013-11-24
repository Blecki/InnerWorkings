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
    public class VisualHardwareGrid : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Assemblage.IN8 CPU;


        private class HardwareDevice
        {
            internal IVisualHardware driver;
            internal byte[] ports;
            internal Rectangle position;
        }

        private List<HardwareDevice> hardware = new List<HardwareDevice>();

        public VisualHardwareGrid(Assemblage.IN8 CPU)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

            this.CPU = CPU;
         
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
        }
            
        public void AddHardware(System.Type hostedHardwareType, Point location, params byte[] ports)
        {
            var newHardware = new HardwareDevice();

            newHardware.ports = ports;
            
            newHardware.driver = Activator.CreateInstance(hostedHardwareType) as IVisualHardware;
            if (newHardware.driver == null) throw new InvalidProgramException("Cannot host devices that are not visual hardware.");

            newHardware.driver.Connect(CPU, ports);
            var size = newHardware.driver.PreferredWindowSize;

            newHardware.position = new Rectangle(location.X, location.Y, size.X, size.Y);

            hardware.Add(newHardware);
        }

        protected override void Initialize()
        {
            foreach (var device in hardware)
                device.driver.Initialize(GraphicsDevice, Content);
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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            var originalViewport = GraphicsDevice.Viewport;
            foreach (var device in hardware)
            {
                GraphicsDevice.Viewport = new Viewport(device.position);
                device.driver.Draw();
            }
            GraphicsDevice.Viewport = originalViewport;
            base.Draw(gameTime);
        }
    }
}
