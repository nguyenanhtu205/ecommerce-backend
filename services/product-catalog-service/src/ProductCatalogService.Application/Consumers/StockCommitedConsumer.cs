namespace ProductCatalogService.Application.Consumers;

public class StockCommitedConsumer(
    IApplicationDbContext dbContext,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer
) : IConsumer<StockCommited>
{
    public async Task Consume(ConsumeContext<StockCommited> context)
    {
        Dictionary<Guid, List<StockCommitedItem>> byProduct = context.Message.StockCommitedItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach ((Guid productId, List<StockCommitedItem> items) in byProduct)
        {
            FilterDefinition<ProductListingView> filter =
                Builders<ProductListingView>.Filter.Eq(x => x.Id, productId.ToString());

            int totalCommitted = items.Sum(i => i.Quantity);

            Dictionary<string, int> quantityByCombination = items
                .ToDictionary(i => i.CombinationId.ToString(), i => i.Quantity);

            BsonDocument quantityMapDoc = new(
                quantityByCombination.ToDictionary(kv => kv.Key, kv => (BsonValue)kv.Value));

            BsonDocument quantityArrayDoc = new("$objectToArray", quantityMapDoc);

            BsonDocument matchedFilterDoc = new("$filter",
                new BsonDocument
                {
                    { "input", quantityArrayDoc },
                    { "as", "kv" },
                    {
                        "cond", new BsonDocument("$eq",
                            new BsonArray { "$$kv.k", "$$vc.combinationId" })
                    }
                });

            BsonDocument committedQtyExpr = new("$let",
                new BsonDocument
                {
                    { "vars", new BsonDocument("match", matchedFilterDoc) },
                    {
                        "in", new BsonDocument("$ifNull",
                            new BsonArray { new BsonDocument("$arrayElemAt", new BsonArray { "$$match.v", 0 }), 0 })
                    }
                });

            PipelineDefinition<ProductListingView, ProductListingView> pipeline = new[]
            {
                new BsonDocument("$set",
                    new BsonDocument
                    {
                        {
                            "variantCombinations", new BsonDocument("$map",
                                new BsonDocument
                                {
                                    { "input", "$variantCombinations" },
                                    { "as", "vc" },
                                    {
                                        "in", new BsonDocument("$mergeObjects",
                                            new BsonArray
                                            {
                                                "$$vc",
                                                new BsonDocument("stock",
                                                    new BsonDocument("$subtract",
                                                        new BsonArray { "$$vc.stock", committedQtyExpr }))
                                            })
                                    }
                                })
                        },
                        {
                            "stockTotal", new BsonDocument("$subtract", new BsonArray { "$stockTotal", totalCommitted })
                        },
                        { "soldCount", new BsonDocument("$add", new BsonArray { "$soldCount", totalCommitted }) },
                        { "syncedAt", DateTime.UtcNow }
                    }),
                new BsonDocument("$set",
                    new BsonDocument
                    {
                        { "isOutOfStock", new BsonDocument("$lte", new BsonArray { "$stockTotal", 0 }) }
                    })
            };

            ProductListingView? updatedView = await dbContext.ProductListingViews.FindOneAndUpdateAsync(
                filter,
                pipeline,
                new FindOneAndUpdateOptions<ProductListingView, ProductListingView>
                {
                    ReturnDocument = ReturnDocument.After
                },
                context.CancellationToken);

            if (updatedView is null)
            {
                continue;
            }

            await listingViewProducer.Produce(
                new ProductListingViewUpdated(
                    updatedView.Id,
                    updatedView.ShopId,
                    updatedView.ShopName,
                    updatedView.Name,
                    updatedView.Description,
                    updatedView.Brand,
                    updatedView.Tags,
                    updatedView.SearchableSpecs,
                    updatedView.ThumbnailUrl,
                    updatedView.Location,
                    [
                        .. updatedView.CategoryPath.Select(c => new CategoryPathItemEvent(c.Id, c.Name))
                    ],
                    updatedView.PriceMin,
                    updatedView.PriceMax,
                    updatedView.OriginalPriceMin,
                    updatedView.DiscountPercent,
                    updatedView.StockTotal,
                    updatedView.IsOutOfStock,
                    updatedView.RatingAverage,
                    updatedView.RatingCount,
                    updatedView.SoldCount,
                    updatedView.SyncedAt
                ),
                context.CancellationToken);
        }
    }
}
