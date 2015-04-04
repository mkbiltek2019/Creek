﻿using System;

namespace Creek.Net.Updates.ftp
{
    public class FtpException : Exception
    {
        public FtpException(int error, string message)
            : base(message)
        {
            _error = error;
        }

        private int _error;

        public int ErrorCode
        {
            get { return _error; }
        }
    }
}
