using System;
using System.Runtime.Serialization;

namespace Discio
{

    [Serializable]
    public class DiscioException : Exception
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public DiscioException()
        {
        }

        /// <summary>
        /// Message constructor
        /// </summary>
        /// <param name="message"></param>
        public DiscioException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Message and inner exception constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public DiscioException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DiscioException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }

}
