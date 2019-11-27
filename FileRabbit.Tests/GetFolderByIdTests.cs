using NUnit.Framework;
using Moq;
using FileRabbit.ViewModels;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.DAL.Entities;
using FileRabbit.BLL.Services;
using AutoMapper;
using FileRabbit.PL;
using FileRabbit.BLL.Exceptions;

namespace FileRabbit.Tests
{
    public class GetFolderByIdTests
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
        public void GetFolderById_ReturnsFolderVM()
        {
            // arrange
            Folder folderFromDB = new Folder { 
                Id = "23",
                Path = "C:\\User\\MyFolder",
                OwnerId = "User",
                ParentFolderId = "1",
                IsShared = false
            };
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("23")).Returns(folderFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            FolderVM expected = new FolderVM
            {
                Id = "23",
                Path = "C:\\User\\MyFolder",
                OwnerId = "User",
                ParentFolderId = "1",
                IsShared = false
            };

            // act
            FolderVM result = service.GetFolderById("23");

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetFolderById_Returns404StatusCodeIfFolderIsNull()
        {
            // arrange
            Folder folderFromDB = null;
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(a => a.GetRepository<Folder>().Get("23")).Returns(folderFromDB);
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 404;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.GetFolderById("23"));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}