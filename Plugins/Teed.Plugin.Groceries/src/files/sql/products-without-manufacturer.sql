BEGIN
  
  SELECT p.[Id] as ProductId, p.[Name] as ProductName
  FROM [dbo].Product p
  FULL JOIN [dbo].[Product_Manufacturer_Mapping] pmm
  ON pmm.ProductId = p.Id
  WHERE pmm.Id IS NULL AND p.Deleted = 0 AND p.Published = 1

END