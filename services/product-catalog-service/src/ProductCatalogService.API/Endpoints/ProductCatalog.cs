using MediatR;
using ProductCatalogService.Application.Common.Dtos;
using ProductCatalogService.Application.Features.Commands.CreateCategory;
using ProductCatalogService.Application.Features.Commands.CreateCategoryAttributes;
using ProductCatalogService.Application.Features.Commands.CreateProduct;
using ProductCatalogService.Application.Features.Commands.UpdateProduct;
using ProductCatalogService.Application.Features.Queries.GetAllCategories;
using ProductCatalogService.Application.Features.Queries.GetCategories;
using ProductCatalogService.Application.Features.Queries.GetCategoriesByShop;
using ProductCatalogService.Application.Features.Queries.GetCategoryAttributes;
using ProductCatalogService.Application.Features.Queries.GetCategorySidebar;
using ProductCatalogService.Application.Features.Queries.GetProductsForSeller;
using ProductCatalogService.Application.Features.Queries.GetProductViewById;
using ProductCatalogService.Application.Features.Queries.GetProductViewsByCondition;
using ProductCatalogService.Application.Features.Queries.GetProductViewsByShop;
using ProductCatalogService.Application.Features.Queries.GetSimilarProducts;

namespace ProductCatalogService.API.Endpoints;

public class ProductCatalog : IEndpointGroup
{
    public static string RoutePrefix => "/product-catalog";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateProduct, "products")
            .Produces<ProductDto>()
            .RequireRateLimiting("post");

        groupBuilder.MapPut(UpdateProduct, "products/{id}")
            .Produces<ProductDto>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(CreateCategory, "category")
            .Produces<CreateCategoryResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(CreateCategoryAttribute, "category-attribute")
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetCategorySidebar, "categories/sidebar/{slug}")
            .Produces<CategorySidebarDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetAllCategories, "categories/all")
            .Produces<List<CategoryDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetCategoriesByShop, "categories/shop/{shopId}")
            .Produces<List<CategoryByShop>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetProductViewById, "products/view/{id}")
            .Produces<ProductViewDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetProductViewsByShop, "products/shop/{shopId}")
            .Produces<PagedResult<ProductViewsDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetProductViewsByCondition, "products/condition")
            .Produces<PagedResult<ProductViewsDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetCategories, "categories")
            .Produces<List<CategoryDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetCategoryAttributes, "categories/{categoryId}/attributes")
            .Produces<List<CategoryAttributeDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetProductsForSeller, "products/shop/me")
            .Produces<List<GetProductsForSellerResponse>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetSimilarProducts, "products/{productId}/similar")
            .Produces<List<ProductViewsDto>>()
            .RequireRateLimiting("get");
    }

    [EndpointSummary("Create product")]
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

    [EndpointSummary("Get category sidebar")]
    public static async Task<IResult> GetCategorySidebar(
        [AsParameters] GetCategorySidebarQuery query, ISender sender, CancellationToken cancellationToken)
    {
        CategorySidebarDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get categories by shop")]
    public static async Task<IResult> GetCategoriesByShop([AsParameters] GetCategoriesByShopQuery query,
        ISender sender, CancellationToken cancellationToken)
    {
        List<CategoryByShop> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get all categories")]
    public static async Task<IResult> GetAllCategories(ISender sender, CancellationToken cancellationToken)
    {
        List<CategoryDto> result = await sender.Send(new GetAllCategoriesQuery(), cancellationToken);
        return Results.Ok(result);
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

    [EndpointSummary("Get product view by id")]
    public static async Task<IResult> GetProductViewById(
        [AsParameters] GetProductViewByIdQuery query, ISender sender, CancellationToken cancellationToken)
    {
        ProductViewDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get product views by shop")]
    public static async Task<IResult> GetProductViewsByShop(
        [AsParameters] GetProductViewsByShopQuery query, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<ProductViewsDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get product views by condition")]
    public static async Task<IResult> GetProductViewsByCondition(
        [AsParameters] GetProductViewsByConditionQuery query, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<ProductViewsDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get categories")]
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

    [EndpointSummary("Get products for seller")]
    public static async Task<IResult> GetProductsForSeller(ISender sender, CancellationToken cancellationToken)
    {
        List<GetProductsForSellerResponse> result =
            await sender.Send(new GetProductsForSellerQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get similar products")]
    public static async Task<IResult> GetSimilarProducts(string productId, ISender sender,
        CancellationToken cancellationToken)
    {
        List<ProductViewsDto> result = await sender.Send(new GetSimilarProductsQuery(productId), cancellationToken);
        return Results.Ok(result);
    }
}
