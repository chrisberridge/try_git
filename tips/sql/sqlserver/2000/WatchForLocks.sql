SELECT spid, blocked, status, cpu,memusage, physical_io,waittime,dbid
, substring(case when dbid = 0 then null when dbid <> 0 then db_name(dbid) end,1,20) dbname
, convert(sysname, rtrim(loginame)) as loginname
, substring(convert(varchar,last_batch,111),6,5) + ' ' + substring(convert(varchar,last_batch,113),13,8)
as 'last_batch_char',hostname
from master.dbo.sysprocesses (nolock)
where status <> 'background' and blocked > 0 order by last_batch_char asc
commit


exec sp_who2

dbcc inputbuffer(66)
commit