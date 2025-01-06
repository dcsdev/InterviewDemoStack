using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class DomainBaseTests
    {
        [Theory]
        [InlineData(0,0)]
        [InlineData(2, 0)]
        [InlineData(-1, 0)]
        public void shouldInstantiateEmptyActivityList(int listSize, int expectedListSize )
        {
            //Arrange
            DomainBase domain = new DomainBase();

            //Assert
            Assert.Equal(listSize, domain.Activities.Count);
        }

        //[Fact]
        //public void shouldInstantiatOriginalGraphResourceLocatorBlankString()
        //{
        //    //Arrange
        //    DomainBase domain = new DomainBase();

        //    //Assert
        //    Assert.Equal(string.Empty, domain.OriginalGraphResourceLocator);
        //}

        //[Fact]
        //public void shouldInstantiatFinalGraphResourceLocatorBlankString()
        //{
        //    //Arrange
        //    DomainBase domain = new DomainBase();

        //    //Assert
        //    Assert.Equal(string.Empty, domain.FinalGraphResourceLocator);
        //}
    }
}


//public string OriginalGraphResourceLocator { get; set; } = string.Empty;
//public string FinalGraphResourceLocator { get; set; } = String.Empty;
//public List<Activity> Activities { get; set; } = new List<Activity>();