using AutoMapper;
using FileRabbit.BLL.Exceptions;
using FileRabbit.BLL.Services;
using FileRabbit.DAL.Entities;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.PL;
using FileRabbit.ViewModels;
using Moq;
using NUnit.Framework;
using System;

namespace FileRabbit.Tests
{
    public class CreateFolderTests
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
            System.IO.Directory.CreateDirectory(_rootPath + "\\ExistsFolder");
        }

        [TearDown]
        public void Cleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.IO.Directory.Delete(_rootPath, true);
        }

        [Test]
        public void CreateFolder_CreatesFolderOnHardDrive()
        {
            // arrange
            string userId = "1234";
            string name = "newFolder";
            FolderVM folder = new FolderVM { Id = "1", Path = _rootPath, OwnerId = userId, ParentFolderId = null, IsShared = false };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()));
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            service.CreateFolder(folder, name, userId);
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(_rootPath + "\\" + name);
            bool result = info.Exists;

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CreateFolder_CreatesFolderInDB()
        {
            // arrange
            string userId = "1234";
            string name = "newFolder";
            FolderVM folder = new FolderVM { Id = "1", Path = _rootPath, OwnerId = userId, ParentFolderId = null, IsShared = false };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()));
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            service.CreateFolder(folder, name, userId);
            mock.Verify(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()), Times.Once);

            // assert
            Assert.IsTrue(true);
        }

        [Test]
        public void CreateFolder_ReturnsFolderIfItIsNew()
        {
            // arrange
            string userId = "1234";
            string name = "newFolder";
            FolderVM folder = new FolderVM { Id = "1", Path = _rootPath, OwnerId = userId, ParentFolderId = null, IsShared = false };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()));
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            ElementVM expected = new ElementVM
            {
                ElemName = name,
                IsFolder = true,
                IsShared = false,
                LastModified = DateTime.Now.ToShortDateString(),
                Size = null,
                Type = ElementVM.FileType.Folder
            };

            // act
            ElementVM result = service.CreateFolder(folder, name, userId);
            expected.Id = result.Id;

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CreateFolder_ReturnsNullIfFolderAlreadyExists()
        {
            // arrange
            string userId = "1234";
            string name = "ExistsFolder";
            FolderVM folder = new FolderVM { Id = "1", Path = _rootPath, OwnerId = userId, ParentFolderId = null, IsShared = false };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()));
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            ElementVM result = service.CreateFolder(folder, name, userId);

            // assert
            Assert.IsNull(result);
        }

        [Test]
        public void CreateFolder_Returns500StatusCodeIfParentIsNull()
        {
            // arrange
            string userId = "1234";
            string name = "newFolder";
            FolderVM folder = null;
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Create(It.IsAny<Folder>()));
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.CreateFolder(folder, name, userId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
