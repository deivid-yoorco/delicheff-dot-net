BEGIN

	SELECT p.Name as ProductName, p.Id as ProductId
	FROM [dbo].[Product] p
	FULL JOIN [dbo].[Product_Picture_Mapping] ppm
	ON ppm.ProductId = p.Id
	Where p.Deleted = 0 AND p.Published = 1 AND ppm.Id IS NULL

END