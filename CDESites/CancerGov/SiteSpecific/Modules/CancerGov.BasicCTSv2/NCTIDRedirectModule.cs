using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Common.Logging;

using CancerGov.ClinicalTrialsAPI;
using CancerGov.ClinicalTrials.Basic.v2.Configuration;

using NCI.Web.CDE;

namespace CancerGov.ClinicalTrials.Basic.v2
{
    /// <summary>
    /// Handles redirection logic for Clinical Trial View pretty URLs.
    /// </summary>
    public class NCTIDRedirectModule : IHttpModule
    {
        /// <summary>Set logging for this class.</summary>
        static ILog log = LogManager.GetLogger(typeof(NCTIDRedirectModule));

        /// <summary>
        /// Get the the path for the Clinical Trials View page from Web.config
        /// </summary>
        private string SearchResultsPrettyUrl
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ClinicalTrialsViewPage"]))
                {
                    return ConfigurationManager.AppSettings["ClinicalTrialsViewPage"];
                }
                return "/about-cancer/treatment/clinical-trials/search/v";
            }
        }


        #region IHttpModule Members

        /// <summary>
        /// Performs any final cleanup work prior to removal of the module from the execution pipeline.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initialize the module. 
        /// </summary>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(OnBeginRequest);
        }

        /// <summary>
        /// Main chain of events in the module execution.
        /// </summary>
        void OnBeginRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            // Get absolute path of the request URL as pretty URL
            String url = context.Server.UrlDecode(context.Request.Url.AbsolutePath);

            // Don't proceed if this is a file.
            // TODO: Make this a configuration setting and reuse (future release)
            if (url.IndexOf(".css") != -1 || url.IndexOf(".gif") != -1 || url.IndexOf(".jpg") != -1 || url.IndexOf(".js") != -1 || url.IndexOf(".axd") != -1)
            {
                return;
            }

            // Check if the PageAssemblyInstruction is not null. If not null then it was already processed as pretty url.
            if (PageAssemblyContext.Current.PageAssemblyInstruction != null)
            {
                return;
            }

            // The URL should match this pattern: '<hostname>/clinicaltrials/<NCTID>. If it does, proceed with retrieving the ID
            // We're only concerned about the NCT ID at this point - not NCI, CDR, or any other trial IDs
            if (context.Request.Url.Segments.Count() >= 3 && context.Request.Url.Segments[1].ToLower() == "clinicaltrials/")
            {
                string id = string.Empty;
                id = context.Request.Url.Segments[2]; // Get the third segment of the URL
                id = id.Replace("/", "");

                // If we have an ID, clean and proceed with redirect logic
                if (!string.IsNullOrWhiteSpace(id))
                {
                    string cleanId = id.Trim();

                    // Do behavior based on the given ID
                    try
                    { 
                        // If the ID matches a trial in the API, go to the view page on www.cancer.gov
                        if (!string.IsNullOrWhiteSpace(cleanId) && IsValidTrial(cleanId))
                        {
                            //In addition to the id param, add the "r" redirect flag
                            string ctViewUrl = string.Format(SearchResultsPrettyUrl + "?id={0}&r=", cleanId.ToUpper());
                            context.Response.Redirect(ctViewUrl, true);
                        }

                        // If there is no matching trial, but it's a valid NCT ID, redirect to clinicaltrials.gov
                        // CTGov URL format is "https://clinicaltrials.gov/show/<NCT_ID>"
                        else if (IsNctID(cleanId))
                        {
                            log.DebugFormat("NCT ID {0} not found in API.", cleanId);
                            String nlmUrl = String.Format("https://clinicaltrials.gov/show/{0}", cleanId.ToUpper());
                            log.DebugFormat("Redirecting to {0}", nlmUrl);
                            context.Response.Redirect(nlmUrl, true);
                        }

                        // If it's not a valid NCT ID, don't do anything; this will result in a Page Not Found.
                        else
                        {
                            log.DebugFormat("NCT ID {0} not found in API and is not formatted correctly for clinicaltrials.cancer.gov", cleanId);
                        }
                    }
                    catch (ThreadAbortException ex)
                    {
                        // Response.Redirect() throws a ThreadAbortException. This is normal behavior.
                        // Hide the "normal error" by swallowing the exception.
                        // TODO: is this still the case? If not, remove this catch block
                        log.Debug("ThreadAbortException thrown by Response.Redirect()");
                    }
                    catch (Exception ex)
                    {
                        log.Error("NCTIDRedirectModule:OnBeginRequest()", ex);
                    }
                }
                else
                {
                    log.Debug("ID is null or empty");
                }
            }

        }
        #endregion

        /// <summary>
        /// Determines whether the value contained by a string is an NCT ID.
        /// </summary>
        /// <param name="IDString">NCT ID string</param>
        /// <returns>True if matches NCTID pattern</returns>
        private bool IsNctID(string idString)
        {
            // Per http://www.nlm.nih.gov/bsd/policy/clin_trials.html, 
            // "The format for the ClinicalTrials.gov registry number is "NCT" followed by an 8-digit number, e.g.: NCT00000419."
            bool isAMatch = false;

            if (!string.IsNullOrWhiteSpace(idString))
            {
                isAMatch = Regex.IsMatch(idString.Trim(), "^NCT[0-9]{1,8}$", RegexOptions.IgnoreCase);
            }

            return isAMatch;
        }

        /// <summary>
        /// Checks whether the given trial ID exists in the API.
        /// </summary>
        /// <param name="idString">NCT ID</param>
        /// <returns>True if trial is found in API.</returns>
        private bool IsValidTrial(string idString)
        {
            // If the ID is a valid NCTID, go to web service and see if trial exists
            try
            {
                String host = BasicClinicalTrialSearchAPISection.GetAPIUrl();
                ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(host);
                ClinicalTrial trial = client.Get(idString);
                if (trial != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error retrieving trial object from API", ex);
                return false;
            }
            return false;
        }

    }
}
