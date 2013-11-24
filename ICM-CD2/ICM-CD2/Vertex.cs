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
    public struct Vertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        //public Vector4 FGColor;
        //public Vector4 BGColor;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            //new VertexElement(sizeof(float)*5, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
            //new VertexElement(sizeof(float)*9, VertexElementFormat.Vector4, VertexElementUsage.Color, 1)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
}
