﻿//    Copyright 2020 EPAM Systems, Inc.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace Wooli.Foundation.Commerce.Tests.Services.Account
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    using Base.Models.Result;

    using Commerce.Mappers.Account;
    using Commerce.Services.Account;

    using Connect.Context.Storefront;
    using Connect.Managers.Account;

    using Models.Entities.Addresses;
    using Models.Entities.Users;

    using NSubstitute;

    using Ploeh.AutoFixture;

    using Sitecore.Commerce.Engine.Connect.Entities;
    using Sitecore.Commerce.Entities;
    using Sitecore.Commerce.Entities.Customers;
    using Sitecore.Commerce.Services;
    using Sitecore.Commerce.Services.Customers;

    using Xunit;

    public class AccountServiceTests
    {
        private readonly IAccountManagerV2 accountManager;

        private readonly Fixture fixture;

        private readonly IAccountMapper mapper;

        private readonly AccountService service;

        private readonly IStorefrontContext storefrontContext;

        public AccountServiceTests()
        {
            this.fixture = new Fixture();

            this.accountManager = Substitute.For<IAccountManagerV2>();
            this.mapper = Substitute.For<IAccountMapper>();
            this.storefrontContext = Substitute.For<IStorefrontContext>();
            this.storefrontContext.ShopName.Returns(this.fixture.Create<string>());

            this.service = Substitute.For<AccountService>(
                this.accountManager,
                this.mapper,
                this.storefrontContext);
        }

        public static IEnumerable<object[]> AddressParameters =>
            new List<object[]>
            {
                new object[] { null, new Address() },
                new object[] { "1", null }
            };

        #region AddAddresses

        [Fact]
        public void AddAddress_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(
                () => this.service.AddAddresses(string.Empty, this.fixture.Create<Address>()));
        }

        [Theory]
        [MemberData(nameof(AddressParameters))]
        public void AddAddress_IfParameterIsNull_ShouldThrowArgumentNullException(string userName, Address address)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.AddAddresses(userName, address));
        }

        [Fact]
        public void AddAddress_IfCustomerResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            this.InitAddAddress(userName, false);

            // assert
            var result = this.service.AddAddresses(userName, address);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void AddAddress_IfAddPartiesResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            this.InitAddAddress(userName, true, false);

            // assert
            var result = this.service.AddAddresses(userName, address);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void AddAddress_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            var addPartiesResult = this.InitAddAddress(userName);

            // assert
            var result = this.service.AddAddresses(userName, address);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.Data);
            this.mapper.Received(1).Map<Address, CommerceParty>(address);
            this.mapper.Received(addPartiesResult.Parties.Count).Map<Party, Address>(Arg.Any<Party>());
        }

        private AddPartiesResult InitAddAddress(
            string userName,
            bool customerResultSuccess = true,
            bool addPartiesResultSuccess = true)
        {
            var customerResult = this.InitGetCustomerByName(userName, true, customerResultSuccess);

            var addPartiesResult = this.fixture
                .Build<AddPartiesResult>()
                .With(gur => gur.Success, addPartiesResultSuccess)
                .With(gur => gur.Parties, this.fixture.CreateMany<Party>().ToList())
                .Create();

            if (!addPartiesResultSuccess)
            {
                addPartiesResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .AddParties(customerResult.CommerceCustomer, Arg.Any<List<Party>>())
                .Returns(addPartiesResult);

            this.mapper
                .Map<Address, CommerceParty>(Arg.Any<Address>())
                .Returns(this.fixture.Create<CommerceParty>());

            return addPartiesResult;
        }

        #endregion

        #region ChangePassword

        [Theory]
        [InlineData("", "1", "1")]
        [InlineData("1", "", "1")]
        [InlineData("1", "1", "")]
        public void ChangePassword_IfParameterIsEmpty_ShouldThrowArgumentException(
            string email,
            string newPassword,
            string oldPassword)
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.ChangePassword(email, newPassword, oldPassword));
        }

        [Theory]
        [InlineData(null, "1", "1")]
        [InlineData("1", null, "1")]
        [InlineData("1", "1", null)]
        public void ChangePassword_IfParameterIsNull_ShouldThrowArgumentNullException(
            string email,
            string newPassword,
            string oldPassword)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.ChangePassword(email, newPassword, oldPassword));
        }

        #endregion

        #region CreateAccount

        [Theory]
        [InlineData("", "1", "1", "1")]
        [InlineData("1", "", "1", "1")]
        [InlineData("1", "1", "", "1")]
        [InlineData("1", "1", "1", "")]
        public void CreateAccount_IfParameterIsEmpty_ShouldThrowArgumentException(
            string email,
            string firstName,
            string lastName,
            string password)
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.CreateAccount(email, firstName, lastName, password));
        }

        [Theory]
        [InlineData(null, "1", "1", "1")]
        [InlineData("1", null, "1", "1")]
        [InlineData("1", "1", null, "1")]
        [InlineData("1", "1", "1", null)]
        public void CreateAccount_IfParameterIsNull_ShouldThrowArgumentNullException(
            string email,
            string firstName,
            string lastName,
            string password)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(
                () => this.service.CreateAccount(email, firstName, lastName, password));
        }

        [Fact]
        public void CreateAccount_IfCreateUserResultFail_ShouldReturnFailResult()
        {
            // act
            var email = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            var password = this.fixture.Create<string>();
            this.InitCreateAccount(email, password, false);

            // assert
            var result = this.service.CreateAccount(email, firstName, lastName, password);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateAccount_IfUpdateResultFail_ShouldReturnFailResult(bool enableUserResultSuccess)
        {
            // act
            var email = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            var password = this.fixture.Create<string>();
            this.InitCreateAccount(email, password, true, enableUserResultSuccess, false);

            // assert
            var result = this.service.CreateAccount(email, firstName, lastName, password);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateAccount_IfManagersResultSuccess_ShouldReturnSuccessResult(bool enableUserResultSuccess)
        {
            // act
            var email = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            var password = this.fixture.Create<string>();
            var updateResult = this.InitCreateAccount(email, password, true, enableUserResultSuccess);

            // assert
            var result = this.service.CreateAccount(email, firstName, lastName, password);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal(firstName, updateResult.CommerceUser.FirstName);
            Assert.Equal(lastName, updateResult.CommerceUser.LastName);
            this.mapper.Received(1).Map<CommerceUser, User>(updateResult.CommerceUser);
        }

        private UpdateUserResult InitCreateAccount(
            string email,
            string password,
            bool createUserResultSuccess = true,
            bool enableUserResultSuccess = true,
            bool updateResultSuccess = true)
        {
            var commerceUser = this.fixture
                .Build<CommerceUser>()
                .With(cu => cu.Email, email)
                .Create();

            var createUserResult = this.fixture
                .Build<CreateUserResult>()
                .With(cu => cu.Success, createUserResultSuccess)
                .With(cu => cu.CommerceUser, commerceUser)
                .Create();

            if (!createUserResultSuccess)
            {
                createUserResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .CreateUser(Arg.Any<string>(), email, password, this.storefrontContext.ShopName)
                .Returns(createUserResult);

            var enableUserResult = this.fixture
                .Build<EnableUserResult>()
                .With(cu => cu.Success, enableUserResultSuccess)
                .With(eu => eu.CommerceUser, commerceUser)
                .Create();

            this.accountManager
                .EnableUser(commerceUser)
                .Returns(enableUserResult);

            return this.InitUpdateUser(commerceUser, updateResultSuccess);
        }

        #endregion

        #region GetAddress

        [Fact]
        public void GetAddress_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.GetAddress(""));
        }

        [Fact]
        public void GetAddress_IfParameterIsNull_ShouldThrowArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.GetAddress(null));
        }

        [Fact]
        public void GetAddress_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var (_, getPartiesResult) = this.InitExecuteWithParties(userName);

            // assert
            var result = this.service.GetAddress(userName);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.Data);
            this.mapper.Received(getPartiesResult.Parties.Count).Map<Party, Address>(Arg.Any<Party>());
        }

        #endregion

        #region RemoveAddress

        [Fact]
        public void RemoveAddress_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(
                () => this.service.RemoveAddress(string.Empty, this.fixture.Create<Address>()));
        }

        [Theory]
        [MemberData(nameof(AddressParameters))]
        public void RemoveAddress_IfParameterIsNull_ShouldThrowArgumentNullException(string userName, Address address)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.RemoveAddress(userName, address));
        }

        [Fact]
        public void RemoveAddress_IfRemovePartiesResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            this.InitRemoveAddress(userName, false);

            // assert
            var result = this.service.RemoveAddress(userName, address);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void RemoveAddress_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            this.InitRemoveAddress(userName);

            // assert
            var result = this.service.RemoveAddress(userName, address);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            this.service.Received(1).GetAddress(userName);
        }

        private void InitRemoveAddress(
            string userName,
            bool removePartyResultSuccess = true)
        {
            var (customerResult, _) = this.InitExecuteWithParties(userName);

            var removePartyResponse = this.fixture
                .Build<CustomerResult>()
                .With(cr => cr.Success, removePartyResultSuccess)
                .Create();

            if (!removePartyResultSuccess)
            {
                removePartyResponse.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .RemoveParties(customerResult.CommerceCustomer, Arg.Any<List<Party>>())
                .Returns(removePartyResponse);
        }

        #endregion

        #region UpdateAccount

        [Theory]
        [InlineData("", "1", "1")]
        [InlineData("1", "", "1")]
        [InlineData("1", "1", "")]
        public void UpdateAccount_IfParameterIsEmpty_ShouldThrowArgumentException(
            string contactId,
            string firstName,
            string lastName)
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.UpdateAccount(contactId, firstName, lastName));
        }

        [Theory]
        [InlineData(null, "1", "1")]
        [InlineData("1", null, "1")]
        [InlineData("1", "1", null)]
        public void UpdateAccount_IfParameterIsNull_ShouldThrowArgumentNullException(
            string contactId,
            string firstName,
            string lastName)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.UpdateAccount(contactId, firstName, lastName));
        }

        [Fact]
        public void UpdateAccount_IfGetUserResultFail_ShouldReturnFailResult()
        {
            // act
            var contactId = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            this.InitUpdateAccount(contactId, false);

            // assert
            var result = this.service.UpdateAccount(contactId, firstName, lastName);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void UpdateAccount_IfUpdateResultFail_ShouldReturnFailResult()
        {
            // act
            var contactId = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            this.InitUpdateAccount(contactId, true, false);

            // assert
            var result = this.service.UpdateAccount(contactId, firstName, lastName);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void UpdateAccount_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var contactId = this.fixture.Create<string>();
            var firstName = this.fixture.Create<string>();
            var lastName = this.fixture.Create<string>();
            var updateResult = this.InitUpdateAccount(contactId);

            // assert
            var result = this.service.UpdateAccount(contactId, firstName, lastName);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal(firstName, updateResult.CommerceUser.FirstName);
            Assert.Equal(lastName, updateResult.CommerceUser.LastName);
        }

        private UpdateUserResult InitUpdateAccount(
            string contactId,
            bool getUserResultSuccess = true,
            bool updateResultSuccess = true)
        {
            var commerceUser = this.fixture.Create<CommerceUser>();

            var getUserResult = this.fixture
                .Build<GetUserResult>()
                .With(cu => cu.Success, getUserResultSuccess)
                .With(cu => cu.CommerceUser, commerceUser)
                .Create();

            if (!getUserResultSuccess)
            {
                getUserResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .GetUser(contactId)
                .Returns(getUserResult);

            return this.InitUpdateUser(commerceUser, updateResultSuccess);
        }

        private UpdateUserResult InitUpdateUser(CommerceUser commerceUser, bool updateResultSuccess)
        {
            var updateResult = this.fixture
                .Build<UpdateUserResult>()
                .With(cu => cu.Success, updateResultSuccess)
                .With(eu => eu.CommerceUser, commerceUser)
                .Create();

            if (!updateResultSuccess)
            {
                updateResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .UpdateUser(commerceUser)
                .Returns(updateResult);

            return updateResult;
        }

        #endregion

        #region UpdateAddress

        [Fact]
        public void UpdateAddress_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(
                () => this.service.UpdateAddress(string.Empty, this.fixture.Create<Address>()));
        }

        [Theory]
        [MemberData(nameof(AddressParameters))]
        public void UpdateAddress_IfParameterIsNull_ShouldThrowArgumentNullException(string userName, Address address)
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.UpdateAddress(userName, address));
        }

        [Fact]
        public void UpdateAddress_IfUpdatePartiesResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            this.InitUpdateAddress(userName, null, false);

            // assert
            var result = this.service.UpdateAddress(userName, address);

            // assert
            Assert.False(result.Success);
        }

        [Fact]
        public void UpdateAddress_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            var address = this.fixture.Create<Address>();
            var party = this.InitUpdateAddress(userName, address);

            // assert
            var result = this.service.UpdateAddress(userName, address);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal(party.ExternalId, address.ExternalId);
            Assert.Equal(party.PartyId, address.PartyId);
            this.mapper.Received(1).Map<Address, CommerceParty>(address);
            this.service.Received(1).GetAddress(userName);
        }

        private Party InitUpdateAddress(
            string userName,
            Address address,
            bool updatePartyResponseSuccess = true)
        {
            var (customerResult, getPartiesResult) = this.InitExecuteWithParties(userName);

            if (address != null)
            {
                getPartiesResult.Parties = new List<Party>
                {
                    this.fixture
                        .Build<Party>()
                        .With(p => p.ExternalId, address.ExternalId)
                        .Create()
                };
            }

            var updatePartyResponse = this.fixture
                .Build<CustomerResult>()
                .With(gur => gur.Success, updatePartyResponseSuccess)
                .Create();

            if (!updatePartyResponseSuccess)
            {
                updatePartyResponse.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .UpdateParties(customerResult.CommerceCustomer, Arg.Any<List<Party>>())
                .Returns(updatePartyResponse);

            this.mapper
                .Map<Address, CommerceParty>(Arg.Any<Address>())
                .Returns(this.fixture.Create<CommerceParty>());

            return getPartiesResult.Parties.First();
        }

        #endregion

        #region ValidateEmail

        [Fact]
        public void ValidateEmail_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.ValidateEmail(""));
        }

        [Fact]
        public void ValidateEmail_IfParameterIsNull_ShouldThrowArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.ValidateEmail(null));
        }

        [Fact]
        public void ValidateEmail_IfEmailIsInUse_ShouldReturnFailResult()
        {
            // arrange
            var email = this.fixture.Create<MailAddress>().Address;

            var commerceUser = this.fixture
                .Build<CommerceUser>()
                .With(cu => cu.Email, email)
                .Create();

            var getUserResult = this.fixture.Create<GetUsersResult>();
            getUserResult.CommerceUsers.Add(commerceUser);

            this.accountManager.GetUsers(email: email).Returns(getUserResult);

            // act
            var result = this.service.ValidateEmail(email);

            // assert
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateEmail_IfEmailIsValid_ShouldReturnValidResult()
        {
            // arrange
            var email = this.fixture.Create<MailAddress>().Address;

            this.accountManager.GetUsers(email: email).Returns(this.fixture.Create<GetUsersResult>());

            // act
            var result = this.service.ValidateEmail(email);

            // assert
            Assert.True(result.Success);
        }

        #endregion

        #region ExecuteWithParties

        [Fact]
        public void ExecuteWithParties_IfCustomerResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            this.InitExecuteWithParties(userName, false);

            // assert
            var result = this.service.ExecuteWithParties<VoidResult>(userName, (c, p, r) => r);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ExecuteWithParties_IfGetPartiesResultFail_ShouldReturnFailResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            this.InitExecuteWithParties(userName, true, false);

            // assert
            var result = this.service.ExecuteWithParties<VoidResult>(userName, (c, p, r) => r);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ExecuteWithParties_IfManagersResultSuccess_ShouldReturnSuccessResult()
        {
            // act
            var userName = this.fixture.Create<string>();
            this.InitExecuteWithParties(userName);

            // assert
            var result = this.service.ExecuteWithParties<VoidResult>(userName, (c, p, r) => r);

            // assert
            Assert.True(result.Success);
        }

        private (GetCustomerResult, GetPartiesResult) InitExecuteWithParties(
            string userName,
            bool customerResultSuccess = true,
            bool getPartiesResultSuccess = true)
        {
            var customerResult = this.InitGetCustomerByName(userName, true, customerResultSuccess);

            var getPartiesResult = this.fixture
                .Build<GetPartiesResult>()
                .With(gur => gur.Success, getPartiesResultSuccess)
                .With(gur => gur.Parties, this.fixture.CreateMany<Party>().ToList())
                .Create();

            if (!getPartiesResultSuccess)
            {
                getPartiesResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager
                .GetParties(customerResult.CommerceCustomer)
                .Returns(getPartiesResult);

            return (customerResult, getPartiesResult);
        }

        #endregion

        #region GetCustomerByName

        [Fact]
        public void GetCustomerByName_IfParameterIsEmpty_ShouldThrowArgumentException()
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => this.service.GetCustomerByName(string.Empty));
        }

        [Fact]
        public void GetCustomerByName_IfParameterIsNull_ShouldThrowArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => this.service.GetCustomerByName(null));
        }

        [Fact]
        public void GetCustomerByName_IfUserResultFail_ShouldReturnFailResult()
        {
            // arrange
            var userName = this.fixture.Create<string>();
            this.InitGetCustomerByName(userName, false);

            // act
            var result = this.service.GetCustomerByName(userName);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void GetCustomerByName_IfCustomerResultFail_ShouldReturnFailResult()
        {
            // arrange
            var userName = this.fixture.Create<string>();
            this.InitGetCustomerByName(userName, true, false);

            // act
            var result = this.service.GetCustomerByName(userName);

            // assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void GetCustomerByName_IfManagerResultsSuccess_ShouldReturnSuccessResult()
        {
            // arrange
            var userName = this.fixture.Create<string>();
            this.InitGetCustomerByName(userName);

            // act
            var result = this.service.GetCustomerByName(userName);

            // assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        private GetCustomerResult InitGetCustomerByName(
            string userName,
            bool userResultSuccess = true,
            bool customerResultSuccess = true)
        {
            var externalId = this.fixture.Create<string>();
            var commerceUser = this.fixture
                .Build<CommerceUser>()
                .With(cu => cu.ExternalId, externalId)
                .Create();
            var userResult = this.fixture
                .Build<GetUserResult>()
                .With(gur => gur.Success, userResultSuccess)
                .With(gur => gur.CommerceUser, commerceUser)
                .Create();

            if (!userResultSuccess)
            {
                userResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager.GetUser(userName).Returns(userResult);

            var parties = new List<CustomerParty>
            {
                this.fixture.Create<CustomerParty>(),
                this.fixture.Create<CustomerParty>(),
                this.fixture.Create<CustomerParty>()
            };
            var commerceCustomer = this.fixture
                .Build<CommerceCustomer>()
                .With(cc => cc.ExternalId, externalId)
                .With(cc => cc.Parties, parties)
                .Create();
            var customerResult = this.fixture
                .Build<GetCustomerResult>()
                .With(gcr => gcr.Success, customerResultSuccess)
                .With(gcr => gcr.CommerceCustomer, commerceCustomer)
                .Create();

            if (!customerResultSuccess)
            {
                customerResult.SystemMessages.Add(this.fixture.Create<SystemMessage>());
            }

            this.accountManager.GetCustomer(externalId).Returns(customerResult);

            return customerResult;
        }

        #endregion
    }
}