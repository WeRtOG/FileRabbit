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
using System.Collections.Generic;

namespace FileRabbit.Tests
{
    public class GetElementsFromFolderTests
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
            System.IO.Directory.CreateDirectory(_rootPath + "\\folder1");
            System.IO.Directory.CreateDirectory(_rootPath + "\\folder2");
            System.IO.File.Create(_rootPath + "\\file1.txt");
            System.IO.File.Create(_rootPath + "\\file2.txt");
        }

        [TearDown]
        public void Cleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.IO.Directory.Delete(_rootPath, true);
        }

        [Test]
        public void GetElementsFromFolder_ReturnsAllElementsIfYouAreOwner()
        {
            // arrange
            string userId = "1234";
            Folder parent = new Folder { Id = "1", Path = _rootPath, OwnerId = userId };
            Folder folder1 = new Folder { Id = "11", ParentFolderId = "1", OwnerId = userId, Path = _rootPath + "\\folder1" };
            Folder folder2 = new Folder { Id = "12", ParentFolderId = "1", OwnerId = userId, Path = _rootPath + "\\folder2" };
            File file1 = new File { Id = "22", FolderId = "1", Path = _rootPath + "\\file1.txt" };
            File file2 = new File { Id = "23", FolderId = "1", Path = _rootPath + "\\file2.txt" };

            List<Folder> foldersListFromDB = new List<Folder> { folder1, folder2 };
            List<File> filesListFromDB = new List<File> { file1, file2 };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parent);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            ICollection<ElementVM> expected = new List<ElementVM> { };
            expected.Add(new ElementVM { Id = "11", ElemName = "folder1", IsFolder = true, LastModified = DateTime.Now.ToShortDateString(),
                Size = null, Type = ElementVM.FileType.Folder });
            expected.Add(new ElementVM { Id = "12", ElemName = "folder2", IsFolder = true, LastModified = DateTime.Now.ToShortDateString(),
                Size = null, Type = ElementVM.FileType.Folder });
            expected.Add(new ElementVM { Id = "22", ElemName = "file1.txt", IsFolder = false, LastModified = DateTime.Now.ToShortDateString(),
                Size = new Tuple<double, ElementVM.Unit>(0, ElementVM.Unit.B), Type = ElementVM.FileType.Other });
            expected.Add(new ElementVM { Id = "23", ElemName = "file2.txt", IsFolder = false, LastModified = DateTime.Now.ToShortDateString(),
                Size = new Tuple<double, ElementVM.Unit>(0, ElementVM.Unit.B), Type = ElementVM.FileType.Other });

            // act
            ICollection<ElementVM> result = service.GetElementsFromFolder(new FolderVM { Id = "1", Path = _rootPath }, userId);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetElementsFromFolder_ReturnsOnlySharedElementsIfYouAreNotOwner()
        {
            // arrange
            string userId = "1234";
            Folder parent = new Folder { Id = "1", Path = _rootPath, OwnerId = "4321" };
            Folder folder1 = new Folder { Id = "11", ParentFolderId = "1", OwnerId = "4321", Path = _rootPath + "\\folder1", IsShared = true };
            Folder folder2 = new Folder { Id = "12", ParentFolderId = "1", OwnerId = "4321", Path = _rootPath + "\\folder2" };
            File file1 = new File { Id = "22", FolderId = "1", Path = _rootPath + "\\file1.txt" };
            File file2 = new File { Id = "23", FolderId = "1", Path = _rootPath + "\\file2.txt", IsShared = true };

            List<Folder> foldersListFromDB = new List<Folder> { folder1, folder2 };
            List<File> filesListFromDB = new List<File> { file1, file2 };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parent);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            ICollection<ElementVM> expected = new List<ElementVM> { };
            expected.Add(new ElementVM { Id = "11", ElemName = "folder1", IsFolder = true, LastModified = DateTime.Now.ToShortDateString(),
                Size = null, Type = ElementVM.FileType.Folder, IsShared = true });
            expected.Add(new ElementVM { Id = "23", ElemName = "file2.txt", IsFolder = false, LastModified = DateTime.Now.ToShortDateString(),
                Size = new Tuple<double, ElementVM.Unit>(0, ElementVM.Unit.B), Type = ElementVM.FileType.Other, IsShared = true });

            // act
            ICollection<ElementVM> result = service.GetElementsFromFolder(new FolderVM { Id = "1", Path = _rootPath }, userId);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetElementsFromFolder_Returns404StatusCodeIfFolderIsNull()
        {
            // arrange
            string userId = "1234";
            Folder parent = null;

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parent);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 404;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => 
                service.GetElementsFromFolder(null, userId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
