using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;



namespace KailashEngine.Render
{
    class UBO
    {

        private int _buffer_id;
        public int buffer_id
        {
            get { return _buffer_id; }
        }

        private EngineHelper.size[] _ubo_stack;

        public UBO(BufferUsageHint buffer_usage, int index, EngineHelper.size[] ubo_stack)
        {
            // Calculate total UBO byte size based on ubo_stack items
            _ubo_stack = ubo_stack;
            int size = 0;
            foreach (EngineHelper.size e in _ubo_stack)
            {
                size += (int)e;
            }  

            // Create Uniform Buffer
            GL.GenBuffers(1, out _buffer_id);
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)size, (IntPtr)null, buffer_usage);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            //Bind uniform buffer to binding index since the block size is set and ubo is created
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, index, _buffer_id);
        }


        public void update<T>(int component_index, T data)
            where T : struct
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);

            // Calculate offest by adding the data length of ubo_stack up until component_index
            int offset = 0;
            for(int i = 0; i < component_index; i++)
            {
                offset += (int)_ubo_stack[i];
            }

            // Get size based on ubo_stack item at this index
            int size = (int)_ubo_stack[component_index];

            // Update UBO data
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, size, ref data);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

    }
}
