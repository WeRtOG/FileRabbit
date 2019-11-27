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

namespace FileRabbit.Tests
{
    public class HasSharedChildrenTests
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
        public void HasSharedChildren_ReturnsTrueIfHasSharedFolder()
        {
            // arrange
            Folder parentFolderFromDB = new Folder
            {
                Id = "1",
                IsShared = false
            };
            IEnumerable<Folder> foldersListFromDB = new List<Folder> { new Folder { Id = "23", IsShared = true, ParentFolderId = "1" } };
            IEnumerable<File> filesListFromDB = new List<File>();
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parentFolderFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.HasSharedChildren("1");

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasSharedChildren_ReturnsTrueIfHasSharedFile()
        {
            // arrange
            Folder parentFolderFromDB = new Folder
            {
                Id = "1",
                IsShared = false
            };
            IEnumerable<Folder> foldersListFromDB = new List<Folder>();
            IEnumerable<File> filesListFromDB = new List<File> { new File { Id = "12", IsShared = true, FolderId = "1" } };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parentFolderFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.HasSharedChildren("1");

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasSharedChildren_ReturnsTrueIfHasSharedAndNonSharedFilesAndFolders()
        {
            // arrange
            Folder parentFolderFromDB = new Folder
            {
                Id = "1",
                IsShared = false
            };
            IEnumerable<Folder> foldersListFromDB = new List<Folder> 
            { 
                new Folder { Id = "23", IsShared = false, ParentFolderId = "1" },
                new Folder { Id = "33", IsShared = true, ParentFolderId = "1" }
            };
            IEnumerable<File> filesListFromDB = new List<File> 
            { 
                new File { Id = "12", IsShared = false, FolderId = "1" },
                new File { Id = "22", IsShared = false, FolderId = "1" },
            };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parentFolderFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.HasSharedChildren("1");

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasSharedChildren_ReturnsFalseIfDoNotHasSharedFoldersAndFiles()
        {
            // arrange
            Folder parentFolderFromDB = new Folder
            {
                Id = "1",
                IsShared = false
            };
            IEnumerable<Folder> foldersListFromDB = new List<Folder> { new Folder { Id = "23", IsShared = false, ParentFolderId = "1" } };
            IEnumerable<File> filesListFromDB = new List<File> { new File { Id = "12", IsShared = false, FolderId = "1" } };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parentFolderFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.HasSharedChildren("1");

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasSharedChildren_ReturnsFalseIfDoNotHasAnyFoldersAndFiles()
        {
            // arrange
            Folder parentFolderFromDB = new Folder
            {
                Id = "1",
                IsShared = false
            };
            IEnumerable<Folder> foldersListFromDB = new List<Folder>();
            IEnumerable<File> filesListFromDB = new List<File>();
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(parentFolderFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Find(It.IsAny<Func<Folder, bool>>())).Returns(foldersListFromDB);
            mock.Setup(a => a.GetRepository<File>().Find(It.IsAny<Func<File, bool>>())).Returns(filesListFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.HasSharedChildren("1");

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasSharedChildren_Returns404StatusCodeIfFolderDoesNotExists()
        {
            // arrange
            Folder folderFromDB = null;
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("23")).Returns(folderFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 404;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.HasSharedChildren("23"));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
