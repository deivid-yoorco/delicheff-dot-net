BEGIN

	SELECT Email, Active
	FROM [dbo].[NewsLetterSubscription]
	order by Email desc

END