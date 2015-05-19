<%
function curPageURLShort()
   dim s, protocol, port

   if Request.ServerVariables("HTTPS") = "on" then
     s = "s"
   else
     s = ""
   end if  
 
   protocol = strleft(LCase(Request.ServerVariables("SERVER_PROTOCOL")), "/") & s 
   if Request.ServerVariables("SERVER_PORT") = "80" then
     port = ""
   else
     port = ":" & Request.ServerVariables("SERVER_PORT")
   end if  
   curPageURLShort = protocol & "://" & Request.ServerVariables("SERVER_NAME") & port 
end function

function curPageURL()
   dim s, protocol, port
   
   if Request.ServerVariables("HTTPS") = "on" then
     s = "s"
   else
     s = ""
   end if  
   protocol = strleft(LCase(Request.ServerVariables("SERVER_PROTOCOL")), "/") & s 
   
   if Request.ServerVariables("SERVER_PORT") = "80" then
     port = ""
   else
     port = ":" & Request.ServerVariables("SERVER_PORT")
   end if  
   curPageURL = protocol & "://" & Request.ServerVariables("SERVER_NAME") & port & Request.ServerVariables("SCRIPT_NAME")
end function

function strLeft(str1,str2)
   strLeft = Left(str1,InStr(str1,str2)-1)
end function

function DrawNavBarByLine(PageCount, iCPage, sHREFURL, sParameterList, iNumJumpPages, jsfunction)
' PageCount is infered from a recordset information to paginate to know the boundaries.
' iCPage refers to the current page we are viewing. This is a parameter value (this
' function returns the correct value in here to jump to that page in the recordset
' but it is done outside this function as shown in the example.
' sHREFURL refers to the base URL which is responsible to continue feeding
' the information.
' If the information is fed by an HTML form then sParameterList is all current
' values for the form, if no parameters are required then this parameter must
' be empty.
' The function returns an empty string if no viewable pagination is required.
' Besides iNumJumpPaes dictates how many page increments the bar will have,
' that is, if 10 is specified the bar jumps from 10 to 10 pages each time, if
' it is 5 then from 5 to 5 and so on.
' The purpose of jsfunction is to invoke a Javascript to get page via jQuery Ajax 
' instead of a form submit.
' CALL Example:
'    iCurrPage = 0
'    sParameterList = ""
'    sParameterList = sParameterList & "lbSearchBy=" & Request("lbSearchBy") & "&"
'    sParameterList = sParameterList & "lbMonth=" & Request("lbMonth") & "&"
'    sParameterList = sParameterList & "lbDay="& Request("lbDay") & "&"
'    sParameterList = sParameterList & "txtSearch=" & Request("txtSearch")
'    sNavBar = DrawNavBarByLine(RSNews.PageCount, iCurrPage, "main.asp?page=" & ID_PAGE_REPOWERHOME & "&srch=1", sParameterList, 10, "")
'    RSNews.AbsolutePage = iCurrPage

' NOTE:
' Draw a navigator bar in the format  < ... 1  2  3  4  5  6  7  8  9  10  ... >
' The purpose of '...' is make note that there are pages below or above what is shown, and
' they appear only if required. The '<' is the previous button, and '>' is next button.
' It jumps from 10 to 10 (dictated by the iNumJumpPages parameter).
' [<< ] jumps 10 pages back, [ >> ] jumps 10 pages forward. (dictated by the iNumJumpPages parameter)
' You have to bear in mind that some parameters are coded regarding the use of
' this nav bar.
' In the URL is formatted the information the navigation do need to properly work
' such as start page, end page, pg indicating if 1 a backward or << move, and 2
' a 2 for  a forward or >> move and the current page viewed where necessary.

