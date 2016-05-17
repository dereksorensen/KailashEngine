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


        public UBO(int size, BufferUsageHint buffer_usage, int index)
        {
            // Create Uniform Buffer
            GL.GenBuffers(1, out _buffer_id);
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)size, (IntPtr)null, buffer_usage);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            //Bind uniform buffer to binding index since the block size is set and ubo is created
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, index, _buffer_id);
        }


        public void updateComponent<T>(int offset, T data)
            where T : struct
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, _buffer_id);

            int size = System.Runtime.InteropServices.Marshal.SizeOf(data);

            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, size, ref data);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }



    }
}
