/*==========================================================================*/
/* Source File:   DOC.CS                                                    */
/* Description:   Holds a Body from an HTML extraction                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.06/2013                                               */
/* Last Modified: Nov.27/2013                                               */
/* Version:       1.17                                                      */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

using EC.Utils.Constants;
using EC.Utils.Domain;
using System.Collections.Generic;

namespace EC.IterwebMigrate.Domain {
    /// <summary>
    /// Document representation from extracted HTML to be saved as JSON.
    /// NOTE: If a new field is added. The whole class must be carefully revised.
    /// </summary>
    public class Doc {
        /// <summary>
        /// Valid values for status are non-negative, but it is initialized
        /// as -1 to logically set it up.
        /// </summary>
        private int status;
        private int templateType;

        /// <summary>
        /// Its value is set when determining the document status. Or by deserializing the Json object
        /// Determines which properties to use when generating to XML
        /// 1: This type uses fields: Title, DocumentText, BodyContent, Bullet (optional), PhotoRelated, ReaderHelp (optional)
        /// 2: This type uses fields: Title, NewsGrouper
        /// 3: This type uses fields: Title, CartoonGrouper
        /// 4: This type uses fields: Title, DocumentText, CreditText, Bullet, PhotoRelated
        /// 5: This type uses fields: Title, DocumentText, PhotoGallery
        /// 6: This type uses fileds: Title, DocumentText
        /// 7: This type uses fields: Title, DocumentText, Bullet, ImageOnlySet
        /// 8: This type uses fields: Title, DocumentText, Bullet, PhotoRelated
        /// 9: This type uses fields: Title, Bullet, CreditText, DocumentText
        /// 10: This type uses fields: Title, ImageOnlySet, DocumentText
        /// 11: This type uses fields: Title, CreditText, BodyContent
        /// 12: This type uses fields: Title and BodyContent
        /// 13: This type uses fields: Title, CreditText, ImageOnly and BodyText (other HTML markup)
        /// 14: This type uses fields: Title, CreditText, and BodyText (other HTML markup) but source comes from other HTML tagging.
        /// 15: This type uses fields: Title, 
        /// </summary>
        public int TemplateType {
            get { return templateType; }
            set { templateType = value; }
        }

        /// <summary>
        /// Returns the status of the extraction.
        /// See EC.Utils.Constants.MigrateStatusCode for explanation
        /// </summary>
        public int Status {
            get { return GetStatus(); }
            set { status = value; }
        }

