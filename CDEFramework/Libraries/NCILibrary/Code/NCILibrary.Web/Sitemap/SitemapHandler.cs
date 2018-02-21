﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Configuration;
using Common.Logging;

namespace NCI.Web.Sitemap
{
    public class SitemapHandler : IHttpHandler
    {
        static ILog log = LogManager.GetLogger(typeof(SitemapHandler));


        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            response.ContentType = "application/xml";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            byte[] utf8;

            string sitemapName = GetSitemapName(request.FilePath);

            // Check if sitemap is already in the cache; if so, print the cached sitemap
            if (HttpContext.Current.Cache[sitemapName] != null)
            {
                utf8 = (byte[])HttpContext.Current.Cache[sitemapName];
                response.OutputStream.Write(utf8, 0, utf8.Length);
            }
            // Check if the exception is already in the cache; if so, go to error page
            else if (HttpContext.Current.Cache[sitemapName + "_ex"] != null)
            {
                response.Status = "500";
                response.End();
            }
            // If it isn't, get the current sitemap, save that in the cache, and output
            else
            {
                // Create a stopwatch object for timing of sitemap retrieval
                Stopwatch stopwatch = new Stopwatch();
                TimeSpan timeSpan;

                try
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, "http://www.sitemaps.org/schemas/sitemap/0.9");
                    XmlSerializer ser = new XmlSerializer(typeof(SitemapUrlSet), "http://www.sitemaps.org/schemas/sitemap/0.9");

                    using (MemoryStream memStream = new MemoryStream())
                    using (XmlWriter writer = XmlWriter.Create(memStream))
                    {
                        stopwatch.Start();
                        ser.Serialize(writer, Sitemaps.GetSitemap(sitemapName), ns);
                        utf8 = memStream.ToArray();
                        HttpContext.Current.Cache.Add(sitemapName, utf8, null, DateTime.Now.AddSeconds(5), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                        response.OutputStream.Write(utf8, 0, utf8.Length);
                        stopwatch.Stop();
                    }

                    // Execution timeout is 110±15 seconds, so we'll log an error if the process takes more than 90 seconds.
                    // This will allow us to track instances where sitemap generation is slow, even if we don't hit a ThreadAbortException.
                    // See https://blogs.msdn.microsoft.com/pedram/2007/10/02/how-the-execution-timeout-is-managed-in-asp-net/)
                    timeSpan = stopwatch.Elapsed;
                    if (timeSpan.TotalSeconds > 90)
                    {
                        log.Error("Warning: XML Sitemap " + sitemapName + " is taking longer than expected to retrieve. Check page and file instruction XML files.\nTime Elapsed for Sitemap " + sitemapName + " Retrieval: " + timeSpan.ToString());
                    }
                    else
                    {
                        log.Debug("Time Elapsed for Sitemap " + sitemapName + " Retrieval: " + timeSpan.ToString());
                    }
                }
                // Save the exception in the cache and send an error email
                catch (Exception ex)
                {
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                    }
                    timeSpan = stopwatch.Elapsed;

                    HttpContext.Current.Cache.Add(sitemapName + "_ex", ex, null, DateTime.Now.AddSeconds(5), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                    log.Fatal("Error generating Sitemap " + sitemapName + ". Check page and file instruction XML files. \nEnvironment: " + System.Environment.MachineName + "\nRequest Host: " + HttpContext.Current.Request.Url.Host
                              + "\nTime Elapsed for Sitemap " + sitemapName + " Retrieval: " + timeSpan.ToString() + " \nSitemapHandler.cs:ProcessRequest()", ex);
                    response.Status = "500";
                    response.End();
                }
            }
        }

        private string GetSitemapName(string requestFilepath)
        {
            // check if filepath matches a sitemap given in the web.config
            string filepath = requestFilepath.Split('/').Last();

            SitemapIndexSection section = (SitemapIndexSection)ConfigurationManager.GetSection("SitemapIndex");
            SitemapIndexProviderConfiguration config = section.Sitemaps;

            foreach (SitemapProviderConfiguration element in config)
            {
                if (String.Equals(element.Name.ToLower(), filepath.ToLower()))
                {
                    string sitemapName = filepath.Replace(".xml", "");
                    return sitemapName;
                }
            }

            // throw error if filepath isn't defined in web config
            return null;
        }

        #endregion
    }
}