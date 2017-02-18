using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Render;
using KailashEngine.World.Model;
using KailashEngine.World.Lights;

namespace KailashEngine.World
{
    static class WorldDrawer
    {

        private static void trySetBindlessTextureUnit(Program program, Render.Objects.Image image, string uTextureUnit, string uEnableTexture, int texture_unit)
        {
            if (image != null)
            {
                program.setUniform1(uTextureUnit, texture_unit);
                program.setUniform1(uEnableTexture, 1);
            }
            else
            {
                program.setUniform1(uEnableTexture, 0);
            }
        }


        // Standard OGL calls to draw meshes / lights
        private static void draw(Mesh mesh, string mesh_category)
        {
            draw(mesh, BeginMode.Triangles, mesh_category);
        }

        private static void draw(Mesh mesh, BeginMode begin_mode, string mesh_category)
        {
            try
            {
                GL.BindVertexArray(mesh.vao);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ibo);
                GL.DrawElements(begin_mode, mesh.index_data.Length, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            catch (Exception e)
            {
                throw new Exception("Failed Drawing: " + mesh.id + " [ " + mesh_category + " ] ", e);
            }
        }


        //------------------------------------------------------
        // Mesh Drawing
        //------------------------------------------------------
        /*
         * Draw Modes:
         * 0 = Basic
         * 1 = 0 + With Materials
         * 2 = 1 + Record Previous Model Matrix
         * 
         */
        public static void drawMeshes(BeginMode begin_mode, List<UniqueMesh> meshes, Program program, Matrix4 transformation, float animation_time, int draw_mode)
        {
            foreach (UniqueMesh unique_mesh in meshes)
            {

                Matrix4 temp_mat = unique_mesh.transformation;
                Matrix4 temp_mat_previous = unique_mesh.previous_transformation;
                
                //------------------------------------------------------
                // Object Animation Matrix
                //------------------------------------------------------
                if (unique_mesh.animated)
                {
                    temp_mat = unique_mesh.animator.getKeyFrame(animation_time, -1);
                }
                
                //------------------------------------------------------
                // Physics Matrix
                //------------------------------------------------------
                if (unique_mesh.physical)
                {
                    if (unique_mesh.animated && unique_mesh.physics_object.kinematic)
                    {
                        BulletSharp.Math.Matrix temp_transform = EngineHelper.otk2bullet(temp_mat.ClearScale());
                        unique_mesh.physics_object.body.MotionState.WorldTransform = temp_transform;
                        //unique_mesh.physics_object.body.ProceedToTransform(temp_transform);
                    }
                    temp_mat = Matrix4.CreateScale(temp_mat.ExtractScale()) * EngineHelper.bullet2otk(unique_mesh.physics_object.body.MotionState.WorldTransform);
                }

                //------------------------------------------------------
                // World Matrix
                //------------------------------------------------------
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);

                // Handle previous frame's world matrix
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel_Previous), false, ref temp_mat_previous);
                if(draw_mode > 1) unique_mesh.previous_transformation = temp_mat;

