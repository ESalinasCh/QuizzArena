using System;
using System.Collections.Generic;
using System.Text;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

public class BlobRepository : IBlobRepository
{
    public Task<string> UploadFileAsync() => throw new NotImplementedException();

}
