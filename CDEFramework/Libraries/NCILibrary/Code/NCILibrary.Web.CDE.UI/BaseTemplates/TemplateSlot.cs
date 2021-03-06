﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NCI.Text;

namespace NCI.Web.CDE.UI
{
    /// <summary>
    /// ASP.Net control which represents a Slot in a Percussion Global/Local template.
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:TemplateSlot runat=server></{0}:TemplateSlot>")]
    public class TemplateSlot : WebControl
    {
        /// <summary>
        /// Removes the HTML output of this control if there are no contents.
        /// </summary>
        [DefaultValue(false)]
        public bool RenderIfEmpty { get; set; }

        

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> value that corresponds to this Web server control. This property is used primarily by control developers.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> enumeration values.
        /// </returns>
        protected override HtmlTextWriterTag TagKey 
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// Gets or Sets HTML to be rendered before this TemplateSlot.
        /// </summary>
        /// <remarks>
        ///   This is only rendered if the TemplateSlot has child controls or if 
        ///   RenderIfEmpty is true.  The HTML should be plain HTML with no ASP.Net controls.
        /// </remarks>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or Sets HTML to be rendered after this TemplateSlot.
        /// </summary>
        /// <remarks>
        ///   This is only rendered if the TemplateSlot has child controls or if 
        ///   RenderIfEmpty is true.  The HTML should be plain HTML with no ASP.Net controls.
        ///   
        /// </remarks>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets the Additional Css Classes xml element to add to a specific slot item
        /// </summary>
        public string AdditionalSnippetClasses { get; set; }

        /// <summary>
        /// Adds a Snippet Control to this TemplateSlot.
        /// </summary>
        /// <param name="snippetInfo">The snippet info.</param>
        public void AddSnippet(SnippetControl snippetControl)
        {
            TemplateSlotItem item = new TemplateSlotItem();
            this.Controls.Add(item);
            item.Controls.Add(snippetControl);
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // If none of the snippet controls in the template slot are visible, 
            // do not render - unless RenderIfEmpty is true
            if (this.Controls.Cast<TemplateSlotItem>().Where(c => c.SnippetControl.Visible).Count() == 0 &&
                !this.RenderIfEmpty)
                return;

            if (!String.IsNullOrEmpty(HeaderHtml))
            {
                string tmpHeaderHtml = MarkupExtensionProcessor.Instance.Process(HeaderHtml);
                writer.Write(tmpHeaderHtml);
            }

            base.Render(writer);

            if (!String.IsNullOrEmpty(FooterHtml))
            {
                string tmpFooterHtml = MarkupExtensionProcessor.Instance.Process(FooterHtml);
                writer.Write(tmpFooterHtml);
            }
        }

        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {

            List<TemplateSlotItem> visibleItems = new List<TemplateSlotItem>();
            visibleItems.AddRange(this.Controls.Cast<TemplateSlotItem>().Where(c => c.SnippetControl.Visible));

            //Currently I am assuming this will only contain template slot items
            foreach (TemplateSlotItem item in visibleItems)
            {

                //Cannot have the snippet info object to be null.
                if (item.SnippetControl != null && item.SnippetControl.SnippetInfo != null)
                {
                    //Additional Information is added to the class attribute  
                    //to help QA team to identify the slots.
                    item.CssClass += item.SnippetControl.SnippetInfo.SlotName
                        + item.SnippetControl.SnippetInfo.ContentID == null ? "" : "contentid-" + item.SnippetControl.SnippetInfo.ContentID;
                
                
                }
                //TODO: Else What??? - Handle this exception.  Log Error then Break??

                item.CssClass += " slot-item";

                if (visibleItems.Count == 1)
                {
                    item.CssClass += " only-SI";
                }
                else if (item == visibleItems[0])
                {
                    item.CssClass += " first-SI";
                }
                if (item == visibleItems[visibleItems.Count - 1] && visibleItems.Count != 1)
                {
                    item.CssClass += " last-SI";
                }

                if (!String.IsNullOrEmpty(this.AdditionalSnippetClasses))
                {
                    item.CssClass += " " + this.AdditionalSnippetClasses;
                }

                item.RenderControl(writer);

            }
        }
    }
}
