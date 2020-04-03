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

namespace Wooli.Foundation.Commerce.Mappers.Profiles
{
    using System.Diagnostics.CodeAnalysis;

    using AutoMapper;

    using Models.Entities.Catalog;

    using Connect = Connect.Models.Catalog;

    [ExcludeFromCodeCoverage]
    public class CatalogProfile : Profile
    {
        public CatalogProfile()
        {
            this.CreateMap<Connect.BaseProduct, BaseProduct>()
                .ForMember(
                    dest => dest.StockStatusName,
                    opt => opt.MapFrom(
                        src => src.StockStatus != null ? src.StockStatus.Name : null));

            this.CreateMap<Connect.Product, Product>()
                .IncludeBase<Connect.BaseProduct, BaseProduct>();

            this.CreateMap<Connect.Variant, Variant>()
                .IncludeBase<Connect.BaseProduct, BaseProduct>()
                .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.Id));
        }
    }
}