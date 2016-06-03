﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;

using Nest;

using NCI.Search;

namespace CancerGov.ClinicalTrials.Basic
{
    public class BasicCTSManager
    {

        private string _clusterName = "";
        private string _trialIndexType = "";
        private string _menuTermIndexType = "";
        private string _geoLocIndexType = "";
        private string _indexName = "";

        public BasicCTSManager(
            string indexName, 
            string trialIndexType,
            string menuTermIndexType,
            string geoLocIndexType, 
            string clusterName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException("indexName cannot be null or empty");

            if (string.IsNullOrWhiteSpace(trialIndexType))
                throw new ArgumentNullException("trialIndexType cannot be null or empty");

            if (string.IsNullOrWhiteSpace(geoLocIndexType))
                throw new ArgumentNullException("geoLocIndexType cannot be null or empty");

            if (string.IsNullOrWhiteSpace(menuTermIndexType))
                throw new ArgumentNullException("menuTermIndexType cannot be null or empty");

            if (string.IsNullOrWhiteSpace(clusterName))
                throw new ArgumentNullException("clusterName cannot be null or empty");

            this._indexName = indexName;
            this._trialIndexType = trialIndexType;
            this._geoLocIndexType = geoLocIndexType;
            this._menuTermIndexType = menuTermIndexType;
            this._clusterName = clusterName;            
        }

        /// <summary>
        /// Gets a trial by the trials NCTID
        /// </summary>
        /// <param name="nctID">The CT.gov ID</param>
        /// <returns></returns>
        public TrialDescription Get(string nctID)
        {
            ElasticClient client = GetESConnection();

            //Using the GenericVersion will magically map the JSON to a strongly typed object.
            var response = client.Get<TrialDescription>(g => g
                .Index(_indexName)
                .Type(_trialIndexType)
                .Id(nctID)
            );

            return response.Source;
        }

        /// <summary>
        /// Gets the Geo Location for a ZipCode
        /// </summary>
        /// <param name="zipCode"></param>
        /// <returns></returns>
        public ZipLookup GetZipLookupForZip(string zipCode)
        {
            ElasticClient client = GetESConnection();

            var response = client.Get<ZipLookup>(g => g
                .Index(_indexName)
                .Type(_geoLocIndexType)
                .Id(zipCode)
            );

            return response.Source;
        }

        /// <summary>
        /// Searches for Trials given a set of parameters
        /// </summary>
        /// <param name="searchParams">The Search Parameters to use</param>
        /// <returns></returns>
        public TrialSearchResults Search(BaseCTSSearchParam searchParams)
        {
            ElasticClient client = GetESConnection();

            var response = client.Search<TrialSearchResult>(sd => 
                    searchParams.ModifySearchParams<TrialSearchResult>(
                        sd
                            .Index(_indexName)
                            .Type(_trialIndexType)
                    )
                );

            // If no results / error / etc then raise error

            return new TrialSearchResults(
                response.Total,
                response.Documents
            );
        }

        /// <summary>
        /// Searches for Trials given a set of parameters using a search template.
        /// </summary>
        /// <param name="searchParams">The Search Parameters to use</param>
        /// <returns></returns>
        public TrialSearchResults SearchTemplate(BaseCTSSearchParam searchParams)
        {
            ElasticClient client = GetESConnection();

            var response = client.SearchTemplate<TrialSearchResult>(sd => {
                sd = sd
                    .Index(_indexName)
                    .Type(_trialIndexType);

                sd = searchParams.SetSearchParams<TrialSearchResult>(sd);

                return sd;
            });

            // If no results / error / etc then raise error

            return new TrialSearchResults(
                response.Total,
                response.Documents
            );
        }

        private ElasticClient GetESConnection()
        {
            //Get The Cluster configuration
            //TODO: Make the cluster name a configurable item
            Uri[] clusterNodes = ElasticSearchConfig.GetClusterNodes(_clusterName);
            NCI.Search.Configuration.ClusterElement clusterConfig = ElasticSearchConfig.GetCluster(_clusterName);

            var connectionPool = new SniffingConnectionPool(clusterNodes);
            var config = new ConnectionSettings(connectionPool)
                            .UsePrettyResponses()
                            .MaximumRetries(clusterConfig.MaximumRetries)//try 5 times if the server is down
                            .ExposeRawResponse()
                            .ThrowOnElasticsearchServerExceptions();
            return new ElasticClient(config);

        }
    }
}