using ForEvolve.ExceptionMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class DocumentNotFoundException : NotFoundException
    {
        public DocumentNotFoundException(string id)
            : base($"Document '{id}' not found.")
        {
        }
    }
}
