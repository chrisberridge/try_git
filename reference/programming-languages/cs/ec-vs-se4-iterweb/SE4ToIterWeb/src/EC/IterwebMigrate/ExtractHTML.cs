/*==========================================================================*/
/* Source File:   EXTRACTHTML.CS                                            */
/* Description:   Analyzes an HTML to extract its useful metadata           */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.06/2013                                               */
/* Last Modified: Nov.10/2014                                               */
/* Version:       1.30                                                      */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

using EC.IterwebMigrate.Domain;
using EC.Utils.Constants;
using EC.Utils.Domain;
using EC.Utils.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace EC.IterwebMigrate {
    /// <summary>
    /// Analyzes an HTML to extract its useful metadata
    /// </summary>
    public class ExtractHTML {
        /// <summary>
        /// Object that holds the structure to be saved as JSON.
        /// </summary>
        private Doc examinedDoc;

        /// <summary>
        /// Helper class to scan HTML DOM in .NET.
        /// </summary>
        private HtmlDocument page;

        /// <summary>
        /// This field is an aid to truly determine the actual template that has been used
        /// In some regard TemplateTypeCode 001 and 004 regions are the same but are
        /// taken from different metadata symbols. This is evidenced when using
        /// this class' retrieve methods.
        /// </summary>
        private int useTemplateTypeInstead;

        /// <summary>
        /// Is the document usable?
        /// See EC.Utils.Constants.MigrateStatusCode for explanation
        /// </summary>
        public int Status { get { return examinedDoc.Status; } }

        /// <summary>
        /// What template is actually to be used.
        /// </summary>
        public int TemplateType { get { return examinedDoc.TemplateType; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExtractHTML() {
            examinedDoc = new Doc();
            page = null;
        }

        /// <summary>
        /// Get page titles
        /// </summary>
        private void RetrieveTitles() {
            var titles = page.DocumentNode.SelectNodes("//span[@class='Titulo' or @class='titulo']");
            if (titles != null) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001;
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                }
            }
            else {
                titles = page.DocumentNode.SelectNodes("//td[@class='Titulo_Principal']");
                if (titles != null) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                    foreach (var t in titles) {
                        Title tit = new Title();
                        tit.Name = MetadataName.OldDocTitleName;
                        tit.Content = t.InnerHtml;
                        examinedDoc.Title.Add(tit);
                    }
                }
            }
        }

        /// <summary>
        /// Get document bullets
        /// </summary>
        private void RetrieveBullets() {
            var selectBullets = page.DocumentNode.SelectNodes("//span[@class='Balas']");
            if (selectBullets != null) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001;
                foreach (var bullet in selectBullets) {
                    if (bullet.InnerHtml != "") {
                        Bullet bl = new Bullet();
                        bl.Name = MetadataName.OldDocBulletName;
                        bl.Content = bullet.InnerHtml;
                        examinedDoc.Bullet.Add(bl);
                    }
                }
            }
            else {
                selectBullets = page.DocumentNode.SelectNodes("//td[@class='Subtitulo_Bala']");
                if (selectBullets != null) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                    foreach (var bullet in selectBullets) {
                        if (bullet.InnerHtml != "") {
                            Bullet bl = new Bullet();
                            bl.Name = MetadataName.OldDocBulletName;
                            bl.Content = bullet.InnerHtml;
                            examinedDoc.Bullet.Add(bl);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get page credit tags.
        /// </summary>
        private void RetrieveCreditContent() {
            string authorName = "";
            string cityName = "";
            string displayDateName = "";
            var mainNode = page.DocumentNode.SelectSingleNode("//*[@id='tdAutorCont']/table/tr");

            if (mainNode != null) {
                var cntItems = mainNode.ChildNodes.Count;

                displayDateName = page.DocumentNode.SelectSingleNode("//*[@id='tdFechaDoc']").InnerHtml;
                if (displayDateName == null) {
                    displayDateName = "";
                }
                if (cntItems == 5) {
                    var node = mainNode.ChildNodes[1];

                    if (node != null) {
                        authorName = node.InnerText;
                    }
                    node = mainNode.ChildNodes[2];
                    if (node != null) {
                        cityName = node.InnerText;
                    }
                }
                if (cntItems == 6) {
                    var node = mainNode.ChildNodes[1];

                    if (node != null) {
                        authorName = node.InnerText;
                    }
                    node = mainNode.ChildNodes[3];
                    if (node != null) {
                        cityName = node.InnerText;
                    }
                }

                if (authorName == "" && cityName == "" && displayDateName == "") {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                }
                else {
                    examinedDoc.CreditText.AuthorName = MetadataName.OldDocCreditAuthorName;
                    examinedDoc.CreditText.AuthorText = authorName;
                    examinedDoc.CreditText.CityName = MetadataName.OldDocCreditCityName;
                    examinedDoc.CreditText.CityText = cityName;
                    examinedDoc.CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateName;
                    examinedDoc.CreditText.DisplayDateText = displayDateName;
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                }
            }
        }

        /// <summary>
        /// Get page content.
        /// </summary>
        private void RetrieveBodyContent() {
            var texts = page.DocumentNode.SelectNodes("//span[@class='Texto']");
            if (texts != null) {
                if (texts.Count != 0) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001;
                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                    examinedDoc.DocumentText.Content = "";
                    foreach (var txt in texts) {
                        examinedDoc.DocumentText.Content += txt.InnerHtml;    
                    }
                }
            }
            else {
                var text = page.DocumentNode.SelectSingleNode("//*[@id='lEditor2']");
                if (text != null) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                    examinedDoc.DocumentText.Content = text.InnerHtml;
                }
                else {
                    var selectBullets = page.DocumentNode.SelectNodes("//span[@class='Balas']");
                    if (selectBullets != null) {
                        var node = selectBullets[selectBullets.Count - 1];
                        node = node.ParentNode.ParentNode.ParentNode.NextSibling;
                        if (node != null) {

                            string txt = "";
                            while (node != null) {
                                txt += node.OuterHtml;
                                node = node.NextSibling;
                            }
                            if (txt != null) {
                                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006;
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = txt;
                            }
                        }
                    }
                    else {
                        texts = page.DocumentNode.SelectNodes("//td[@class='Cuerpo_texto_nota_interior']");
                        if (texts != null) {
                            if (texts.Count != 0) {
                                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                                var txt = texts[0];
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = txt.InnerHtml;
                            }
                        }
                        else {
                            texts = page.DocumentNode.SelectNodes("//span[@class='Credito']");
                            if (texts != null) {
                                if (texts.Count != 0) {
                                    var txt = "";
                                    string tmp = "";
                                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                                    if (texts.Count == 1) {
                                        txt = texts[0].InnerHtml;
                                    }
                                    else {
                                        txt = texts[1].InnerHtml;
                                        var node = texts[0];
                                        var txtCounter = 0;
                                        while (node != null) {
                                            node = node.NextSibling;
                                            if (node.Name == "#text") {
                                                txtCounter++;
                                            }
                                            if (txtCounter == 2) {
                                                tmp += node.InnerText;
                                                break;
                                            }
                                        }
                                    }
                                    if (tmp != "") {
                                        txt += tmp;
                                        examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                        examinedDoc.DocumentText.Content = txt;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Related photos (these include an image with is footer) set aside.
        /// </summary>
        private void RetrieveRelatedPhotos() {
            var photoContentList = page.DocumentNode.SelectNodes("//td[@class='Cuerpo_Foto']");

            if (photoContentList != null) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004;
                int numItems = photoContentList.Count;
                var photoList = page.DocumentNode.SelectNodes("//table[@id='Table1']/tr[1]/td/img");
                var photoCreditList = page.DocumentNode.SelectNodes("//table[@id='Table1' and @bgcolor='#f4f7fc']/tr[2]/td");

                if (photoList != null && photoCreditList != null) {
                    for (int i = 0; i < numItems; i++) {
                        PhotoRelated pr = new PhotoRelated();
                        var imgSrcRef = photoList[i].Attributes["src"];
                        if (imgSrcRef != null) {
                            var s = imgSrcRef.Value;
                            if (s.Contains("BancoMedios/Imagenes")) {
                                pr.ImageName = MetadataName.OldDocImageName;
                                pr.ImageSrc = s;
                            }
                            else {
                                pr.ImageName = MetadataName.OldDocImageName;
                                pr.ImageSrc = "";
                            }
                        }
                        pr.FooterName = MetadataName.OldDocImageFooterName;
                        pr.Footer = photoCreditList[i].InnerHtml + "<br>" + photoContentList[i].InnerHtml;
                        examinedDoc.PhotoRelated.Add(pr);
                    }
                }
            }
            else {
                List<string> imageNameList = null;
                List<string> imgFooterNameList = null;
                var images = page.DocumentNode.SelectNodes("//img");

                if (images != null) {
                    imageNameList = new List<string>();
                    foreach (var img in images) {
                        var imgSrcRef = img.Attributes["src"];
                        if (imgSrcRef != null) {
                            var s = imgSrcRef.Value;
                            if (s.Contains("BancoMedios/Imagenes")) {
                                imageNameList.Add(s);
                            }
                        }
                    }
                }

                var imgFooter = page.DocumentNode.SelectNodes("//td[@class='PieFoto']");
                if (imgFooter != null) {
                    imgFooterNameList = new List<string>();
                    foreach (var imf in imgFooter) {
                        var footerTxt = imf.InnerHtml;
                        if (footerTxt != "") {
                            imgFooterNameList.Add(imf.InnerText);
                        }                        
                    }
                }

                // It is required that both imageNameList and imgFooterNameList
                // both contain the same number of elements
                if (imageNameList != null && imgFooterNameList != null) {
                    if (imgFooterNameList.Count == imageNameList.Count) {
                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001;
                        for (int i = 0; i < imageNameList.Count; i++) {
                            PhotoRelated pr = new PhotoRelated();
                            var imageNameContent = imageNameList[i];
                            var imageFooterNameContent = imgFooterNameList[i];

                            pr.ImageName = MetadataName.OldDocImageName;
                            pr.ImageSrc = imageNameContent;
                            pr.FooterName = MetadataName.OldDocImageFooterName;
                            pr.Footer = imageFooterNameContent;
                            examinedDoc.PhotoRelated.Add(pr);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the reader help section about the document being examined.
        /// </summary>
        private void RetrieveReaderHelp() {
            List<string> helpTitleNameList = null;
            List<string> helpTextNameList = null;

            var helpTitles = page.DocumentNode.SelectNodes("//span[@class='AyudaLector_Titulo']");
            if (helpTitles != null) {
                helpTitleNameList = new List<string>();
                foreach (var ht in helpTitles) {
                    helpTitleNameList.Add(ht.InnerHtml);
                }
            }

            var helpTexts = page.DocumentNode.SelectNodes("//span[@class='AyudaLector_Texto']");
            if (helpTexts != null) {
                helpTextNameList = new List<string>();
                foreach (var htxt in helpTexts) {
                    helpTextNameList.Add(htxt.InnerHtml);
                }
            }

            // It is required that both helpTitleNameList and helpTextNameList
            // both contain the same number of elements
            if (helpTitleNameList != null && helpTextNameList != null) {
                if (helpTitleNameList.Count == helpTextNameList.Count) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001;
                    for (int i = 0; i < helpTitleNameList.Count; i++) {
                        ReaderHelp rh = new ReaderHelp();

                        rh.NameTitle = MetadataName.OldDocReaderHelpTitleName;
                        rh.NameTitleContent = helpTitleNameList[i];
                        rh.NameText = MetadataName.OldDocReaderHelpTextName;
                        rh.NameTextContent = helpTextNameList[i];
                        examinedDoc.ReaderHelp.Add(rh);
                    }
                }
            }
        }

        /// <summary>
        /// Get the News Grouper section about the document being examined.
        /// </summary>
        private void RetrieveNewsGrouper() {
            var newsGrouperTitles = page.DocumentNode.SelectNodes("//span[@class='TituloTerciario']");
            if (newsGrouperTitles != null) {
                foreach (var ngt in newsGrouperTitles) {
                    NewsGrouper ng = new NewsGrouper();
                    ng.TitleName = MetadataName.OldDocNewsGrouperTitleName;
                    ng.TextName = MetadataName.OldDocNewsGrouperTextName;
                    ng.TitleContent = ngt.InnerHtml;

                    var node = ngt.NextSibling;
                    var oldNode = node;
                    string txtData = "";
                    while (node != null) {
                        if (node == null) {
                            break;
                        }
                        if (node.Name.ToUpper() == "TABLE") {
                            node = node.PreviousSibling;
                            break;
                        }
                        txtData += node.OuterHtml;
                        node = node.NextSibling;
                    }
                    ng.TextContent = txtData.Replace("\r\n", "").Trim();
                    examinedDoc.NewsGrouper.Add(ng);
                }
                if (examinedDoc.NewsGrouper.Count != 0) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002;
                }
            }
        }

        /// <summary>
        /// Get the Cartoon Grouper section about the document being examined.
        /// </summary>
        private void RetrieveCartoonGrouper() {
            var cartoonGrouperTitles = page.DocumentNode.SelectNodes("//span[@class='AyudaLector_Titulo']");
            var cartoonGrouperImages = page.DocumentNode.SelectNodes("//span//img");
            var cartoonGrouperFooter = page.DocumentNode.SelectNodes("//span[@class='PieFoto']");

            // Apparently the HTML uses three PieFoto, but we are only interested in second one.            
            if (cartoonGrouperTitles != null && cartoonGrouperImages != null && cartoonGrouperFooter != null) {
                var cntCartoonGouperFooter = cartoonGrouperFooter.Count - 2;
                if (cartoonGrouperTitles.Count == cartoonGrouperImages.Count &&
                    cartoonGrouperTitles.Count == cntCartoonGouperFooter &&
                    cartoonGrouperImages.Count == cntCartoonGouperFooter) {
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003;
                    for (int i = 0; i < cartoonGrouperTitles.Count; i++) {
                        var title = cartoonGrouperTitles[i].InnerHtml;
                        var image = cartoonGrouperImages[i].Attributes["src"].Value;
                        var footer = "";

                        // Author name is stored in second position in array or one more than indexer 'i' value.
                        var j = i + 1;
                        if (j < cartoonGrouperFooter.Count) {
                            footer = cartoonGrouperFooter[j].InnerHtml;
                        }
                        CartoonGrouper cg = new CartoonGrouper() {
                            TitleName = MetadataName.OldDocCartoonGrouperTitleName,
                            TitleContent = title,
                            TextName = MetadataName.OldDocCartoonGrouperImageName,
                            TextContent = image,
                            AuthorName = MetadataName.OldDocCarttonGrouperFooterName,
                            AuthorContent = footer
                        };
                        examinedDoc.CartoonGrouper.Add(cg);
                    }
                }
            }
        }

        /// <summary>
        /// Get the Photo Gallery section about the document being examined.
        /// </summary>
        private void GetTemplatePhotoGallery() {
            var titles = page.DocumentNode.SelectNodes("//span[@class='AyudaLector_Titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                }
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005;
            }

            // Let's continue evaluating this template compilation only if there is a title set.
            if (examinedDoc.Title.Count != 0) {
                var docEntryText = page.DocumentNode.SelectSingleNode("//td[@class='PieFoto']");
                if (docEntryText != null) {
                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                    examinedDoc.DocumentText.Content = docEntryText.InnerHtml;
                    useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005;
                }
                else {
                    docEntryText = page.DocumentNode.SelectSingleNode("//td[@align='left' and @valign='top']");
                    if (docEntryText != null) {
                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005;
                        examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                        examinedDoc.DocumentText.Content = docEntryText.InnerHtml;
                    }
                }

                var bigPhotoList = page.DocumentNode.SelectNodes("//input[@type='hidden' and @name='FotoGrande']");
                var creditList = page.DocumentNode.SelectNodes("//input[@type='hidden' and @name='Credito']");
                var footerList = page.DocumentNode.SelectNodes("//input[@type='hidden' and @name='PieFoto']");
                if (bigPhotoList != null && creditList != null && footerList != null) {
                    var cntBigPhotoList = bigPhotoList.Count;
                    var cntCreditList = creditList.Count;
                    var cntFooterList = footerList.Count;

                    if (!((cntBigPhotoList == cntCreditList) && (cntBigPhotoList == cntFooterList) && (cntCreditList == cntFooterList))) {
                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                    }
                    else {
                        String[] validImgExtensions = { ".gif", ".jpg" };
                        for (int i = 0; i < cntBigPhotoList; i++) {
                            var smallPhotoVal = bigPhotoList[i].Attributes["value"];
                            var creditVal = creditList[i].Attributes["value"];
                            var footerVal = footerList[i].Attributes["value"];
                            PhotoGallery pg = new PhotoGallery() {
                                PhotoBigName = MetadataName.OldDocPhotoGalleryPhotoBigName, PhotoBigContent = smallPhotoVal.Value.IncludeInsideToImageName("_g", validImgExtensions).Trim(),
                                PhotoSmallName = MetadataName.OldDocPhotoGalleryPhotoSmallName, PhotoSmallContent = smallPhotoVal.Value.Trim(),
                                PhotoCreditName = MetadataName.OldDocPhotoGalleryPhotoCreditName, PhotoCreditContent = creditVal.Value,
                                PhotoFooterName = MetadataName.OldDocPhotoGalleryPhotoFooterName, PhotoFooterContent = footerVal.Value
                            };
                            examinedDoc.PhotoGallery.Add(pg);
                        }
                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005;
                    }
                }
            }
        }

        /// <summary>
        /// Try to gather a title, a content, a bullet, and a side image. For template to be considered usable, all these
        /// items must be present.
        /// </summary>
        private void GetTemplateTitlePhotoContent() {
            bool isTitleSet, isContentSet, isBulletSet, isLeftPhotoSet;
            isTitleSet = isContentSet = isBulletSet = isLeftPhotoSet = false;

            var titles = page.DocumentNode.SelectNodes("//span[@class='Titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                }
                isTitleSet = true;
            }
            var selectBullets = page.DocumentNode.SelectNodes("//span[@class='Balas']");
            if (selectBullets != null) {
                foreach (var bullet in selectBullets) {
                    if (bullet.InnerHtml != "") {
                        Bullet bl = new Bullet();
                        bl.Name = MetadataName.OldDocBulletName;
                        bl.Content = bullet.InnerHtml;
                        examinedDoc.Bullet.Add(bl);
                    }
                }
                isBulletSet = true;
            }
            var images = page.DocumentNode.SelectNodes("//td/img");
            List<String> imageNameList = new List<String>();
            if (images != null) {
                imageNameList = new List<string>();
                foreach (var img in images) {
                    var imgSrcRef = img.Attributes["src"];
                    if (imgSrcRef != null) {
                        var s = imgSrcRef.Value;
                        if (s.Contains("BancoMedios/Imagenes")) {
                            imageNameList.Add(s);
                        }
                    }
                }
                if (imageNameList.Count != 0) {
                    examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                    examinedDoc.ImageOnlySet.Content = imageNameList[0];
                    isLeftPhotoSet = true;
                }
            }

            // We are now about to try to catch the content paragraph.
            // It happens that there are two paths about it.
            // We start from Title node.
            var node = page.DocumentNode.SelectSingleNode("//span[@class='Titulo']");
            if (node != null) {
                node = node.ParentNode.NextSibling;

                int numBR = 1;
                while (node != null && numBR <= 2) {
                    node = node.NextSibling;
                    if (node == null) {
                        break;
                    }
                    if (node.Name.ToLower() == "br") {
                        numBR++;
                        if (numBR == 2) {
                            break;
                        }
                    }
                }
                if (node != null) {
                    node = node.NextSibling;
                }
                if (node != null) {
                    while (node != null) {
                        if (node.Name.ToLower() == "table") {
                            break;
                        }
                        else {
                            if (node.Name.ToLower() == "br") {
                                break;
                            }
                        }
                        node = node.NextSibling;
                    }
                }
                if (node != null) {
                    node = node.NextSibling;
                }

                var txt = "";
                if (node != null) {
                    txt = node.InnerHtml;
                }

                if (txt != "") {
                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                    examinedDoc.DocumentText.Content = txt;
                    isContentSet = true;
                }
            }
            if (isTitleSet && isContentSet && isBulletSet && isLeftPhotoSet) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_007;
            }
            else {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                examinedDoc.Reset();
            }
        }

        /// <summary>
        /// Try to gather a title, a content, a bullet, and a side image with a footer. For template to be considered usable, all these
        /// items must be present.
        /// </summary>
        private void GetTemplateTitleBulletPhotoFooterContent() {
            bool isTitleSet, isContentSet, isBulletSet, isLeftPhotoFooterSet;
            isTitleSet = isContentSet = isBulletSet = isLeftPhotoFooterSet = false;

            var titles = page.DocumentNode.SelectNodes("//div[@class='Titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                }
                isTitleSet = true;
            }
            HtmlNode documentTextNode = null;
            var selectBullets = page.DocumentNode.SelectNodes("//p[@class='Balas']/img");
            if (selectBullets != null) {
                foreach (var bullet in selectBullets) {
                    var node = bullet.NextSibling;
                    if (node.InnerHtml != "") {
                        Bullet bl = new Bullet();
                        bl.Name = MetadataName.OldDocBulletName;
                        bl.Content = node.InnerHtml.Replace("\r\n", "").Trim();
                        examinedDoc.Bullet.Add(bl);
                    }
                }
                isBulletSet = true;
                if (selectBullets.Count != 0) {
                    documentTextNode = selectBullets[selectBullets.Count - 1];
                }
            }
            if (documentTextNode != null) {
                documentTextNode = documentTextNode.ParentNode.NextSibling;
                string txt = "";
                while (documentTextNode != null) {
                    txt += documentTextNode.OuterHtml;
                    documentTextNode = documentTextNode.NextSibling;
                    while (documentTextNode != null) {
                        if (documentTextNode.Name.ToUpper() != "P") {
                            documentTextNode = documentTextNode.NextSibling;
                        }
                        if (documentTextNode == null) {
                            break;
                        }
                        if (documentTextNode.Name.ToUpper() == "P") {
                            break;
                        }
                    }
                }
                if (txt != null && txt != "") {
                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                    examinedDoc.DocumentText.Content = txt;
                    isContentSet = true;
                }
            }

            var images = page.DocumentNode.SelectNodes("//td/img");
            var footers = page.DocumentNode.SelectNodes("//td[@class='PieFoto']");
            List<String> imageNameList = new List<String>();
            if (images != null && footers != null) {
                imageNameList = new List<string>();
                foreach (var img in images) {
                    var imgSrcRef = img.Attributes["src"];
                    if (imgSrcRef != null) {
                        var s = imgSrcRef.Value;
                        if (s.Contains("BancoMedios/Imagenes")) {
                            imageNameList.Add(s);
                        }
                    }
                }
                if (imageNameList.Count != 0) {
                    for (int i = 0; i < imageNameList.Count; i++) {
                        PhotoRelated pr = new PhotoRelated();
                        pr.ImageName = MetadataName.OldDocImageName;
                        pr.ImageSrc = imageNameList[i];
                        pr.FooterName = MetadataName.OldDocImageFooterName;
                        pr.Footer = footers[i].InnerHtml;
                        examinedDoc.PhotoRelated.Add(pr);
                    }
                    isLeftPhotoFooterSet = true;
                }
            }
            if (isTitleSet && isContentSet && isBulletSet && isLeftPhotoFooterSet) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_008;
            }
            else {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                examinedDoc.Reset();
            }
        }

        /// <summary>
        /// Try to gather a title, a content, a bullet. For template to be considered usable, all these
        /// items must be present.
        /// </summary>
        private void GetTemplateTitleBulletCreditContent() {
            bool isTitleSet, isCreditSet, isContentSet, isBulletSet;
            isTitleSet = isCreditSet = isContentSet = isBulletSet = false;

            var titles = page.DocumentNode.SelectNodes("//td[@name='tbTituloDoc']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                }
                isTitleSet = true;
            }

            var selectBullets = page.DocumentNode.SelectNodes("//td[@class='Subtitulo_Bala']");
            if (selectBullets != null) {
                foreach (var bullet in selectBullets) {
                    var node = bullet;
                    if (node.InnerHtml != "") {
                        Bullet bl = new Bullet();
                        bl.Name = MetadataName.OldDocBulletName;
                        bl.Content = node.InnerHtml.Replace("\r\n", "").Trim();
                        examinedDoc.Bullet.Add(bl);
                    }
                }
                isBulletSet = true;
            }

            var nodeContent = page.DocumentNode.SelectSingleNode("//td[@class='Cuerpo_texto_nota_interior']");
            if (nodeContent != null) {
                var txt = "";
                var nodeContentChildren = nodeContent.ChildNodes;
                if (nodeContentChildren != null) {
                    foreach (var p in nodeContentChildren) {
                        if (p.Name.ToUpper() == "P") {
                            txt += p.OuterHtml;
                        }
                    }
                    if (txt != null && txt != "") {
                        examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                        examinedDoc.DocumentText.Content = txt;
                        isContentSet = true;
                    }
                }
            }
            var nodeCredit = page.DocumentNode.SelectSingleNode("//table[@class='Credito_Periodista']");
            if (nodeCredit != null) {
                var nodeCreditChildren = nodeCredit.ChildNodes;
                if (nodeCreditChildren != null && nodeCreditChildren.Count == 1) {
                    var n0 = nodeCreditChildren[0];
                    var n0Children = n0.ChildNodes;
                    if (n0Children != null && n0Children.Count == 6) {
                        var author = n0Children[1].InnerText;
                        var city = n0Children[3].InnerText;
                        var displayData = n0Children[5].InnerText;

                        if (author != null && city != null && displayData != null) {
                            if (author != "" && city != "" && displayData != "") {
                                examinedDoc.CreditText.AuthorName = MetadataName.OldDocCreditAuthorName;
                                examinedDoc.CreditText.AuthorText = author;
                                examinedDoc.CreditText.CityName = MetadataName.OldDocCreditCityName;
                                examinedDoc.CreditText.CityText = city;
                                examinedDoc.CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateName;
                                examinedDoc.CreditText.DisplayDateText = displayData;
                                isCreditSet = true;
                            }
                        }
                    }
                }
            }
            if (isTitleSet && isContentSet && isBulletSet && isCreditSet) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_009;
            }
            else {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                examinedDoc.Reset();
            }
        }

        /// <summary>
        /// Parses a template trying to gather Title, ImageOnly and Body content.
        /// </summary>
        private bool GetTemplateTitleImageOnlyContent() {
            bool rslt = false;
            bool isTitleSet = false;
            bool isImageOnlySet = false;
            bool isContentSet = false;
            bool isPhotoRelatedSet = false;

            // up to this point we are sure that the Title for this template
            // is correctly gathered 
            isTitleSet = (examinedDoc.Title.Count > 0);

            // If isContentSet and isPhotoRelatedSet then this template must not be checked.
            isContentSet = (examinedDoc.DocumentText.Name != "" && examinedDoc.DocumentText.Content != "");
            isPhotoRelatedSet = (examinedDoc.PhotoRelated.Count > 0);

            if (isTitleSet && isContentSet && isPhotoRelatedSet) {
                return false;
            }

            // Now let's try to get Image and Content
            var imgList = page.DocumentNode.SelectNodes("//img[@border='0']");
            if (imgList != null) {
                int numItems = imgList.Count;
                for (int i = 0; i < numItems; i++) {
                    var imgSrcRef = imgList[i].Attributes["src"];
                    if (imgSrcRef != null) {
                        var s = imgSrcRef.Value;
                        if (s.Contains("BancoMedios/Imagenes")) {
                            examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                            examinedDoc.ImageOnlySet.Content = s;
                            isImageOnlySet = true;
                            break;
                        }
                    }
                }
            }

            var texts = page.DocumentNode.SelectNodes("//td[@width= '180' and @valign='top']");
            if (texts != null) {
                var txt = texts[0];
                if (txt.HasChildNodes) {
                    var txtChildNodes = txt.ChildNodes;
                    var cntChildNodes = txtChildNodes.Count;
                    if (cntChildNodes >= 5) {
                        var txtData = txtChildNodes[4].InnerHtml;
                        if (txtData != "") {
                            examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                            examinedDoc.DocumentText.Content = txtData;
                            isContentSet = true;
                        }
                    }
                }
            }

            if (isTitleSet && isImageOnlySet && isContentSet) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010;
                rslt = true;
            }
            else {
                // Clear imageOnlySet as all items were not fully completed.
                // NOTE: Title is already set apparently
                if (isImageOnlySet) {
                    examinedDoc.ImageOnlySet.Name = "";
                    examinedDoc.ImageOnlySet.Content = "";
                }
            }
            return rslt;
        }

        /// <summary>
        /// Parses document to locate only Title, Credit, Body Content elements only.
        /// </summary>
        /// <returns>True if a match is found</returns>
        private bool GetTemplateTitletBodyContentCreditOnlyContent() {
            bool rslt = false;
            var isTitleSet = false;
            var isCreditSet = false;
            var isBodyContentSet = false;

            var titles = page.DocumentNode.SelectNodes("//h1[@id='titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                    isTitleSet = true;
                }
            }

            var signatureDiv = page.DocumentNode.SelectSingleNode("//div[@id='firma']");
            if (signatureDiv != null) {
                var node = signatureDiv.FirstChild;

                if (node.FirstChild != null) {
                    var authorName = node.FirstChild.InnerHtml;

                    node = node.NextSibling;
                    var cityName = node.InnerHtml.Replace("|", "").Trim();

                    node = node.NextSibling;
                    var displayDateName = node.InnerHtml;

                    examinedDoc.CreditText.AuthorName = MetadataName.OldDocCreditAuthorName;
                    examinedDoc.CreditText.AuthorText = authorName;
                    examinedDoc.CreditText.CityName = MetadataName.OldDocCreditCityName;
                    examinedDoc.CreditText.CityText = cityName;
                    examinedDoc.CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateName;
                    examinedDoc.CreditText.DisplayDateText = displayDateName;
                    isCreditSet = true;
                }
            }

            var bodyContent = page.DocumentNode.SelectSingleNode("//div[@id='segundoParrafo']");
            if (bodyContent != null) {
                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                examinedDoc.DocumentText.Content = bodyContent.InnerHtml;
                isBodyContentSet = true;
            }

            rslt = isTitleSet && isCreditSet && isBodyContentSet;
            if (rslt) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_011;
            }

            return rslt;
        }

        /// <summary>
        /// Tries to match Title, Credit/signature, Image Only and body Content only.
        /// NOTE: This method is called only if 'examinedDoc' is empty.
        /// </summary>
        /// <returns>True if a match is found</returns>
        private bool GetTemplateTitleImageOnlyBodyContentCreditOnlyContent() {
            bool rslt = false;
            var isTitleSet = false;
            var isImageOnlySet = false;
            var isBodyContentSet = false;
            var isCreditSet = false;

            var titles = page.DocumentNode.SelectNodes("//h1[@id='titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                    isTitleSet = true;
                }
            }

            var mediaImage = page.DocumentNode.SelectSingleNode("//div[@class='medioIzquierdaNotaInterior']");
            if (mediaImage != null) {
                var imgRef = mediaImage.FirstChild;
                if (imgRef != null) {
                    var imgRefSrc = imgRef.Attributes["src"];
                    if (imgRefSrc != null) {
                        examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                        examinedDoc.ImageOnlySet.Content = imgRefSrc.Value;
                        isImageOnlySet = true;
                    }
                }
            }
            var signatureDiv = page.DocumentNode.SelectSingleNode("//div[@id='firma']");
            if (signatureDiv != null) {
                var node = signatureDiv.FirstChild;

                if (node != null) {
                    var authorName = node.InnerHtml.Replace("|", "").Trim();

                    node = node.NextSibling;
                    var cityName = node.InnerHtml.Replace("|", "").Trim();

                    node = node.NextSibling;
                    var displayDateName = node.InnerHtml.Replace("\r\n", "").Trim();

                    if (displayDateName == "") {
                        displayDateName = cityName;
                        cityName = authorName;
                        authorName = "";
                    }

                    examinedDoc.CreditText.AuthorName = MetadataName.OldDocCreditAuthorName;
                    examinedDoc.CreditText.AuthorText = authorName;
                    examinedDoc.CreditText.CityName = MetadataName.OldDocCreditCityName;
                    examinedDoc.CreditText.CityText = cityName;
                    examinedDoc.CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateName;
                    examinedDoc.CreditText.DisplayDateText = displayDateName;
                    isCreditSet = true;
                }
            }

            // Body content is stored in a context DIV.
            var contentTitle = page.DocumentNode.SelectSingleNode("//div[@class='tituloSubseccionPrincipal']");
            var contentText = page.DocumentNode.SelectSingleNode("//div[@class='contenidoContexto']");
            var extractContentText = "";
            if (contentTitle != null && contentText != null) {
                extractContentText = contentTitle.InnerHtml + contentText.InnerHtml;
                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                examinedDoc.DocumentText.Content = extractContentText.Trim();
                isBodyContentSet = true;
            }

            rslt = isTitleSet && isCreditSet && isImageOnlySet && isBodyContentSet;
            if (rslt) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_013;
            }
            else {
                // Clear any contents found so far as none of all items were found.
                examinedDoc.Reset();
            }
            return rslt;
        }

        /// <summary>
        /// Tries to match Title, Credit/signature, and body Content only.
        /// NOTE: This method is called only if 'examinedDoc' is empty.
        /// </summary>
        /// <returns>True if a match is found</returns>
        private bool GetTemplateTitletBodyContentCreditOnlyContentOtherMarkup() {
            bool rslt = false;
            var isTitleSet = false;
            var isBodyContentSet = false;
            var isCreditSet = false;

            var titles = page.DocumentNode.SelectNodes("//h1[@id='titulo']");
            if (titles != null) {
                foreach (var t in titles) {
                    Title tit = new Title();
                    tit.Name = MetadataName.OldDocTitleName;
                    tit.Content = t.InnerHtml;
                    examinedDoc.Title.Add(tit);
                    isTitleSet = true;
                }
            }

            var signatureDiv = page.DocumentNode.SelectSingleNode("//div[@id='firma']");
            if (signatureDiv != null) {
                var node = signatureDiv.FirstChild;

                if (node != null) {
                    var authorName = node.InnerHtml.Replace("|", "").Trim();

                    node = node.NextSibling;
                    var cityName = node.InnerHtml.Replace("|", "").Trim();

                    node = node.NextSibling;
                    var displayDateName = node.InnerHtml.Replace("\r\n", "").Trim();

                    if (displayDateName == "") {
                        displayDateName = cityName;
                        cityName = authorName;
                        authorName = "";
                    }

                    examinedDoc.CreditText.AuthorName = MetadataName.OldDocCreditAuthorName;
                    examinedDoc.CreditText.AuthorText = authorName;
                    examinedDoc.CreditText.CityName = MetadataName.OldDocCreditCityName;
                    examinedDoc.CreditText.CityText = cityName;
                    examinedDoc.CreditText.DisplayDateName = MetadataName.OldDocCreditDisplayDateName;
                    examinedDoc.CreditText.DisplayDateText = displayDateName;
                    isCreditSet = true;
                }
            }

            // Body content is stored in a context DIV.
            var contentTitle = page.DocumentNode.SelectSingleNode("//div[@class='tituloSubseccionPrincipal']");
            var contentText = page.DocumentNode.SelectSingleNode("//div[@class='contenidoContexto']");
            var extractContentText = "";
            if (contentTitle != null && contentText != null) {
                extractContentText = contentTitle.InnerHtml + contentText.InnerHtml;
                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                examinedDoc.DocumentText.Content = extractContentText.Trim();
                isBodyContentSet = true;
            }

            rslt = isTitleSet && isCreditSet && isBodyContentSet;
            if (rslt) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_014;
            }
            else {
                // Clear any contents found so far as none of all items were found.
                examinedDoc.Reset();
            }
            return rslt;
        }

        /// <summary>
        /// Tries to match Title, body Content only and a photo gallery with one footer in it.
        /// NOTE: This method is called only if 'examinedDoc' is empty.
        /// </summary>
        /// <returns>True if a match is found</returns>
        private bool GetTemplateTitleBodyContentPhotoFooterGalleryContent() {
            bool rslt = false;
            var isTitleSet = false;
            var isBodyContentSet = false;
            var isPhotoFooterGallerySet = false;
            var contentTextFirst = "";
            var contentTextSecond = "";

            var titles = page.DocumentNode.SelectNodes("//span[@class='TituloPrincipal']");
            if (titles != null) {
                Title tit = new Title();
                tit.Name = MetadataName.OldDocTitleName;
                tit.Content = titles[0].InnerHtml;
                examinedDoc.Title.Add(tit);
                isTitleSet = true;
            }

            var images = page.DocumentNode.SelectNodes("//img");
            if (images != null) {
                var imageNameList = new List<string>();
                foreach (var img in images) {
                    var imgSrcRef = img.Attributes["src"];
                    if (imgSrcRef != null) {
                        var s = imgSrcRef.Value;
                        if (s.Contains("BancoMedios/Imagenes")) {
                            imageNameList.Add(s);
                        }
                    }
                }
                if (imageNameList != null) {
                    foreach (var img in imageNameList) {
                        PhotoOnly pho = new PhotoOnly();
                        pho.ImageName = MetadataName.OldDocImageName;
                        pho.ImageSrc = img;
                        examinedDoc.PhotoFooterGallery.PhotoList.Add(pho);
                    }
                }
            }

            var footerText = "";
            if (titles != null) {
                if (titles.Count >= 2) {
                    examinedDoc.PhotoFooterGallery.FooterTitleName = MetadataName.OldDocPhotoOnlyGalleryImageFooterTitleName;
                    examinedDoc.PhotoFooterGallery.FoooterTitle = titles[1].InnerHtml.Trim();

                    var node = titles[1];
                    while (node != null) {
                        if (node.Name.ToUpper() == "P") {
                            break;
                        }
                        node = node.NextSibling;
                    }
                    if (node != null) {
                        node = node.NextSibling;
                        while (node != null) {
                            footerText += node.InnerHtml.Replace("\r\n", "").Trim();
                            node = node.NextSibling;
                        }
                        if (footerText != "") {
                            examinedDoc.PhotoFooterGallery.FooterName = MetadataName.OldDocPhotoOnlyGalleryImageFooterName;
                            examinedDoc.PhotoFooterGallery.Footer = footerText;
                        }
                    }
                }
            }
            isPhotoFooterGallerySet = !examinedDoc.PhotoFooterGallery.IsEmpty();

            // Let's retrieve Body content text.
            // In fact for this template, it is layout in two different parts.
            if (titles != null) {
                // First part
                if (titles.Count >= 2) {
                    var node = titles[0];
                    while (node != null) {
                        if (node.Name.ToUpper() == "P") {
                            break;
                        }
                        node = node.NextSibling;
                    }
                    if (node != null) {
                        contentTextFirst = node.InnerHtml;
                    }
                }
            }

            // Second Part retrieval.
            var nodeText = page.DocumentNode.SelectSingleNode("//p[@align='left']");
            if (nodeText != null) {
                contentTextSecond = nodeText.InnerHtml;
            }

            if (contentTextFirst != "" && contentTextSecond != "") {
                var s = contentTextFirst + contentTextSecond;
                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                examinedDoc.DocumentText.Content = s.Trim();
                isBodyContentSet = true;
            }

            rslt = isTitleSet && isPhotoFooterGallerySet && isBodyContentSet;
            if (rslt) {
                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_015;
            }
            else {
                // Clear any contents found so far as none of all items were found.
                examinedDoc.Reset();
            }
            return rslt;
        }

        /// <summary>
        /// If parameters match then try to get ReaderHelp contents if found.
        /// </summary>
        /// <param name="status">Document Status</param>
        /// <param name="templateType">Template used</param>
        private void CheckReaderHelpFor(int status, int templateType) {
            if (status == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS && templateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004) {
                if (examinedDoc.ReaderHelp.Count == 0) {
                    var text = page.DocumentNode.SelectSingleNode("//*[@id='lEditor3']");
                    var textTitle = page.DocumentNode.SelectSingleNode("//*[@class='Titulo_Contexto']");
                    if (text != null) {
                        var textInfo = text.InnerHtml;
                        string textTitleInfo = "";
                        if (textTitle != null) {
                            textTitleInfo = textTitle.InnerHtml;
                        }
                        ReaderHelp rh = new ReaderHelp();
                        rh.NameTitle = MetadataName.OldDocReaderHelpTitleName;
                        rh.NameTitleContent = textTitleInfo;
                        rh.NameText = MetadataName.OldDocReaderHelpTextName;
                        rh.NameTextContent = textInfo;
                        examinedDoc.ReaderHelp.Add(rh);
                    }
                }
            }
        }

        /// <summary>
        /// Given the templateTypeCode parameter, it tries to catch other fields necessary for that template type
        /// before giving up to not use the doc in that template type.
        /// </summary>
        /// <param name="html">The raw HTML if needed to extract from it</param>
        /// <param name="kind">A number which indicates what portion to check to map against 'templateTypeCode'</param>
        /// <param name="templateTypeCode">Match fields for this template type code.</param>
        /// <returns>true if mapped document match field in template type code</returns>
        private bool MapIntoExistingTemplate(string html, int kind, int templateTypeCode) {
            bool rslt = false;
            HtmlNode node = null;
            HtmlNode title = null;
            HtmlNodeCollection nodes = null;
            HtmlNodeCollection images = null;
            bool isTitleSet = false;
            bool isImageOnlySet = false;
            bool isBodyContentSet = false;
            bool firstTR = true;
            List<String> imageNameList = null;
            string tableHtml = null;
            string txt = "";
            string txtAll = "";

            switch (templateTypeCode) {
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010:
                    switch (kind) {
                        case 1:
                            // Title (already set so far)
                            isTitleSet = true;

                            // ImageOnly
                            // NOTE: Image is contained inside a javascript function inside HTML, 
                            // that said, it is needed to be extracted by hand.
                            string ss = "";
                            var pos1 = html.IndexOf("imagenes[1].src");
                            var pos2 = html.IndexOf("imagenes[2].src");

                            if (pos1 != -1 && pos2 != -1) {
                                ss = html.Substring(pos1, pos2 - pos1 + 1);
                                if (ss != "") {
                                    var extracted = ss.ExtractCharactersUsingDelimiters('"', '"');
                                    if (extracted == "") {
                                        extracted = ss.ExtractCharactersUsingDelimiters('\'', '\'');
                                    }
                                    if (extracted != "") {
                                        examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                                        examinedDoc.ImageOnlySet.Content = extracted;
                                        isImageOnlySet = true;
                                    }
                                }
                            }

                            // DocumentText
                            node = page.DocumentNode.SelectSingleNode("//img[@name='secuencia']");
                            if (node != null) {
                                node = node.NextSibling;
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = node.InnerHtml;
                                isBodyContentSet = true;
                            }
                            rslt = isTitleSet && isImageOnlySet && isBodyContentSet;

                            // If not matched then clean up.
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.ImageOnlySet.Name = examinedDoc.ImageOnlySet.Content = "";
                            }
                            break;
                        case 2:
                            // Let's try Title
                            title = page.DocumentNode.SelectSingleNode("//td[@class='TituloFicha']");
                            if (title != null) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = title.InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // Let's try ImageOnly
                            images = page.DocumentNode.SelectNodes("//td/img");
                            imageNameList = new List<String>();
                            if (images != null) {
                                imageNameList = new List<string>();
                                foreach (var img in images) {
                                    var imgSrcRef = img.Attributes["src"];
                                    if (imgSrcRef != null) {
                                        var s = imgSrcRef.Value;
                                        if (s.Contains("BancoMedios/Imagenes")) {
                                            imageNameList.Add(s);
                                        }
                                    }
                                }
                                if (imageNameList.Count != 0) {
                                    examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                                    examinedDoc.ImageOnlySet.Content = imageNameList[0];
                                    isImageOnlySet = true;
                                }
                            }

                            // Let's try DocumentText (here, DocumentText is two parts, one for a table and second a text.
                            // table is to be emitted unformatted.
                            tableHtml = "<table width='100%' border='0' align='center' cellpadding='1' cellspacing='0'>";
                            txt = "";
                            firstTR = true;
                            nodes = page.DocumentNode.SelectNodes("//table[@bgcolor='#202020']/tr");
                            if (nodes != null) {
                                foreach (var n in nodes) {
                                    tableHtml += "<tr>";
                                    if (n.HasChildNodes) {
                                        var children = n.ChildNodes;
                                        int numTD = 1;
                                        foreach (var ntd in children) {
                                            if (ntd.Name.ToUpper() == "TD") {
                                                if (firstTR) {
                                                    switch (numTD) {
                                                        case 1:
                                                            tableHtml += "<td width='23%'>" + ntd.InnerText + "</td>";
                                                            break;
                                                        case 2:
                                                            tableHtml += "<td width='77%'>" + ntd.InnerText + "</td>";
                                                            break;
                                                    }
                                                    numTD++;
                                                }
                                                else {
                                                    tableHtml += "<td>" + ntd.InnerText + "</td>";
                                                }
                                            }
                                        }
                                        firstTR = false;
                                    }
                                    tableHtml += "</tr>";
                                }
                                tableHtml += "</table>";
                            }

                            node = page.DocumentNode.SelectSingleNode("//span[@class='Destacado']");
                            if (node != null) {
                                txt = "";
                                node = node.NextSibling;
                                while (node != null) {
                                    txt += node.InnerHtml;
                                    node = node.NextSibling;
                                }
                            }
                            txtAll = "";
                            if (tableHtml != "" && txt != "") {
                                txtAll = tableHtml + "<p>" + txt + "</p>";
                                isBodyContentSet = true;
                            }
                            else {
                                if (tableHtml != "") {
                                    txtAll = tableHtml;
                                    isBodyContentSet = true;
                                }
                                else {
                                    txtAll = "<p>" + txt + "</p>";
                                }
                            }
                            if (isBodyContentSet) {
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = txtAll;
                                isBodyContentSet = true;
                            }
                            rslt = isTitleSet && isImageOnlySet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.ImageOnlySet.Name = examinedDoc.ImageOnlySet.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                        case 3:
                            // Title
                            var titles = page.DocumentNode.SelectNodes("//span[@class='TituloSecundario']");
                            nodes = titles;
                            if (titles != null && titles.Count >= 2) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = titles[1].InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // Let's try ImageOnly
                            images = page.DocumentNode.SelectNodes("//td/img");
                            imageNameList = new List<String>();
                            if (images != null) {
                                imageNameList = new List<string>();
                                foreach (var img in images) {
                                    var imgSrcRef = img.Attributes["src"];
                                    if (imgSrcRef != null) {
                                        var s = imgSrcRef.Value;
                                        if (s.Contains("BancoMedios/Imagenes")) {
                                            imageNameList.Add(s);
                                        }
                                    }
                                }
                                if (imageNameList.Count != 0) {
                                    examinedDoc.ImageOnlySet.Name = MetadataName.OldDocImageOnlySetName;
                                    examinedDoc.ImageOnlySet.Content = imageNameList[0];
                                    isImageOnlySet = true;
                                }
                            }

                            // DocumentText
                            if (nodes != null) {
                                node = nodes[0].NextSibling;
                                txtAll = "";
                                while (node != null) {
                                    txtAll += node.InnerHtml;
                                    node = node.NextSibling;
                                }
                                if (txtAll != "") {
                                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                    examinedDoc.DocumentText.Content = txtAll;
                                    isBodyContentSet = true;
                                }
                            }

                            rslt = isTitleSet && isImageOnlySet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.ImageOnlySet.Name = examinedDoc.ImageOnlySet.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012:
                    switch (kind) {
                        case 1:
                            // Let's try Title
                            title = page.DocumentNode.SelectSingleNode("//td[@class='TituloFicha']");
                            if (title != null) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = title.InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // Let's try DocumentText (here, DocumentText is two parts, one for a table and second a text.
                            // table is to be emitted unformatted.
                            tableHtml = "<table width='100%' border='0' align='center' cellpadding='1' cellspacing='0'>";
                            txt = "";
                            firstTR = true;
                            nodes = page.DocumentNode.SelectNodes("//table[@bgcolor='#202020']/tr");
                            if (nodes != null) {
                                foreach (var n in nodes) {
                                    tableHtml += "<tr>";
                                    if (n.HasChildNodes) {
                                        var children = n.ChildNodes;
                                        int numTD = 1;
                                        foreach (var ntd in children) {
                                            if (ntd.Name.ToUpper() == "TD") {
                                                if (firstTR) {
                                                    switch (numTD) {
                                                        case 1:
                                                            tableHtml += "<td width='23%'>" + ntd.InnerText + "</td>";
                                                            break;
                                                        case 2:
                                                            tableHtml += "<td width='77%'>" + ntd.InnerText + "</td>";
                                                            break;
                                                    }
                                                    numTD++;
                                                }
                                                else {
                                                    tableHtml += "<td>" + ntd.InnerText + "</td>";
                                                }
                                            }
                                        }
                                        firstTR = false;
                                    }
                                    tableHtml += "</tr>";
                                }
                                tableHtml += "</table>";
                            }

                            node = page.DocumentNode.SelectSingleNode("//span[@class='Destacado']");
                            if (node != null) {
                                txt = "";
                                node = node.NextSibling;
                                while (node != null) {
                                    txt += node.InnerHtml;
                                    node = node.NextSibling;
                                }
                            }
                            txtAll = "";
                            if (tableHtml != "" && txt != "") {
                                txtAll = tableHtml + "<p>" + txt + "</p>";
                                isBodyContentSet = true;
                            }
                            else {
                                if (tableHtml != "") {
                                    txtAll = tableHtml;
                                    isBodyContentSet = true;
                                }
                                else {
                                    txtAll = "<p>" + txt + "</p>";
                                }
                            }
                            if (isBodyContentSet) {
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = txtAll;
                                isBodyContentSet = true;
                            }
                            rslt = isTitleSet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                        case 2:
                            // Title
                            title = page.DocumentNode.SelectSingleNode("//span[@class='TituloPrincipal']");
                            if (title != null) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = title.InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // DocumentText
                            if (title != null) {
                                node = title.NextSibling;
                            }
                            else {
                                node = null;
                            }
                            txtAll = "";
                            while (node != null) {
                                if (node.Name == "#text") {
                                    txtAll += node.InnerText;
                                }
                                node = node.NextSibling;
                            }
                            if (txtAll != "") {
                                examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                examinedDoc.DocumentText.Content = txtAll;
                                isBodyContentSet = true;
                            }
                            rslt = isTitleSet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                        case 3:
                            // Title
                            node = page.DocumentNode.SelectSingleNode("//td[@class='Vitrinas-Vineta']");
                            if (node != null) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = node.InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // DocumentText
                            txtAll = "";
                            node = page.DocumentNode.SelectSingleNode("//td[@class='VitrinaTexto']");
                            if (node != null) {
                                if (node.HasChildNodes) {
                                    node = node.FirstChild;
                                    while (node != null) {
                                        txtAll += node.InnerHtml;
                                        node = node.NextSibling;
                                    }
                                    if (txtAll != "") {
                                        examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                        examinedDoc.DocumentText.Content = txtAll;
                                        isBodyContentSet = true;
                                    }
                                }
                            }
                            rslt = isTitleSet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                        case 4:
                            // Title
                            var titles = page.DocumentNode.SelectNodes("//span[@class='TituloSecundario']");
                            nodes = titles;
                            if (titles != null && titles.Count >= 2) {
                                Title tit = new Title();
                                tit.Name = MetadataName.OldDocTitleName;
                                tit.Content = titles[1].InnerText;
                                examinedDoc.Title.Add(tit);
                                isTitleSet = true;
                            }

                            // DocumentText
                            if (nodes != null) {
                                node = nodes[0].NextSibling;
                                txtAll = "";
                                while (node != null) {
                                    txtAll += node.InnerHtml;
                                    node = node.NextSibling;
                                }
                                if (txtAll != "") {
                                    examinedDoc.DocumentText.Name = MetadataName.OldDocTextName;
                                    examinedDoc.DocumentText.Content = txtAll;
                                    isBodyContentSet = true;
                                }
                            }

                            rslt = isTitleSet && isBodyContentSet;
                            if (!rslt) {
                                examinedDoc.DocumentText.Name = examinedDoc.DocumentText.Content = "";
                                examinedDoc.Title.Clear();
                            }
                            break;
                    }
                    break;
            }

            if (rslt) {
                examinedDoc.TemplateType = templateTypeCode;
                examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS;
            }
            return rslt;
        }

        /// <summary>
        /// Analyezes an HTML in search for a series of elements. They can be SPAN, DIV, whatsoever.
        /// We are only interested in those URLs that resides under 'Bancoconocimiento', thus, if an
        /// URL is named like 'http://www.elcolombiano.com/cine.asp' then it is not considered as a valid ASP to 
        /// process, or it can be like 'http://www.elcolombiano.com/proyectos/Colombiamoda/2003/Galerias/vanidades/07.htm', then
        /// it is discarded as well.
        /// </summary>
        /// <param name="urlSource">Url as read from database.</param>
        /// <param name="urlToUse">Which URL to scan</param>
        /// <param name="knownDomainList">Valid domains to use</param>
        /// <param name="replaceKnownDomainList">Valid replace domain to use. If one is to be picked, it is the same index found in 'knownDomainList'</param>
        /// <returns>a json representation for examined doc, noting it can have empty values so it may be unusable</returns>
        public string ExtractToJson(string urlSource, string urlToUse, List<String> knownDomainList, List<String> replaceKnownDomainList) {
            string s = "";
            string readHtml = "";
            bool isTemplateAlreadyAnalyzed = false;

            examinedDoc.Reset();
            if (!urlSource.ValidateUrlDomain(knownDomainList)) {
                examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NOT_ASP_PAGE;
                return s;
            }

            useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
            page = new HtmlDocument();
            using (var client = new WebClient()) {
                using (var stream = client.OpenRead(urlToUse)) {
                    var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-9"));
                    var html = reader.ReadToEnd();

                    readHtml = html;

                    // Our server masks a 404 error page when something wrong executed at server side.
                    // Let's try to catch this fact and act accordingly.
                    if (html.Contains("Error 404")) {
                        examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_404;
                    }
                    else {
                        isTemplateAlreadyAnalyzed = false;
                        page.LoadHtml(html);
                        RetrieveTitles();
                        RetrieveBullets();
                        RetrieveCreditContent();
                        RetrieveBodyContent();
                        RetrieveRelatedPhotos();
                        RetrieveReaderHelp();
                        RetrieveNewsGrouper();
                        RetrieveCartoonGrouper();

                        // Nov.08/2013 COQ: A special validation must actually be made for 
                        // TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001 as with other code around
                        // it broke when a full reprocess was done, discarding true 
                        // TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001. Fixed since Nov.08/2013
                        var isTemplate001ActuallySet = examinedDoc.IsTemplateTypeFieldsSetTo(TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001);
                        if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001 && isTemplate001ActuallySet) {
                            isTemplateAlreadyAnalyzed = true;
                        }
                        if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004) {
                            // Nov.14/2013 COQ: If we have gotten a document status of MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS=2
                            // and TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004=4, then we go further to 
                            // add ReaderHelp field if one found.
                            int statusSoFar = this.Status;
                            if (statusSoFar == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                                CheckReaderHelpFor(statusSoFar, useTemplateTypeInstead);
                            }
                        }

                        // If special check was not met then continue as normal.
                        if (!isTemplateAlreadyAnalyzed) {
                            // Nov.01/2013 COQ: Two new templates are to be added. As existing
                            // templates up to #10 is  analyzed with this version V1.2.1.1-Oct.31/2013
                            // This next logic is implemented in V1.2.2.3-Nov.01/2013
                            // The new strategy for these two new templates is to evaluate one by one.
                            // Nov.06/2013 COQ: Two more new templates, #13, #14 implemented since V1.2.5.20-Nov.06/2013

                            // Template 12 is similar in markap to template 1, but lacks Credit altogether, so let's decide if 
                            // it is absolutely a Template 12 document.
                            if (examinedDoc.IsTitleBodyContentOnlyFilled()) {
                                useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012;
                                isTemplateAlreadyAnalyzed = true;
                            }

                            if (examinedDoc.IsEmpty()) {
                                // Template 13 can be only checked if and only if the examinedDoc is totally empty.
                                isTemplateAlreadyAnalyzed = GetTemplateTitleImageOnlyBodyContentCreditOnlyContent();

                                // Templat 15 can be only checked if and only if the examinedDoc is totally empty.
                                if (!isTemplateAlreadyAnalyzed) {
                                    isTemplateAlreadyAnalyzed = GetTemplateTitleBodyContentPhotoFooterGalleryContent();
                                }

                                // Template 14 can be only checked if and only if the examinedDoc is totally empty.
                                if (!isTemplateAlreadyAnalyzed) {
                                    isTemplateAlreadyAnalyzed = GetTemplateTitletBodyContentCreditOnlyContentOtherMarkup();
                                }

                                // Template 11 can be only checked if and only if the examinedDoc is totally empty.
                                if (!isTemplateAlreadyAnalyzed) {
                                    isTemplateAlreadyAnalyzed = GetTemplateTitletBodyContentCreditOnlyContent();
                                }
                            }
                        }

                        if (!isTemplateAlreadyAnalyzed) {
                            if (!examinedDoc.IsTitleBulletDocumentPhotoRelatedReaderHelpOnlyFilled()) {
                                // Before trying to catch other templates, let's try to catch 
                                // data for Title, ImageOnly and BodyContent, and if matched then
                                // assign OLD_DOC_TEMPLATE_TYPE_010 as its type.                        
                                if (!GetTemplateTitleImageOnlyContent()) {
                                    // 1) If basic templates analyzed are not found, then analyze next template.
                                    // if only PhotoRelated list is filled then the document is not usable, 
                                    // it must be checked if it is an old photo gallery document.
                                    if (examinedDoc.IsEmpty()) {
                                        examinedDoc.Reset();
                                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                                        GetTemplatePhotoGallery();
                                        if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                            examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                                        }
                                    }
                                    if (examinedDoc.IsPhotoRelatedOnlyFilled()) {
                                        examinedDoc.Reset();
                                        useTemplateTypeInstead = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                                        GetTemplatePhotoGallery();
                                        if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                            examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                                        }
                                    }

                                    // 2) if last template is not found, then analyze next template
                                    if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006) {
                                        // If it happens that template 6 has been gotten, 
                                        // then no more checks for exceptions made in ELSE part herein
                                        ;
                                    }
                                    else {
                                        if (examinedDoc.IsTitleBulletOnlyFilled()) {
                                            examinedDoc.Reset();
                                            GetTemplateTitlePhotoContent();
                                            if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                                examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                                            }
                                        }
                                        else {
                                            if (examinedDoc.IsTitleBullentDocumetTextOnlyFilled()) {
                                                examinedDoc.Reset();
                                                GetTemplateTitleBulletCreditContent();
                                                if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                                    examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                                                }
                                            }
                                        }
                                    }

                                    // 3) if last template is not found, then analyze next template
                                    if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                        examinedDoc.Reset();
                                        GetTemplateTitleBulletPhotoFooterContent();
                                        if (useTemplateTypeInstead == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                                            examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE;
                                        }
                                    }
                                }
                            }
                        }

                        if (useTemplateTypeInstead != TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED) {
                            examinedDoc.TemplateType = useTemplateTypeInstead;
                        }
                    }
                }
            }
            int status = this.Status;
            examinedDoc.UrlUsed = urlSource;

            // Before giving up with documents in status 
            // MigrateStatusCode.OLD_DOC_STATUS_CODE_WARNING or 
            // MigrateStatusCode.OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED
            // Let's try to match in existing templates other revised URLs so far
            if (status == MigrateStatusCode.OLD_DOC_STATUS_CODE_WARNING || status == MigrateStatusCode.OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED) {
                // Let's try to map document into template 10
                // Template 10 can be only checked if and only if the examinedDoc title is only set.
                isTemplateAlreadyAnalyzed = false;
                if (examinedDoc.IsTitleOnlyFilled()) {
                    isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 1, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010);
                    if (isTemplateAlreadyAnalyzed) {
                        examinedDoc.TemplateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010;
                        examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS;
                    }
                }
            }
            else {
                if (status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE && examinedDoc.IsEmpty()) {
                    // This is another exceptional entry point.
                    isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 2, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010);
                    if (!isTemplateAlreadyAnalyzed) {
                        isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 3, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010);
                    }
                    if (!isTemplateAlreadyAnalyzed) {
                        isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 1, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012);
                    }
                    if (!isTemplateAlreadyAnalyzed) {
                        isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 2, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012);
                    }
                    if (!isTemplateAlreadyAnalyzed) {
                        isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 3, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012);
                    }
                    if (!isTemplateAlreadyAnalyzed) {
                        isTemplateAlreadyAnalyzed = MapIntoExistingTemplate(readHtml, 4, TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012);
                    }
                }
            }

            status = this.Status;
            s = (status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NOT_ASP_PAGE ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_EXCEPTION ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_400 ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_403 ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_404 ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_500 ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_WARNING ||
                 status == MigrateStatusCode.OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED)
                 ? "" :
                 JsonConvert.SerializeObject(examinedDoc);
            return s;
        }

        /// <summary>
        /// Uses the parameter to load the HTML for page and scans if it has a &lt;td class='NotaPrincipalTitulo'&gt;&lt;/td&gt;
        /// If one is found it is assumed that an <a></a> element is found and extract its href attribute, then
        /// if that attribute contains 'BancoConocimiento' it is then returned as the URL extracted.
        /// An example follows
        /// &lt;td class="NotaPrincipalTitulo"&gt;&lt;a href="http://www.elcolombiano.com/BancoConocimiento/O/olac_gobierno_autorizo_propuesta_chavez_lcg_26122007/olac_gobierno_autorizo_propuesta_chavez_lcg_26122007.asp?CodSeccion=46"> Gobierno colombiano autorizó misión humanitaria para recibir a los rehenes &lt;/a&lt;&gt;/td&lt;
        /// </summary>
        /// <param name="urlToUse">The URL to examine</param>
        /// <param name="knownDomainList">Valid domains to use</param>        
        /// <returns>Empty if no embedded URL found.</returns>
        public string ExtractURLFromOldDocUsing(string urlToUse, List<string> knownDomainList, List<string> replaceKnownDomainList) {
            string rslt = "";

            if (!urlToUse.ValidateUrlDomain(knownDomainList)) {
                examinedDoc.Status = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_NOT_ASP_PAGE;
                return rslt;
            }
            urlToUse = urlToUse.ReconfigureHostNameFrom(knownDomainList, replaceKnownDomainList);
            page = new HtmlDocument();
            using (var client = new WebClient()) {
                using (var stream = client.OpenRead(urlToUse)) {
                    var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-9"));
                    var html = reader.ReadToEnd();
                    page.LoadHtml(html);

                    var linkRef = page.DocumentNode.SelectSingleNode("//td[@class='NotaPrincipalTitulo']");
                    if (linkRef != null) {
                        var refChild = linkRef.FirstChild;
                        if (refChild != null) {
                            var href = refChild.Attributes["href"];
                            if (href != null) {
                                rslt = href.Value;
                            }
                        }
                    }

                    var linkRef1 = page.DocumentNode.SelectSingleNode("//span[@class='Seccion']");
                    if (linkRef1 != null) {
                        var node = linkRef1.NextSibling;
                        while (node != null) {
                            if (node.Name.ToLower() == "a") {
                                var href = node.Attributes["href"];
                                if (href != null) {
                                    rslt = href.Value;
                                }
                                break;
                            }
                            node = node.NextSibling;
                        }
                    }

                    var linkRef2 = page.DocumentNode.SelectSingleNode("//td[@class='ModuloServicios']");
                    if (linkRef2 != null) {
                        var node = linkRef2.FirstChild;
                        if (node != null && node.Name.ToLower() == "a") {
                            var href = node.Attributes["href"];
                            if (href != null) {
                                rslt = href.Value;
                            }
                        }
                    }
                }
            }

            // Last step is to know if 'rslt' string is not empty and have the proper domain name (http://www.elcolombiano)
            if (rslt != "") {
                var domainName = "http://www.elcolombiano.com";
                if (!rslt.Contains(domainName)) {
                    if (!rslt.StartsWith("/")) {
                        rslt = domainName + "/" + rslt;
                    }
                    else {
                        rslt = domainName + rslt;
                    }
                }
            }
            return rslt;
        }
    }
}