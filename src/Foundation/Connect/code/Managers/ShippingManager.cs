//    Copyright 2019 EPAM Systems, Inc.
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

namespace Wooli.Foundation.Connect.Managers
{
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Commerce.Engine.Connect.Entities;
    using Sitecore.Commerce.Entities.Carts;
    using Sitecore.Commerce.Entities.Shipping;
    using Sitecore.Commerce.Services.Shipping;
    using Sitecore.Commerce.Services.Shipping.Generics;
    using Sitecore.Diagnostics;

    using Wooli.Foundation.Connect.ModelMappers;
    using Wooli.Foundation.Connect.Models;
    using Wooli.Foundation.Connect.Providers;
    using Wooli.Foundation.DependencyInjection;

    [Service(typeof(IShippingManager))]
    public class ShippingManager : IShippingManager
    {
        private readonly IConnectEntityMapper connectEntityMapper;

        private ShippingServiceProvider shippingServiceProvider;

        public ShippingManager(IConnectServiceProvider connectServiceProvider, IConnectEntityMapper connectEntityMapper)
        {
            Assert.ArgumentNotNull((object)connectServiceProvider, nameof(connectServiceProvider));
            Assert.ArgumentNotNull((object)connectEntityMapper, nameof(connectEntityMapper));
            this.connectEntityMapper = connectEntityMapper;
            this.shippingServiceProvider = connectServiceProvider.GetShippingServiceProvider();
        }

        public ManagerResponse<GetShippingMethodsResult, IReadOnlyCollection<ShippingMethod>> GetShippingMethods(
            string shopName,
            Cart cart,
            ShippingOptionType shippingOptionType,
            PartyEntity address,
            List<string> cartLineExternalIdList)
        {

            if (cartLineExternalIdList != null && cartLineExternalIdList.Any<string>())
            {
            }

            CommerceParty commerceParty = null;
            if (address != null)
            {
                commerceParty = this.connectEntityMapper.MapToCommerceParty(address);
            }

            var shippingOption = new ShippingOption { ShippingOptionType = shippingOptionType };
            var request = new Sitecore.Commerce.Engine.Connect.Services.Shipping.GetShippingMethodsRequest(shippingOption, commerceParty, cart as CommerceCart);
            GetShippingMethodsResult shippingMethods = this.shippingServiceProvider.GetShippingMethods<GetShippingMethodsRequest, GetShippingMethodsResult>(request);
            return new ManagerResponse<GetShippingMethodsResult, IReadOnlyCollection<ShippingMethod>>(shippingMethods, shippingMethods.ShippingMethods);

        }

        public virtual ManagerResponse<GetShippingOptionsResult, List<ShippingOption>> GetShippingPreferences(Cart cart)
        {
            var request = new GetShippingOptionsRequest(cart);
            GetShippingOptionsResult shippingOptions = this.shippingServiceProvider.GetShippingOptions(request);
            if (shippingOptions.Success && shippingOptions.ShippingOptions != null)
            {
                return new ManagerResponse<GetShippingOptionsResult, List<ShippingOption>>(shippingOptions, shippingOptions.ShippingOptions.ToList());
            }
            return new ManagerResponse<GetShippingOptionsResult, List<ShippingOption>>(shippingOptions, null);
        }
    }
}
