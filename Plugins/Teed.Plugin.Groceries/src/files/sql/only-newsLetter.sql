BEGIN

	SELECT Email, Active
	FROM [dbo].[NewsLetterSubscription]
	WHERE EMAIL NOT IN (SELECT Email FROM [dbo].[Customer] where Email IS NOT NULL)
	order by Email desc

END