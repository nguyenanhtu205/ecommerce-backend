using MediatR;
using ProductCatalogService.Application.Common.Dtos;
using ProductCatalogService.Application.Features.Commands.CreateCategory;
using ProductCatalogService.Application.Features.Commands.CreateCategoryAttributes;
using ProductCatalogService.Application.Features.Commands.CreateProduct;
using ProductCatalogService.Application.Features.Commands.UpdateProduct;
using ProductCatalogService.Application.Features.Queries.GetCategories;
using ProductCatalogService.Application.Features.Queries.GetCategoryAttributes;
using ProductCatalogService.Application.Features.Queries.GetProductById;
using ProductCatalogService.Application.Features.Queries.GetProductListings;
using ProductCatalogService.Application.Features.Queries.GetProductsByShop;

namespace ProductCatalogService.API.Endpoints;

public class ProductCatalog : IEndpointGroup
{
    public static string RoutePrefix => "/product-catalog";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateProduct, "products")
            .Produces<ProductDto>()
            //.RequireAuthorization()
            .RequireRateLimiting("post");

        groupBuilder.MapPut(UpdateProduct, "products/{id}")
            .Produces<ProductDto>()
            //.RequireAuthorization()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(CreateCategory, "category")
            .Produces<CreateCategoryResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(CreateCategoryAttribute, "category-attribute")
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetProductById, "products/{id}");

        groupBuilder.MapGet(GetProductsByShop, "products/shop/{shopId}");

        groupBuilder.MapGet(GetCategories, "categories");

        groupBuilder.MapGet(GetCategoryAttributes, "categories/{categoryId}/attributes");

        groupBuilder.MapGet(GetProductListings, "listings");
    }

    [EndpointSummary("Create product")]
    [EndpointDescription("Seller creates a new product. Status defaults to Draft.")]
    public static async Task<IResult> CreateProduct(
        CreateProductCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Create category")]
    public static async Task<IResult> CreateCategory(
        CreateCategoryCommand command, ISender sender, CancellationToken cancellationToken)
    {
        CreateCategoryResponse result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Create category attribute")]
    public static async Task<IResult> CreateCategoryAttribute(
        CreateCategoryAttributesCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Update product")]
    public static async Task<IResult> UpdateProduct(
        string id, UpdateProductCommand command, ISender sender, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest("The ID in the route does not match the ID in the request body.");
        }

        ProductDto result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get product by id")]
    public static async Task<IResult> GetProductById(
        [AsParameters] GetProductByIdQuery query, ISender sender, CancellationToken cancellationToken)
    {
        ProductDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get products by shop")]
    [EndpointDescription("Used by seller dashboard to list products of a shop.")]
    public static async Task<IResult> GetProductsByShop(
        [AsParameters] GetProductsByShopQuery query, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<ProductDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get categories")]
    [EndpointDescription("Returns root categories when parentId is omitted, or child categories otherwise.")]
    public static async Task<IResult> GetCategories(
        [AsParameters] GetCategoriesQuery query, ISender sender, CancellationToken cancellationToken)
    {
        List<CategoryDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get category attributes")]
    public static async Task<IResult> GetCategoryAttributes(
        [AsParameters] GetCategoryAttributesQuery query, ISender sender, CancellationToken cancellationToken)
    {
        List<CategoryAttributeDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get product listings")]
    [EndpointDescription("Buyer-facing search/browse endpoint, backed by product_listing_view.")]
    public static async Task<IResult> GetProductListings(
        [AsParameters] GetProductListingsQuery query, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<ProductListingDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
