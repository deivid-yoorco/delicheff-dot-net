select Sku as ProductSku, Id as ProductId, Name as ProductName
from Product
where Sku in (select SKU
	from Product
	where Deleted = 0
	and Published = 1
	group by Sku
	HAVING COUNT(Id)>1)
order by sku