                // Convert matrix for normals
                try
                {
                    temp_mat = Matrix4.Invert(temp_mat);
                    temp_mat = Matrix4.Transpose(temp_mat);
                }
                catch { }              
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel_Normal), false, ref temp_mat);


                //------------------------------------------------------
                // Skinning
                //------------------------------------------------------
                if(unique_mesh.mesh.skinned)
                {
                    DAE_Skeleton temp_skeleton = unique_mesh.mesh.skeleton;
                    if(temp_skeleton.animated)
                    {
                        temp_skeleton.updateBones(temp_skeleton.root, temp_skeleton.animator.getKeyFrame(animation_time, 2));
                    }
                    Matrix4[] skinning_matrices = temp_skeleton.getBoneMatrices();
                    GL.Uniform1(program.getUniform(RenderHelper.uEnableSkinning), 1);
                    GL.UniformMatrix4(program.getUniform(RenderHelper.uBoneMatrices), skinning_matrices.Length, true, EngineHelper.createArray(skinning_matrices));
                }
                else
                {
                    GL.Uniform1(program.getUniform(RenderHelper.uEnableSkinning), 0);
                }

                foreach (Mesh submesh in unique_mesh.mesh.submeshes)
                {
                    if (draw_mode > 0)
                    {
                        //------------------------------------------------------
                        // Set Material Properties
                        //------------------------------------------------------

                        GL.Uniform3(program.getUniform(RenderHelper.uDiffuseColor), submesh.material.diffuse_color);
                        GL.Uniform1(program.getUniform(RenderHelper.uEmission), submesh.material.emission);
                        GL.Uniform3(program.getUniform(RenderHelper.uSpecularColor), submesh.material.specular_color);
                        GL.Uniform1(program.getUniform(RenderHelper.uSpecularShininess), submesh.material.specular_shininess);
                        GL.Uniform1(program.getUniform(RenderHelper.uDisplacementStrength), submesh.material.displacement_strength);

                        // Diffuse
                        trySetBindlessTextureUnit(program, submesh.material.diffuse_image, RenderHelper.uDiffuseTextureUnit, RenderHelper.uEnableDiffuseTexture, submesh.material.diffuse_image_unit);

                        // Specular
                        trySetBindlessTextureUnit(program, submesh.material.specular_image, RenderHelper.uSpecularTextureUnit, RenderHelper.uEnableSpecularTexture, submesh.material.specular_image_unit);

                        // Normal
                        trySetBindlessTextureUnit(program, submesh.material.normal_image, RenderHelper.uNormalTextureUnit, RenderHelper.uEnableNormalTexture, submesh.material.normal_image_unit);

                        // Displacement
                        trySetBindlessTextureUnit(program, submesh.material.displacement_image, RenderHelper.uDisplacementTextureUnit, RenderHelper.uEnableDisplacementTexture, submesh.material.displacement_image_unit);

                        // Parallax
                        trySetBindlessTextureUnit(program, submesh.material.parallax_image, RenderHelper.uParallaxTextureUnit, RenderHelper.uEnableParallaxTexture, submesh.material.parallax_image_unit);
                    }

                    draw(submesh, begin_mode, "mesh");

                }
            }
        }


        //------------------------------------------------------
        // Light Drawing
        //------------------------------------------------------
        public static void drawLights(BeginMode begin_mode, List<Light> lights, Program program, Matrix4 transformation, float animation_time, bool display_light_bounds)
        {
            // Disable skinning
            GL.Uniform1(program.getUniform(RenderHelper.uEnableSkinning), 0);

            //------------------------------------------------------
            // Light Objects
            //------------------------------------------------------
            foreach (Light light in lights)
            {
                if (light.unique_mesh == null) continue;

                // Load Mesh's pre-transformation Matrix
                Matrix4 temp_mat = light.unique_mesh.transformation;
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);

                // Handle previous frame's world matrix
                Matrix4 temp_mat_previous = light.unique_mesh.previous_transformation;
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel_Previous), false, ref temp_mat_previous);
                light.unique_mesh.previous_transformation = temp_mat;

                // Convert matrix for normals
                temp_mat = Matrix4.Invert(temp_mat);
                temp_mat = Matrix4.Transpose(temp_mat);
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel_Normal), false, ref temp_mat);



                GL.Uniform3(program.getUniform(RenderHelper.uDiffuseColor), light.color);
                GL.Uniform1(program.getUniform(RenderHelper.uEmission), light.object_emission);
                GL.Uniform3(program.getUniform(RenderHelper.uSpecularColor), new Vector3(0.0f));
                GL.Uniform1(program.getUniform(RenderHelper.uSpecularShininess), 0.0f);
                GL.Uniform1(program.getUniform(RenderHelper.uDisplacementStrength), 0.0f);


                // Diffuse
                program.setUniform1(RenderHelper.uEnableDiffuseTexture, 0);

                // Specular
                program.setUniform1(RenderHelper.uEnableSpecularTexture, 0);

                // Normal
                program.setUniform1(RenderHelper.uEnableNormalTexture, 0);

                // Displacement
                program.setUniform1(RenderHelper.uEnableDisplacementTexture, 0);

                // Parallax
                program.setUniform1(RenderHelper.uEnableParallaxTexture, 0);



                draw(light.unique_mesh.mesh.submeshes.ElementAt(0), begin_mode, "light");

                //------------------------------------------------------
                // Display Light Bounds
                //------------------------------------------------------
                if (display_light_bounds)
                {
                    GL.Disable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

                    // Load Mesh's pre-transformation Matrix
                    temp_mat = light.bounding_unique_mesh.transformation;
                    GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);

                    draw(light.bounding_unique_mesh.mesh.submeshes.ElementAt(0), begin_mode, "light bounds");

                    GL.Enable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }
        }


        //------------------------------------------------------
        // Light Bounds Drawing
        //------------------------------------------------------
        public static void drawLightBounds(Light light, Program program)
        {
            // Load light bounds transformation
            Matrix4 temp_mat = light.bounding_unique_mesh.transformation;
            GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);

            draw(light.bounding_unique_mesh.mesh.submeshes.ElementAt(0), "light bounds for lighting");
        }
    }
}
