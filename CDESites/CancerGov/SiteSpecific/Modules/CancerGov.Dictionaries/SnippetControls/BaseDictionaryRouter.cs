﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

using NCI.Web;
using NCI.Web.CDE;
using NCI.Web.CDE.Modules;
using NCI.Web.CDE.UI;
using NCI.Util;

namespace CancerGov.Dictionaries.SnippetControls
{
    public abstract class BaseDictionaryRouter : SnippetControl
    {
        /// <summary>
        /// Gets or sets the PrettyUrl of the page this component lives on.
        /// </summary>
        protected string PrettyUrl { get; set; }

        /// <summary>
        /// Gets or sets the current Url as an NciUrl object.
        /// </summary>
        protected NciUrl CurrentUrl { get; set; }

        /// <summary>
        /// Gets or sets the Current AppPath, which is usually something like
        /// https://www.cancer.gov(PURL)(AppPath)
        /// (Where both PURL and AppPath start with /)
        /// </summary>
        protected string CurrAppPath
        {
            get
            {
                //Get the Current Application Path, e.g. if the URL is /foo/bar/results/chicken,
                //and the pretty URL is /foo/bar, then the CurrAppPath should be /results/chicken
                if (this.CurrentUrl.UriStem.Length == this.PrettyUrl.Length)
                    return "/";
                else
                    return this.CurrentUrl.UriStem.Substring(PrettyUrl.Length);
            }
        }

        /**
         * 
         * This section is the pipeline of events in order to render the trials.
         * 
         **/

        /// <summary>
        /// This sets up the Original Pretty URL, the current full URL and the app path
        /// </summary>
        private void SetupUrls()
        {
            //We want to use the PURL for this item.
            //NOTE: THIS 
            NciUrl purl = this.PageInstruction.GetUrl(PageAssemblyInstructionUrls.PrettyUrl);

            if (purl == null || string.IsNullOrWhiteSpace(purl.ToString()))
                throw new Exception("Dictionary requires current PageAssemblyInstruction to provide its PrettyURL through GetURL.  PrettyURL is null or empty.");

            //It is expected that this is pure and only the pretty URL of this page.
            //This means that any elements on the same page as this app should NOT overwrite the
            //PrettyURL URL Filter.
            this.PrettyUrl = purl.ToString();

            //Now, that we have the PrettyURL, let's figure out what the app paths are...
            NciUrl currURL = new NciUrl();
            currURL.SetUrl(HttpContext.Current.Request.RawUrl);
            currURL.SetUrl(currURL.UriStem);

            //Make sure this URL starts with the pretty url
            if (currURL.UriStem.ToLower().IndexOf(this.PrettyUrl.ToLower()) != 0)
                throw new Exception(String.Format("JSApplicationProxy: Cannot Determine App Path for Page, {0}, with PrettyURL {1}.", currURL.UriStem, PrettyUrl));

            this.CurrentUrl = currURL;
        }

        /// <summary>
        /// Method called to load dictionary page control
        /// </summary>
        protected abstract void LoadDictionaryControl(string controlType);

        /// <summary>
        /// Implement OnLoad Event to handle fetching of results.
        /// Prevents derrived classes from implementing.
        /// </summary>
        /// <param name="e"></param>
        sealed protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Step 1. Parse URL and pull out parameters
            this.SetupUrls();

            String searchString = Strings.Clean(Request.QueryString["q"]);
            String legacySearchString = Strings.Clean(Request.QueryString["search"]);
            String legacyTerm = Strings.Clean(Request.QueryString["term"]);
            String legacyCdrId = Strings.Clean(Request.QueryString["cdrid"]);
            String legacyId = Strings.Clean(Request.QueryString["id"]);
            // default results to 'A' if no term chosen
            String expand = Strings.Clean(Request.QueryString["expand"], "A");
            String language = Strings.Clean(Request.QueryString["language"]);

            if (!String.IsNullOrEmpty(legacyTerm))
            {
                searchString = legacyTerm;
            }

            // Load control depending on path
            // Path is either /search?q=<term> or /def/<term or code>
            List<string> route = this.CurrAppPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            if (route[0].Equals("search"))
            {
                // If path is /search, load the ResultsList control
                LoadDictionaryControl("search");
            }
            else if(route[0].Equals("def"))
            {
                // If path is /def, load DefinitionView control
                LoadDictionaryControl("view");
                            }
            else if(route.Count > 2)
            {
                // If path extends further than /search or /def/<term>, raise a 404 error
                NCI.Web.CDE.Application.ErrorPageDisplayer.RaisePageByCode("Dictionary", 400, "Invalid parameters for dictionary");
            }

            if (!String.IsNullOrEmpty(legacySearchString))
            {
                // redirect to new search URL using searchString as term
                NciUrl redirectURL = new NciUrl();
                redirectURL.SetUrl(this.PrettyUrl + "/search");
                redirectURL.QueryParameters.Add("q", searchString);
                DoPermanentRedirect(Response, redirectURL.ToString());
            }
            else if (!String.IsNullOrEmpty(legacyCdrId))
            {
                // redirect to new view URL using cdrId
                // check for friendly name
                NciUrl redirectURL = new NciUrl();
                redirectURL.SetUrl(this.PrettyUrl + "/def/" + legacyCdrId);
                DoPermanentRedirect(Response, redirectURL.ToString());
            }
            else if (!String.IsNullOrEmpty(legacyId))
            {
                // redirect to new view URL using cdrId or id
                // check for friendly name
                NciUrl redirectURL = new NciUrl();
                redirectURL.SetUrl(this.PrettyUrl + "/def/" + legacyId);
                DoPermanentRedirect(Response, redirectURL.ToString());
            }
            // TODO: legacy term???
            else if (!String.IsNullOrEmpty(expand))
            {
                LoadDictionaryControl("expand");
            }
            else
            {
                LoadDictionaryControl("home");
            }
        }

        /// <summary>
        /// Clears the Response text, issues an HTTP redirect using status 301, and ends
        /// the current request.
        /// </summary>
        /// <param name="Response">The current response object.</param>
        /// <param name="url">The redirection's target URL.</param>
        /// <remarks>Response.Redirect() issues its redirect with a 301 (temporarily moved) status code.
        /// We want these redirects to be permanent so search engines will link to the new
        /// location. Unfortunately, HttpResponse.RedirectPermanent() isn't implemented until
        /// at version 4.0 of the .NET Framework.</remarks>
        /// <exception cref="ThreadAbortException">Called when the redirect takes place and the current
        /// request is ended.</exception>
        protected void DoPermanentRedirect(HttpResponse Response, String url)
        {
            Response.Clear();
            Response.Status = "302 Found";
            Response.AddHeader("Location", url);
            Response.End();
        }
    }
}