' Also keep in mind that this function is a table per se and thus must be embedded as is
' NOTE: Returns a TABLE HTML tag.
  Dim s, i
  Dim sURL, sTmp
  Dim iCurrPage, iPageCount
  Dim iStartPage, iEndPage
  Dim iPG ' User clicked the [ << ] or [ >> ] link. (NOTE: Changed to '...' either way.
  Dim sURLFixed, bFirst
  Dim iBtn ' User clicked the [ < ] or [ > ] link.

  iBtn = 0
  if not isNull(Request("b")) then
     iBtn = CLng(Request("b"))
  end if
  sURLFixed = ""
  sURL = ""
  bFirst = false
  iNumJumpPages = CLng(iNumJumpPages)

  sURLFixed = sHREFURL
  if InStr(sURLFixed, "?") = 0 then
     sURLFixed = sURLFixed & "?"
     bFirst = true
  end if

  iPageCount = PageCount
  iPG = CLng(Request("pg"))
  iStartPage = CLng(Request("s"))
  if iStartPage = 0 then
     iStartPage = 1
  end if

  iEndPage = CLng(Request("e"))
  if iEndPage = 0 then
     iEndPage = iNumJumpPages
  end if
  iCurrPage = CLng(Request("cpage"))
  if iCurrPage = 0 then
     iCurrPage = 1
  end if

  if iBtn = 1 then
     if iCurrPage < iStartPage then
        iStartPage = iStartPage - iNumJumpPages
        iEndPage = iEndPage - iNumJumpPages
        if iStartPage < 0 then
           iStartPage = 1
           iEndPage = iNumJumpPages
        end if	    
	 end if
  end if
  if iBtn = 2 then
     if iCurrPage > iEndPage then
	    iStartPage = iStartPage + iNumJumpPages
        iEndPage = iEndPage + iNumJumpPages
	 end if
  end if

  ' Need to change group
  select case iPG
    case 1 ' [ << ]
      iStartPage = iStartPage - iNumJumpPages
      iEndPage = iEndPage - iNumJumpPages
      if iStartPage < 0 then
         iStartPage = 1
         iEndPage = iNumJumpPages
      end if
      iCurrPage = iStartPage
    case 2 ' [ >> ]
      iStartPage = iStartPage + iNumJumpPages
      iEndPage = iEndPage + iNumJumpPages
      iCurrPage = iStartPage
  end select
  if (iEndPage - iStartPage + 1) <> iNumJumpPages then
     iEndPage = iStartPage + (iNumJumpPages - 1)
  end if
  if iEndPage > iPageCount then
     iEndPage = iPageCount
  end if
  iPrevPage = (iCurrPage - 1)
  iNextPage = (iCurrPage + 1)
  iCPage = iCurrPage ' Return the current page to see.
  sURL = sURLFixed
  if not bFirst then
     sURL = sURL & "&s=" & iStartPage
  else
     sURL = sURL & "s=" & iStartPage
  end if
  sURL = sURL & "&e=" & iEndPage
  
  'Response.Write("iPrevPage=[" & iPrevPage & "]<br>")
  'Response.Write("iNextPage=[" & iNextPage & "]<br>")
  'Response.Write("iCurrPage=[" & iCurrPage & "]<br>")
  'Response.Write("iStartPage=[" & iStartPage & "]<br>")
  'Response.Write("iEndPage=[" & iEndPage & "]<br><hr>")
  
  s= ""
  if iPageCount > 1 then
     s = s & "<div class='notas-clave-paginador'>"
	 s = s & "<ol id='EC100A_ModuloPrincipal_pagination'>"	 
	 
	 ' Previous Button
	 if iPrevPage <> 0 then
		sTmp = sUrl & "&b=1&cpage=" & iPrevPage
		if sParameterList <> "" then
		   sTmp = sTmp & "&" & sParameterList
		end if
		if jsfunction <> "" then		   
		   sTmp = jsfunction & "('" & sTmp & "');"
		   s = s & "<li><a onclick=""" & sTmp & """>&lt;</a></li>"
		else
		   s = s & "<li><a href='" & sTmp & "'>&lt;</a></li>"
		end if
	 end if
	 
	 ' Fast forward left button, aka [ << ]
     if iCurrPage > iNumJumpPages then	    
	    sTmp = sURL & "&pg=1"
		if sParameterList <> "" then
		   sTmp = sTmp & "&" & sParameterList
		end if
		if jsfunction <> "" then
		   sTmp = jsfunction & "('" & sTmp & "');"
		   s = s & "<li><a onclick=""" & sTmp & """>...</a></li>"
		else
		   s = s & "<li><a href='" & sTmp & "'>...</a></li>"
		end if		
	 else
	    s = s & "" ' Show nothing
     end if	 
	 for i = iStartPage to iEndPage
	   sTmp = sUrl & "&cpage=" & i
       if sParameterList <> "" then
	      sTmp = sTmp & "&" & sParameterList
       end if
	   if iCurrPage = i then
	       s = s & "<li class='EC100A_ModuloPrincipal_pagination_current'>" & i & "</li>"
	   else
	       if jsfunction <> "" then
		      sTmp = jsfunction & "('" & sTmp & "');"
		      s = s & "<li><a onclick=""" & sTmp & """>" & i & "</a></li>"
		   else
   		      s = s & "<li><a href='" & sTmp & "'>" & i & "</a></li>"
		   end if	       
	   end if
	 next
	 
	 ' Fast forward right button, aka [ >> ]
	 if iEndPage < iPageCount then
	    sTmp = sUrl & "&pg=2"
		if sParameterList <> "" then
		   sTmp = sTmp & "&" & sParameterList
		end if
		if jsfunction <> "" then
		   sTmp = jsfunction & "('" & sTmp & "');"
		   s = s & "<li><a onclick=""" & sTmp & """>...</a></li>"
		else
		   s = s & "<li><a href='" & sTmp & "'>...</a></li>"
		end if		
	 else
	    s = s & "" ' Show nothing
	 end if
	 
	 ' Next Button
	 if iNextPage <= PageCount then
	    sTmp = sUrl & "&b=2&cpage=" & iNextPage
        if sParameterList <> "" then
	       sTmp = sTmp & "&" & sParameterList
        end if
	    if jsfunction <> "" then
   	       sTmp = jsfunction & "('" & sTmp & "');"
	       s = s & "<li><a onclick=""" & sTmp & """>&gt;</a></li>"
	    else
   	       s = s & "<li><a href='" & sTmp & "'>&gt;</a></li>"
	    end if
	 end if
	 s = s & "</ol>"
	 s = s & "</div>"
  end if
  DrawNavBarByLine = s
end Function

function StringAHTMLCode(sHtmlData)
   ' http://www.ascii.cl/htmlcodes.htm
   ' http://homepage1.nifty.com/tabotabo/ccc/asci.htm
   if not isNull(sHTmlData) then
	   sHtmlData = Replace(sHtmlData, "á", "&aacute;")
	   sHtmlData = Replace(sHtmlData, "é", "&eacute;")
	   sHtmlData = Replace(sHtmlData, "í", "&iacute;")
	   sHtmlData = Replace(sHtmlData, "ú", "&uacute;")
	   sHtmlData = Replace(sHtmlData, "ó", "&oacute;")
	   sHtmlData = Replace(sHtmlData, "ñ", "&ntilde;")
	   sHtmlData = Replace(sHtmlData, "Á", "&Aacute;")
	   sHtmlData = Replace(sHtmlData, "É", "&Eacute;")
	   sHtmlData = Replace(sHtmlData, "Í", "&Iacute;")
	   sHtmlData = Replace(sHtmlData, "Ú", "&Uacute;")
	   sHtmlData = Replace(sHtmlData, "Ó", "&Oacute;")
	   sHtmlData = Replace(sHtmlData, "Ñ", "&Ntilde;")
	   sHtmlData = Replace(sHtmlData, """", "&quot;")
	   sHtmlData = Replace(sHtmlData, chr(147), "&#8220;")
	   sHtmlData = Replace(sHtmlData, chr(148), "&#8221;")
	   sHtmlData = Replace(sHtmlData, chr(145), "&#8216;")
	   sHtmlData = Replace(sHtmlData, chr(146), "&#8217;")
	   sHtmlData = Replace(sHtmlData, chr(33), "&#33;")
	   sHtmlData = Replace(sHtmlData, chr(36), "&#36;")
	   sHtmlData = Replace(sHtmlData, chr(37), "&#37;")	   
	   sHtmlData = Replace(sHtmlData, chr(39), "&#39;")
	   sHtmlData = Replace(sHtmlData, chr(40), "&#40;")
	   sHtmlData = Replace(sHtmlData, chr(41), "&#41;")
	   sHtmlData = Replace(sHtmlData, chr(42), "&#42;")
	   sHtmlData = Replace(sHtmlData, chr(43), "&#43;")
	   sHtmlData = Replace(sHtmlData, chr(45), "&#45;")
	   sHtmlData = Replace(sHtmlData, chr(47), "&#47;")
	   sHtmlData = Replace(sHtmlData, chr(60), "&#60;")
	   sHtmlData = Replace(sHtmlData, chr(61), "&#61;")
	   sHtmlData = Replace(sHtmlData, chr(62), "&#62;")
	   sHtmlData = Replace(sHtmlData, chr(63), "&#63;")
	   sHtmlData = Replace(sHtmlData, chr(64), "&#64;")
	   sHtmlData = Replace(sHtmlData, chr(96), "&#96;")
	   sHtmlData = Replace(sHtmlData, chr(123), "&#123;")
	   sHtmlData = Replace(sHtmlData, chr(124), "&#124;")
	   sHtmlData = Replace(sHtmlData, chr(125), "&#125;")
	   sHtmlData = Replace(sHtmlData, chr(126), "&#126;")
	   sHtmlData = Replace(sHtmlData, chr(161), "&#161;")
	   sHtmlData = Replace(sHtmlData, chr(162), "&#162;")
	   sHtmlData = Replace(sHtmlData, chr(163), "&#163;")
	   sHtmlData = Replace(sHtmlData, chr(164), "&#164;")
	   sHtmlData = Replace(sHtmlData, chr(165), "&#165;")
	   sHtmlData = Replace(sHtmlData, chr(166), "&#166;")
	   sHtmlData = Replace(sHtmlData, chr(167), "&#167;")
	   sHtmlData = Replace(sHtmlData, chr(168), "&#168;")
	   sHtmlData = Replace(sHtmlData, chr(169), "&#169;")
	   sHtmlData = Replace(sHtmlData, chr(170), "&#170;")
	   sHtmlData = Replace(sHtmlData, chr(171), "&#171;")
	   sHtmlData = Replace(sHtmlData, chr(172), "&#172;")
	   sHtmlData = Replace(sHtmlData, chr(173), "&#173;")
	   sHtmlData = Replace(sHtmlData, chr(174), "&#174;")
	   sHtmlData = Replace(sHtmlData, chr(175), "&#175;")
	   sHtmlData = Replace(sHtmlData, chr(176), "&#176;")
	   sHtmlData = Replace(sHtmlData, chr(177), "&#177;")
	   sHtmlData = Replace(sHtmlData, chr(178), "&#178;")
	   sHtmlData = Replace(sHtmlData, chr(179), "&#179;")
	   sHtmlData = Replace(sHtmlData, chr(180), "&#180;")
	   sHtmlData = Replace(sHtmlData, chr(181), "&#181;")
	   sHtmlData = Replace(sHtmlData, chr(182), "&#182;")
	   sHtmlData = Replace(sHtmlData, chr(183), "&#183;")
	   sHtmlData = Replace(sHtmlData, chr(184), "&#184;")
	   sHtmlData = Replace(sHtmlData, chr(185), "&#185;")
	   sHtmlData = Replace(sHtmlData, chr(186), "&#186;")
	   sHtmlData = Replace(sHtmlData, chr(187), "&#187;")
	   sHtmlData = Replace(sHtmlData, chr(188), "&#188;")
	   sHtmlData = Replace(sHtmlData, chr(189), "&#189;")
	   sHtmlData = Replace(sHtmlData, chr(190), "&#190;")
	   sHtmlData = Replace(sHtmlData, chr(191), "&#191;")
	   sHtmlData = Replace(sHtmlData, chr(192), "&#192;")
	   sHtmlData = Replace(sHtmlData, chr(193), "&#193;")
	   sHtmlData = Replace(sHtmlData, chr(194), "&#194;")
	   sHtmlData = Replace(sHtmlData, chr(195), "&#195;")
	   sHtmlData = Replace(sHtmlData, chr(196), "&#196;")
	   sHtmlData = Replace(sHtmlData, chr(197), "&#197;")
	   sHtmlData = Replace(sHtmlData, chr(198), "&#198;")
	   sHtmlData = Replace(sHtmlData, chr(199), "&#199;")
	   sHtmlData = Replace(sHtmlData, chr(200), "&#200;")
	   sHtmlData = Replace(sHtmlData, chr(201), "&#201;")
	   sHtmlData = Replace(sHtmlData, chr(202), "&#202;")
	   sHtmlData = Replace(sHtmlData, chr(203), "&#203;")
	   sHtmlData = Replace(sHtmlData, chr(204), "&#204;")
	   sHtmlData = Replace(sHtmlData, chr(205), "&#205;")
	   sHtmlData = Replace(sHtmlData, chr(206), "&#206;")
	   sHtmlData = Replace(sHtmlData, chr(207), "&#207;")
	   sHtmlData = Replace(sHtmlData, chr(231), "&#231;")
	   sHtmlData = Replace(sHtmlData, chr(252), "&#252;")
	   sHtmlData = Replace(sHtmlData, chr(224), "&#224;")
	   sHtmlData = Replace(sHtmlData, chr(226), "&#226;")
	   sHtmlData = Replace(sHtmlData, chr(227), "&#227;")
	   sHtmlData = Replace(sHtmlData, chr(228), "&#228;")
	   sHtmlData = Replace(sHtmlData, chr(229), "&#229;")
	   sHtmlData = Replace(sHtmlData, chr(230), "&#230;")
	   sHtmlData = Replace(sHtmlData, chr(232), "&#232;")
	   sHtmlData = Replace(sHtmlData, chr(234), "&#234;")
	   sHtmlData = Replace(sHtmlData, chr(235), "&#235;")
	   sHtmlData = Replace(sHtmlData, chr(236), "&#236;")
	   sHtmlData = Replace(sHtmlData, chr(238), "&#238;")
	   sHtmlData = Replace(sHtmlData, chr(239), "&#239;")
	   sHtmlData = Replace(sHtmlData, chr(240), "&#240;")
	   sHtmlData = Replace(sHtmlData, chr(242), "&#242;")
	   sHtmlData = Replace(sHtmlData, chr(244), "&#244;")
	   sHtmlData = Replace(sHtmlData, chr(245), "&#245;")
	   sHtmlData = Replace(sHtmlData, chr(246), "&#246;")
	   sHtmlData = Replace(sHtmlData, chr(247), "&#247;")
	   sHtmlData = Replace(sHtmlData, chr(248), "&#248;")
	   sHtmlData = Replace(sHtmlData, chr(249), "&#249;")
	   sHtmlData = Replace(sHtmlData, chr(251), "&#251;")
	   sHtmlData = Replace(sHtmlData, chr(252), "&#252;")
	   sHtmlData = Replace(sHtmlData, chr(253), "&#253;")
	   sHtmlData = Replace(sHtmlData, chr(254), "&#254;")
	   sHtmlData = Replace(sHtmlData, chr(255), "&#255;")	   
   end if
   StringAHTMLCode = sHtmlData
