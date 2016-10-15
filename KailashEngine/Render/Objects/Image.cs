using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Render.Objects
{
    class Image
    {

        private string _filename;
        public string filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private System.Drawing.Bitmap _bitmap;
        private System.Drawing.Imaging.BitmapData _bitmap_data;

        private Texture _texture;
        public Texture texture
        {
            get { return _texture; }
        }

        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public Image(string filename, bool use_srgb, TextureWrapMode wrap_mode = TextureWrapMode.Repeat)
        {

            _filename = filename.Replace("%20", " ");

            if (File.Exists(_filename))
            {
                _bitmap = new System.Drawing.Bitmap(_filename);
                
                int texture_width = _bitmap.Width;
                int texture_height = _bitmap.Height;

                _bitmap_data = _bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, texture_width, texture_height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    _bitmap.PixelFormat
                    );

                PixelInternalFormat pif;
                PixelFormat pf;
                PixelType pt;

                switch (_bitmap.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: // misses glColorTable setup
                        pif = PixelInternalFormat.Rgb8;
                        pf = PixelFormat.ColorIndex;
                        pt = PixelType.Bitmap;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: // does not work
                        pif = PixelInternalFormat.Rgb5A1;
                        pf = PixelFormat.Bgr;
                        pt = PixelType.UnsignedShort5551Ext;
                        break;
                    //case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    //    pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.R5G6B5IccSgix;
                    //    pf = OpenTK.Graphics.OpenGL.PixelFormat.R5G6B5IccSgix;
                    //    pt = OpenTK.Graphics.OpenGL.PixelType.UnsignedByte;
                    //    break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb: // works
                        pif = PixelInternalFormat.Rgb8;
                        pf = PixelFormat.Bgr;
                        pt = PixelType.UnsignedByte;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb: // has alpha too? wtf?
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb: // works
                        pif = PixelInternalFormat.Rgba;
                        pf = PixelFormat.Bgra;
                        pt = PixelType.UnsignedByte;
                        break;
                    default:
                        Console.WriteLine(_bitmap.PixelFormat);
                        throw new ArgumentException(Debug.DebugHelper.format("[ ERROR ] Unsupported Pixel Format on " + Path.GetFileName(_filename), _bitmap.PixelFormat.ToString()));

                }

                if (use_srgb) pif = PixelInternalFormat.Srgb;

                // Load new texture
                _texture = new Texture(
                    TextureTarget.Texture2D,
                    texture_width, texture_height, 0,
                    true, true,
                    pif, pf, pt,
                    TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, wrap_mode);
            }
            else
            {
                Debug.DebugHelper.logError("Texture Not Found", _filename);
            }
        }


        //------------------------------------------------------
        // Main Methods
        //------------------------------------------------------

        public void load()
        {
            // Load texture into GL memory
            _texture.load(_bitmap_data.Scan0);

            // Dispose of bitmap in System memory
            _bitmap.UnlockBits(_bitmap_data);
            _bitmap.Dispose();
        }

        public void bind(int texture_uniform, int index)
        {
            _texture.bind(texture_uniform, index);
        }

    }
}
