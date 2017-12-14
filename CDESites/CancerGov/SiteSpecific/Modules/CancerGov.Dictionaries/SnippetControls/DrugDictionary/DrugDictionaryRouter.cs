﻿using System;
using System.Web.UI;
using NCI.Util;
using NCI.Web.CDE.UI;

namespace CancerGov.Dictionaries.SnippetControls
{
    public class DrugDictionaryRouter : BaseDictionaryRouter
    {
        protected Control localControl;

        protected override Control LoadHomeControl()
        {
            localControl = Page.LoadControl("~/SnippetTemplates/DrugDictionary/Views/DrugDictionaryHome.ascx");
            return localControl;
        }

        protected override Control LoadResultsListControl()
        {
            localControl = Page.LoadControl("~/SnippetTemplates/DrugDictionary/Views/DrugDictionaryResultsList.ascx");
            return localControl;
        }

        protected override Control LoadDefinitionViewControl()
        {
            localControl = Page.LoadControl("~/SnippetTemplates/DrugDictionary/Views/DrugDictionaryDefinitionView.ascx");
            return localControl;
        }
    }
}