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

        private struct ImageData
        {
            public System.Drawing.Bitmap bitmap;
            public System.Drawing.Imaging.BitmapData bitmap_data;

            public ImageData(System.Drawing.Bitmap bitmap, System.Drawing.Imaging.BitmapData bitmap_data)
            {
                this.bitmap = bitmap;
                this.bitmap_data = bitmap_data;
            }
        }

        private List<ImageData> _image_data_list;

        private Texture _texture;
        public Texture texture
        {
            get { return _texture; }
        }

        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------
        public Image(string filename, bool use_srgb)
            : this(filename, use_srgb, TextureTarget.Texture2D, TextureWrapMode.Repeat)
        { }

        public Image(string filename, bool use_srgb, TextureWrapMode wrap_mode = TextureWrapMode.Repeat)
            : this(filename, use_srgb, TextureTarget.Texture2D, wrap_mode)
        { }

        public Image(string filename, bool use_srgb, TextureTarget texture_target = TextureTarget.Texture2D, TextureWrapMode wrap_mode = TextureWrapMode.Repeat)
        {
            _image_data_list = new List<ImageData>();

            _filename = filename.Replace("%20", " ");

            if (File.Exists(_filename))
            {
                ImageData image_data = new ImageData();

                image_data.bitmap = new System.Drawing.Bitmap(_filename);
                
                int texture_width = image_data.bitmap.Width;
                int texture_height = image_data.bitmap.Height;

                image_data.bitmap_data = image_data.bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, texture_width, texture_height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    image_data.bitmap.PixelFormat
                    );

                // Add image data to list so we can load it into textures later
                _image_data_list.Add(image_data);

                PixelInternalFormat pif;
                PixelFormat pf;
                PixelType pt;

                switch (image_data.bitmap.PixelFormat)
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
                        throw new ArgumentException(Debug.DebugHelper.format("[ ERROR ] Unsupported Pixel Format on " + Path.GetFileName(_filename), image_data.bitmap.PixelFormat.ToString()));

                }

                if (use_srgb) pif = PixelInternalFormat.Srgb;

                // Load new texture
                _texture = new Texture(
                    texture_target,
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

        public Image(string[] filenames, bool use_srgb, TextureTarget texture_target = TextureTarget.Texture2D, TextureWrapMode wrap_mode = TextureWrapMode.Repeat)
        {
            _image_data_list = new List<ImageData>();

            PixelInternalFormat pif = PixelInternalFormat.Rgba;
            PixelFormat pf = PixelFormat.Bgra;
            PixelType pt = PixelType.UnsignedByte;

            int texture_width = 0;
            int texture_height = 0;

            foreach (string file in filenames)
            {
                _filename = file.Replace("%20", " ");

                if (File.Exists(_filename))
                {
                    ImageData image_data = new ImageData();

                    image_data.bitmap = new System.Drawing.Bitmap(_filename);

                    texture_width = image_data.bitmap.Width;
                    texture_height = image_data.bitmap.Height;

                    image_data.bitmap_data = image_data.bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, texture_width, texture_height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        image_data.bitmap.PixelFormat
                        );

                    // Add image data to list so we can load it into textures later
                    _image_data_list.Add(image_data);



                    switch (image_data.bitmap.PixelFormat)
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
                            throw new ArgumentException(Debug.DebugHelper.format("[ ERROR ] Unsupported Pixel Format on " + Path.GetFileName(_filename), image_data.bitmap.PixelFormat.ToString()));

                    }


                }
                else
                {
                    Debug.DebugHelper.logError("Texture Not Found", _filename);
                }



            }


            if (use_srgb) pif = PixelInternalFormat.Srgb;

            // Load new texture
            _texture = new Texture(
                texture_target,
                texture_width, texture_height, filenames.Length,
                false, false,
                pif, pf, pt,
                TextureMinFilter.Linear, TextureMagFilter.Linear, wrap_mode);
        }


        //------------------------------------------------------
        // Main Methods
        //------------------------------------------------------

        public void load()
        {
            if(_image_data_list.Count == 1)
            {
                // Load texture into GL memory
                _texture.load(_image_data_list[0].bitmap_data.Scan0);

                // Dispose of bitmap in System memory
                _image_data_list[0].bitmap.UnlockBits(_image_data_list[0].bitmap_data);
                _image_data_list[0].bitmap.Dispose();
            }
            else
            {
                _texture.load(_image_data_list.Select(d => d.bitmap_data.Scan0).ToArray());
                foreach(ImageData data in _image_data_list)
                {
                    data.bitmap.UnlockBits(data.bitmap_data);
                    data.bitmap.Dispose();
                }
            }
            
        }

        public void bind(int texture_uniform, int index)
        {
            _texture.bind(texture_uniform, index);
        }

    }
}
