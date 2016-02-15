-- Since: Feb.15/2016
-- Script to POC to insert a sequence in a movements table concurrently.

CREATE TABLE [dbo].[movements](
	[idMovement] [int] IDENTITY(1,1) NOT NULL,
	[idTransaction] [int] NULL,
	[seqNumberNext] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[idMovement] ASC
)
);

CREATE TABLE [dbo].[sequences](
	[idTransaction] [int] NOT NULL,
	[seqNumber] [int] NOT NULL,
    primary key clustered ( idTransaction ASC)
);


insert into sequences values(1, 0);
insert into sequences values(2, 0);
insert into sequences values(3, 0);

go

CREATE procedure [dbo].[spInsertSequence] @idTransaction int , @nextSeq int output
as
begin

begin tran
  begin try
   declare @currSeq int

   select @currSeq = seqNumber from sequences with(updlock) where idTransaction = @idTransaction
   set @currSeq = @currSeq + 1
   
   set @nextSeq = @currSeq
   update sequences with(updlock) set seqNumber = @nextSeq where idTransaction = @idTransaction
  end try
  begin ca5/tch
      select
        error_number() AS ErrorNumber
        ,error_severity() AS ErrorSeverity
        ,error_state() AS ErrorState
        ,error_procedure() AS ErrorProcedure
        ,error_line() AS ErrorLine
        ,error_message() AS ErrorMessage;

      if @@trancount > 0
         rollback tran;
  end catch
  if @@trancount > 0
     commit tran
end
go


create procedure [dbo].[spInsertMovements] @idTransaction int
as

begin transaction
begin try
declare @nextSeq int
exec spInsertSequence @idTransaction, @nextSeq output
insert into movements(idTransaction, seqNumberNext) values(@idTransaction, @nextSeq)
end try
begin catch
    select
        error_number() AS ErrorNumber
        ,error_severity() AS ErrorSeverity
        ,error_state() AS ErrorState
        ,error_procedure() AS ErrorProcedure
        ,error_line() AS ErrorLine
        ,error_message() AS ErrorMessage;

    if @@trancount > 0
       rollback tran;
end catch
if @@trancount > 0
   commit tran
go
