using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ICM_CD2
{
    public class SevenSegment : IVisualHardware
    {
        SpriteBatch batch;
        Texture2D texture;
        byte state = 0;

        public void Initialize(
            Microsoft.Xna.Framework.Graphics.GraphicsDevice device,
            Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            batch = new SpriteBatch(device);
            texture = Content.Load<Texture2D>("7-segment");
        }

        public void Draw()
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullNone);
           
            var ls = state;
            var i = 0;
            while (ls != 0)
            {
                if (ls % 2 == 1) batch.Draw(texture, new Rectangle(0, 0, 32, 64),
                    new Rectangle(32 * i, 0, 32, 64), Color.White);
                ls >>= 1;
                i += 1;
            }

            batch.End();
        }

        public Point PreferredWindowSize { get { return new Point(128, 128); } }

        public void Connect(Assemblage.IN8 CPU, params byte[] ports)
        {
            if (ports.Length != 1) throw new InvalidOperationException("7-segment display takes exactly one port");
            CPU.AttachHardware(this, ports);
        }

        public void PortWritten(byte port, byte value)
        {
            state = value;
        }

        public void Update()
        {
        }
    }
}
