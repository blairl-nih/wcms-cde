﻿using System;
using System.Web;
using System.Web.SessionState;
using System.Globalization;
using System.Configuration;
using System.IO;
using NCI.Web.CDE.InformationRequest;
using NCI.Web.CDE.Configuration;
using NCI.Web.CDE.CapabilitiesDetection;
using NCI.Logging;
using System.Reflection;

namespace NCI.Web.CDE
{
    public class MobileRedirect : IHttpModule
    {
        private static readonly object REQUEST_URL_KEY = new object();
        private static readonly string _cookieName =
            System.Configuration.ConfigurationManager.AppSettings["MobileRedirectCookieName"];
        
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>

        public void Dispose()
        {
            //clean-up code here.
        }
        /// <summary>
        /// Instantiated  event OnBeginRequest
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(OnBeginRequest);

        }

        void OnBeginRequest(object sender, EventArgs e)
        {
            bool doRedirect = false;
            HttpContext context = ((HttpApplication)sender).Context;

            // if page assembly instructions are loaded
            if (PageAssemblyContext.Current.PageAssemblyInstruction != null) 
            {   
                
                // if mobile device and mobile url exist, redirect to it
                if ((DisplayDeviceDetector.DisplayDevice == DisplayDevices.BasicMobile) ||
                    (DisplayDeviceDetector.DisplayDevice == DisplayDevices.AdvancedMobile))
                {

                    string[] alternateContentVersionsKeys;
                    //NciUrl mUrl = PageAssemblyContext.Current.PageAssemblyInstruction.GetUrl("mobileurl");

                    alternateContentVersionsKeys = PageAssemblyContext.Current.PageAssemblyInstruction.AlternateContentVersionsKeys;

                    if (Array.Find(alternateContentVersionsKeys,
                                    element => element.StartsWith("mobileurl", StringComparison.Ordinal)) == "mobileurl")
                    {
                        HttpCookie mobileRedirectCookie = context.Request.Cookies[_cookieName];

                        if (mobileRedirectCookie != null)
                        {
                            if (mobileRedirectCookie.Value != "true")
                                doRedirect = true;
                        }
                        else 
                            doRedirect = true;

                        if(doRedirect)
                        {
                            //drop a cookie so mobile redirect happens only once per session
                            HttpCookie responseCookie = new HttpCookie(_cookieName,"true");
                            responseCookie.Expires = DateTime.MinValue; // Make this a session cookie
                            responseCookie.Domain = "cancer.gov";
                            context.Response.Cookies.Add(responseCookie); 
                            
                            NciUrl mobileUrl = PageAssemblyContext.Current.PageAssemblyInstruction.GetUrl("mobileurl");
                            // We are adding query parameter to the mobile URL for the page to accommodate 
                            // the dictionary of cancer terms - so a link to a term dictionary definition 
                            // actually goes to the mobile dictionary definition, not just the dictionary home.
                            string redirectUrl = InformationRequestConfig.MobileHost + mobileUrl.ToString() + context.Request.Url.Query;
                            context.Response.Redirect(redirectUrl, true);
                        }
                    }
                }
            }
        }


    }
}