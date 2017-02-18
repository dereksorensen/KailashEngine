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
        private int[] _ubo_stack_offets;

        public UniformBuffer(BufferStorageFlags storage_flags, int index, EngineHelper.size[] ubo_stack)
        {
            // Calculate total UBO byte size based on ubo_stack items
            _ubo_stack = ubo_stack;
            _ubo_stack_offets = new int[ubo_stack.Length];
            int size = 0;
            for(int i = 0; i < ubo_stack.Length; i++)
            {
                _ubo_stack_offets[i] = size;
                size += (int)ubo_stack[i];
            }

            load(storage_flags, index, size);
        }

        public UniformBuffer(BufferStorageFlags storage_flags, int index, EngineHelper.size ubo_item_size, int length)
        {
            // Calculate total UBO byte size based on ubo_stack items
            _ubo_stack = new EngineHelper.size[length];
            _ubo_stack_offets = new int[length];
            int size = (int)ubo_item_size * length;
            int size_increment = 0;
            for(int i = 0; i < length; i++)
            {
                _ubo_stack[i] = ubo_item_size;
                _ubo_stack_offets[i] = size_increment;
                size_increment += (int)ubo_item_size;
            }

            load(storage_flags, index, size);
        }

        public UniformBuffer(BufferStorageFlags storage_flags, int index, EngineHelper.size[] ubo_sub_stack, int length)
        {
            // Calculate total UBO byte size based on ubo_stack items
            int stack_length = ubo_sub_stack.Length * length;
            _ubo_stack = new EngineHelper.size[stack_length];
            _ubo_stack_offets = new int[stack_length];
            int size = 0;

            for (int i = 0; i < length; i++)
            {           
                for(int j = 0; j < ubo_sub_stack.Length; j ++)
                {
                    int stack_index = i * ubo_sub_stack.Length + j;
                    _ubo_stack[stack_index] = ubo_sub_stack[j];
                    _ubo_stack_offets[stack_index] = size;
                    size += (int)ubo_sub_stack[j];
                }
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
            // Get size based on ubo_stack item at this index
            int size = (int)_ubo_stack[component_index];

            update(component_index, size, data);
        }

        public void update<T>(int component_index, int component_size, T data)
            where T : struct
        {
            // Calculate offest by adding the data length of ubo_stack up until component_index
            int offset = _ubo_stack_offets[component_index];

            // Update UBO data
            GL.NamedBufferSubData(_buffer_id, (IntPtr)offset, component_size, ref data);
        }

    }
}
