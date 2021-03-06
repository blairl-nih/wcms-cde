﻿using System;
using System.Web.UI;
using NCI.Util;
using NCI.Web.CDE.UI;

namespace CancerGov.Dictionaries.SnippetControls
{
    public class GeneticsTermDictionaryRouter : BaseDictionaryRouter
    {
        protected BaseDictionaryControl localControl;

        protected override BaseDictionaryControl LoadExpandListControl()
        {
            localControl = (BaseDictionaryControl)Page.LoadControl("~/SnippetTemplates/GeneticsTermDictionary/Views/GeneticsTermDictionaryExpandList.ascx");
            return localControl;
        }

        protected override BaseDictionaryControl LoadResultsListControl()
        {
            localControl = (BaseDictionaryControl)Page.LoadControl("~/SnippetTemplates/GeneticsTermDictionary/Views/GeneticsTermDictionaryResultsList.ascx");
            return localControl;
        }

        protected override BaseDictionaryControl LoadDefinitionViewControl()
        {
            localControl = (BaseDictionaryControl)Page.LoadControl("~/SnippetTemplates/GeneticsTermDictionary/Views/GeneticsTermDictionaryDefinitionView.ascx");
            return localControl;
        }
    }
}