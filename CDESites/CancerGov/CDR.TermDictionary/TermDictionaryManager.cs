﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CancerGov.UI;
using CancerGov.Common.ErrorHandling;
using NCI.Util;
using CancerGov.CDR.TermDictionary;
using NCI.Web.CDE;
namespace CancerGov.CDR.TermDictionary
{
    /// <summary>
    /// This Class is the business layer that interfaces between the user interface and
    /// the database layer. 
    /// </summary>
    public static class TermDictionaryManager
    {

        /// <summary>
        /// This methods filters the information passed to it in order to refine the query
        /// that will be called in the database layer.
        /// </summary>
        /// <param name="language">enumeration indicating language</param>
        /// <param name="criteria">The partial text used to query the database</param>
        /// <param name="maxRows">The maximum number of rows that the database will return. a value of zero will return the entire set</param>
        /// <param name="contains">indicator as to whether the text is to be searched starting from the beginning or anywhere
        ///                        in the string</param>
        /// <param name="dictionary">The maximum number of rows that the database will return. a value of zero will return the entire set</param>

        /// <returns>Returns the search results</returns>
        public static TermDictionaryCollection Search(string language, string criteria, int maxRows, bool contains)
        {
            return Search(language, criteria, maxRows, contains, "Cancer.gov", "Patient");

        }


        /// <summary>
        /// This methods filters the information passed to it in order to refine the query
        /// that will be called in the database layer.
        /// </summary>
        /// <param name="language">enumeration indicating language</param>
        /// <param name="criteria">The partial text used to query the database</param>
        /// <param name="maxRows">The maximum number of rows that the database will return. a value of zero will return the entire set</param>
        /// <param name="contains">indicator as to whether the text is to be searched starting from the beginning or anywhere
        ///                        in the string</param>
        /// <param name="dictionary">Which Term dicitonary to search - Cancer.gov or Genetics</param>
        /// <param name="audience">Definition audience - Patient or Health professional</param>
        /// <returns>Returns the search results</returns>
        public static TermDictionaryCollection Search(string language, string criteria, int maxRows, bool contains, string dictionary, string audience)
        {
            TermDictionaryCollection dc = new TermDictionaryCollection();

            // Find out how we should search for the string
            if (Strings.Clean(criteria) != null)
            {
                // replace any '[' with '[[]'
                //criteria = criteria.Replace("[", "[[]");

                // put the '%' at the end to indicate that the search starts
                // with the criteria passed.
                criteria += "%";

                // put the '%' at the beginning to indicate that the search
                // data contains the criteria passed
                if (contains)
                    criteria = "%" + criteria;

                // Find out the field we need to get to build our list
                string fieldName = "TermName";
                if (language == "Spanish")
                {
                    fieldName = language.ToString() + fieldName;
                }

                try
                {
                    // Call the database layer and get data
                    DataTable dt = TermDictionaryQuery.Search(
                            language.ToString(),
                            criteria,
                            maxRows,
                            dictionary,
                            audience);

                    // use Linq to move information from the dataTable
                    // into the TermDictionaryCollection
                    dc.AddRange(
                        from entry in dt.AsEnumerable()
                        select GetEntryFromDR(entry)
                    );

                }
                catch (Exception ex)
                {
                    CancerGovError.LogError("TermDictionatyManager", 2, ex);
                    throw ex;
                }
            }

            return dc;
        }

        /// <summary>
        /// This methods filters the information passed to it in order to refine the query
        /// that will be called in the database layer.
        /// </summary>
        /// <param name="language">enumeration indicating language</param>
        /// <param name="criteria">The partial text used to query the database</param>
        /// <param name="maxRows">The maximum number of rows that the database will return. a value of zero will return the entire set</param>
        /// <param name="contains">indicator as to whether the text is to be searched starting from the beginning or anywhere
        /// in the string</param>
        /// <param name="pageNumber">The page for which the records should be returned.The records returned 
        /// will always equal maxRows in every page.</param>
        /// <returns>Returns the search results</returns>
        public static TermDictionaryCollection GetTermDictionaryList(string language, string criteria, bool contains, int maxRows, int pageNumber, ref int totalRecordCount)
        {
            TermDictionaryCollection dc = new TermDictionaryCollection();
            totalRecordCount = 0;

            // Find out how we should search for the string
            if (Strings.Clean(criteria) != null)
            {
                // replace any '[' with '[[]'
                //criteria = criteria.Replace("[", "[[]");

                // put the '%' at the end to indicate that the search starts
                // with the criteria passed.
                criteria += "%";

                // put the '%' at the beginning to indicate that the search
                // data contains the criteria passed
                if (contains)
                    criteria = "%" + criteria;

                // Find out the field we need to get to build our list
                string fieldName = "TermName";
                if (language == "Spanish")
                {
                    fieldName = language.ToString() + fieldName;
                }

                try
                {
                    // Call the database layer and get data
                    DataTable dt = TermDictionaryQuery.GetTermDictionaryList(
                            language.ToString(),
                            criteria,
                            maxRows,
                            pageNumber,
                            ref totalRecordCount);

                    // use Linq to move information from the dataTable
                    // into the TermDictionaryCollection
                    dc.AddRange(
                        from entry in dt.AsEnumerable()
                        select GetEntryFromDR(entry)
                    );

                }
                catch (Exception ex)
                {
                    CancerGovError.LogError("TermDictionatyManager", 2, ex);
                    throw ex;
                }
            }

            return dc;
        }

