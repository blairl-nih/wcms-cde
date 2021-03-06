﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Common.Logging;
using NCI.Services.Dictionary.BusinessObjects;
using NCI.Util;

namespace NCI.Services.Dictionary
{
    /// <summary>
    /// Infrastructure for the dictionary service.
    /// Implments the business logic of retrieving dictionary data.
    /// </summary>
    internal class DictionaryManager
    {
        static ILog log = Common.Logging.LogManager.GetLogger(typeof(DictionaryManager));

         // Where are the Audio and Image files found? (JSON will contain placeholders.)
        private String _audioFileLocation;
        private String _imageFileLocation;

        const string IMAGE_PLACEHOLDER_TEXT = "[__imagelocation]";
        const string AUDIO_PLACEHOLDER_TEXT = "[__audiolocation]";

        const char LIST_DELIMITER = '|';

        /// <summary>
        /// Initialization
        /// </summary>
        public DictionaryManager()
        {
            // Prevent null value.
            _imageFileLocation = ConfigurationManager.AppSettings["CDRImageLocation"] ?? String.Empty;
            _imageFileLocation = _imageFileLocation.Trim();
            if (String.IsNullOrEmpty(_imageFileLocation))
                log.Error("appSetting value 'CDRImageLocation' not set.");
            // Make sure the path ends with a slash.
            else if (_imageFileLocation[_imageFileLocation.Length - 1] != '/')
                _imageFileLocation += '/';

            // Prevent null value.
            _audioFileLocation = ConfigurationManager.AppSettings["CDRAudioMediaLocation"] ?? String.Empty;
            _audioFileLocation = _audioFileLocation.Trim();
            if(String.IsNullOrEmpty(_audioFileLocation))
                log.Error("appSetting value 'CDRAudioMediaLocation' not set.");
            // Make sure the path ends with a slash.
            else if (_audioFileLocation[_audioFileLocation.Length - 1] != '/')
                _audioFileLocation += '/';
        }


