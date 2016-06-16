using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.Client;
using KailashEngine.Output;

namespace KailashEngine.Render.FX
{
    abstract class RenderEffect
    {

        protected ProgramLoader _pLoader;
        protected string _path_glsl_effect;

        protected Resolution _resolution;


        public RenderEffect(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
        {
            _pLoader = pLoader;
            _path_glsl_effect = glsl_effect_path;
            _resolution = full_resolution;
        }

        protected abstract void load_Programs();

        protected abstract void load_Buffers();

        public abstract void load();

        public abstract void unload();

        public abstract void reload();
    }
}