        /// <summary>
        /// Method will return a single TermDictionaryItem with its associated
        /// TermDictionaryNeighbors
        /// </summary>
        /// <param name="language"></param>
        /// <param name="termName"></param>
        /// <param name="nMatches"></param>
        /// <returns></returns>
        public static TermDictionaryDataItem GetDefinitionByTermName(DisplayLanguage language, string termName, string audience, int nNeighborMatches)
        {
            TermDictionaryDataItem di = null;

            // default the audience if there was none supplied
            if (string.IsNullOrEmpty(audience))
                audience = "Patient";

            try
            {
                // Call the database layer and get data
                DataTable dt =
                    TermDictionaryQuery.GetDefinitionByTermName(
                        language.ToString(),
                        termName,
                        audience);

                // Get the entry record
                if (dt.Rows.Count == 1)
                {
                    // build the data item
                    di = GetEntryFromDR(dt.Rows[0]);

                    // Get the neighbors
                    GetTermNeighbors(di, language.ToString(), nNeighborMatches);
                }
                else if (dt.Rows.Count > 0)
                {
                    throw new Exception("GetDefinitionByTermName returned more than 1 record.");
                }
            }
            catch (Exception ex)
            {
                CancerGovError.LogError("TermDictionaryManager", 2, ex);
                throw ex;
            }

            return di;
        }


        /// <summary>
        /// Method will return a all letters that have items that start with the specific letter 
        /// </summary>
        /// <param name="language"></param>

        /// <returns></returns>
        public static ArrayList GetAZListLettersWithData(string language)
        {
            DataTable dt = null;
            ArrayList al = null;

            try
            {
                // Call the database layer and get data
                dt = TermDictionaryQuery.AZListLettersWithData(language);
                al = new ArrayList(dt.Rows.Count);
                foreach (DataRow row in dt.Rows)
                {
                        al.Add(row[0].ToString().ToUpper());
                }
               
            }
            catch (Exception ex)
            {
                CancerGovError.LogError("TermDictionaryManager.GetAZListLettersWithData", 2, ex);
                throw ex;
            }

            return al;
        }



        /// <summary>
        /// Method will return a single TermDictionaryItem with its associated
        /// TermDictionaryNeighbors
        /// </summary>
        /// <param name="language"></param>
        /// <param name="termName"></param>
        /// <param name="nMatches"></param>
        /// <returns></returns>
        public static TermDictionaryDataItem GetDefinitionByTermID(string language, string termID, string audience, int nNeighborMatches)
        {
            TermDictionaryDataItem di = null;

            // default the audience if there was none supplied
            if (string.IsNullOrEmpty(audience))
                audience = "Patient";

            try
            {
                // Call the database layer and get data
                DataTable dt =
                    TermDictionaryQuery.GetDefinitionByTermID(
                        language.ToString(),
                        termID,
                        audience);

                // Get the entry record
                if (dt.Rows.Count == 1)
                {
                    // build the data item
                    di = GetEntryFromDR(dt.Rows[0]);

                    // Get the neighbors
                    GetTermNeighbors(di, language, nNeighborMatches);
                }
                else if (dt.Rows.Count > 0)
                {
                    throw new Exception("GetDefinitionByTermID returned more than 1 record.");
                }
            }
            catch (Exception ex)
            {
                CancerGovError.LogError("TermDictionaryManager", 2, ex);
                throw ex;
            }

            return di;
        }

        /// <summary>
        /// Extracts information from the datarow that has the term names
        /// that are before and after the term name that is passed to this
        /// method. Two lists get built and are set inside the TermDictionaryDataItem
        /// object.
        /// </summary>
        /// <param name="di"></param>
        public static void GetTermNeighbors(TermDictionaryDataItem di, string language, int nMatches)
        {
            try
            {
                // Call the database layer and get data
                DataTable dt =
                    TermDictionaryQuery.GetTermNeighbors(
                        language.ToString(), di.TermName, nMatches);

                // use Linq to move information from the dataTable
                // into the TermDictionaryCollection
                di.PreviousNeighbors.AddRange(
                    from entry in dt.AsEnumerable()
                    where entry["isPrevious"].ToString() == "Y"
                    select GetEntryFromDR(entry)
                );

                // use Linq to move information from the dataTable
                // into the TermDictionaryCollection
                di.NextNeighbors.AddRange(
                    from entry in dt.AsEnumerable()
                    where entry["isPrevious"].ToString() == "N"
                    select GetEntryFromDR(entry)
                );
            }
            catch (Exception ex)
            {
                CancerGovError.LogError("TermDictionaryManager", 2, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Extracts information from a DataRow and builds a new
        /// TermDictionaryDataItem
        /// </summary>
        /// <param name="dr"></param>
        /// <returns>TermDictionaryDataItem</returns>
        private static TermDictionaryDataItem GetEntryFromDR(DataRow dr)
        {
            // return the new object
            TermDictionaryDataItem ddi = new TermDictionaryDataItem(
                Strings.ToInt(dr["GlossaryTermID"]),
                Strings.Clean(dr["TermName"]),
                Strings.Clean(dr["OLTermName"]),
                Strings.Clean(dr["TermPronunciation"]),
                Strings.Clean(dr["DefinitionHTML"]),
                Strings.Clean(dr["MediaHTML"]),
                Strings.Clean(dr["MediaCaption"]),
                Strings.ToInt(dr["MediaID"]),
                Strings.Clean(dr["AudioMediaHTML"]),
                Strings.Clean(dr["RelatedInformationHtml"])
             );

            return ddi;
        }


        public static ArrayList GetDefinition(string type, string param, PDQVersion pdqVersion, string language)
        {
            ArrayList returnvalue = new ArrayList(3);
           
            returnvalue = CancerGov.CDR.TermDictionary.TermDictionaryQuery.GetPopDefinition(type, param, pdqVersion.ToString(), language);            
            return returnvalue;
        }
    }
}
