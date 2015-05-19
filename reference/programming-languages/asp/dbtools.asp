<%
Sub OpenDatabase(Connection, dbName)
  OpenDatabase_OLEDB Connection, dbName
End Sub

Sub OpenDatabase_OLEDB(Connection, dbName)
  Dim s
  s = dbName
  Set Connection = Server.CreateObject("ADODB.Connection")
  Connection.ConnectionString = s
  Connection.Open
End Sub

Sub CloseDataBase(Connection)
  Connection.Close
  Set Connection = Nothing
End Sub

Sub OpenRecordset(Connection, Recordset, szQuery)
  Set Recordset = Server.CreateObject("ADODB.Recordset")
  'Response.Write "dbsnl_szQuery = <b>'" & szQuery & "'</b><br>"
  'Response.Flush
  'Recordset.Open szQuery, Connection, 1, 4   ' 1=adOpenKeySet, 4=adLockBatchOptimistic
  Recordset.Open szQuery, Connection, 3, -1
End Sub

Sub OpenRecordsetStatic(Connection, Recordset, szQuery)
  Set Recordset = Server.CreateObject("ADODB.Recordset")
  'Response.Write "dbsnl_szQuery = <b>'" & szQuery & "'</b><br>"
  'Response.Flush
  Recordset.Open szQuery, Connection, 3, -1
End Sub

Sub OpenRecordsetPage(Connection, Recordset, szQuery, iNumRecsPage)
  Set Recordset = Server.CreateObject("ADODB.Recordset")
  'Response.Write "dbsnl_szQuery = [<b>" & szQuery & "</b>]<br>"
  'Response.Flush
  RecordSet.cursortype=2 'adusedynamic
  RecordSet.cursorlocation=3 'aduseclient
  RecordSet.pagesize=iNumRecsPage
  RecordSet.open szQuery, Connection
End Sub

Sub CloseRecordset(Recordset)
  Recordset.Close
  Set Recordset = Nothing
End Sub
%>