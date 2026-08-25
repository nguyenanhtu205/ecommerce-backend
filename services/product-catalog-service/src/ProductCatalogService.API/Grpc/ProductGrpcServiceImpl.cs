using Common.Contracts.Grpc.Product;
using Grpc.Core;
using MongoDB.Driver;
using ProductCatalogService.Application.Common.Interfaces;
using ProductCatalogService.Domain.Entities;

namespace ProductCatalogService.API.Grpc;

public class ProductGrpcServiceImpl(IApplicationDbContext dbContext) : ProductGrpcService.ProductGrpcServiceBase
{
    public override async Task<GetProductInfosResponse> GetProductInfos(
        GetProductInfosRequest request, ServerCallContext context)
    {
        List<string> productIds = [.. request.ProductIds];

        FilterDefinition<Product> filter = Builders<Product>.Filter.In(p => p.Id, productIds);

        List<Product> products = await dbContext.Products
            .Find(filter)
            .ToListAsync(context.CancellationToken);

        GetProductInfosResponse response = new();

        response.Items.AddRange(products.Select(p => new ProductInfoItem
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ThumbnailUrl = p.ThumbnailMediaId,
            WeightGram = p.ShippingInfo.WeightGrams,
            Length = p.ShippingInfo.Dimensions.Length,
            Width = p.ShippingInfo.Dimensions.Width,
            Height = p.ShippingInfo.Dimensions.Height
        }));

        return response;
    }
}
