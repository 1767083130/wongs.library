using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wongs.Services.Log
{
    public interface ILog
    {
        void WarnFormat(string text, params object[] args);
        void Error(Exception ex);
    }
}
