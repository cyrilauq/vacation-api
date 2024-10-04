using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tests.Utils;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using VacationApi.Repository;

namespace Tests.Repositories
{
    public class VacationsRepositoryTests
    {
        private IFormatProvider provider = new CultureInfo("fr-FR");
        private VacationRepository _repository;
        private VacationApiDbContext _context;
        private User _user;
        private User _user2;

        [SetUp]
        public void SetUp()
        {

            _context = DbContextFactory.Create();
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima",
                FirstName = "Touka",
                UserName = "touka_ki",
                PicturePath = "url"
            });
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki2@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima2",
                FirstName = "Touka2",
                UserName = "touka_ki2",
                PicturePath = "url"
            });
            _context.SaveChanges();
            _user = _context.Users.Where(x => x.UserName == "touka_ki").FirstOrDefault()!;
            _user2 = _context.Users.Where(x => x.UserName == "touka_ki2").FirstOrDefault()!;

            _repository = new VacationRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            DbContextFactory.Destroy(_context);
        }

        [Test]
        public async Task WhenAddANewVacanceThenVacationsCountIncrease()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(1));
        }

        [Test]
        public void WhenAVacationWithSameTitleExistThenDontAddIt()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Country = "Belgique",
                    Place = "Una place in una paya",
                },
                _user.Id
            );


            Assert.Throws<VacationAlreadyExistsException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una titro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public async Task WhenDifferentUserAddVacationWithSameNameThenTheTwoAreAdded()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    Country = "Belgique",
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            _repository = new VacationRepository(_context);

            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _context.Users.Where(x => x.UserName == "touka_ki2").FirstOrDefault()!.Id
            );

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(2));
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una tixcvcxvtro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused2()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una xcvcxbvcxvtitro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            ;

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(13).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                        Description = "Una dvvvescriptione",
                        Title = "Una titro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused3()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );
            ;

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddYears(1).AddMonths(1).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddYears(1).AddMonths(1).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una titvfdbfvbro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused4()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Country = "Belgique",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(13).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(13).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una titcbcvxbro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused5()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(13).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(13).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una tidfsfsdftro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public void WhenAUserAddAVacationWhileAnotherIsGoingOnThenTheAddIsRefused6()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<PeriodNotFreeException>(() =>
                _repository.AddVacation(
                    new AddVacationModel
                    {
                        DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                        DateEnd = DateTime.Now.AddMonths(3).ToString("dd/MM/yyyy"),
                        HourBegin = DateTime.Now.ToString("HH:mm"),
                        HourEnd = DateTime.Now.AddMonths(3).ToString("HH:mm"),
                        Description = "Una descriptione",
                        Title = "Una tidfsfsdftro",
                        Latitude = "100",
                        Longitude = "150",
                        Place = "Una place in una paya",
                    },
                    _user.Id
                )
            );
        }

        [Test]
        public async Task ToName1()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Country = "Belgique",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            _repository = new VacationRepository(_context);

            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(3).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(3).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una tidfsfsdftro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user2.Id
            );

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task ToName2()
        {
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(2).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(2).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                    Country = "Belgique",
                },
                _user.Id
            );
            _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(3).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(4).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(3).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(4).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una tidfsfsdftro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(2));
        }

        [Test]
        public void WhenVacationIdExistsThenRetrieveTheVacation()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.IsNotNull(_repository.GetVacationByIdFor(vacation.Id, _user.Id));
        }

        [Test]
        public void WhenVacationIdNotExistsThenReturnNull()
        {
            Assert.Throws<VacationNotFoundException>(() => _repository.GetVacationByIdFor("khkjhkjhlkhlkh", _user.Id));
        }

        [Test]
        public void WhenVacationIdExistsButUserIsNotInsediItThenThrowsException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                    Country = "Belgique",
                },
                _user.Id
            );

            _repository = new VacationRepository(_context);

            Assert.Throws<CannotSeeVacationException>(() => _repository.GetVacationByIdFor(vacation.Id, _user2.Id));
        }

        [Test]
        public async Task WhenUsersUidExistThenAddThemToTheVacationAsync()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Country = "Belgique",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.IsTrue(_repository.AddUsersToVacation(vacation.Id, _user.Id, new List<string>() { _user2.Id }.ToArray()).Count > 0);
        }

        [Test]
        public void WhenAddTwoTimesSameThingThenAddIdJustOne()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            _repository.AddUsersToVacation(vacation.Id, _user.Id, new List<string>() { _user2.Id }.ToArray());
            Assert.That(1, Is.EqualTo(_context.Invitations.Count()));
        }

        [Test]
        public async Task WhenUsersUidNotExistThenThrowsError()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<UserNotFoundException>(() => _repository.AddUsersToVacation(vacation.Id, _user.Id, new List<string>() { "hkjhh" }.ToArray()));
        }

        [Test]
        public async Task WhenUidOfIncitorDoesNotExistThenThrowsException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<UserNotFoundException>(() => _repository.AddUsersToVacation(vacation.Id, "lkmjmj", new List<string>() { _user2.Id }.ToArray()));
        }

        [Test]
        public async Task WhenVacationIdIsNotRelatedToAnyVacationThenThrowsException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Country = "Belgique",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.Throws<VacationNotFoundException>(() => _repository.AddUsersToVacation("dfdfxgdfg", _user.Id, new List<string>() { _user2.Id }.ToArray()));
        }
        [Test]
        public async Task WhenIdUserIsNotTheSameAsOwnerIdThenThrowException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            _repository = new VacationRepository(_context);

            Assert.Throws<WrongCredentialsException>(() => _repository.AddUsersToVacation(vacation.Id, _user2.Id, new List<string>() { _user2.Id }.ToArray()));
        }

        [Test]
        public void WhenPublishVacationThenReturnTrue()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    Country = "Belgique",
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            Assert.IsTrue(_repository.PublishVacation(vacation.Id, _user.Id));
        }

        [Test]
        public void WhenUserDoesNotOwnVacationThenThrowException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Country = "Belgique",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            _repository = new VacationRepository(_context);

            Assert.Throws<WrongCredentialsException>(() => _repository.PublishVacation(vacation.Id, _user2.Id));
        }

        [Test]
        public void WhenVacationIsAlreadyPublishedThenThrowException()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).DateFormat(),
                    DateEnd = DateTime.Now.AddMonths(12).DateFormat(),
                    HourBegin = DateTime.Now.AddMonths(1).TimeFormat(),
                    HourEnd = DateTime.Now.AddMonths(12).TimeFormat(),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Country = "Belgique",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _user.Id
            );

            vacation.IsPublished = true;
            _context.SaveChanges();

            Assert.Throws<VacationPublishedException>(() => _repository.PublishVacation(vacation.Id, _user.Id));
        }

        [Test]
        public void WhenVacationIdIsNotRelatedToAnyVacationThenThrowException()
        {
            Assert.Throws<VacationNotFoundException>(() => _repository.PublishVacation("a-fake-id", _user.Id));
        }
    }
}