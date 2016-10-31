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
                Debug.DebugHelper.logInfo(2, "[ INFO ] FrameBuffer (" + _name + ")", "SUCCESS");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


        public void unload()
        {

        }

        // Bind Draw Attachements Only
        public void bindAttachements(DrawBuffersEnum draw_attachment)
        {
            DrawBuffersEnum[] temp_attchements = new DrawBuffersEnum[] { draw_attachment };

            int buffer_count = temp_attchements.Length;
            GL.DrawBuffers(buffer_count, temp_attchements);
        }
        public void bindAttachements(DrawBuffersEnum[] draw_attachements)
        {
            int buffer_count = draw_attachements.Length;
            GL.DrawBuffers(buffer_count, draw_attachements);
        }

        // Bind Read Attachements Only
        public void bindAttachements(ReadBufferMode read_attachement)
        {
            GL.ReadBuffer(read_attachement);
        }

        // Bind to Draw
        public void bind(DrawBuffersEnum draw_attachment)
        {
            bind(new DrawBuffersEnum[] { draw_attachment });
        }
        public void bind(DrawBuffersEnum[] draw_attachements)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _id);

            bindAttachements(draw_attachements);
        }

        // Bind to Read
        public void bind(ReadBufferMode read_attachement)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _id);

            bindAttachements(read_attachement);
        }


        public void bindTexture(FramebufferAttachment attachement, int texture_id)
        {
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachement, texture_id, 0);

            // Check for FBO errors
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Debug.DebugHelper.logError("[ ERROR ] FrameBuffer (" + _name + ")", GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
            }
        }
    }
}
