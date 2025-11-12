using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared.Exceptions
{
    /// <summary>
    /// Used when a SqlColumn has validation errors.
    /// </summary>
    /// <remarks>
    /// Creates a new SqlColumnException with the given message.
    /// </remarks>
    /// <param name="message"></param>
    [Serializable]
    public class SqlColumnException(string message) : Exception(message) { }
}
