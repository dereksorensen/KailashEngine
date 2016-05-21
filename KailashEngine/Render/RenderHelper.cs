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
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    );


                GL.GenTextures(1, out textureID);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    pixelFormat,
                    tWidth,
                    tHeight,
                    0,
                    PixelFormat.Bgra,
                    pixelType,
                    tBitmapData.Scan0);

                //GL.TexEnv(
                //    TextureEnvTarget.TextureEnv,
                //    TextureEnvParameter.TextureEnvMode,
                //    (float)TextureEnvMode.Modulate);
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
