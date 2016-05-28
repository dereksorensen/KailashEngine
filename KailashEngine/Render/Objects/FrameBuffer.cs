using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Render.Objects
{
    class FrameBuffer
    {

        private int _id;
        public int id
        {
            get { return _id; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
        }


        private Dictionary<FramebufferAttachment, Texture> _attachements;
        public Dictionary<FramebufferAttachment, Texture> attachements
        {
            get { return _attachements; }
        }


        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public FrameBuffer(string name)
        {
            // Create Frame Buffer Object
            _id = 0;
            GL.GenFramebuffers(1, out _id);

            _name = name;
        }


        //------------------------------------------------------
        // Main Methods
        //------------------------------------------------------

        public void load(Dictionary<FramebufferAttachment, Texture> attachements)
        {
            _attachements = attachements;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _id);

            // Loop through and attach each FBO item
            foreach (var a in attachements)
            {               
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, a.Key, a.Value.id, 0);
            }

            // Check for FBO errors
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Debug.DebugHelper.logError("[ ERROR ] FrameBuffer (" + _name + ")", GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
            }
            else
            {
                Debug.DebugHelper.logInfo(2, "[ INFO ] FrameBuffer (" + _name + ")", "Loading successful");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


        public void unload()
        {

        }
        
        // Bind to Draw
        public void bind(DrawBuffersEnum[] draw_attachements)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _id);

            int buffer_count = draw_attachements.Length;
            GL.DrawBuffers(buffer_count, draw_attachements);
        }

        // Bind to Read
        public void bind(ReadBufferMode read_attachement)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _id);

            GL.ReadBuffer(read_attachement);
        }
    }
}