        /// <summary>
        /// Stores the url that was used to generate the JSON and
        /// to reference and back reference
        /// </summary>
        public string UrlUsed { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Doc() {
            Title = new List<Title>();
            DocumentText = new BodyContent();
            CreditText = new CreditContent();
            ImageOnlySet = new ImageOnly();
            Bullet = new List<Bullet>();
            PhotoRelated = new List<PhotoRelated>();
            ReaderHelp = new List<ReaderHelp>();
            NewsGrouper = new List<NewsGrouper>();
            CartoonGrouper = new List<CartoonGrouper>();
            PhotoGallery = new List<PhotoGallery>();
            PhotoFooterGallery = new PhotoOnlyGallery();
            Reset();
        }

        /// <summary>
        /// Reset status
        /// </summary>
        public void Reset() {
            Title.Clear();
            DocumentText.Name = DocumentText.Content = "";
            CreditText.AuthorName = CreditText.AuthorText = "";
            CreditText.CityName = CreditText.CityText = "";
            CreditText.DisplayDateName = CreditText.DisplayDateText = "";
            ImageOnlySet.Name = ImageOnlySet.Content = "";
            Bullet.Clear();
            PhotoRelated.Clear();
            ReaderHelp.Clear();
            NewsGrouper.Clear();
            CartoonGrouper.Clear();
            PhotoGallery.Clear();
            PhotoFooterGallery.Clean();
            status = -1;
            templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
            UrlUsed = "";
        }

        /// <summary>
        /// Ask if all necessary template fields are set given the template number. 
        /// NOTE: It may happen that some fields may be optional. Besides. Not all template types are examined.
        /// Actually only Template 001.
        /// </summary>
        /// <param name="templateTypeNum">Template Type to check against.</param>
        /// <returns>True if template satisfies the condition.</returns>
        public bool IsTemplateTypeFieldsSetTo(int templateTypeNum) {
            bool rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;

            switch (templateTypeNum) {
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001:
                    var cntItems = 0;
                    rslt = false;
                    if (cntTitle != 0) {
                        cntItems++;
                    }
                    if (!documentTextEmpty) {
                        cntItems++;
                    }
                    if (cntBullet != 0) {
                        cntItems++;
                    }
                    if (cntPhotoRelated != 0) {
                        cntItems++;
                    }

                    if (cntReaderHelp != 0) {
                        cntItems++;
                    }

                    if (cntItems >= 3) {
                        rslt = true;
                    }
                    break;
                default:
                    break;
            }

            return rslt;
        }

        /// <summary>
        /// Check if and only if all fields as follows are all filled.
        /// Field list: Title, Bullet, Document Text, PhotoRelated and ReaderHelp
        /// </summary>
        /// <returns>True if all fields are present and filled.</returns>
        public bool IsTitleBulletDocumentPhotoRelatedReaderHelpOnlyFilled() {
            var rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (cntBullet != 0 && cntTitle != 0 & !documentTextEmpty && cntPhotoRelated != 0 && cntReaderHelp != 0 &&
                    creditTextEmpty && imageOnlySetEmpty && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                    cntNewsGrouper == 0 && cntCartoonGrouper == 0 && cntPhotoGallery == 0 &&
                    photoFooterGalleryEmpty);
            return rslt;
        }

        /// <summary>
        /// Check if Title and Bullet and Document Text are set but Credit Text is not set.
        /// These fields are used in Template 009.
        /// </summary>
        /// <returns>true only if criteria met</returns>
        public bool IsTitleBullentDocumetTextOnlyFilled() {
            var rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (!documentTextEmpty && creditTextEmpty && cntBullet != 0 && cntTitle != 0 &&
                    imageOnlySetEmpty && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                    cntNewsGrouper == 0 && cntCartoonGrouper == 0 && cntPhotoGallery == 0 &&
                    photoFooterGalleryEmpty);
            return rslt;
        }

        /// <summary>
        /// Examines the old document structure to find if only title and bullet are set.
        /// Check if Title, Bullet are set but Document Text and ImageOnly are not set (required for Template 007). 
        /// Especial case.
        /// </summary>
        /// <returns>true only if criteria met</returns>
        public bool IsTitleBulletOnlyFilled() {
            var rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (documentTextEmpty && imageOnlySetEmpty && cntBullet != 0 && cntTitle != 0 &&
                    creditTextEmpty && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                    cntNewsGrouper == 0 && cntCartoonGrouper == 0 &&
                    cntPhotoGallery == 0 && photoFooterGalleryEmpty);
            return rslt;
        }

        /// <summary>
        /// Examines the old document structure to find if only PhotoRelated was filled and all other data is not filled.
        /// </summary>
        /// <returns>True if only PhotoRelated is filled</returns>
        public bool IsPhotoRelatedOnlyFilled() {
            var rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (documentTextEmpty && creditTextEmpty && cntTitle == 0 && cntBullet == 0 &&
                    cntPhotoRelated != 0 && cntReaderHelp == 0 && cntNewsGrouper == 0 &&
                    cntCartoonGrouper == 0 && cntPhotoGallery == 0 && photoFooterGalleryEmpty);
            return rslt;
        }

        /// <summary>
        /// Determines if only Title and Body Content is set
        /// </summary>
        /// <returns></returns>
        public bool IsTitleBodyContentOnlyFilled() {
            bool rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (!documentTextEmpty && creditTextEmpty && imageOnlySetEmpty &&
                    cntTitle != 0 &&
                    cntBullet == 0 && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                    cntNewsGrouper == 0 && cntCartoonGrouper == 0 && cntPhotoGallery == 0 &&
                    photoFooterGalleryEmpty
                    );
            return rslt;
        }

        /// <summary>
        /// Checks against fields to see if only Title is set, by testing
        /// title field is not empty but all others do.
        /// </summary>
        /// <returns>True if only title is filled</returns>
        public bool IsTitleOnlyFilled() {
            bool rslt;
            var cntBullet = Bullet.Count;
            var cntTitle = Title.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == ""
                                  );
            var imageOnlyEmpty = (ImageOnlySet.Content == "" && ImageOnlySet.Name == "");
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (cntTitle != 0 && documentTextEmpty && creditTextEmpty && imageOnlyEmpty &&
                    cntBullet == 0 && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                    cntNewsGrouper == 0 && cntCartoonGrouper == 0 &&
                    cntPhotoGallery == 0 && photoFooterGalleryEmpty);
            return rslt;
        }

