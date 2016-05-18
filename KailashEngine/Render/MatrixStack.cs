using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Render
{
    class MatrixStack
    {

        
        private Stack<Matrix4> _stack_current;
        private Queue<Matrix4> _stack_previous;
        private Queue<Matrix4> _stack_temp;

        private int _count_current;
        private int _count_previous;

        public MatrixStack()
        {
            _stack_current = new Stack<Matrix4>();
            _stack_previous = new Queue<Matrix4>();
            _stack_temp = new Queue<Matrix4>();

            _count_current = _count_previous = 0;
        }


        // Start new empty working stack
        public void push()
        {
            push(Matrix4.Identity);
        }

        // Start new working stack with initial matrix
        public void push(Matrix4 matrix)
        {
            _stack_current.Push(matrix);
        }

        // Remove and return the current working stack
        public Matrix4 pop()
        {
            return _stack_current.Pop();
        }

        // Add matrix to current working stack
        public void add(Matrix4 matrix)
        {
            _stack_current.Push(matrix * _stack_current.Pop());
        }

        public Matrix4 getStack()
        {
            Matrix4 full_stack = Matrix4.Identity;
            foreach(Matrix4 m in _stack_current)
            {
                full_stack = m * full_stack;
            }
            return full_stack;
        }


        // Send full stack to shader
        public void send(int[] model_uniform_ids)
        {

            Matrix4 full_stack = getStack();

            GL.UniformMatrix4(model_uniform_ids[0], false, ref full_stack);

            full_stack = Matrix4.Invert(full_stack);
            full_stack = Matrix4.Transpose(full_stack);

            GL.UniformMatrix4(model_uniform_ids[1], false, ref full_stack);

        }

    }
}
