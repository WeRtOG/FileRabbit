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
    public class CheckAccessToViewTests
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
        public void CheckAccessToView_ReturnsTrueIfFolderIsShared()
        {
            // arrange
            FolderVM folder = new FolderVM { IsShared = true, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            string currUserId = "4321";

            // act
            bool result = service.CheckAccessToView(folder, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckAccessToView_ReturnsTrueIfFolderIsNonSharedButYouAreOwner()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = new FolderVM { IsShared = false, OwnerId = currUserId };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckAccessToView(folder, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckAccessToView_ReturnsFalseIfFolderIsNonSharedAndYouAreNotOwner()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = new FolderVM { IsShared = false, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckAccessToView(folder, currUserId);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CheckAccessToView_Returns500StatusCodeIfFolderIsNull()
        {
            // arrange
            string currUserId = "4321";
            FolderVM folder = null;
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.CheckAccessToView(folder, currUserId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }

        [Test]
        public void CheckAccessToView_ReturnsTrueIfFileIsShared()
        {
            // arrange
            FileVM file = new FileVM { IsShared = true, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            string currUserId = "4321";

            // act
            bool result = service.CheckAccessToView(file, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckAccessToView_ReturnsTrueIfFilerIsNonSharedButYouAreOwner()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = new FileVM { IsShared = false, OwnerId = currUserId };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckAccessToView(file, currUserId);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckAccessToView_ReturnsFalseIfFileIsNonSharedAndYouAreNotOwner()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = new FileVM { IsShared = false, OwnerId = "1234" };
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);

            // act
            bool result = service.CheckAccessToView(file, currUserId);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CheckAccessToView_Returns500StatusCodeIfFileIsNull()
        {
            // arrange
            string currUserId = "4321";
            FileVM file = null;
            var mock = new Mock<IUnitOfWork>();
            FileSystemService service = new FileSystemService(mock.Object, _mapper);
            int expected = 500;

            // act
            StatusCodeException ex = Assert.Throws<StatusCodeException>(() => service.CheckAccessToView(file, currUserId));

            // assert
            Assert.AreEqual(expected, ex.Data["Status code"]);
        }
    }
}
