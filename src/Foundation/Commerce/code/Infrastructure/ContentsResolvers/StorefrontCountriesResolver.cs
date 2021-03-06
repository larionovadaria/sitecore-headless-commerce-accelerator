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

namespace Wooli.Foundation.Commerce.Infrastructure.ContentsResolvers
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using Sitecore.LayoutService.Configuration;
    using Sitecore.LayoutService.ItemRendering.ContentsResolvers;
    using Sitecore.Mvc.Presentation;

    using Wooli.Foundation.Commerce.Context;
    using Wooli.Foundation.Commerce.ModelMappers;
    using Wooli.Foundation.Connect.Models;
    using Wooli.Foundation.DependencyInjection;

    [Service(Lifetime = Lifetime.Transient)]
    public class StorefrontCountriesContentsResolver : IRenderingContentsResolver
    {
        private readonly IStorefrontContext storefrontContext;

        private readonly IEntityMapper mapper;

        public StorefrontCountriesContentsResolver(
            IStorefrontContext storefrontContext,
            IEntityMapper mapper)
        {
            this.storefrontContext = storefrontContext;
            this.mapper = mapper;
        }


        public object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            IEnumerable<ICountryRegionModel> model =
                this.storefrontContext.CurrentStorefront.CountriesRegionsConfiguration.CountriesRegionsModel;

            return new { Countries = this.mapper.MapToCountryRegionModel(model) };
        }

        public bool IncludeServerUrlInMediaUrls { get; set; }

        public bool UseContextItem { get; set; }

        public string ItemSelectorQuery { get; set; }

        public NameValueCollection Parameters { get; set; }
    }
}