end function

function LinkToUrl(url, txt)
   if txt <> "" then
      urlFixed = curPageURLShort() & "/" & url
      linkData = "<a href='" & urlFixed & "'>" & txt & "</a>"
      LinkToUrl = linkData   
   else
      LinkToUrl = ""
   end if
end function

sub GoogleNewsKeywords()
   if isNull(infoGoogleNewsKeys) or infoGoogleNewsKeys = "" then      
      newsKeys = "periódico, prensa, Colombia, Medellín, noticias, actualidad, opinión, deportes, cultura, entretenimiento, multimedia"
      infoGoogleNewsKeys = "<meta name=""news_keywords"" content=""[nkeys]"">"
	  infoGoogleNewsKeys = Replace(infoGoogleNewsKeys, "[nkeys]", newsKeys)
   end if
   Response.Write infoGoogleNewsKeys
end sub

sub SiteKeywords()
   dataKeys = ""
   if isNull(seccionSEOKeywords) or seccionSEOKeywords = "" then
      dataKeys = "periódico, prensa, Colombia, Medellín, noticias, actualidad, opinión, deportes, cultura, entretenimiento, multimedia"
   else
      dataKeys = seccionSEOKeywords	  
   end if
   infoKeys = "<meta name=""keywords"" content=""[nkeys]"">"
   infoKeys = Replace(infoKeys, "[nkeys]", dataKeys)   
   Response.Write infoKeys
