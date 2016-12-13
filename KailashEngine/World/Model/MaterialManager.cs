using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.Render.Objects;

namespace KailashEngine.World.Model
{
    class MaterialManager
    {

        private UniformBuffer _ubo_bindless;
        public UniformBuffer ubo_bindless
        {
            get { return _ubo_bindless; }
        }



        public MaterialManager()
        {
            // Create UBO for bindless material textures
            _ubo_bindless = new UniformBuffer(OpenTK.Graphics.OpenGL.BufferStorageFlags.DynamicStorageBit, 2, EngineHelper.size.ui64, 256);
        }

    }
}
