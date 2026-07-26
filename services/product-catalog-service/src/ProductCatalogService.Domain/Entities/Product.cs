namespace ProductCatalogService.Domain.Entities;

public class Product
{
    public required string Id { get; init; }

    public required string ShopId { get; init; }

    public required string CategoryId { get; set; }

    public List<CategoryPathItem> CategoryPath { get; set; } = [];

    public required string Name { get; set; }

    public required string Description { get; set; }

    public List<string> Tags { get; set; } = [];

    public ProductCondition Condition { get; set; } = ProductCondition.New;

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public required string ThumbnailMediaId { get; set; }

    public string? VideoMediaId { get; set; }

    public List<string> GalleryMediaIds { get; set; } = [];

    public List<Specification> Specifications { get; set; } = [];

    public List<VariantGroup> VariantGroups { get; set; } = [];

    public List<VariantCombination> VariantCombinations { get; set; } = [];

    public required ShippingInfo ShippingInfo { get; set; }

    public bool IsPreOrder { get; set; }

    public int? PreOrderDays { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; set; }
}

public class Specification
{
    public required string AttributeId { get; init; }

    public required string Title { get; init; }

    public required string Value { get; init; }
}

public class VariantGroup
{
    public required string Name { get; init; }

    public List<VariantOption> Options { get; init; } = [];
}

public class VariantOption
{
    public required string Value { get; init; }

    public string? MediaId { get; init; }
}

public class VariantCombination
{
    public required string CombinationId { get; init; }

    public List<string> OptionValues { get; init; } = [];

    public required string Sku { get; init; }
}

public class ShippingInfo
{
    public int WeightGrams { get; init; }

    public required Dimensions Dimensions { get; init; }
}

public class Dimensions
{
    public double Length { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }
}
