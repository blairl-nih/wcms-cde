﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using CancerGov.UI;

namespace CancerGov.CDR.TermDictionary
{
    /// <summary>
    /// This class is used by the WCF for data serialization to a calling html request, such
    /// as AJAX.  The Service Item is filled by the Service layer which calls the business
    /// layer. The Serializable and DataContract decorations declares the class for the WCF.
    /// The datamember tag identifies the data that needs to be serialized.
    /// </summary>
    [Serializable]
    [DataContract]
    public class TermDictionaryServiceItem
    {
        [DataMember]
        public int id;

        [DataMember]
        public string item = string.Empty;

        [DataMember]
        public string info=string.Empty;

        [DataMember]
        public TermDictionaryDataItem TermDictionaryDetail
        {
            get;
            set;
        }
        /// <summary>
        /// Constructor requires that all item be accounted for when created. This ensures
        /// that any new items or removal of items is address in all places that use this
        /// object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
        public TermDictionaryServiceItem(int id, string item, string info)
        {
            this.id = id;
            this.item = item;
            this.info = info;
        }
    }
}
