using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;



namespace KailashEngine.Render.Objects
{
    class UniformBuffer
    {

        private int _buffer_id;
        public int buffer_id
        {
            get { return _buffer_id; }
        }

        private EngineHelper.size[] _ubo_stack;

        public UniformBuffer(BufferStorageFlags storage_flags, int index, EngineHelper.size[] ubo_stack)
        {
            // Calculate total UBO byte size based on ubo_stack items
            _ubo_stack = ubo_stack;
            int size = 0;
            foreach (EngineHelper.size e in _ubo_stack)
            {
                size += (int)e;
            }

            load(storage_flags, index, size);
        }

        public UniformBuffer(BufferStorageFlags storage_flags, int index, EngineHelper.size ubo_item_size, int length)
        {
            // Calculate total UBO byte size based on ubo_stack items
            _ubo_stack = new EngineHelper.size[length];
            int size = (int)ubo_item_size * length;

            for(int i = 0; i < length; i++)
            {
                _ubo_stack[i] = ubo_item_size;
            }

            load(storage_flags, index, size);
        }

        private void load(BufferStorageFlags storage_flags, int index, int size)
        {
            // Create Uniform Buffer
            GL.GenBuffers(1, out _buffer_id);
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);
            GL.BufferStorage(BufferTarget.UniformBuffer, size, (IntPtr)null, storage_flags);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            //Bind uniform buffer to binding index since the block size is set and ubo is created
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, index, _buffer_id);
        }

        public void bind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);
        }

        public void unbind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }


        public void update<T>(int component_index, T data)
            where T : struct
        {
            // Calculate offest by adding the data length of ubo_stack up until component_index
            int offset = 0;
            for(int i = 0; i < component_index; i++)
            {
                offset += (int)_ubo_stack[i];
            }

            // Get size based on ubo_stack item at this index
            int size = (int)_ubo_stack[component_index];

            // Update UBO data
            GL.NamedBufferSubData(_buffer_id, (IntPtr)offset, size, ref data);
        }



    }
}
