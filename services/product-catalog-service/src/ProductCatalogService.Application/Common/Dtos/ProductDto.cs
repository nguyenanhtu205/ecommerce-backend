namespace ProductCatalogService.Application.Common.Dtos;

public class ProductDto
{
    public required string Id { get; set; }

    public required string ShopId { get; set; }

    public required string CategoryId { get; set; }

    public List<CategoryPathItemDto> CategoryPath { get; set; } = [];

    public required string Name { get; set; }

    public required string Description { get; set; }

    public List<string> Tags { get; set; } = [];

    public required string Condition { get; set; }

    public required string Status { get; set; }

    public required string ThumbnailMediaId { get; set; }

    public string? VideoMediaId { get; set; }

    public List<string> GalleryMediaIds { get; set; } = [];

    public List<SpecificationDto> Specifications { get; set; } = [];

    public List<VariantGroupDto> VariantGroups { get; set; } = [];

    public List<VariantCombinationDto> VariantCombinations { get; set; } = [];

    public required ShippingInfoDto ShippingInfo { get; set; }

    public bool IsPreOrder { get; set; }

    public int? PreOrderDays { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public class SpecificationDto
{
    public required string AttributeId { get; init; }

    public required string Title { get; init; }

    public required string Value { get; init; }
}

public class VariantGroupDto
{
    public required string Name { get; init; }

    public List<VariantOptionDto> Options { get; init; } = [];
}

public class VariantOptionDto
{
    public required string Value { get; init; }

    public string? MediaId { get; init; }
}

public class VariantCombinationDto
{
    public string? CombinationId { get; init; }

    public List<string> OptionValues { get; init; } = [];

    public required string Sku { get; init; }

    public int InitialPrice { get; set; }

    public int InitialStock { get; set; }
}

public class ShippingInfoDto
{
    public int WeightGrams { get; init; }

    public double Length { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }
}
