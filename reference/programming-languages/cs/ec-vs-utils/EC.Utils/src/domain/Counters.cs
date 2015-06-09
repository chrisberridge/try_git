/*==========================================================================*/
/* Source File:   COUNTERS.CS                                               */
/* Description:   Object to track different sequences for internal          */
/*                processing.                                               */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.12/2013                                               */
/* Last Modified: Aug.21/2013                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.12/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Object to track differente sequences for internal processing.
    /// The state for this object must be persisted somehow.
    /// </summary>
    public class Counters {
        /// <summary>
        /// Sequence for ZIP file generation.
        /// </summary>
        public long IterWebManifestFile { get; set; }

        /// <summary>
        /// Sequence for images in manifest
        /// </summary>
        public long Image { get; set; }

        /// <summary>
        /// Sequence for multimedia in manifest, be it video, audio, galleries to name a few.
        /// </summary>
        public long Multimedia { get; set; }

        /// <summary>
        /// A sequence for older documents not in SE4
        /// </summary>
        public long OldDoc { get; set; }

        /// <summary>
        /// Sequence for page content in manifest
        /// </summary>
        public long PageContent { get; set; }
    }
}
