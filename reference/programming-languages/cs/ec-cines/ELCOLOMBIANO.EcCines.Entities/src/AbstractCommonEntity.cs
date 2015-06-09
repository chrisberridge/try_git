/*==========================================================================*/
/* Source File:   ABSTRACTCOMMONENTITY.CS                                   */
/* Description:   Base class with common methods for subclasses to use.     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.24/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.24/2015 COQ File created.
============================================================================*/

using System.Reflection;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ELCOLOMBIANO.EcCines.Entities {
    /// <summary>
    /// Base class with common methods for subclasses to use. 
    /// </summary>
    public abstract class AbstractCommonEntity {
        protected readonly ILog log = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public AbstractCommonEntity() {
            this.log = LogManager.GetLogger(this.GetType());
        }
    }
}
