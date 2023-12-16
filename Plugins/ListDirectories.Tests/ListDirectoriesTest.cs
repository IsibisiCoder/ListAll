using ListAll.Business.Services;
using Moq;
using Newtonsoft.Json.Linq;

namespace ListAll.Plugins.Directories.Tests;

[TestClass]
public class ListDirectoriesTest : ListDirectoriesBase
{
    [TestMethod]
    [ExpectedException (typeof (ArgumentNullException))]
    public void TestSetParameter_ParamIsNull_ArgumentNullException()
    {
        var value = "test";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(null!, value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestSetParameter_ParamIsEmpty_ArgumentNullException()
    {
        var value = "test";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(string.Empty, value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestSetParameter_ValueIsNull_ArgumentNullException()
    {
        var param = "test";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestSetParameter_ValueIsEmpty_ArgumentNullException()
    {
        var param = "test";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, string.Empty);
    }

    [TestMethod]
    public void TestSetParameter_OutputFile()
    {
        var param = "OutputFile";
        var value = "test";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(value, listDirectories.OutputFile);
    }

    [TestMethod]
    public void TestSetParameter_RootDirFile()
    {
        var param = "RootDir";
        var value = "test2";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(value, listDirectories.RootDir);
    }

    [TestMethod]
    public void TestSetParameter_RecursiveFile()
    {
        var param = "Recursive";
        var value = true;

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value.ToString());

        Assert.AreEqual(value, listDirectories.Recursive);
    }

    [TestMethod]
    public void TestSetParameter_ExtensionsFile_WithoutAsterisk()
    {
        var param = "Extensions";
        var value = "ext";
        var expectedvalue = "*.ext";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(1, listDirectories.Extensions.Count);

        Assert.AreEqual(expectedvalue, listDirectories.Extensions[0]);
    }

    [TestMethod]
    public void TestSetParameter_ExtensionsFile_WithAsterisk()
    {
        var param = "Extensions";
        var value = "*.ext";
        var expectedvalue = "*.ext";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(1, listDirectories.Extensions.Count);

        Assert.AreEqual(expectedvalue, listDirectories.Extensions[0]);
    }

    [TestMethod]
    public void TestSetParameter_ExtensionsFile_WithTwoAsterisk()
    {
        var param = "Extensions";
        var value = "**.ext";
        var expectedvalue = "**.ext";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(1, listDirectories.Extensions.Count);

        Assert.AreEqual(expectedvalue, listDirectories.Extensions[0]);
    }


    [TestMethod]
    public void TestSetParameter_ExtensionsFile_OnlyDot()
    {
        var param = "Extensions";
        var value = ".ext";
        var expectedvalue = "*.ext";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(1, listDirectories.Extensions.Count);

        Assert.AreEqual(expectedvalue, listDirectories.Extensions[0]);
    }

    [TestMethod]
    public void TestSetParameter_ExtensionsFile_ThreeExtensions()
    {
        var param = "Extensions";
        var value1 = "ext";
        var value2 = "txt";
        var value3 = "pdf";
        var expectedvalue1 = "*.ext";
        var expectedvalue2 = "*.txt";
        var expectedvalue3 = "*.pdf";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value1);
        listDirectories.SetParameter(param, value2);
        listDirectories.SetParameter(param, value3);

        Assert.AreEqual(3, listDirectories.Extensions.Count);

        Assert.AreEqual(expectedvalue1, listDirectories.Extensions[0]);
        Assert.AreEqual(expectedvalue2, listDirectories.Extensions[1]);
        Assert.AreEqual(expectedvalue3, listDirectories.Extensions[2]);
    }

    [TestMethod]
    public void TestSetParameter_SettingPathFile()
    {
        var param = "SettingPath";
        var value = "mydir/set.json";

        var fileServiceMock = new Mock<IFileService>();

        var listDirectories = new ListDirectories(fileServiceMock.Object);

        listDirectories.SetParameter(param, value);

        Assert.AreEqual(value, listDirectories.SettingPath);
    }

    [TestMethod]
    public void TestGetConfiguration_ReadSettings()
    {
        var expectedvalue1 = true;
        var expectedvalue2 = true;
        var expectedvalue3 = "Filename;FileExtention";
        var expectedHeader1 = "Filename;Ext;Size;Date;Path";
        var expectedHeader2 = "--|--|--|--|--";

        var fileServiceMock = GetTestFileService();

        var listDirectories = new ListDirectories(fileServiceMock);

        listDirectories.GetConfiguration();

        Assert.AreEqual(expectedvalue1, listDirectories.OnlyDir);
        Assert.AreEqual(expectedvalue2, listDirectories.FilenameWithExtension);
        Assert.AreEqual(expectedvalue3, listDirectories.OutputFormat);
        Assert.AreEqual(2, listDirectories.Headers.Count);
        Assert.AreEqual(expectedHeader1, listDirectories.Headers[0]);
        Assert.AreEqual(expectedHeader2, listDirectories.Headers[1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestProcess_WrongParameter_OutputFile()
    {
        var fileServiceMock = GetTestFileService();

        var listDirectories = new ListDirectories(fileServiceMock);

        listDirectories.Process();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestProcess_WrongParameter_RootDir()
    {
        var fileServiceMock = GetTestFileService();

        var listDirectories = new ListDirectories(fileServiceMock);
        listDirectories.SetParameter("OutputFile", "test1");

        listDirectories.Process();
    }

    [TestMethod]
    public void TestProcess()
    {
        var fileServiceMock = GetTestFileService();

        var listDirectories = new ListDirectories(fileServiceMock);
        listDirectories.SetParameter("OutputFile", "test1");
        listDirectories.SetParameter("RootDir", "c:\test");

        listDirectories.Process();
    }
}