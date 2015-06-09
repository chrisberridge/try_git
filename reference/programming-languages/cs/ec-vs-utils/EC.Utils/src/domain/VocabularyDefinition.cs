/*==========================================================================*/
/* Source File:   VOCABULARYDEFINITION.CS                                   */
/* Description:   Defines a Vocabulary in system                            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.07/2014                                               */
/* Last Modified: May.07/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
May.07/2014 COQ File created.
============================================================================*/

using System.Collections.Generic;

namespace EC.Utils.Domain {
    public class VocabularyDefinition {
        public string Name { get; set; }
        public string ApplyTo { get; set; }
        public List<CategoryDefinition> Categories { get; set; }
    }
}
