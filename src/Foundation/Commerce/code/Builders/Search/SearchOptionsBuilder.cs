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

namespace Wooli.Foundation.Commerce.Builders.Search
{
    using System.Collections.Generic;
    using System.Linq;

    using DependencyInjection;

    using Mappers.Search;

    using Models;
    using Models.Entities.Search;

    using Sitecore.Diagnostics;

    using Connect = Connect.Models.Search;

    [Service(typeof(ISearchOptionsBuilder), Lifetime = Lifetime.Singleton)]
    public class SearchOptionsBuilder : ISearchOptionsBuilder
    {
        private readonly ISearchMapper searchMapper;

        public SearchOptionsBuilder(ISearchMapper searchMapper)
        {
            Assert.ArgumentNotNull(searchMapper, nameof(searchMapper));
            this.searchMapper = searchMapper;
        }

        public Connect.SearchOptions Build(Connect.SearchSettings searchSettings, ProductSearchOptions searchOptions)
        {
            var pageSize = searchOptions.PageSize > 0 ? searchOptions.PageSize : searchSettings.ItemsPerPage;
            var startPageIndex = (searchOptions.PageNumber - 1) * pageSize;

            return new Connect.SearchOptions
            {
                SearchKeyword = searchOptions.SearchKeyword,
                Facets = this.GetFacetsIntersection(searchSettings.Facets, searchOptions.Facets),
                StartPageIndex = startPageIndex > 0 ? startPageIndex : 0,
                NumberOfItemsToReturn = pageSize,
                SortField = !string.IsNullOrEmpty(searchOptions.SortField)
                    ? searchOptions.SortField
                    : searchSettings.SortFieldNames?.FirstOrDefault(),
                SortDirection = searchOptions.SortDirection == SortDirection.Asc
                    ? Connect.SortDirection.Asc
                    : Connect.SortDirection.Desc
            };
        }

        private IEnumerable<Connect.Facet> GetFacetsIntersection(IEnumerable<Connect.Facet> searchSettingsFacets, IEnumerable<Facet> searchOptionsFacets)
        {
            var facets = searchOptionsFacets.Where(
                searchOptionsFacet =>
            {
                var searchSettingsFacet = searchSettingsFacets.FirstOrDefault(facet => facet.Name == searchOptionsFacet.Name);
                if (searchSettingsFacet == null)
                {
                    return false;
                }

                searchOptionsFacet.DisplayName = searchSettingsFacet.DisplayName;
                return true;
            });

            return this.searchMapper.Map<IEnumerable<Facet>, IEnumerable<Connect.Facet>>(facets);
        }
    }
}