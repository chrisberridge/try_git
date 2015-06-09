/*==========================================================================*/
/* Source File:   VALIDMAPFIELD.CS                                          */
/* Description:   Holds a mapping of valid fields to integrate to IterWeb   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.15/2013                                               */
/* Last Modified: Aug.16/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.15/2013 COQ File created.
============================================================================*/

using System.Collections.Generic;

namespace EC.Utils.Domain {
    /// <summary>
    /// Holds a mapping of valid fields to integrate to IterWeb
    /// </summary>
    public class ValidMapField {

        /// <summary>
        /// Constructor with field
        /// </summary>
        public ValidMapField(string source, string target, bool use, string targetType, string targetIndexType, List<ValidMapFieldAttributes> attributes) {
            this.Source = source;
            this.Target = target;
            this.Use = use;
            this.TargetType = targetType;
            this.TargetIndexType = targetIndexType;
            this.Attributes = attributes;
        }

        /// <summary>
        /// Actual SE4 Element name
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name given in Iterweb
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// True this field can be used and eventually gets generated.
        /// </summary>
        public bool Use { get; set; }

        /// <summary>
        /// Used to map iterweb field mode type.
        /// </summary>
        public string TargetType { get; set; }

        /// <summary>
        /// Used to map iterweb field mode index-type
        /// </summary>
        public string TargetIndexType { get; set; }

        /// <summary>
        /// Some elements in source have attributes. 
        /// If this field is NULL then it does not have related attributes
        /// </summary>
        public List<ValidMapFieldAttributes> Attributes { get; set; }
    }
}
