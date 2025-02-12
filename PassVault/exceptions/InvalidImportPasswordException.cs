using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.exceptions
{
    internal class InvalidImportPasswordException : Exception
    {
        public InvalidImportPasswordException() : base("Senha de importação inválida.") { }

        public InvalidImportPasswordException(string message) : base(message) { }

        public InvalidImportPasswordException(string message, Exception innerException) : base(message, innerException) { }       
    }
}
