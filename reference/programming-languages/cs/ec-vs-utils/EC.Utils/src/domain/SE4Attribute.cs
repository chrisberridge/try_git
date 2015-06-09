/*==========================================================================*/
/* Source File:   SE4ATTRIBUTE.CS                                           */
/* Description:   Retrieve for a document in SE4 all of its attributes      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.12/2013                                               */
/* Last Modified: Oct.23/2013                                               */
/* Version:       1.4                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.12/2013 COQ File created.
============================================================================*/

using EC.Utils.Constants;

namespace EC.Utils.Domain {
    /// <summary>
    /// Retrieve for a document in SE4 all of its attributes
    /// </summary>
    public class SE4Attribute {
        /// <summary>
        /// Default constructor with parameters
        /// </summary>
        /// <param name="order">Sets field Order</param>
        /// <param name="elementName">Sets field elementName</param>
        /// <param name="attributeOrder">sets field AttributeOrder</param>
        /// <param name="attributeName">sets field AttributeName</param>
        /// <param name="text">sets field Text</param>
        /// <param name="hugeContentCode">sets field hugeContentCode</param>
        /// <param name="hugeText">sets field HugeText</param>
        /// <param name="MultimediaAttribute">
        /// Indicates if the field is an image, a video, or any multimedia type.
        /// If it is an image EC_IMG in XML generated file is used.
        /// If it is a video, then EC_VID is used in XML generated file is used.
        /// If it is an audio file, then EC_AUDIO is used in XML generated file is used.
        /// </param>
        public SE4Attribute(int order, string elementName, int attributeOrder, string attributeName,
            string text, long hugeContentCode, string hugeText) {
            this.Order = order;
            this.ElementName = elementName;
            this.AttributeOrder = attributeOrder;
            this.AttributeName = attributeName;
            this.Text = text;
            this.HugeContentCode = hugeContentCode;
            this.HugeText = hugeText;
            this.MultimediaAttribute = GlobalConstants.SE4ATTR_MULTIMEDIA_NONE;
        }

        /// <summary>
        /// A sequence for reading rows in from SQL statement.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// SE4 element naming
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// If there are many element meaning, then a grouping for elements is stored in this ordering.
        /// </summary>
        public int AttributeOrder { get; set; }

        /// <summary>
        /// A name for the attribute inside an element grouping
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// The value for the element if it is not a huge content.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// If content is larger to be held in text field, it is stored in this code.
        /// </summary>
        public long HugeContentCode { get; set; }

        /// <summary>
        /// The content value for a huge content accessed via hugeContentCode field
        /// </summary>
        public string HugeText { get; set; }

        /// <summary>
        /// Indicates if the field is an image, a video, or any multimedia type.
        /// If it is an image EC_IMG in XML generated file is used.
        /// If it is a video, then EC_VID is used in XML generated file is used.
        /// If it is an audio file, then EC_AUDIO is used in XML generated file is used.
        /// Values taken:
        /// 0: No multimedia attribute.
        /// 1: Image
        /// 2: Video
        /// 3: Audio
        /// </summary>
        public int MultimediaAttribute { get; set; }
    }
}