using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Render
{
    static class RenderHelper
    {


        //------------------------------------------------------
        // Standard Program Uniform Names
        //------------------------------------------------------
        // Model Loading
        public static readonly string uModel = "model";
        public static readonly string uModel_Normal = "model_normal";

        public static readonly string uDiffuseColor = "diffuse_color";
        public static readonly string uDiffuseTexture = "diffuse_texture";
        public static readonly string uEmission = "emission_strength";

        public static readonly string uSpecularColor = "specular_color";
        public static readonly string uSpecularShininess = "specular_shininess";
        public static readonly string uSpecularTexture = "specular_texture";

        public static readonly string uDispacementTexture = "displacement_texture";

        public static readonly string uParallaxTexture = "parallax_texture";


        // Samplers
        public static readonly string uSamplerBase = "sampler";


        //------------------------------------------------------
        // Image Loading
        //------------------------------------------------------
        public static void getMaxMip(int width, int height)
        {

        }

        public static void loadImage(string filename, TextureWrapMode wrapmode, PixelInternalFormat pixelFormat, PixelType pixelType, ref int textureID)
        {
            if (System.IO.File.Exists(filename))
            {

                System.Drawing.Bitmap tBitmap = new System.Drawing.Bitmap(filename);

                int tWidth = tBitmap.Width;
                int tHeight = tBitmap.Height;


                System.Drawing.Imaging.BitmapData tBitmapData = tBitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, tWidth, tHeight),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    tBitmap.PixelFormat
                    );

                PixelInternalFormat pif;
                PixelFormat pf;
                PixelType pt;

                switch (tBitmap.PixelFormat)
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
                        Console.WriteLine(tBitmap.PixelFormat);
                        throw new ArgumentException("ERROR: Unsupported Pixel Format " + tBitmap.PixelFormat);

                }
                Console.WriteLine(tBitmap.PixelFormat);


                GL.GenTextures(1, out textureID);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    pif,
                    tWidth,
                    tHeight,
                    0,
                    pf,
                    pt,
                    tBitmapData.Scan0);


                GL.TexEnv(
                    TextureEnvTarget.TextureEnv,
                    TextureEnvParameter.TextureEnvMode,
                    (float)TextureEnvMode.Modulate);
                GL.TexParameter(
                    TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (float)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(
                    TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (float)TextureMagFilter.Linear);
                GL.TexParameter(
                    TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (float)wrapmode);
                GL.TexParameter(
                    TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (float)wrapmode);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, (float)7);
                GL.GenerateTextureMipmap(textureID);

                float fLargest;
                fLargest = GL.GetFloat((GetPName)All.MaxTextureMaxAnisotropyExt);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureMaxAnisotropyExt, fLargest);


                tBitmap.UnlockBits(tBitmapData);
                tBitmap.Dispose();

            }
            else
            {
                Debug.DebugHelper.logError("Texture Not Found", filename);
            }

        }


    }
}
