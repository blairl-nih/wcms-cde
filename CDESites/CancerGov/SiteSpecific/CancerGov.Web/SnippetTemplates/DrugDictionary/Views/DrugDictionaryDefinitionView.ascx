﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DrugDictionaryDefinitionView.ascx.cs" Inherits="CancerGov.Web.SnippetTemplates.DrugDictionaryDefinitionView" %>
<%@ Register TagPrefix="TermDictionaryHome" TagName="SearchBlock" Src="~/SnippetTemplates/TermDictionary/Views/TermDictionaryHome.ascx" %>
<%@ Import Namespace="NCI.Web.Dictionary.BusinessObjects" %>
 
<TermDictionaryHome:SearchBlock id="dictionarySearchBlock" runat="server" />
    
<asp:Repeater ID="termDictionaryDefinitionView" runat="server" OnItemDataBound="termDictionaryDefinitionView_OnItemDataBound">
<ItemTemplate> 
        <!-- Term and def -->
        <div class="results">
            <dl class="dictionary-list">
                <dt>
                    <dfn>
                        <%# ((DictionaryTerm)(Container.DataItem)).Term%>
                       
                    </dfn>
                </dt>
                <asp:PlaceHolder ID="phPronunciation" runat="server">
                    <dd class="pronunciation">
                        <a id="pronunciationLink" runat="server" class="CDR_audiofile"><span class="hidden">listen</span></a>
                        <asp:Literal ID="pronunciationKey" runat="server" />
                    </dd>
                </asp:PlaceHolder>
                <dd class="definition">
                    <%# ((DictionaryTerm)(Container.DataItem)).Definition.Text%>
                    
                    <asp:Panel runat="server" ID="pnlRelatedInfo">
                        <div class="related-resources">
                            <h6><asp:Literal ID="litMoreInformation" runat="server" /></h6>
                            <asp:Repeater ID="relatedExternalRefs" runat="server" Visible="false">
                                <HeaderTemplate> 
                                    <ul class="no-bullets">
                                </HeaderTemplate>
                                <ItemTemplate>
                                        <li><a href="<%# ((RelatedExternalLink)(Container.DataItem)).Url  %>"><%# ((RelatedExternalLink)(Container.DataItem)).Text  %></a></li>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </ul>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:Repeater ID="relatedSummaryRefs" runat="server" Visible="false">
                                 <HeaderTemplate> 
                                    <ul class="no-bullets">
                                </HeaderTemplate>
                                <ItemTemplate>
                                       <li><a href="<%# ((RelatedSummary)(Container.DataItem)).url  %>"><%# ((RelatedSummary)(Container.DataItem)).Text  %></a></li>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </ul>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:Repeater ID="relatedDrugInfoSummaries" runat="server" Visible="false">
                                 <HeaderTemplate> 
                                    <ul class="no-bullets">
                                </HeaderTemplate>
                                <ItemTemplate>
                                        <li><a href="<%# ((RelatedDrugSummary)(Container.DataItem)).url  %>"><%# ((RelatedDrugSummary)(Container.DataItem)).Text  %></a></li>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </ul>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder ID="phRelatedTerms" runat="server" Visible="false">
                                <p><asp:Label ID="labelDefintion" runat="server" class="related-definition-label"/>
                                 <asp:Repeater ID="relatedTerms" runat="server" OnItemDataBound="relatedTerms_OnItemDataBound">
                                   
                                    <ItemTemplate>
                                         <asp:HyperLink ID="relatedTermLink" runat="server" /><asp:Literal ID="relatedTermSeparator" runat="server" Text="," Visible="false" />
                                         
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                                </p>
                            </asp:PlaceHolder>
                         
                            <asp:Repeater ID="relatedImages" runat="server" Visible="false" OnItemDataBound="relatedImages_OnItemDataBound">
                              <ItemTemplate>
                                <figure class="image-left-medium">
                                    <a id="termEnlargeImage" runat="server" target="_blank" class="article-image-enlarge no-resize">Enlarge</a>
                                    <img id="termImage" runat="server" src="" alt="" />
                                        <figcaption>
                                            <div class="caption-container no-resize">
                                            <p><%# ((ImageReference)(Container.DataItem)).Caption  %></p>
                                            </div>
                                        </figcaption>
                                 </figure>
                              </ItemTemplate>
                         </asp:Repeater>
                      
                         </div>
                         
                    </asp:Panel>
                    
                </dd>
            </dl>
        </div>
        
</ItemTemplate> 
</asp:Repeater>