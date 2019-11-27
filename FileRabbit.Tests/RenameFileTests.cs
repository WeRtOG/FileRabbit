using AutoMapper;
using FileRabbit.BLL.Services;
using FileRabbit.DAL.Entities;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.PL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.Tests
{
    public class RenameFileTests
    {
        private IMapper _mapper;
        private string _rootPath;

        [SetUp]
        public void Setup()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            _mapper = mappingConfig.CreateMapper();

            _rootPath = "C:\\FileRabbitStorage\\TestFolder";
            System.IO.Directory.CreateDirectory(_rootPath);
            System.IO.File.Create(_rootPath + "\\file1.txt");
        }

        [TearDown]
        public void Clean()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.IO.Directory.Delete(_rootPath, true);
        }

        [Test]
        public void RenameFile_RenamesFileOnHardDrive()
        {
            // arrange
            string fileId = "20";
            string newName = "myRenamedFile";
            File file = new File { Id = "20", Path = _rootPath + "\\file1.txt" };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get(fileId)).Returns(file);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            GC.Collect();

            // act
            service.RenameFile(newName, fileId);
            System.IO.FileInfo info = new System.IO.FileInfo(_rootPath + "\\myRenamedFile.txt");
            bool result = info.Exists;

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RenameFile_RenamesFileInDB()
        {
            // arrange
            string fileId = "20";
            string newName = "myRenamedFile";
            File file = new File { Id = "20", Path = _rootPath + "\\file1.txt" };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get(fileId)).Returns(file);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            GC.Collect();
            string expected = _rootPath + "\\myRenamedFile.txt";

            // act
            service.RenameFile(newName, fileId);
            string result = file.Path;

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameFile_ReturnsFalseIfFileAlreadyExists()
        {
            // arrange
            string fileId = "20";
            string newName = "file1";
            File file = new File { Id = "20", Path = _rootPath + "\\file1.txt" };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get(fileId)).Returns(file);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            GC.Collect();

            // act
            bool result = service.RenameFile(newName, fileId);

            // assert
            Assert.IsFalse(result);
        }
    }
}
