using AutoMapper;
using FileRabbit.BLL.Exceptions;
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
    public class ChangeAccessTests
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
        public void ChangeAccess_ReturnsParentId()
        {
            // arrange
            string userId = "4321";
            string currFolderId = "1";
            string[] foldersId = { "22", "33" };
            string[] filesId = { "23" };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(new Folder
            {
                Id = currFolderId,
                OwnerId = userId
            });
            mock.Setup(a => a.GetRepository<Folder>().Get("22")).Returns(new Folder
            {
                Id = "22",
                OwnerId = userId,
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Get("33")).Returns(new Folder
            {
                Id = "33",
                OwnerId = userId,
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<File>().Get("23")).Returns(new File
            {
                Id = "23",
                FolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(new List<Folder>());
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(new List<File>());
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            string expected = "1";

            // act
            string result = service.ChangeAccess(currFolderId, userId, foldersId, filesId, true);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ChangeAccess_ReturnsSharedFolderIdIfItIsTheOnlyOne()
        {
            // arrange
            string userId = "4321";
            string currFolderId = "1";
            string[] foldersId = { "22" };
            string[] filesId = { };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(new Folder
            {
                Id = currFolderId,
                OwnerId = userId
            });
            mock.Setup(a => a.GetRepository<Folder>().Get("22")).Returns(new Folder
            {
                Id = "22",
                OwnerId = userId,
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(new List<Folder>());
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(new List<File>());
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            string expected = "22";

            // act
            string result = service.ChangeAccess(currFolderId, userId, foldersId, filesId, true);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ChangeAccess_Returns404StatusCodeIfParentIsNull()
        {
            // arrange
            string userId = "4321";
            string currFolderId = "1";
            string[] foldersId = { "22" };
            string[] filesId = { };
            Folder empty = null;

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(empty);
            mock.Setup(a => a.GetRepository<Folder>().Get("22")).Returns(new Folder
            {
                Id = "22",
                OwnerId = userId,
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(new List<Folder>());
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(new List<File>());
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 404;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => 
                service.ChangeAccess(currFolderId, userId, foldersId, filesId, true));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }

        [Test]
        public void ChangeAccess_Returns403StatusCodeIfYouAreNotOwner()
        {
            // arrange
            string userId = "4321";
            string currFolderId = "1";
            string[] foldersId = { "22" };
            string[] filesId = { };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(new Folder
            {
                Id = "1",
                OwnerId = "1234",
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Get("22")).Returns(new Folder
            {
                Id = "22",
                OwnerId = "1234",
                ParentFolderId = currFolderId
            });
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(new List<Folder>());
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(new List<File>());
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 403;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() =>
                service.ChangeAccess(currFolderId, userId, foldersId, filesId, true));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }

        [Test]
        public void ChangeAccess_UpdateMethodsAreCalledForEachFileInHierarchy()
        {
            // arrange
            string userId = "4321";
            string currFolderId = "1";
            string[] foldersId = { "33" };
            string[] filesId = { };

            Folder parent = new Folder { Id = currFolderId, OwnerId = userId };
            Folder folder = new Folder { Id = "33", OwnerId = userId, ParentFolderId = currFolderId };

            File nestedFile1 = new File { Id = "40", FolderId = "33" };
            File nestedFile2 = new File { Id = "41", FolderId = "33" };

            List<Folder> nestedFolders = new List<Folder> { };
            List<File> nestedFiles = new List<File> { nestedFile1, nestedFile2 };

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parent);
            mock.Setup(a => a.GetRepository<Folder>().Get("33")).Returns(folder);
            mock.Setup(a => a.GetRepository<File>().Get("40")).Returns(nestedFile1);
            mock.Setup(a => a.GetRepository<File>().Get("41")).Returns(nestedFile2);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(nestedFolders);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(nestedFiles);
            
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            service.ChangeAccess(currFolderId, userId, foldersId, filesId, true);
            mock.Verify(a => a.GetRepository<Folder>().Update(parent), Times.Never);
            mock.Verify(a => a.GetRepository<Folder>().Update(folder), Times.Once);
            mock.Verify(a => a.GetRepository<File>().Update(nestedFile1), Times.Once);
            mock.Verify(a => a.GetRepository<File>().Update(nestedFile2), Times.Once);

            // assert
            Assert.IsTrue(true);
        }
    }
}
