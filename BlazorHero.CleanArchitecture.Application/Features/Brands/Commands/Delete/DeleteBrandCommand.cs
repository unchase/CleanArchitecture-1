﻿using BlazorHero.CleanArchitecture.Domain.Entities.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BlazorHero.CleanArchitecture.Application.Constants.Application;
using BlazorHero.CleanArchitecture.DataAccess.Interfaces.Repositories;
using BlazorHero.CleanArchitecture.Utils.Wrapper;
using Microsoft.Extensions.Localization;

namespace BlazorHero.CleanArchitecture.Application.Features.Brands.Commands.Delete
{
    public class DeleteBrandCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result<int>>
        {
            private readonly IProductRepository _productRepository;
            private readonly IStringLocalizer<DeleteBrandCommandHandler> _localizer;
            private readonly IUnitOfWork _unitOfWork;

            public DeleteBrandCommandHandler(IUnitOfWork unitOfWork, IProductRepository productRepository, IStringLocalizer<DeleteBrandCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _productRepository = productRepository;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(DeleteBrandCommand command, CancellationToken cancellationToken)
            {
                var isBrandUsed = await _productRepository.IsBrandUsed(command.Id);
                if (!isBrandUsed)
                {
                    var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(command.Id);
                    await _unitOfWork.Repository<Brand>().DeleteAsync(brand);
                    await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllBrandsCacheKey);
                    return await Result<int>.SuccessAsync(brand.Id, _localizer["Brand Deleted"]);
                }
                else
                {
                    return await Result<int>.FailAsync(_localizer["Deletion Not Allowed"]);
                }
            }
        }
    }
}