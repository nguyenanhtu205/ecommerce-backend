namespace Common.Contracts.Events;

public record ProductCreated(
    string ProductId,
    string ShopId,
    List<VariantCombinationInit> VariantCombinations,
    DateTimeOffset CreatedAt);

public record VariantCombinationInit(string CombinationId, string Sku, int InitialPrice, int InitialStock);
