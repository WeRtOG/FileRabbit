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
    public class GetFolderPathTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            _mapper = mappingConfig.CreateMapper();
        }

        [Test]
        public void GetFolderPath_ReturnsAllFoldersToTheRootIfYouAreOwner()
        {
            // arrange
            string currFolderId = "20";
            string userId = "1234";
            Folder folder1 = new Folder { Id = "1", ParentFolderId = null, OwnerId = userId, Path = "C:\\Folder1" };
            Folder folder2 = new Folder { Id = "5", ParentFolderId = "1", OwnerId = userId, Path = "C:\\Folder1\\Folder2" };
            Folder folder3 = new Folder { Id = "20", ParentFolderId = "5", OwnerId = userId, Path = "C:\\Folder1\\Folder2\\Folder3" };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(folder1);
            mock.Setup(a => a.GetRepository<Folder>().Get("5")).Returns(folder2);
            mock.Setup(a => a.GetRepository<Folder>().Get("20")).Returns(folder3);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            Stack<FolderShortInfoVM> expected = new Stack<FolderShortInfoVM>();
            expected.Push(new FolderShortInfoVM { Id = "20", Name = "Folder3" });
            expected.Push(new FolderShortInfoVM { Id = "5", Name = "Folder2" });
            expected.Push(new FolderShortInfoVM { Id = "1", Name = "Your drive" });

            // act
            Stack<FolderShortInfoVM> result = service.GetFolderPath(currFolderId, userId);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetFolderPath_Returns500StatusCodeIfOneOfFoldersIsNull()
        {
            // arrange
            string currFolderId = "20";
            string userId = "1234";
            Folder folder1 = new Folder { Id = "1", ParentFolderId = null, OwnerId = userId, Path = "C:\\Folder1" };
            Folder folder2 = null;
            Folder folder3 = new Folder { Id = "20", ParentFolderId = "5", OwnerId = userId, Path = "C:\\Folder1\\Folder2\\Folder3" };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(folder1);
            mock.Setup(a => a.GetRepository<Folder>().Get("5")).Returns(folder2);
            mock.Setup(a => a.GetRepository<Folder>().Get("20")).Returns(folder3);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.GetFolderPath(currFolderId, userId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }

        //[Test]
        //public void GetFolderPath_ReturnsIncompletePathIfNotAllFoldersAreSharedAndYouAreNotOwner()
        //{
        //    // arrange
        //    string currFolderId = "20";
        //    string userId = "1234";
        //    Folder folder1 = new Folder { Id = "1", ParentFolderId = null, OwnerId = "4321", Path = "C:\\Folder1", IsShared = false };
        //    Folder folder2 = new Folder { Id = "5", ParentFolderId = "1", OwnerId = "4321", Path = "C:\\Folder1\\Folder2", IsShared = false };
        //    Folder folder3 = new Folder { Id = "20", ParentFolderId = "5", OwnerId = "4321", Path = "C:\\Folder1\\Folder2\\Folder3", IsShared = true };
        //    var mock = new Mock<IUnitOfWork>();
        //    mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(folder1);
        //    mock.Setup(a => a.GetRepository<Folder>().Get("5")).Returns(folder2);
        //    mock.Setup(a => a.GetRepository<Folder>().Get("20")).Returns(folder3);

        //    IEnumerable<Folder> foldersListFromDB = new List<Folder> { folder3 };
        //    IEnumerable<File> filesListFromDB = new List<File>();
        //    mock.Setup(a => a.GetRepository<Folder>().Get("5")).Returns(folder2);
        //    mock.Setup(a => a.GetRepository<Folder>().Get("5")).Returns();
        //    mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
        //    mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);

        //    FileSystemService service = new FileSystemService(mock.Object, _mapper);
        //    Stack<FolderShortInfoVM> expected = new Stack<FolderShortInfoVM>();
        //    expected.Push(new FolderShortInfoVM { Id = "20", Name = "Folder3" });
        //    expected.Push(new FolderShortInfoVM { Id = "5", Name = "Folder2" });

        //    // act
        //    Stack<FolderShortInfoVM> result = service.GetFolderPath(currFolderId, userId);

        //    // assert
        //    Assert.AreEqual(expected, result);
        //}
    }
}
