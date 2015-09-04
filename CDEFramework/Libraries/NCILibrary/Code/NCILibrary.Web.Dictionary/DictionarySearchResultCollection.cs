﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCI.Web.Dictionary.BusinessObjects;
using System.Collections;

namespace NCILibrary.Web.Dictionary
{
    /// <summary>
    /// Dictionary Collection for Search and Expand
    /// </summary>
    public class DictionarySearchResultCollection : IEnumerable<DictionarySearchResult>
    {
  
        private IEnumerable<DictionarySearchResult> searchResults;
        public int ResultsCount { get { return ResultsCount; } set { ResultsCount = value; } }

        /// <summary>
        /// Constructor Method that sets the enumerator
        /// </summary>
        /// <param name="results"> the list passed in as an enumerable</param>
        public DictionarySearchResultCollection(IEnumerable<DictionarySearchResult> results)
        {
            this.searchResults = results;
        }




        #region IEnumerable<DictionarySearchResult> Members
        /// <summary>
        /// Get Enumerator object 
        /// </summary>
        /// <returns>Enumerable list</returns>
        public IEnumerator<DictionarySearchResult> GetEnumerator()
        {
            return searchResults.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Get IEnumerable
        /// </summary>
        /// <returns>the Enumerable list</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return searchResults.GetEnumerator();
        }

        #endregion
    }
}
