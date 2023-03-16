BEGIN

	SELECT MAX(product.Name) as ProductName, orderReport.CreatedOnUtc, OrderShippingDate, MAX(customer.Email) as ReportedBy
	  FROM [dbo].[OrderReport] orderReport
	  FULL JOIN [dbo].[Product] product
	  ON orderReport.ProductId = product.Id
	  FULL JOIN [dbo].[Customer] customer
	  ON customer.Id = orderReport.OriginalBuyerId
	  WHERE ProductId > 0 AND 
		(ManufacturerId IS NULL OR ManufacturerId = 0) AND 
		(ShoppingStoreId IS NULL OR ShoppingStoreId = '') AND 
		orderReport.OrderShippingDate >= CONVERT(datetime, @initDate) AND
		orderReport.OrderShippingDate <= CONVERT(datetime, @endDate)
	  GROUP BY orderReport.ProductId, orderReport.CreatedOnUtc, orderReport.OrderShippingDate, OriginalBuyerId
	ORDER BY ProductId

END