        /// <summary>
        /// Retrieves a single dictionary term based on its specific Term ID.
        /// </summary>
        /// <param name="termId">The ID of the Term to be retrieved</param>
        /// <param name="dictionary">The dictionary to retreive the Term from.
        ///     Valid values are
        ///        Term - Dictionary of Cancer Terms
        ///        drug - Drug Dictionary
        ///        genetic - Dictionary of Genetics Terms
        /// </param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns>A data structure containing both meta data about the request and a string containing a JSON representation
        /// of the particular definition identified by the inputs to the method.
        /// </returns>
        [Obsolete("Use the five-argument GetTerm method instead.")]
        public TermReturn GetTerm(int termId, DictionaryType dictionary, Language language, String version)
        {
            log.DebugFormat("Enter GetTerm( {0}, {1}, {2}).", termId, dictionary, language, version);

            #region Argument Validation

            if (termId <= 0)
            {
                string msg = string.Format("termId - expected a positive value, found '{0}'.", termId);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(DictionaryType), dictionary) || dictionary == DictionaryType.Unknown)
            {
                string msg = string.Format("dictionary contains invalid value '{0}'.", dictionary);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(Language), language) || language == Language.Unknown)
            {
                string msg = string.Format("language contains invalid value '{0}'.", language);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (string.IsNullOrEmpty(version))
            {
                string msg = "version is null or empty.";
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            #endregion

            // In the initial implementation, the audience is implied by the particular dictionary being used.
            AudienceType audience = GetDefaultAudienceFromDictionaryType(dictionary);

            return GetTerm(termId, dictionary, language, audience, version);
        }

        /// <summary>
        /// Retrieves a single dictionary term based on its specific Term ID.
        /// </summary>
        /// <param name="termId">The ID of the Term to be retrieved</param>
        /// <param name="dictionary">The dictionary to retreive the Term from.
        ///     Valid values are
        ///        Term - Dictionary of Cancer Terms
        ///        drug - Drug Dictionary
        ///        genetic - Dictionary of Genetics Terms
        /// </param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="audience">The Term's desired audience.
        ///     Supported values are:
        ///         Patient
        ///         HealthProfessional
        /// </param>
        ///<param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns>A data structure containing both meta data about the request and a string containing a JSON representation
        /// of the particular definition identified by the inputs to the method.
        /// </returns>
        public TermReturn GetTerm(int termId, DictionaryType dictionary, Language language, AudienceType audience, String version)
        {
            log.DebugFormat("Enter GetTerm( {0}, {1}, {2}, {4}).", termId, dictionary, language, version, audience);

            #region Argument Validation

            if (termId <= 0)
            {
                string msg = string.Format("termId - expected a positive value, found '{0}'.", termId);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(DictionaryType), dictionary) || dictionary == DictionaryType.Unknown)
            {
                string msg = string.Format("dictionary contains invalid value '{0}'.", dictionary);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(Language), language) || language == Language.Unknown)
            {
                string msg = string.Format("language contains invalid value '{0}'.", language);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(AudienceType), audience) || audience == AudienceType.Unknown)
            {
                string msg = string.Format("audience contains invalid value '{0}'.", audience);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (string.IsNullOrEmpty(version))
            {
                string msg = "version is null or empty.";
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            #endregion
                      
            DictionaryQuery query = new DictionaryQuery();
            DataTable dtTerm = query.GetTerm(termId, dictionary, language, audience, version);

            return GetTermCommon(dtTerm, termId, language, audience);
        }

        /// <summary>
        /// Retrieves a single dictionary term based on its specific Term ID.
        /// Similar, but not identical, to GetTerm().  Instead of retrieving the term for a specific
        /// dictionary, the term is fetched for a preferred audience.  If no records are available for that audience,
        /// then any other avaiable records are returned instead.
        /// </summary>
        /// <param name="termId">The ID of the Term to be retrieved</param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="audience">The Term's desired audience.
        ///     Supported values are:
        ///         Patient
        ///         HealthProfessional
        /// </param>
        ///<param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns>A data structure containing both meta data about the request and a string containing a JSON representation
        /// of the particular definition identified by the inputs to the method.
        /// </returns>
        [Obsolete("Use the five-argument GetTerm method instead.")]
        public TermReturn GetTermForAudience(int termId, Language language, AudienceType audience, String version)
        {
            log.DebugFormat("Enter GetTerm( {0}, {1}, {2}).", termId, language, version);

            #region Argument Validation

            if (termId <= 0)
            {
                string msg = string.Format("termId - expected a positive value, found '{0}'.", termId);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(Language), language) || language == Language.Unknown)
            {
                string msg = string.Format("language contains invalid value '{0}'.", language);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (!Enum.IsDefined(typeof(AudienceType), audience) || audience == AudienceType.Unknown)
            {
                string msg = string.Format("audience contains invalid value '{0}'.", audience);
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            if (string.IsNullOrEmpty(version))
            {
                string msg = "version is null or empty.";
                log.Error(msg);
                throw new ArgumentException(msg);
            }

            #endregion

            DictionaryQuery query = new DictionaryQuery();
            DataTable dtTerm = query.GetTermForAudience(termId, language, audience, version);

            return GetTermCommon(dtTerm, termId, language, audience);
        }

        /// <summary>
        /// Infrastructure for turning the DataTable returned for one of the GetTerm family of query
        /// methods into a TermReturn object.
        /// </summary>
        /// <param name="termId">The ID of the Term which was retrieved</param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="audience">The Term's desired audience.
        ///     Supported values are:
        ///         Patient
        ///         HealthProfessional
        /// </param>
        /// <returns>A data structure containing both meta data about the request and a string containing a JSON representation
        /// of the particular definition identified by the inputs to the method.
        /// </returns>
        private TermReturn GetTermCommon(DataTable dtTerm, int termId, Language language, AudienceType audience)
        {
            List<String> messages = new List<string>();

            String term = string.Empty;

            // Normal, found 1 match.
            int resultCount = dtTerm.Rows.Count;
            if (resultCount == 1)
            {
                log.Debug("Found 1 result.");
                messages.Add("OK");
                term = dtTerm.Rows[0].Field<string>("object");
                term = RewriteMediaFileLocations(term);
            }
            // "Normal", found no matches.
            else if (resultCount == 0)
            {
                log.Debug("Found 0 results.");
                messages.Add("No result found.");
                term = string.Empty;
            }
            // "Odd" case. With multiple matches, return the first one.
            else // result count must be greater than 1
            {
                log.WarnFormat("Expected to find one result for term {0}, found {1} instead.", termId, resultCount);
                messages.Add("OK");
                term = dtTerm.Rows[0].Field<string>("object");
                term = RewriteMediaFileLocations(term);
            }


            // Build up the return data structure.
            TermReturn trmReturn = new TermReturn();

            TermReturnMeta meta = new TermReturnMeta();
            meta.Language = language.ToString();
            meta.Audience = audience.ToString();
            meta.Messages = messages.ToArray();

            trmReturn.Meta = meta;
            trmReturn.Term = term;

            return trmReturn;
        }

        /// <summary>
        /// Performs a search for terms with names matching searchText.
        /// </summary>
        /// <param name="searchText">text to search for.</param>
        /// <param name="searchType">The type of search to perform.
        ///     Valid values are:
        ///         Begins - Search for terms beginning with searchText.
        ///         Contains - Search for terms containing searchText.
        ///         Magic - Search for terms beginning with searchText, followed by those containing searchText.
        /// </param>
        /// <param name="offset">Offset into the list of matches for the first result to return.</param>
        /// <param name="numResults">The maximum number of results to return. Must be at least 10.</param>
        /// <param name="dictionary">The dictionary to retreive the Term from.
        ///     Valid values are
        ///        Term - Dictionary of Cancer Terms
        ///        drug - Drug Dictionary
        ///        genetic - Dictionary of Genetics Terms
        /// </param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns></returns>
        public SearchReturn Search(String searchText, SearchType searchType, int offset, int numResults, DictionaryType dictionary, Language language, String version)
        {
            log.DebugFormat("Enter Search( {0}, {1}, {2}, {3}, {4}, {5}, {6}).", searchText, searchType, offset, 
                numResults, dictionary, language, version);

            // Sanity check for the offset and numResults
            if (offset < 0) offset = 0;
            if (numResults < 10) numResults = 200;


            // In the initial implementation, the audience is implied by the particular dictionary being used.
            AudienceType audience = GetDefaultAudienceFromDictionaryType(dictionary);

            DictionaryQuery query = new DictionaryQuery();
            SearchResults results = query.Search(searchText, searchType, offset, numResults, dictionary, language, audience, version);

            return BuildSearchResultsStructure(results, language, audience, offset);
        }

        /// <summary>
        /// Lightweight method to search for terms matching searchText. This method is intended for use with autosuggest
        /// and returns a maximum of 10 results
        /// </summary>
        /// <param name="searchText">text to search for.</param>
        /// <param name="searchType">The type of search to perform.
        ///     Valid values are:
        ///         Begins - Search for terms beginning with searchText.
        ///         Contains - Search for terms containing searchText.
        ///         Magic - Search for terms beginning with searchText, followed by those containing searchText.
        /// </param>
        /// <param name="numResults">Maximum number of results to return.</param>
        /// <param name="dictionary">The dictionary to retreive the Term from.
        ///     Valid values are
        ///        Term - Dictionary of Cancer Terms
        ///        drug - Drug Dictionary
        ///        genetic - Dictionary of Genetics Terms
        /// </param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns></returns>
        public SuggestReturn SearchSuggest(String searchText, SearchType searchType, int numResults, DictionaryType dictionary, Language language, String version)
        {
            log.DebugFormat("Enter ValidateSearchSuggest( {0}, {1}, {2}, {3}, {4}, {5}).", searchText, searchType, numResults, dictionary, language, version);

            // Sanity check for numResults
            if (numResults < 10) numResults = 10;

            // In the initial implementation, the audience is implied by the particular dictionary being used.
            AudienceType audience = GetDefaultAudienceFromDictionaryType(dictionary);

            DictionaryQuery query = new DictionaryQuery();
            SuggestionResults results = query.SearchSuggest(searchText, searchType, numResults, dictionary, language, audience, version);

            List<String> messages = new List<string>();

            int resultCount = results.MatchCount;

            // Report the count in a human-readable format.
            String message = String.Format("Found {0} results.", resultCount);
            log.Debug(message);
            messages.Add(message);

            // Retrieve results.  We already know the number of results, so let's preset the
            // list to the size we know we're going to need.
            List<DictionarySuggestion> foundTerms = new List<DictionarySuggestion>(resultCount);
            foreach (DataRow row in results.Data.Rows)
            {
                int ID = row.Field<int>("TermID");
                string term = row.Field<String>("TermName");
                DictionarySuggestion suggestion = new DictionarySuggestion(ID, term);
                foundTerms.Add(suggestion);
            }

            // Populate return metadata structure
            SuggestReturnMeta meta = new SuggestReturnMeta()
            {
                ResultCount = resultCount,
                Messages = messages.ToArray()
            };

            // Combine meta and results to create the final return object.
            SuggestReturn suggestReturn = new SuggestReturn()
            {
                Result = foundTerms.ToArray(),
                Meta = meta
            };

            return suggestReturn;

        }

        /// <summary>
        /// Perform a search for terms with names or aliases that start with searchText, sorted by the  matched term name or alias.
        /// </summary>
        /// <param name="searchText">text to search for.</param>
        /// <param name="includeTypes">A filter for the types of name aliases to include.  Multiple values are separated by the pipe character (|).
        /// If no filter is supplied, the result </param>
        /// <param name="offset">Offset into the list of matches for the first result to return.</param>
        /// <param name="numResults">The maximum number of results to return. Must be at least 10.</param>
        /// <param name="dictionary">The dictionary to retreive the Term from.
        ///     Valid values are
        ///        Term - Dictionary of Cancer Terms
        ///        drug - Drug Dictionary
        ///        genetic - Dictionary of Genetics Terms
        /// </param>
        /// <param name="language">The Term's desired language.
        ///     Supported values are:
        ///         en - English
        ///         es - Spanish
        /// </param>
        /// <param name="version">String identifying which vereion of the JSON structure to retrieve.</param>
        /// <returns>An object structure containing the results of the search and various metadata.</returns>
        public SearchReturn Expand(String searchText, String includeTypes, int offset, int numResults, DictionaryType dictionary, Language language, String version)
        {
            log.Debug("Enter ValidateSearchSuggest().");

            // Sanity check for the offset and numResults
            if (offset < 0) offset = 0;
            if (numResults < 10) numResults = 200;

            // In the initial implementation, the audience is implied by the particular dictionary being used.
            AudienceType audience = GetDefaultAudienceFromDictionaryType(dictionary);

            // Convert delimited list to an array of distinct values.
            String[] includeFilter = Strings.ToListOfTrimmedStrings(includeTypes, LIST_DELIMITER);

            DictionaryQuery query = new DictionaryQuery();
            SearchResults results = query.Expand(searchText, includeFilter, offset, numResults, dictionary, language, audience, version);

            return BuildSearchResultsStructure(results, language, audience, offset);
        }

        /// <summary>
        /// Performs a query for items from the DictionaryEntryMetadata list that are valid in the database..
        /// </summary>
        /// <param name="entriesList">A list of DictionaryEntryMetadata items, whose existence in the DB will be checked.</param>
        /// <returns>A list of DictionaryEntryMetadata items.</returns>
        public List<DictionaryEntryMetadata> DoDictionaryEntriesExist(List<DictionaryEntryMetadata> entriesList)
        {
            // In the initial implementation, the audience is implied by the particular dictionary being used.
            foreach (DictionaryEntryMetadata entry in entriesList)
            {
                AudienceType audience = GetDefaultAudienceFromDictionaryType(entry.Dictionary);
                entry.Audience = audience;
            }

            // Query that returns which DictionaryEntryMetadata items in the given list are valid in the database
            DictionaryQuery query = new DictionaryQuery();
            DataTable results = query.DoDictionaryEntriesExist(entriesList);

            List<DictionaryEntryMetadata> validEntries = new List<DictionaryEntryMetadata>();

            // Converts the datatable of entries into a list of DictionaryEntryMetadata items
            foreach (DataRow row in results.Rows)
            {
                DictionaryEntryMetadata entry = new DictionaryEntryMetadata();
                entry.CDRID = row.Field<int>("CDRID");
                entry.Dictionary = (DictionaryType)System.Enum.Parse(typeof(DictionaryType), row.Field<string>("Dictionary"));
                entry.Language = (Language)System.Enum.Parse(typeof(Language), row.Field<string>("Language"));
                entry.Audience = (AudienceType)System.Enum.Parse(typeof(AudienceType), row.Field<string>("Audience"));

                validEntries.Add(entry);
            }

            return validEntries;
        }


        /// <summary>
        /// Common code for building the results data structure from Search and Expand.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private SearchReturn BuildSearchResultsStructure(SearchResults results, Language language, AudienceType audience, int offset)
        {
            List<String> messages = new List<string>();

            int resultCount = results.MatchCount;

            // Report the count in a human-readable format.
            String message = String.Format("Found {0} results.", resultCount);
            log.Debug(message);
            messages.Add(message);

            // Retrieve results.  We already know the number of results, so let's preset the
            // list to the size we know we're going to need.  (Use the number of rows in the results
            // since MatchCount/resultCount is conceivably much larger than we need and might even be int.MaxValue.)
            List<DictionarySearchResultEntry> foundTerms = new List<DictionarySearchResultEntry>(results.Data.Rows.Count);
            foreach (DataRow row in results.Data.Rows)
            {
                try
                {
                    int id = row.Field<int>("termID");
                    string matchName = row.Field<string>("TermName");
                    string detail = row.Field<string>("object");
                    detail = RewriteMediaFileLocations(detail);
                    foundTerms.Add(new DictionarySearchResultEntry(id, matchName, detail));
                }
                catch (Exception ex)
                {
                    log.Debug("Error retrieving search results.", ex);
                }
            }

            // Populate return metadata structure
            SearchReturnMeta meta = new SearchReturnMeta()
            {
                Language = language.ToString(),
                Audience = audience.ToString(),
                Offset = offset,
                ResultCount = resultCount,
                Messages = messages.ToArray()
            };


            // Combine meta and results to create the final return object.
            SearchReturn srchReturn = new SearchReturn()
            {
                Result = foundTerms.ToArray(),
                Meta = meta
            };

            return srchReturn;
        }

        /// <summary>
        /// In version one of the API, the requested dictionary is used to determine which term audience to
        /// use for filtering. This method puts that logic in one place.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public AudienceType GetDefaultAudienceFromDictionaryType(DictionaryType dictionary)
        {
            AudienceType audience;

            switch (dictionary)
            {
                case DictionaryType.term:
                    audience = AudienceType.Patient;
                    break;
                case DictionaryType.drug:
                    audience = AudienceType.HealthProfessional;
                    break;
                case DictionaryType.genetic:
                    audience = AudienceType.HealthProfessional;
                    break;

                case DictionaryType.Unknown:
                default:
                    {
                        string msg = string.Format("Unspported dictionary type '{0}'.", dictionary);
                        log.Debug(msg);
                        throw new ArgumentException(msg);
                    }
            }

            return audience;
        }

        /// <summary>
        /// Substitute image and audio path placeholder values with actual values.
        /// </summary>
        /// <param name="json">String containing the JSON structure for a dictionary entry.</param>
        /// <returns>A new string containing the rewritten JSON string.</returns>
        private String RewriteMediaFileLocations(String json)
        {
            if (!String.IsNullOrEmpty(json))
            {
                json = json.Replace(IMAGE_PLACEHOLDER_TEXT, _imageFileLocation);
                json = json.Replace(AUDIO_PLACEHOLDER_TEXT, _audioFileLocation);
            }

            return json;
        }
    }
}
