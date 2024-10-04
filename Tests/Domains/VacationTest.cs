using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;

namespace Tests.Domains
{
    public class VacationTest
    {
        DateTime now;
        string empty;

        [SetUp]
        public void SetUp()
        {
            now = DateTime.Now;
            empty = "  ";
        }

        [Test]
        public void WhenVacationHasNoTitleThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("", "sdfdsf", "sdfdsgfg", 100, 100, now.AddDays(1), now.AddDays(2), "dsgfdfgdsg");
                Vacation.New(empty, "sdfdsf", "sdfdsgfg", 100, 100, now.AddDays(1), now.AddDays(2), "dsgfdfgdsg");
            });
        }

        [Test]
        public void WhenVacationHasNoDescriptionThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("dfgdfg", "", "sdfdsgfg", 100, 100, now.AddDays(1), now.AddDays(2), "xcvcvfdf");
                Vacation.New("dfgdfg", empty, "sdfdsgfg", 100, 100, now.AddDays(1), now.AddDays(2), "xcvcvfdf");
            });
        }

        [Test]
        public void WhenVacationHasNoPlaceThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("dfgdfg", "gxfbhfgh", "", 100, 100, now.AddDays(1), now.AddDays(2), "sdfdsfsdg");
                Vacation.New("dfgdfg", "gxfbhfgh", empty, 100, 100, now.AddDays(1), now.AddDays(2), "sdfdsfsdg");
            });
        }

        [Test]
        public void WhenVacationBeginInThePastThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("dfgdfg", "gxfbhfgh", "dfsdfdsf", 100, 100, now.AddDays(-1), now, "sdgfdfgdfg");
            });
        }

        [Test]
        public void WhenVacationEndWhenItBeginsThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("dfgdfg", "gxfbhfgh", "dfsdfdsf", 100, 100, now.AddDays(1), now.AddDays(1), "dfgdfgdfg");
            });
        }

        [Test]
        public void WhenVacationHasNoUserIdThenThrowException()
        {
            Assert.Throws<InvalidVacationInformation>(() =>
            {
                Vacation.New("dfgdfg", "gxfbhfgh", "dfsdfdsf", 100, 100, now.AddDays(1), now.AddDays(2), "");
                Vacation.New("dfgdfg", "gxfbhfgh", "dfsdfdsf", 100, 100, now.AddDays(1), now.AddDays(2), empty);
            });
        }
    }
}
