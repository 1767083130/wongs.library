using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Wongs.Common
{

    public sealed class Globals
    {
        // global constants for the life of the application ( set in Application_Start )
        private static string _applicationPath;
        private static string _applicationMapPath;

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public static string ApplicationPath
        {
            get
            {
                if (_applicationPath == null && (HttpContext.Current != null))
                {
                    _applicationPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
                }

                return _applicationPath;
            }
        }

        /// <summary>
        /// Gets or sets the application map path.
        /// </summary>
        /// <value>
        /// The application map path.
        /// </value>
        public static string ApplicationMapPath
        {
            get
            {
                return _applicationMapPath ?? (_applicationMapPath = GetCurrentDomainDirectory());
            }
        }

        private static string GetCurrentDomainDirectory()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("/", "\\");
            if (dir.Length > 3 && dir.EndsWith("\\"))
            {
                dir = dir.Substring(0, dir.Length - 1);
            }
            return dir;
        }
    }
}
