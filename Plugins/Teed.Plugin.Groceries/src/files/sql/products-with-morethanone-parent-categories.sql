BEGIN

	SELECT ProductId, ProductName FROM 
		(SELECT MAX(p.Name) as ProductName, p.Id as productId, COUNT(CASE WHEN c.ParentCategoryId = 0 THEN 1 ELSE NULL END) as ParentCategoryCount
			FROM [dbo].[Product] p
			FULL JOIN [dbo].[Product_Category_Mapping] pcm
			ON pcm.ProductId = p.Id
			FULL JOIN [dbo].[Category] c
			ON pcm.CategoryId = c.Id
			Where p.Deleted = 0 AND p.Published = 1
			AND (c.Deleted = 0 OR c.Deleted IS NULL)
		GROUP BY p.Id) as result
	WHERE result.ParentCategoryCount > 1

END