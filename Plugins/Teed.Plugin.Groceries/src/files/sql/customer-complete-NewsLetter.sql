SELECT cc.Id,
(SELECT TOP (1) [Value] 
	FROM [dbo].[GenericAttribute] 
	where EntityId = cc.Id 
	AND KeyGroup = 'Customer' 
	AND [Key] = 'FirstName' ) as Nombre,
(SELECT TOP (1) [Value] 
	FROM [dbo].[GenericAttribute] 
	where EntityId = cc.Id 
	AND KeyGroup = 'Customer' 
	AND [Key] = 'LastName' ) as Apellido,
MAX(cc.[Email]) As Correo,
MAX(dir.PhoneNumber) As Telefono,
(SELECT TOP (1) [Value] 
	FROM [dbo].[GenericAttribute] 
	where EntityId = cc.Id 
	AND KeyGroup = 'Customer' 
	AND [Key] = 'Gender' ) as Genero,
MAX(dir.ZipPostalCode) As CP,
MAX(cc.CreatedOnUtc) As Fecha_registro,
(SELECT TOP (1) [Active]
  FROM [dbo].[NewsLetterSubscription]
  WHERE Email = MAX(cc.Email)) As ActiveNewsletter
FROM [dbo].[Customer] As cc
FULL OUTER JOIN [dbo].[CustomerAddresses] As ca
ON cc.Id = ca.Customer_Id
FULL OUTER JOIN [dbo].[Address] As dir
ON ca.Address_Id = dir.Id
where cc.Email IS NOT NULL
Group by cc.Id
order by cc.Id Asc