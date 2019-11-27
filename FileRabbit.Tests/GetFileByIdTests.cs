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
using System.Text;

namespace FileRabbit.Tests
{
    public class GetFileByIdTests
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
        public void GetFileById_ReturnsTheSameFileVM()
        {
            // arrange
            File fileFromDB = new File
            {
                Id = "23",
                Path = "C:\\User\\MyFolder\\file.txt",
                FolderId = "1",
                IsShared = false
            };
            Folder folderFromDB = new Folder
            {
                Id = "1",
                Path = "C:\\User\\MyFolder",
                OwnerId = "User",
                ParentFolderId = null,
                IsShared = false
            };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get("23")).Returns(fileFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(folderFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            FileVM expected = new FileVM
            {
                Id = "23",
                Path = "C:\\User\\MyFolder\\file.txt",
                OwnerId = "User",
                Name = "file.txt",
                ContentType = "text/plain",
                FolderId = "1",
                IsShared = false
            };

            // act
            FileVM result = service.GetFileById("23");

            // assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(".txt", "text/plain")]
        [TestCase(".mp4", "video/mp4")]
        [TestCase(".mp3", "audio/mpeg")]
        [TestCase(".jpg", "image/jpeg")]
        [TestCase(".pdf", "application/pdf")]
        [TestCase(".mkv", "application/octet-stream")]
        public void GetFileById_ReturnsFileVMWithCorrectContentType(string extension, string expected)
        {
            // arrange
            File fileFromDB = new File
            {
                Id = "23",
                Path = "C:\\User\\MyFolder\\file" + extension,
                FolderId = "1",
                IsShared = false
            };
            Folder folderFromDB = new Folder
            {
                Id = "1",
                Path = "C:\\User\\MyFolder",
                OwnerId = "User",
                ParentFolderId = null,
                IsShared = false
            };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get("23")).Returns(fileFromDB);
            mock.Setup(a => a.GetRepository<Folder>().Get("1")).Returns(folderFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            FileVM fileVM = service.GetFileById("23");
            string result = fileVM.ContentType;

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetFileById_Returns404StatusCodeIfFileIsNull()
        {
            // arrange
            File fileFromDB = null;
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<File>().Get("23")).Returns(fileFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 404;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.GetFileById("23"));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
