﻿<%@ Page language="c#" Codebehind="definition.aspx.cs" AutoEventWireup="True" Inherits="CancerGov.Web.Common.PopUps.Definition" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <HEAD id="header" runat="server">
<script language="JavaScript" src="/JS/imgEvents.js"></script>
<script language="JavaScript" src="/JS/popEvents.js"></script>
<link rel="stylesheet" href="/stylesheets/nci.css" type="text/css">
  </HEAD>
	<body leftmargin="0" topmargin="0" marginwidth="0" marginheight="0">
	<div align="center">
		<p>
		<table border="0" cellpadding="10" cellspacing="0" width="100%">
			<tr>
				<td align="left" valign="top">
					<div style="width:100%;">
					<%=strHeading%>
					<%=this.Content.Render()%>
					</div>
				</td>
			</tr>
			<tr>
				<td valign="top">
					<div style="width:100%;">
						<a href="Javascript:window.print();"><%=strSendPrinter%></a>
					</div>
				</td>
			</tr>
		</table>
		<p>
	</div>
	<asp:Literal ID="litOmniturePageLoad" mode="PassThrough" runat="server" />
  </body>

</HTML>