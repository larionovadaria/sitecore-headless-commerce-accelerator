//    Copyright 2020 EPAM Systems, Inc.
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

namespace Wooli.Foundation.Connect.Managers.Account
{
    using System.Collections.Generic;

    using Sitecore.Commerce.Entities;
    using Sitecore.Commerce.Entities.Customers;
    using Sitecore.Commerce.Services.Customers;

    /// <summary>
    /// Executes CustomerServiceProvider methods
    /// </summary>
    public interface IAccountManagerV2
    {
        /// <summary>
        /// Adds party to customer
        /// </summary>
        /// <param name="customer">Commerce customer</param>
        /// <param name="parties">List of party</param>
        /// <returns>Add parties result</returns>
        AddPartiesResult AddParties(CommerceCustomer customer, IEnumerable<Party> parties);

        /// <summary>
        /// Creates user
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <param name="shopName">Shop name</param>
        /// <returns>Create user result</returns>
        CreateUserResult CreateUser(string userName, string email, string password, string shopName);

        /// <summary>
        /// Enables user
        /// </summary>
        /// <param name="commerceUser">Commerce user</param>
        /// <returns>Enable user result</returns>
        EnableUserResult EnableUser(CommerceUser commerceUser);

        /// <summary>
        /// Gets customer by id
        /// </summary>
        /// <param name="externalId">External id</param>
        /// <returns>Get customer result</returns>
        GetCustomerResult GetCustomer(string externalId);

        /// <summary>
        /// Gets parties for customer by contactId
        /// </summary>
        /// <param name="contactId">Contact id</param>
        /// <returns>Get parties result</returns>
        GetPartiesResult GetCustomerParties(string contactId);

        /// <summary>
        /// Gets parties for commerce customer
        /// </summary>
        /// <param name="customer">Commerce customer</param>
        /// <returns>Get parties result</returns>
        GetPartiesResult GetParties(CommerceCustomer customer);

        /// <summary>
        /// Gets user by user name
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>Get user result</returns>
        GetUserResult GetUser(string userName);

        /// <summary>
        /// Gets users with filter
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="email">Email</param>
        /// <param name="externalCustomerIDs">External customer ids</param>
        /// <param name="externalIDs">External ids</param>
        /// <param name="shopName">Shop name</param>
        /// <param name="isDisabled">Is disabled</param>
        /// <returns>Get users result</returns>
        GetUsersResult GetUsers(
            string userName = default,
            string email = default,
            IEnumerable<string> externalCustomerIDs = null,
            IEnumerable<string> externalIDs = null,
            string shopName = default,
            bool? isDisabled = null);

        /// <summary>
        /// Removes parties
        /// </summary>
        /// <param name="customer">Commerce customer</param>
        /// <param name="parties">List of party</param>
        /// <returns>Customer result</returns>
        CustomerResult RemoveParties(CommerceCustomer customer, IEnumerable<Party> parties);

        /// <summary>
        /// Updates parties
        /// </summary>
        /// <param name="customer">Commerce customer</param>
        /// <param name="parties">List of party</param>
        /// <returns>Customer result</returns>
        CustomerResult UpdateParties(CommerceCustomer customer, IEnumerable<Party> parties);

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="commerceUser">Commerce user</param>
        /// <returns>Enable user result</returns>
        UpdateUserResult UpdateUser(CommerceUser commerceUser);
    }
}