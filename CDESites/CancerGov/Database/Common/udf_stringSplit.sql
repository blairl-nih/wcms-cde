if object_id('udf_stringsplit') is not NULL
	drop FUNCTION [dbo].[udf_StringSplit]
go

CREATE FUNCTION [dbo].[udf_StringSplit]
	(
	@ComaSeparatedID varchar(max),
	@delimiter varchar(2) = ' '
	)
RETURNS @ResultTable TABLE
	(
	ObjectID     varchar(500)
	)
AS
BEGIN
	
	DECLARE @delimeterPosition  int,
		@tmpStr varchar(8000) ,
		@tmpStrLen int
	
	SELECT @tmpStrLen = LEN(@ComaSeparatedID),
		@tmpStr = @ComaSeparatedID
	
	SELECT @tmpStrLen = ISNULL(@tmpStrLen, 0)

	WHILE  (@tmpStrLen > 0 )
	BEGIN
		SELECT @delimeterPosition = CHARINDEX ( @delimiter, @tmpStr) 

		IF (@delimeterPosition = 0)  
		BEGIN
			INSERT INTO @ResultTable
			SELECT @tmpStr 
			BREAK
		END

		INSERT INTO @ResultTable
		SELECT LTRIM(LEFT( @tmpStr , @delimeterPosition - 1) )

		SELECT @tmpStr = RIGHT ( @tmpStr , (LEN( @tmpStr ) - @delimeterPosition) )

		SELECT @tmpStrLen = ISNULL( LEN(@tmpStr) , 0)
	END

	RETURN 
END
GO
grant select on [dbo].[udf_StringSplit] to CDEUser










