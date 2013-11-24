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
    public class TextDisplay
    {
        private class Line
        {
            internal VertexBuffer buffer;
            internal bool dirty = true;
            internal Vertex[] verts;
        }
        
        Line[] lines;
        IndexBuffer indexBuffer;
        Texture2D font;
        Effect effect;
        float cursorTime = 0.0f;
        internal int cursorPosition = 0;

        float fontXScale = 1.0f / 16.0f;
        float fontYScale = 1.0f / 8.0f;

        public int width { get; private set; }
        public int height { get; private set; }

        int topRow = 0;
        public int TopRow { get { return topRow; } set { topRow = value % height; } }

        float cellWidth;
        float cellHeight;

        public TextDisplay(int width, int height, GraphicsDevice device, ContentManager content)
        {
            this.width = width;
            this.height = height;

            cellWidth = 1.0f / width;
            cellHeight = 1.0f / height;

            font = content.Load<Texture2D>("small-font");
            effect = content.Load<Effect>("draw_console");

            lines = new Line[height];
            var indicies = new short[width * 6];
            for (int i = 0; i < width; ++i)
            {
                indicies[i * 6 + 0] = (short)(i * 4 + 0);
                indicies[i * 6 + 1] = (short)(i * 4 + 1);
                indicies[i * 6 + 2] = (short)(i * 4 + 2);
                indicies[i * 6 + 3] = (short)(i * 4 + 2);
                indicies[i * 6 + 4] = (short)(i * 4 + 3);
                indicies[i * 6 + 5] = (short)(i * 4 + 0);
            }

            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indicies.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indicies);

            for (int y = 0; y < height; ++y)
            {
                lines[y] = new Line();
                lines[y].verts = new Vertex[width * 4];
                for (int i = 0; i < width; ++i)
                {
                    lines[y].verts[i * 4 + 0].Position = new Vector3(i * cellWidth, y * cellHeight, 0);
                    lines[y].verts[i * 4 + 1].Position = new Vector3((i + 1) * cellWidth, y * cellHeight, 0);
                    lines[y].verts[i * 4 + 2].Position = new Vector3((i + 1) * cellWidth, (y + 1) * cellHeight, 0);
                    lines[y].verts[i * 4 + 3].Position = new Vector3(i * cellWidth, (y + 1) * cellHeight, 0);

                    //lines[y].verts[i * 4 + 0].FGColor = Color.White.ToVector4();
                    //lines[y].verts[i * 4 + 1].FGColor = Color.White.ToVector4();
                    //lines[y].verts[i * 4 + 2].FGColor = Color.White.ToVector4();
                    //lines[y].verts[i * 4 + 3].FGColor = Color.White.ToVector4();
                    //lines[y].verts[i * 4 + 0].BGColor = Color.Black.ToVector4();
                    //lines[y].verts[i * 4 + 1].BGColor = Color.Black.ToVector4();
                    //lines[y].verts[i * 4 + 2].BGColor = Color.Black.ToVector4();
                    //lines[y].verts[i * 4 + 3].BGColor = Color.Black.ToVector4();
                }
                lines[y].buffer = new VertexBuffer(device, typeof(Vertex), lines[y].verts.Length, BufferUsage.None);
            }
        }

        public void SetChar(int place, int character, Color? fg = null, Color? bg = null)
        {
            //character -= ' ';
            character &= 127;
            var charX = character % 16;
            var charY = character / 16;

            place += topRow * width;
            if (place >= width * height) place -= width * height;

            var row = place / width;
            place %= width;

            var Kerning = 0;// fontXScale / 6.0f;
            lines[row].verts[place * 4 + 0].TextureCoordinate = new Vector2((charX * fontXScale) + Kerning, charY * fontYScale);
            lines[row].verts[place * 4 + 1].TextureCoordinate = new Vector2((charX + 1) * fontXScale - Kerning , charY * fontYScale);
            lines[row].verts[place * 4 + 2].TextureCoordinate = new Vector2((charX + 1) * fontXScale - Kerning, (charY + 1) * fontYScale);
            lines[row].verts[place * 4 + 3].TextureCoordinate = new Vector2((charX * fontXScale) + Kerning, (charY + 1) * fontYScale);

            //if (fg != null)
            //{
            //    lines[row].verts[place * 4 + 0].FGColor = fg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 1].FGColor = fg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 2].FGColor = fg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 3].FGColor = fg.Value.ToVector4();
            //}

            //if (bg != null)
            //{
            //    lines[row].verts[place * 4 + 0].BGColor = bg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 1].BGColor = bg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 2].BGColor = bg.Value.ToVector4();
            //    lines[row].verts[place * 4 + 3].BGColor = bg.Value.ToVector4();
            //}


            lines[row].dirty = true;
        }

        private void drawRows(int start, int end, GraphicsDevice device)
        {
            while (start < end)
            {
                if (lines[start].dirty)
                {
                    lines[start].buffer.SetData(lines[start].verts);
                    lines[start].dirty = false;
                }
                device.SetVertexBuffer(lines[start].buffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, width * 4, 0, width * 2);
                ++start;
            }
        }

        public void Draw(GraphicsDevice device)
        {
            effect.Parameters["Texture"].SetValue(font);
            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(-0.025f, 1.025f, 1.025f, -0.025f, -1f, 1f));
            effect.Parameters["View"].SetValue(Matrix.Identity);
            effect.CurrentTechnique = effect.Techniques[0];

            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.AlphaBlend;

            device.Indices = indexBuffer;

            if (topRow == 0)
            {
                effect.Parameters["World"].SetValue(Matrix.Identity);
                effect.CurrentTechnique.Passes[0].Apply();
                drawRows(0, height, device);
            }
            else
            {
                effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, -cellHeight * topRow, 0)));
                effect.CurrentTechnique.Passes[0].Apply();
                drawRows(topRow, height, device);
                effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, cellHeight * (height - topRow), 0)));
                effect.CurrentTechnique.Passes[0].Apply();
                drawRows(0, topRow, device);
            }

            device.SetVertexBuffer(null);
        }
    }
}
