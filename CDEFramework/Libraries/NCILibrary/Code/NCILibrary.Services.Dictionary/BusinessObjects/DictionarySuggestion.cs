﻿using System;
using System.Runtime.Serialization;

namespace NCI.Services.Dictionary.BusinessObjects
{
    [DataContract()]
    public class DictionarySuggestion : IJsonizable
    {
        // For serialization
        public DictionarySuggestion()
        {
        }

        public DictionarySuggestion(int id, String term)
        {
            this.ID = id;
            this.Term = term;
        }

        /// <summary>
        ///  The Term's ID
        /// </summary>
        [DataMember(Name = "id")]
        public int ID { get; set; }

        /// <summary>
        /// The Term's name
        /// </summary>
        [DataMember(Name = "term")]
        public String Term { get; set; }


        public void Jsonize(Jsonizer builder)
        {
            builder.AddMember("id", ID, false);
            builder.AddMember("term", Term, true);
        }
    }
}
