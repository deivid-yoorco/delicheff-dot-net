BEGIN

	SELECT Id as ProductId, Name as ProductName
	FROM [dbo].[Product] p
	Where p.Deleted = 0 AND GiftProductEnable = 0 AND p.Published = 1 AND ROUND(p.Price, 2) = ROUND(p.ProductCost, 2)

END