using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ICM_CD2
{
    interface IVisualHardware : Assemblage.Hardware
    {
        void Initialize(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager Content);
        void Draw();
        void Connect(Assemblage.IN8 CPU, params byte[] ports);
        Point PreferredWindowSize { get; }
    }
}
