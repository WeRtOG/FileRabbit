using AutoMapper;
using FileRabbit.BLL.Exceptions;
using FileRabbit.BLL.Services;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.PL;
using FileRabbit.ViewModels;
using Moq;
using NUnit.Framework;

namespace FileRabbit.Tests
{
    public class CheckEditAccessTests
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
        public void CheckEditAccess_ReturnsTrueIfYouAreFolderOwner()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = new FolderVM { IsShared = true, OwnerId = currUserId };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckEditAccess(folder, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckEditAccess_ReturnsFalseIfYouAreNotFolderOwner()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = new FolderVM { IsShared = true, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckEditAccess(folder, currUserId);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CheckEditAccess_Returns500StatusCodeIfFolderIsNull()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = null;
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.CheckEditAccess(folder, currUserId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }

        [Test]
        public void CheckEditAccess_ReturnsTrueIfYouAreFileOwner()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = new FileVM { IsShared = true, OwnerId = currUserId };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckEditAccess(file, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckEditAccess_ReturnsFalseIfYouAreNotFileOwner()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = new FileVM { IsShared = true, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckEditAccess(file, currUserId);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CheckEditAccess_Returns500StatusCodeIfFileIsNull()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = null;
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.CheckEditAccess(file, currUserId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
