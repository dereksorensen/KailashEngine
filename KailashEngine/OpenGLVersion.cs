using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine
{
    class OpenGLVersion
    {

        private int _major;
        public int major
        {
            get { return _major; }
            set { _major = value; }
        }


        private int _minor;
        public int minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        public string version
        {
            get
            {
                return _major + "." + _minor;
            }
        }

        public OpenGLVersion(int major_version, int minor_version)
        {
            _major = major_version;
            _minor = minor_version;
        }

    }
}
