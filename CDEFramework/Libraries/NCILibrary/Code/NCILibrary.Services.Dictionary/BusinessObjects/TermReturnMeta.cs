﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCI.Services.Dictionary.BusinessObjects
{
    /// <summary>
    /// Metadata about a call to GetTerm().
    /// </summary>
    public class TermReturnMeta : MetaCommon
    {
        /// <summary>
        /// The term's audience Patient or HealthProfessional
        /// </summary>
        public String Audience { get; set; }

        /// <summary>
        /// The term's language
        /// </summary>
        public String Language { get; set; }
    }
}
