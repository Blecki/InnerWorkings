using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICM_CD2
{
    public class ICM : IVisualHardware
    {
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Texture2D backGround;
        Assemblage.IN8 CPU;
        byte dataPort = 0;
        byte controlPort = 0;
        byte dataByte = 0;
        byte currentMode = 0;

        DisplayMode[] availableModes;
        Color[] palette = new Color[16];
        byte memory_map_start;
        byte[] MEM = new byte[256 * 64]; //Enough space for the largest display mode.

        byte[] long_table = { 0x00, 0x24, 0x48, 0x6C, 0x90, 0xB4, 0xD8, 0xFF };
        byte[] short_table = { 0x00, 0x55, 0xAA, 0xFF };

        String streamBuffer = "";

        public ICM()
        {
            for (var i = 0; i < 16; ++i) palette[i] = Color.Black;
        }

        public void Initialize(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            this.device = device;
            spriteBatch = new SpriteBatch(device);
            backGround = Content.Load<Texture2D>("black");

            availableModes = new DisplayMode[] {
                new DisplayMode(true, device, Content, 42, 32, 7), //Compatible
                new DisplayMode(false, device, Content, 128, 128, 1), // High02
                new DisplayMode(false, device, Content, 256, 256, 1), // Super02
                new DisplayMode(false,device, Content, 64, 64, 2), // Mid04
                new DisplayMode(false, device, Content, 128, 128, 2), // High04
                new DisplayMode(false, device, Content, 256, 256, 2), // Super04
                new DisplayMode(false, device, Content, 32, 32, 4), // Low16
                new DisplayMode(false, device, Content, 64, 64, 4), // Mid16
                new DisplayMode(false, device, Content, 128, 128, 4), // High16
                new DisplayMode(true, device, Content, 84, 32, 7), //Wide compatible
            };
        }

        public void Draw()
        {
            if (availableModes != null &&
                currentMode >= 0 &&
                currentMode < availableModes.Length &&
                availableModes[currentMode] != null)
            {
                spriteBatch.Begin(
                    SpriteSortMode.BackToFront,
                    BlendState.Opaque,
                    SamplerState.PointClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone);
                spriteBatch.Draw(
                    backGround, 
                    new Rectangle(0, 0, device.Viewport.Bounds.Width, device.Viewport.Bounds.Height),
                    Color.White);
                spriteBatch.End();

                if (availableModes[currentMode].streamingMode)
                {
                    var rect = device.Viewport.Bounds;
                    rect.Inflate(-8, -8);
                    device.Viewport = new Viewport(rect);
                    var buffSize = availableModes[currentMode].width * availableModes[currentMode].height;
                    if (streamBuffer.Length > buffSize)
                        streamBuffer = streamBuffer.Substring(streamBuffer.Length - buffSize + availableModes[currentMode].width
                            - (streamBuffer.Length % availableModes[currentMode].width));
                    var textDisplay = DisplayMode.PrepareTextDisplay(availableModes[currentMode], streamBuffer);
                    textDisplay.Draw(device);
                    device.Textures[0] = null;
                }
                else
                {
                    DisplayMode.PrepareScreenBuffer(availableModes[currentMode], MEM, 0, palette);

                    spriteBatch.Begin(
                        SpriteSortMode.BackToFront,
                        BlendState.Opaque,
                        SamplerState.PointClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);
                    spriteBatch.Draw(availableModes[currentMode].target, 
                        new Rectangle(8,8,512,512),
                        Color.White);
                    spriteBatch.End();
                    device.Textures[0] = null;
                }
            }
        }

        public void Connect(Assemblage.IN8 CPU, params byte[] ports)
        {
            if (ports.Length != 2) throw new InvalidOperationException("ICM-CD2 display must be connected to 2 ports");
            this.CPU = CPU;
            dataPort = ports[0];
            controlPort = ports[1];

            CPU.AttachHardware(this, ports);
        }

        public Point PreferredWindowSize { get { return new Point(528, 528); } }

        public void PortWritten(byte port, byte value)
        {
            if (port == dataPort)
            {
                dataByte = value;
                streamBuffer += (char)value;
                if (streamBuffer.Length > (100 * 50))
                    streamBuffer = streamBuffer.Substring(streamBuffer.Length - (100 * 50));
            }
            else if (port == controlPort)
            {
                if (value == 0x01)
                {
                    currentMode = dataByte;
                }
                else if (value == 0x02)
                {
                    memory_map_start = dataByte;
                }
                else if (value == 0x03)
                {
                    if (currentMode == 0 || currentMode == 9)
                        streamBuffer = "";
                    else
                        Array.Copy(CPU.MEM,
                            memory_map_start * 256, MEM, 0, Math.Min(MEM.Length, 0x10000 - (memory_map_start * 256)));
                }
                else if (value >= 0xF0)
                {
                    int palettePosition = value & 0x0F;

                    var r = long_table[dataByte >> 5];
                    var g = long_table[(dataByte >> 2) & 0x07];
                    var b = short_table[dataByte & 3];

                    palette[palettePosition] = new Color(r, g, b);
                }
            }
        }

        public void Update()
        {

        }
    }
}
