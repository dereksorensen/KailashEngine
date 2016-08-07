using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Objects;

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

        public static readonly string uEnableDiffuseTexture = "enable_diffuse_texture";
        public static readonly string uDiffuseTexture = "diffuse_texture";
        public static readonly string uDiffuseColor = "diffuse_color";
        public static readonly string uEmission = "emission_strength";

        public static readonly string uEnableSpecularTexture = "enable_specular_texture";
        public static readonly string uSpecularTexture = "specular_texture";
        public static readonly string uSpecularColor = "specular_color";
        public static readonly string uSpecularShininess = "specular_shininess";

        public static readonly string uEnableNormalTexture = "enable_normal_texture";
        public static readonly string uNormalTexture = "normal_texture";

        public static readonly string uEnableDisplacementTexture = "enable_displacement_texture";
        public static readonly string uDisplacementTexture = "displacement_texture";
        public static readonly string uDisplacementStrength = "displacement_strength";

        public static readonly string uEnableParallaxTexture = "enable_parallax_texture";
        public static readonly string uParallaxTexture = "parallax_texture";

        public static readonly string uEnableSkinning = "enable_skinning";
        public static readonly string uBoneMatrices = "bone_matrices";

        // Light Calculation
        public static readonly string uLightPosition = "light_position";
        public static readonly string uLightDirection = "light_direction";
        public static readonly string uLightColor = "light_color";
        public static readonly string uLightIntensity = "light_intensity";
        public static readonly string uLightFalloff = "light_falloff";
        public static readonly string uLightSpotAngle = "light_spot_angle";
        public static readonly string uLightSpotBlur = "light_spot_blur";

        // Samplers
        public static readonly string uSamplerBase = "sampler";



    }
}
