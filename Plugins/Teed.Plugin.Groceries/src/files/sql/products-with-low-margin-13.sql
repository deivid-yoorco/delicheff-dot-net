BEGIN

	SELECT Id as ProductId, Name as ProductName, 1 - (p.ProductCost / p.Price) as Calculate
	FROM [dbo].[Product] p
	Where p.Price > 0 AND p.Deleted = 0 AND p.Published = 1 AND ROUND((1 - (p.ProductCost / p.Price)), 4) >= 0.1 AND ROUND((1 - (p.ProductCost / p.Price)), 4) < 0.13
	Order by 1 - (p.ProductCost / p.Price)

END