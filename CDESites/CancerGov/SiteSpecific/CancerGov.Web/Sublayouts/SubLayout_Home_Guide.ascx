﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="NCI.Web.CDE.UI.SnippetControls.SubLayoutControl" %>
<!-- BEGIN CARDS ROW -->
<div class="row guide-card accordion" data-match-height="">
    <NCI:TemplateSlot
        id="nvcgSlLayoutGuideA"
        AdditionalSnippetClasses="equalheight medium-4 columns card featured-card gutter"
        runat="server" />
    <NCI:TemplateSlot
        id="nvcgSlLayoutGuideB"
        AdditionalSnippetClasses="equalheight medium-4 columns card gutter"
        runat="server" />
</div>
<!-- END CARDS ROW -->