using System;

namespace Wongs.Services.Log
{
    public sealed class Exceptions
    {
        /// <summary>
        /// Logs all the basic exception.
        /// </summary>
        /// <param name="exc">The exc.</param>
        public static void LogException(Exception exc)
        {
            //todo
            //Logger.Error(exc);
            //var objExceptionLog = new ExceptionLogController();
            //objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.GENERAL_EXCEPTION);
        }
    }
}
