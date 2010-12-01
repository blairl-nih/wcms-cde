﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewPublicationsByCancerType.ascx.cs"
    Inherits="TCGA.Web.SnippetTemplates.ViewPublicationsByCancerType" %>
<div class="ViewPublications">
    <h1>
        Publications</h1>
    <p>
        All data generated by The Cancer Genome Atlas (TCGA) Research Network are made open
        to the public through the Data Coordinating Center and the TCGA Data Portal.</p>
    <p class="cancerTypeHeader">
        View Publications by Cancer Type</p>
    <asp:DropDownList CssClass="ddlCancerType" runat="server" ID="ddlCancerType" Width="600px">
    </asp:DropDownList>
    <p>
        TCGA Research Network</p>
    <asp:Repeater runat="server" ID="rptPublicationResults">
        <ItemTemplate>
            <div>
                <p>
                    Description<a class="pubLink" href="">Link Title</a></p>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
