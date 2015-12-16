﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;

using NCI.Search.Configuration;
using NCI.Search;
using NCI.Logging;



namespace NCI.Search
{
    /// <summary>
    /// 
    /// </summary>
    public class ESSiteWideSearchProvider : NCI.Search.SiteWideSearchProviderBase
    {
        static Log log = new Log(typeof(ESSiteWideSearchProvider));

        /// <summary>
        /// Gets the search results from this SiteWideSearch provider.
        /// </summary>
        /// <param name="searchCollection">The search collection.</param>
        /// <param name="term">The term.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public override ISiteWideSearchResultCollection GetSearchResults(string searchCollection, string term, int pageSize, int offset)
        {

            ESSiteWideSearchCollectionElement searchCollConfig = ElasticSearchConfig.GetSearchCollectionConfig(searchCollection);

            var connectionPool = new SniffingConnectionPool(searchCollConfig.ClusterNodes);
            var config = new ConnectionConfiguration(connectionPool);
            var esClient = new ElasticsearchClient(config);

            string[] fieldList = ElasticSearchConfig.GetFields(searchCollection);

            //do search_template 
            var _body = new
            {
                template = new
                {
                    id = searchCollConfig.Template
                },
                @params = new
                {
                    my_value = term,
                    my_size = pageSize.ToString(),
                    my_from = offset.ToString(),
                    my_site = searchCollConfig.Site,
                    my_fields = fieldList
                }
            };

            ElasticsearchResponse<DynamicDictionary> results = esClient.SearchTemplate(searchCollConfig.Index, _body);

            var stringResponse = results.Response;
            List<ESSiteWideSearchResult> foundTerms = new List<ESSiteWideSearchResult>(pageSize);

            try
            {


                foreach (var hit in results.Response["hits"].hits)
                {

                    string title = "";
                    try
                    {
                        if (hit.fields.title != null)
                        {
                            title = hit.fields.title[0];
                            if (title.Trim().Length == 0)
                            {
                                title = "Untitled";
                            }
                        }
                        else 
                        { 
                            title = "Untitled"; 
                        }
                    }

                    catch (KeyNotFoundException keynotfound)
                    {
                        title = "";
                    }

                    string url = "";
                    try
                    {
                        if (hit.fields.url != null)
                        {
                            url = hit.fields.url[0];
                        }
                    }

                    catch (KeyNotFoundException keynotfound)
                    {
                        url = "";
                    }

                    string description = "";
                    try
                    {
                        if (hit.fields["metatag.description"][0] != null)
                        {
                            description = hit.fields["metatag.description"][0];
                        }
                    }

                    catch (KeyNotFoundException keynotfound)
                    {
                        description = "";
                        
                    }
                    foundTerms.Add(new ESSiteWideSearchResult(title, url, description));


                }


            }

            catch (Exception ex)
            {
                log.error("Error retrieving search results.", ex);
                throw ex;
                
            }


            //Return the results
            return new ESSiteWideSearchResultCollection(foundTerms.AsEnumerable())
            {
                ResultCount = results.Response["hits"].total
            };
        }
    }
}
