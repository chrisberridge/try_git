/*==========================================================================*/
/* Source File:   TEMPLATETYPECODE.CS                                       */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.16/2013                                               */
/* Last Modified: Nov.26/2013                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.16/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Constants {
    /// <summary>
    /// Used to drive metadata extraction field mapping.
    /// </summary>
    public sealed class TemplateTypeCode {
        // Fields list for each template is as follows:
        // 1: This type uses fields: Title, DocumentText, BodyContent, Bullet (optional), PhotoRelated, ReaderHelp (optional)
        // 2: This type uses fields: Title, NewsGrouper
        // 3: This type uses fields: Title, CartoonGrouper
        // 4: This type uses fields: Title, DocumentText, CreditText, Bullet, PhotoRelated
        // 5: This type uses fields: Title, DocumentText, PhotoGallery
        // 6: This type uses fileds: Title, DocumentText
        // 7: This type uses fields: Title, DocumentText, Bullet, ImageOnlySet
        // 8: This type uses fields: Title, DocumentText, Bullet, PhotoRelated
        // 9: This type uses fields: Title, Bullet, CreditText, DocumentText
        // 10: This type uses fields: Title, ImageOnlySet, DocumentText
        // 11: This type uses fields: Title, Credit, DocumentText
        // 12: This type uses fields: Title and DocumentText
        // 13: This type uses fields: Title, CreditText, ImageOnly and Body Content (other HTML markup)
        // 14: This type uses fields: Title, CreditText, and DocumentText (other HTML markup)
        // 15: This type uses fields: Title, DocumentText, PhotoOnlyGallery
        public const int OLD_DOC_TEMPLATE_TYPE_UNASSIGNED = -1;
        public const int OLD_DOC_TEMPLATE_TYPE_001 = 1; // First type found.
        public const int OLD_DOC_TEMPLATE_TYPE_002 = 2; // News Group
        public const int OLD_DOC_TEMPLATE_TYPE_003 = 3; // Cartoon only
        public const int OLD_DOC_TEMPLATE_TYPE_004 = 4; // Similar to Type 001 but uses other markup
        public const int OLD_DOC_TEMPLATE_TYPE_005 = 5; // Photo Gallery (Request/response method).
        public const int OLD_DOC_TEMPLATE_TYPE_006 = 6; // A special case as the OLD_DOC_TEMPLATE_TYPE_001 but body text is found in another HTML markup
        public const int OLD_DOC_TEMPLATE_TYPE_007 = 7; // A kind of template that only has a Title, a content and a photo with bullets.
        public const int OLD_DOC_TEMPLATE_TYPE_008 = 8; // A kind of template that only has a Title, a content and a photo/footer with bullets.
        public const int OLD_DOC_TEMPLATE_TYPE_009 = 9; // A kind of template that only has a Title, Bullets and content.
        public const int OLD_DOC_TEMPLATE_TYPE_010 = 10; // Holds only Title, ImageOnly and DocumentText
        public const int OLD_DOC_TEMPLATE_TYPE_011 = 11; // Holds only Title, Credit, DocumentText
        public const int OLD_DOC_TEMPLATE_TYPE_012 = 12; // Holds only Title and DocumentText
        public const int OLD_DOC_TEMPLATE_TYPE_013 = 13; // Holds only Title, CreditText, ImageOnly and Body Content (other HTML markup)
        public const int OLD_DOC_TEMPLATE_TYPE_014 = 14; // Holds only Title, CreditText, and DocumentText (other HTML markup)
        public const int OLD_DOC_TEMPLATE_TYPE_015 = 15; // Holds only Title, DocumentText, PhotoOnlyGallery (other HTML markup)
    }
}
