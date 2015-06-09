/*==========================================================================*/
/* Source File:   VALIDMAPFIELDATTRIBUTES.CS                                */
/* Description:   Holds a mapping of valid fields to integrate to IterWeb   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.16/2013                                               */
/* Last Modified: Aug.21/2013                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.16/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Holds a mapping of valid fields to integrate to IterWeb
    /// </summary>
    public class ValidMapFieldAttributes {

        /// <summary>
        /// Constructor with field
        /// </summary>
        public ValidMapFieldAttributes(string source, string sourceAttribute, string target, string targetType, string targetIndexType, bool use) {
            this.Source = source;
            this.SourceAttribute = sourceAttribute;
            this.Target = target;
            this.TargetType = targetType;
            this.TargetIndexType = targetIndexType;
            this.Use = use;
        }

        /// <summary>
        /// Actual SE4 Element name
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Actual Se4 Element attribute name
        /// </summary>
        public string SourceAttribute { get; set; }

        /// <summary>
        /// Name given in Iterweb
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Used to map iterweb field mode type.
        /// </summary>
        public string TargetType { get; set; }

        /// <summary>
        /// Used to map iterweb field mode index-type
        /// </summary>
        public string TargetIndexType { get; set; }

        /// <summary>
        /// True this field can be used and eventually gets generated.
        /// </summary>
        public bool Use { get; set; }
    }
}
