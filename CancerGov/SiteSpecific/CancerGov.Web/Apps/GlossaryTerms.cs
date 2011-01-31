﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NCI.Web.CDE;
using NCI.Web.CDE.UI;
using CancerGov.Common.Extraction;
using System.Collections;
using NCI.Web.CDE.UI.SnippetControls;
using NCI.Web.Extensions;

namespace NCI.Web.CancerGov.Apps
{
    public class GlossaryTerms : AppsBaseUserControl
    {
        #region Control Members
        protected Literal ltGlossaryTerms;
        #endregion
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            string language = string.Empty;
            string snippetXmlData = string.Empty;
            string linksTableTitle;
            string glossaryTerms = "";

            if (PageAssemblyContext.Current.PageAssemblyInstruction.Language == "en")
            {
                language = "English";
                linksTableTitle = "Glossary Terms";
            }
            else
            {
                language = "Spanish";
                linksTableTitle = "Glosario";
            }

            GlossaryTermExtractor gte = new GlossaryTermExtractor();

            string data = string.Empty;

            foreach (GenericHtmlContentSnippet slot in Page.FindControlByType<GenericHtmlContentSnippet>())
            {
                if (slot.SnippetInfo.SlotName != "cgvSiteBannerPrint" && slot.SnippetInfo.SlotName != "cgvContentHeader" && slot.SnippetInfo.SlotName != "cgvSiteBanner" && slot.SnippetInfo.SlotName != "cgvLanguageDate" && slot.SnippetInfo.SlotName != "cgvBodyHeader")
                {
                    data = slot.SnippetInfo.Data;
                    data = gte.ExtractGlossaryTerms(data);
                    slot.SnippetInfo.Data = data;
                    glossaryTerms = glossaryTerms + gte.BuildGlossaryTable(linksTableTitle);
                    glossaryTerms= glossaryTerms.Replace("<table border=0 width=699 cellspacing=0 cellpadding=0><tr><td>\n<BR><BR><a name=\"Glossary Terms\"></a><h2>Glossary Terms</h2>\n", "").Replace("</td></tr></table>", "");

                }
            }
            glossaryTerms = "<table border=0 width=699 cellspacing=0 cellpadding=0><tr><td><BR><BR><a name=\"Glossary Terms\"></a><h2>Glossary Terms</h2>" + glossaryTerms + "</td></tr></table>";

            LiteralControl lit = new LiteralControl(glossaryTerms);
            this.Controls.Add(lit);

        }
    }
}
