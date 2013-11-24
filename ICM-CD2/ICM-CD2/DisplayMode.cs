using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICM_CD2
{
    public class DisplayMode
    {
        public bool streamingMode { get;  private set; }

        public Texture2D target;
        internal Color[] data;

        public int width { get; private set; }
        public int height { get; private set; }
        private int pixel_bit_width;
        private int pixel_bit_mask;
        private int pixels_per_byte;

        private TextDisplay streamingDisplay;

        public DisplayMode(
            bool Streaming, 
            GraphicsDevice Device,
            Microsoft.Xna.Framework.Content.ContentManager Content,
            int width, int height, int pixel_size)
        {
            this.streamingMode = Streaming;
            this.width = width;
            this.height = height;

            if (!Streaming)
            {
                this.pixel_bit_width = pixel_size;
                this.pixels_per_byte = 8 / pixel_size;
                this.pixel_bit_mask = 0;
                while (pixel_size > 0)
                {
                    this.pixel_bit_mask <<= 1;
                    this.pixel_bit_mask += 1;
                    pixel_size -= 1;
                }

                target = new Texture2D(Device, width, height);
                data = new Color[width * height];
            }
            else
            {
                streamingDisplay = new TextDisplay(width, height, Device, Content);
            }
        }

        public static void PrepareScreenBuffer(
            DisplayMode mode,
            byte[] MEM, 
            byte start_page, 
            Color[] palette)
        {
            for (int p = 0; p < mode.width * mode.height; ++p)
               mode.data[p] = palette[ExtractPixel(mode, MEM, start_page, p)];
            mode.target.SetData(mode.data);
        }

        public static TextDisplay PrepareTextDisplay(DisplayMode mode, String buffer)
        {
            int i = 0;
            for (; i < buffer.Length && i < mode.width * mode.height; ++i)
                mode.streamingDisplay.SetChar(i, buffer[i]);
            for (; i < mode.width * mode.height; ++i)
                mode.streamingDisplay.SetChar(i, ' ');
            return mode.streamingDisplay;
        }

        static int ExtractPixel(DisplayMode mode, byte[] MEM, byte start_page, int p)
        {
            var raw_data = MEM[(start_page * 256) + ( p / mode.pixels_per_byte )];
            raw_data >>= (mode.pixels_per_byte - (p % mode.pixels_per_byte) - 1) * mode.pixel_bit_width;
            return raw_data & mode.pixel_bit_mask;
        }
    }
}
