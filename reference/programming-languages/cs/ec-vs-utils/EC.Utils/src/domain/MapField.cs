/*==========================================================================*/
/* Source File:   MAPFIELD.CS                                               */
/* Description:   Map SE4 metadata to a named field in IterWeb              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.15/2013                                               */
/* Last Modified: Aug.15/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.15/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Map SE4 metadata to a named field in IterWeb
    /// </summary>
    public class MapField {
        /// <summary>
        /// Actual SE4 Attribute name
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name given in Iterweb
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Taken value for this field mapping.
        /// </summary>
        public string Value { get; set; }

    }
}
