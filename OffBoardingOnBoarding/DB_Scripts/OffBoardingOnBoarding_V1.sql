--Create table to track report status
CREATE TABLE [customer].[OffBoardOnBoardStatusReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Source] [varchar](50) NOT NULL,
	[TransactionStartTime] [datetime] NOT NULL,
	[TransactionEndTime] [datetime] NULL,
	[ReportGenerationFromDate] [datetime] NULL,
	[ReportGenerationToDate] [datetime] NULL,
	[Status] [varchar](50) NOT NULL,
	[TotalRecordCount] [int] NULL,
	[Comments] [ntext] NULL,
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
     @TransactionStartTime varchar(50), -- convert to datetie when inserting      
     @TransactionEndTime varchar(50) , -- convert to datetime when inserting      
     @Comments ntext , -- additional details like sql query or odataurl      
     @Status varchar(50) , -- status completed or error      
     @ReportGenerationToDate varchar(50), -- last successful runtime (should be same as ReportGenerationFromDate)      
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
      
       if isnull(@ReportGenerationToDate,'')= ''        
      begin      
         set  @ReportGenerationToDate = null;      
      set  @TransactionEndTime = null ;      
      select @ReportGenerationFromDate = ReportGenerationToDate      
     from customer.offboardonboardstatusreport       
       where id = (select max(id)       
           from customer.offboardonboardstatusreport       
           where [STATUS] = 'OK');      
    if isnull(@ReportGenerationFromDate,'')= ''      
      set @ReportGenerationFromDate = ''     
      
      end      
          
     INSERT INTO  OffBoardOnBoardStatusReport([Source],[TransactionStartTime],[TransactionEndTime],[Comments],[Status],[ReportGenerationToDate],[TotalRecordCount],[OutputFileName], [OutputFileLocation], [UserId],[DateAdded],[DateLstMod],[ReportGenerationFromDate])      
      VALUES (@Source,      
        CAST(@TransactionStartTime as DateTime),      
        CAST(@TransactionEndTime as DateTime),      
        @Comments,      
        @Status,      
        CAST(@ReportGenerationToDate as datetime),      
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
   @Status varchar(50), -- Error,Inprogress,OK     
   @reportGenerationToDate varchar(50),         
   @totalRecordCount int,      
   @outputfileName varchar(50),       
   @outputfileLocation varchar(2500),        
   @transactionendtime varchar(50)     
   AS      
   SET NOCOUNT ON;           
  update OffBoardOnBoardStatusReport        
 set [status] = @status,            
     [Comments]=@Comments,       
     [ReportGenerationToDate] = DATEADD(yy,-1,cast (@reportGenerationToDate as datetime)),        
     [totalRecordCount] = @totalRecordCount,         
     [TransactionEndtime] = cast(@transactionendtime as datetime),         
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
  @reportGenerationToDate varchar(50)      
   AS       
     SET NOCOUNT ON;      
           
     declare @maxid int;      
     declare @lastReportGeneratedDate datetime;      
      
     -- get most recent id (for first run it will be 0)      
     select  @maxid = coalesce(max(id),0)       
      from customer.offboardonboardstatusreport       
       where [status] = 'OK';      
      
   -- first run - get from today      
   if ( @maxid = 0)      
     select * from customer.vw_OffBoardOnBoardStudents s      
       where StatusDate <= DateAdd(yy,-1,CAST(@reportGenerationToDate AS Datetime))      
          
      else --  not the first run, get the last successfule runtime      
    begin       
     select @lastReportGeneratedDate = ReportGenerationToDate     
      from customer.offboardonboardstatusreport       
          where id = @maxid;           
         
             -- get students       
     -- print '@lastReporteGeneratedDate :' + convert(varchar(24),@lastReporteGeneratedDate,121)      
    select * from customer.vw_OffBoardOnBoardStudents s      
    where StatusDate >= @lastReportGeneratedDate and StatusDate <= DateAdd(yy,-1,CAST(@reportGenerationToDate AS Datetime));      
    end;      
             
  RETURN   
  GO
