﻿using BlazorHero.CleanArchitecture.Domain.Entities.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BlazorHero.CleanArchitecture.DataAccess.Interfaces.Repositories;
using BlazorHero.CleanArchitecture.Utils.Wrapper;
using Microsoft.Extensions.Localization;

namespace BlazorHero.CleanArchitecture.Application.Features.Products.Commands.Delete
{
    public class DeleteProductCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<int>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IStringLocalizer<DeleteProductCommandHandler> _localizer;

            public DeleteProductCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<DeleteProductCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(command.Id);
                await _unitOfWork.Repository<Product>().DeleteAsync(product);
                await _unitOfWork.Commit(cancellationToken);
                return await Result<int>.SuccessAsync(product.Id, _localizer["Product Deleted"]);
            }
        }
    }
}