end sub

function NormalizeStringToJSUse(ByVal s)
   rsltJS = s
   rsltJS = Replace(rsltJS, chr(34), chr(92) & chr(34))
   rsltJS = Replace(rsltJS, chr(147), chr(92) & chr(34))
   rsltJS = Replace(rsltJS, chr(148), chr(92) & chr(34))
   rsltJS = Replace(rsltJS, "&quot;", chr(92) & chr(34))
   rsltJS = Replace(rsltJS, "&#34;", chr(92) & chr(34))
   'Response.Write "Normalize r=[" & rsltJS & "]"
   NormalizeStringToJSUse = rsltJS
end function

'-- Don't forget to change domain when in production environment.
sub StoreCookie(id, value, ndaysexpire)
   domainName = ".elcolombiano.com"
   Response.Cookies(id).Domain = domainName
   Response.Cookies(id) = value
   if ndaysexpire <> 0 then
      Response.Cookies(id).Expires = Date() + ndaysexpire	  
   end if
end sub

function RetrieveCookieValue(id)
    value = Request.Cookies(id)
	if isNull(value) then
	   value = ""
	end if
	RetrieveCookieValue = value
end function

function ExtractHREFFromSEOBJURL(url, urlPrefix)
   sExtracted = LCase(url)
   if urlPrefix <> "" then
      sExtracted = Replace(sExtracted, "<a href='", urlPrefix)
   else
      sExtracted = Replace(sExtracted, "<a href='", "")
   end if
   sExtracted = Replace(sExtracted, "'", "")
   sExtracted = Replace(sExtracted, ">", "")
   sExtracted = Trim(sExtracted)
   ExtractHREFFromSEOBJURL = sExtracted
end function
%>