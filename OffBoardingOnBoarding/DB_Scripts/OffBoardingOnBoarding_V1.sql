--Create table to track report status
CREATE TABLE [customer].[OffBoardOnBoardStatusReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Source] [varchar](50) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[Comments] [ntext] NULL,
	[Status] [varchar](50) NOT NULL,
	[SuccessfulRuntime] [datetime] NULL,
	[ReportGenerationFromDate] [datetime] NULL,
	[TotalRecordCount] [int] NULL,
	[OutputFileName] [varchar](2500) NULL,
	[OutputFileLocation] [varchar](2500) NULL,
	[UserId] [int] NOT NULL,
	[DateAdded] [datetime] NULL,
	[DateLstMod] [datetime] NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

--Insert into [Customer.OffBoardOnBoardStatusReport] table using SP
 CREATE PROCEDURE [customer].[SaveOffBoardOnBoardStatusReport]   
     @Source varchar(50),-- sql or odata  
     @StartTime varchar(50), -- convert to datetie when inserting  
     @EndTime varchar(50) , -- convert to datetime when inserting  
     @Comments ntext , -- additional details like sql query or odataurl  
     @Status varchar(50) , -- status completed or error  
     @SuccessfulRuntime varchar(50), -- last successful runtime (should be same as ReportGenerationFromDate)  
     @TotalRecordCount int, -- no of records in report  
     @OutputFileName varchar (250), --   
     @OutputFileLocation varchar(2500), --   
     @UserId int, -- report generation starts from (null for first run)  
     @id int output  
 AS  
  BEGIN  
  SET NOCOUNT ON;  
   declare @dt datetime;  
   declare @ReportGenerationFromDate datetime;  
  
       if isnull(@SuccessfulRuntime,'')= ''    
      begin  
         set  @SuccessfulRuntime = null;  
      set  @EndTime = null ;  
      select @ReportGenerationFromDate = DateAdd(yy,-1,successfulruntime)  
     from customer.offboardonboardstatusreport   
       where id = (select max(id)   
           from customer.offboardonboardstatusreport   
           where [STATUS] = 'completed');  
    if isnull(@ReportGenerationFromDate,'')= ''  
      set @ReportGenerationFromDate = DateAdd(yy,-1,GETDATE());  
  
      end  
      
     INSERT INTO  OffBoardOnBoardStatusReport([Source],[StartTime],[EndTime],[Comments],[Status],[SuccessfulRuntime],[TotalRecordCount],[OutputFileName], [OutputFileLocation], [UserId],[DateAdded],[DateLstMod],[ReportGenerationFromDate])  
      VALUES (@Source,  
        CAST(@StartTime as DateTime),  
        CAST(@EndTime as DateTime),  
        @Comments,  
        @Status,  
        CAST(@SuccessfulRuntime as datetime),  
        @TotalRecordCount,  
        @OutputFileName,  
        @OutputFileLocation,  
        @UserId,  
        getdate(),  
        getdate(),  
        @ReportGenerationFromDate  
        )  
        
   SET @id=SCOPE_IDENTITY()  
   RETURN  @id  
 END  
 GO

 --Update [Customer.OffBoardOnBoardStatusReport] table using SP
 CREATE PROCEDURE [customer].[UpdateOffBoardOnBoardStatusReport]        
         @id int,
		 @Comments NVARCHAR(MAX),
		 @Status varchar(50), -- error? inprogress,completed   
		 @successfulruntime varchar(50),   --@ReportGenerationFromDate varchar(50),      
		 @totalRecordCount int,      @outputfileName varchar(50),     
         @outputfileLocation varchar(2500),      
		 @endtime varchar(50)     AS      SET NOCOUNT ON;         
  update OffBoardOnBoardStatusReport      
	set [status] = @status,          
	    [Comments]=@Comments,     
        [successfulruntime] = cast (@successfulruntime as datetime), --  getdate(),    
		--[ReportGenerationFromDate] = cast(@ReportGenerationFromDate as datetime),       
		[totalRecordCount] = @totalRecordCount,       
		[endtime] = cast(@endtime as datetime), --   getdate() ,       
		[OutputFileName]=@outputfileName,       
		[OutputFileLocation]=@outputfileLocation,       
		DateLstMod = getdate()      
	where [id] = @id;
GO

--View contains report data without filters
CREATE view [customer].vw_OffBoardOnBoardStudents  
  
  AS   
    SELECT   
         SS.SyStudentId,  
  
          SS.SySchoolStatusId,  
  
          FullName,  
  
          ss.email,  
  
          sysc.code,  
  
          max(AD.StatusDate) as statusdate  
  
FROM SyStudent SS (NOLOCK)  
  
LEFT JOIN AdEnroll AD (NOLOCK) ON SS.SyStudentId=AD.SyStudentID  
  
inner join syschoolstatus sysc (NOLOCK) on sysc.syschoolstatusid = ss.syschoolstatusid  
  
WHERE SS.SySchoolStatusID IN (10,11,17,51,67,66,86)  
  
GROUP BY SS.SyStudentId,SS.SySchoolStatusID,SS.FullName,ss.email,sysc.code  
 
 GO 

--Get daily report using SP
CREATE PROCEDURE Customer.uspGetOffBoardOnBoardStudents     
  @reportGenerationToTime varchar(50)      
   AS       
     SET NOCOUNT ON;      
           
     declare @maxid int;      
     declare @lastReportGeneratedDate datetime;      
      
     -- get most recent id (for first run it will be 0)      
     select  @maxid = coalesce(max(id),0)       
      from customer.offboardonboardstatusreport       
       where [status] = 'completed';      
      
   -- first run - get from today      
   if ( @maxid = 0)      
     select * from customer.vw_OffBoardOnBoardStudents s      
       where StatusDate <= DateAdd(yy,-1,CAST(@reportGenerationToTime AS Datetime))      
          
      else --  not the first run, get the last successfule runtime      
    begin       
     select @lastReportGeneratedDate = DateAdd(yy,-1,SuccessfulRuntime)      
      from customer.offboardonboardstatusreport       
          where id = @maxid;           
         
             -- get students       
     -- print '@lastReporteGeneratedDate :' + convert(varchar(24),@lastReporteGeneratedDate,121)      
    select * from customer.vw_OffBoardOnBoardStudents s      
    where StatusDate >= @lastReportGeneratedDate and StatusDate <= DateAdd(yy,-1,CAST(@reportGenerationToTime AS Datetime));      
    end;      
             
  RETURN   
  GO
