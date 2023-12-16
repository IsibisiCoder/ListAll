using ListAll.Business.Services;
using ListAll.Business.Tests.FileServiceMocks;

namespace ListAll.Plugins.Directories.Tests
{
    public class ListDirectoriesBase
    {
        public IFileService GetTestFileService()
        {
            var fsMock = new FileServiceMock();
            return fsMock;
        }
    }
}