        /// <summary>
        /// Determines if all data are not set.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty() {
            bool rslt = false;
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;
            var photoFooterEmpty = PhotoFooterGallery.IsEmpty();

            rslt = (documentTextEmpty && creditTextEmpty && imageOnlySetEmpty &&
                    cntTitle == 0 && cntBullet == 0 &&
                    cntPhotoRelated == 0 && cntReaderHelp == 0 && cntNewsGrouper == 0 &&
                    cntCartoonGrouper == 0 && cntPhotoGallery == 0 & photoFooterEmpty);
            return rslt;
        }

        /// <summary>
        /// Checks the inner data to determine if it is usable
        /// If property Status is already set a value, then it just
        /// returns it, else it evaluates the document data to determine
        /// if document is usable.
        /// </summary>
        /// <returns></returns>
        private int GetStatus() {
            int rslt = 0;
            if (status != MigrateStatusCode.OLD_DOC_STATUS_CODE_NOT_PROCESS) {
                // the status of this document has been determined outside this class
                // by having set the status property value.
                rslt = status;
            }
            else {
                var cntBullet = Bullet.Count;
                var cntTitle = Title.Count;
                var cntPhotoRelated = PhotoRelated.Count;
                var cntReaderHelp = ReaderHelp.Count;
                var cntNewsGrouper = NewsGrouper.Count;
                var cntCartoonGrouper = CartoonGrouper.Count;
                var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
                var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                       CreditText.CityName == "" && CreditText.CityText == "" &&
                                       CreditText.DisplayDateName == "" && CreditText.DisplayDateText == ""
                                      );
                var imageOnlyEmpty = (ImageOnlySet.Content == "" && ImageOnlySet.Name == "");
                var cntPhotoGallery = PhotoGallery.Count;
                var photoFooterGalleryEmpty = PhotoFooterGallery.IsEmpty();

                rslt = MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS;
                if (IsEmpty()) {
                    rslt = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                }
                else {
                    // if only title is set but other fields are not fully set then
                    // record is set as MigrateStatusCode.OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED
                    if (cntTitle != 0) {
                        if (documentTextEmpty && creditTextEmpty && imageOnlyEmpty &&
                            cntBullet == 0 && cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                            cntNewsGrouper == 0 && cntCartoonGrouper == 0 &&
                            cntPhotoGallery == 0 && photoFooterGalleryEmpty
                            ) {
                            templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                            rslt = MigrateStatusCode.OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED;
                        }
                        else {
                            if (cntTitle != 0 && cntBullet == 0 &&
                                cntPhotoRelated == 0 && cntReaderHelp == 0 &&
                                cntNewsGrouper == 0 && cntCartoonGrouper == 0 &&
                                documentTextEmpty) {
                                templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                                rslt = MigrateStatusCode.OLD_DOC_STATUS_CODE_WARNING;
                            }
                        }
                    }
                    else {
                        if (cntNewsGrouper == 0) {
                            templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                            rslt = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                        }
                    }
                }

                if (rslt == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    if (cntTitle != 0 && cntNewsGrouper != 0) {
                        templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002;
                    }
                    else {
                        if (cntCartoonGrouper != 0) {
                            templateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003;
                        }
                    }
                }
            }
            return rslt;
        }

        /// <summary>
        /// The read JSON file metadata names are changed for final names accordingly.
        /// </summary>
        public void SetMetadataNameToFinal() {
            var documentTextEmpty = (DocumentText.Name == "" && DocumentText.Content == "");
            var creditTextEmpty = (CreditText.AuthorName == "" && CreditText.AuthorText == "" &&
                                   CreditText.CityName == "" && CreditText.CityText == "" &&
                                   CreditText.DisplayDateName == "" && CreditText.DisplayDateText == "");
            var imageOnlySetEmpty = (ImageOnlySet.Name == "" && ImageOnlySet.Content == "");
            var cntTitle = Title.Count;
            var cntBullet = Bullet.Count;
            var cntPhotoRelated = PhotoRelated.Count;
            var cntReaderHelp = ReaderHelp.Count;
            var cntNewsGrouper = NewsGrouper.Count;
            var cntCartoonGrouper = CartoonGrouper.Count;
            var cntPhotoGallery = PhotoGallery.Count;

            if (!documentTextEmpty) {
                DocumentText.Name = MetadataName.OldDocTextNameFinal;
            }
            if (!creditTextEmpty) {
                CreditText.AuthorName = MetadataName.OldDocCreditAuthorNameFinal;
                CreditText.CityName = MetadataName.OldDocCreditCityNameFinal;
                CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateNameFinal;
            }
            if (!imageOnlySetEmpty) {
                ImageOnlySet.Name = MetadataName.OldDocImageOnlySetNameFinal;
            }
            if (cntTitle != 0) {
                foreach (var t in Title) {
                    t.Name = MetadataName.OldDocTitleNameFinal;
                }
            }
            if (cntBullet != 0) {
                foreach (var b in Bullet) {
                    b.Name = MetadataName.OldDocBulletNameFinal;
                }
            }
            if (cntPhotoRelated != 0) {
                foreach (var pr in PhotoRelated) {
                    pr.ImageName = MetadataName.OldDocImageNameFinal;
                    pr.FooterName = MetadataName.OlddocImageFooterNameFinal;
                }
            }
            if (cntReaderHelp != 0) {
                foreach (var rh in ReaderHelp) {
                    rh.NameText = MetadataName.OldDocReaderHelpTextNameFinal;
                    rh.NameTitle = MetadataName.OldDocReaderHelpTitleNameFinal;
                }
            }
            if (cntNewsGrouper != 0) {
                foreach (var ng in NewsGrouper) {
                    ng.TextName = MetadataName.OldDocNewsGrouperTextNameFinal;
                    ng.TitleName = MetadataName.OldDocNewsGrouperTitleNameFinal;
                }
            }
            if (cntCartoonGrouper != 0) {
                foreach (var cg in CartoonGrouper) {
                    cg.TitleName = MetadataName.OldDocCartoonGrouperTitleNameFinal;
                    cg.TextName = MetadataName.OldDocCartoonGrouperImageNameFinal;
                    cg.AuthorName = MetadataName.OldDocCarttonGrouperFooterNameFinal;
                }
            }
            if (cntPhotoGallery != 0) {
                foreach (var pg in PhotoGallery) {
                    pg.PhotoBigName = MetadataName.OldDocPhotoGalleryPhotoBigNameFinal;
                    pg.PhotoSmallName = MetadataName.OldDocPhotoGalleryPhotoSmallNameFinal;
                    pg.PhotoCreditName = MetadataName.OldDocPhotoGalleryPhotoCreditNameFinal;
                    pg.PhotoFooterName = MetadataName.OldDocPhotoGalleryPhotoFooterNameFinal;
                }
            }
        }

        public BodyContent DocumentText { get; set; }
        public CreditContent CreditText { get; set; }
        public ImageOnly ImageOnlySet { get; set; }
        public List<Title> Title { get; set; }
        public List<Bullet> Bullet { get; set; }
        public List<PhotoRelated> PhotoRelated { get; set; }
        public List<ReaderHelp> ReaderHelp { get; set; }
        public List<NewsGrouper> NewsGrouper { get; set; }
        public List<CartoonGrouper> CartoonGrouper { get; set; }
        public List<PhotoGallery> PhotoGallery { get; set; }
        public PhotoOnlyGallery PhotoFooterGallery { get; set; }
    }
}
