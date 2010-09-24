﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using NCI.Web.CDE.Modules;
using NCI.DataManager;
using NCI.Web.UI.WebControls;
using NCI.Web.CDE.UI.Configuration;

namespace NCI.Web.CDE.UI.SnippetControls
{
    /// <summary>
    /// This class is the base implemenation for Dynamic Search & display control and Content Search & 
    /// display control.
    /// </summary>
    public abstract class BaseSearchSnippet : SnippetControl
    {
        #region Private Members
        SearchList _searchList = null;

        /// <summary>
        /// The current page that is being used.
        /// </summary>
        private int CurrentPage
        {
            get
            {
                if (string.IsNullOrEmpty(this.Page.Request.Params["page"]))
                    return 1;
                return Int32.Parse(this.Page.Request.Params["page"]);
            }
        }
        #endregion

        #region Protected
        /// <summary>
        /// Keyword is a search criteria used in searching
        /// </summary>
        protected string KeyWords
        {
            get
            {
                return string.IsNullOrEmpty(this.Page.Request.Params["KeyWords"]) ? this.SearchList.SearchParameters.Keyword : this.Page.Request.Params["KeyWords"];
            }
        }

        /// <summary>
        /// Startdate is a search criteria used in searching.if 
        /// StartDate value is present then both StartDate and 
        /// EndDate value should exist.
        /// </summary>
        protected DateTime StartDate
        {
            get
            {
                if (string.IsNullOrEmpty(this.Page.Request.Params["startdate"]))
                    return DateTime.Parse(this.SearchList.SearchParameters.StartDate);
                return DateTime.Parse(this.Page.Request.Params["startdate"]);
            }
        }

        /// <summary>
        /// Enddate is a search criteria used in searching, if 
        /// StartDate value is present then both StartDate and 
        /// EndDate value should exist.
        /// </summary>
        protected DateTime EndDate
        {
            get
            {
                if (string.IsNullOrEmpty(this.Page.Request.Params["enddate"]))
                    return DateTime.Parse(this.SearchList.SearchParameters.EndDate);
                return DateTime.Parse(this.Page.Request.Params["enddate"]);
            }
        }

        protected virtual SearchList SearchList
        {get;set;}

        #endregion

        #region Public
        public void Page_Load(object sender, EventArgs e)
        {
            processData();
        }
        
        #endregion

        #region Private Methods
        private void processData()
        {
            try
            {
                if (this.SearchList != null)
                {
                    Validate();

                    int actualMaxResult = this.SearchList.MaxResults;
                    // Call the  datamanger to perform the search
                    ICollection<SearchResult> searchResults =
                                SearchDataManager.Execute(CurrentPage, StartDate, EndDate, KeyWords,
                                    this.SearchList.RecordsPerPage, this.SearchList.MaxResults, this.SearchList.SearchFilter,
                                    this.SearchList.ExcludeSearchFilter, this.SearchList.ResultsSortOrder, this.SearchList.Language, Settings.IsLive , out actualMaxResult);

                    DynamicSearch dynamicSearch = new DynamicSearch();
                    dynamicSearch.Results = searchResults;
                    dynamicSearch.StartDate = String.Format("{0:MM/dd/yyyy}", StartDate);
                    dynamicSearch.EndDate = String.Format("{0:MM/dd/yyyy}", EndDate);
                    dynamicSearch.KeyWord = KeyWords;

                    if (CurrentPage > 1)
                        dynamicSearch.StartCount = this.SearchList.RecordsPerPage * CurrentPage - 1;
                    else
                        dynamicSearch.StartCount = 1;

                    if (CurrentPage == 1)
                    {
                        dynamicSearch.EndCount = this.SearchList.RecordsPerPage;
                        if (searchResults.Count < this.SearchList.RecordsPerPage)
                            dynamicSearch.EndCount = actualMaxResult;
                    }
                    else
                    {
                        dynamicSearch.EndCount = dynamicSearch.StartCount + this.SearchList.RecordsPerPage - 1;
                        if (searchResults.Count < this.SearchList.RecordsPerPage)
                            dynamicSearch.EndCount = actualMaxResult;
                    }

                    int recCount = 0;
                    foreach (SearchResult sr in searchResults)
                        sr.RecNumber = dynamicSearch.StartCount + recCount++;

                    dynamicSearch.ResultCount = actualMaxResult;

                    LiteralControl ltl = new LiteralControl(VelocityTemplate.MergeTemplateWithResults(this.SearchList.ResultsTemplate, dynamicSearch));
                    Controls.Add(ltl);
                    SetupPager(this.SearchList.RecordsPerPage, actualMaxResult);
                }
            }
            catch (Exception ex)
            {
                NCI.Logging.Logger.LogError("this.SearchListSnippet:processData", NCI.Logging.NCIErrorLevel.Error, ex);
            }
        }
        /// <summary>
        /// Helper method to setup the pager
        /// </summary>
        private void SetupPager(int recordsPerPage, int totalRecordCount)
        {
            SimplePager pager = new SimplePager();
            pager.RecordCount = totalRecordCount;
            pager.RecordsPerPage = recordsPerPage;
            pager.CurrentPage = CurrentPage;
            pager.PageParamName = "page";
            pager.PagerStyleSettings.SelectedIndexCssClass = "pager-SelectedPage";
            pager.BaseUrl = PageInstruction.GetUrl(PageAssemblyInstructionUrls.PrettyUrl).ToString();
            Controls.Add(pager);
        }
        /// <summary>
        /// Validates the data received from the xml, throws an exception if the required 
        /// fields are null or empty.
        /// </summary>
        /// <param name="this.SearchList">The object whose properties are being validated.</param>
        private void Validate()
        {
            if (string.IsNullOrEmpty(this.SearchList.SearchFilter) ||
                string.IsNullOrEmpty(this.SearchList.ResultsTemplate) ||
                string.IsNullOrEmpty(this.SearchList.SearchType))
                throw new Exception("One or more of these fields SearchFilter,ResultsTemplate,SearchType cannot be empty, correct the xml data.");

            if (this.SearchList.SearchParameters == null ||
                (string.IsNullOrEmpty(this.SearchList.SearchParameters.Keyword) &&
                    (string.IsNullOrEmpty(this.SearchList.SearchParameters.StartDate) ||
                string.IsNullOrEmpty(this.SearchList.SearchParameters.EndDate))))
            {
                throw new Exception("SearchParameters.Keyword,SearchParameters.StartDate,SearchParameters.EndDate cannot be empty, correct the xml data.");
            }
        }
        #endregion

    }
}
