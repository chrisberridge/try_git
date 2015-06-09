/*==========================================================================*/
/* Source File:   ABSTRACTCOMMONDOMAIN.CS                                   */
/* Description:   Base class with common methods for subclasses to use.     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.24/2015                                               */
/* Last Modified: Apr.27/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.24/2015 COQ File created.
============================================================================*/
using System;
using ELCOLOMBIANO.EcCines.Common;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    /// <summary>
    /// Base class with common methods for subclasses to use. 
    /// </summary>
    public abstract class AbstractCommonDomain {
        /// <summary>
        /// Overrides base object class method to print class instance properties/values.
        /// </summary>
        /// <returns>String containing a properties/values.</returns>
        public override String ToString() {
            return ToStringBuilder.reflectionToString(this);
        }
    }
}
