/*==========================================================================*/
/* Source File:   SE4DOCMIGRATE.CS                                          */
/* Description:   Abstract class to hold common methods for migration       */
/*                purposes.                                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Nov.24/2014                                               */
/* Version:       1.94                                                      */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
May.02/2014 COQ See note (1) below.
============================================================================*/

/*NOTES:
 (1) May.02/2014
  ProtectMedia in its Iterweb current version changes the structure for importing
  old documents to the Iterweb system. Thus, if you need old structure version, use Tag V1.2.17.90-Nov.29/2013.
  Output generation is contained in file SE4DOCMIGRATE.CS in methods 
  GeneratePackagesForOldDocs and GeneratePackagesForSE4Docs.
  
 (2) May.12/2014
 Both 'CreateOutputArticleContentDocToXml' and 'CreateDocumentAttributesToXml' have signatures to deal with the fact that a
 document is from SE4 or it is an old document format not stored in SE4 database, 'CreateOutputArticleContentDocToXml' is 
 used for new output format which uses 'articles' and 'CreateDocumentAttributesToXml' is used to output format which does 
 not use 'articles'. Any change in its contents in one method must be synced in the another. That is, if you change 
 for some reason 'CreateOutputArticleContentDocToXml' then you must sync 'CreateDocumentAttributesToXml' accordingly.
 NOTE: If you need which changes were actually been applied, use GIT compare (as this project uses it).
 NOTE: 'CreateOutputArticleContentDocToXml' does not save its XML content to disk, whereas 'CreateDocumentAttributesToXml' does.
 */
using EC.IterwebMigrate.Domain;
using EC.Utils;
using EC.Utils.Constants;
using EC.Utils.Domain;
using EC.Utils.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using System.Web;
using System.Text;

namespace EC.IterwebMigrate {
    /// <summary>
    /// Abstract class to hold common methods for migration purposes.  In fact a
    /// concrete implementation of this abtract class.
    /// </summary>
    public class SE4DocMigrate : MigrationAbstract {
        private Counters cnts = null;
        private Dictionary<string, List<ValidMapField>> se4TemplateFieldMapping = null;

        // Used to process media name files in Old format XML (deprecated!)
        private List<string> mediaNameOldList = new List<string>();
        protected List<SectionDefinition> sections = null;
        private string DocumentImageDir = "/documents/Global/0/";
        private string _docPath = "";
        private bool isIterWebManifestFileAffected = false;
        private List<ImageOnly> galleryImgList = null;
        private String imageGalleryTitle = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public SE4DocMigrate(string cmdParamsPropertiesPath = "")
            : base() {
            _se4DocList = new List<IterwebMapInfo>();
            if (cmdParamsPropertiesPath != "") {
                OverrideParameters(cmdParamsPropertiesPath);
            }
            SetupEnvironment();
        }

        /// <summary>
        /// Setups the environment, such as cleaning target folders and counter initialization.
        /// </summary>
        private void SetupEnvironment() {
            if (log.IsDebugEnabled) {
                log.Debug("SetupEnvironment start");
            }
            if (_outputStructure == 2) {
                // Configure sections, categories
                if (_resetJSonFiles == 1) {
                    sections = new List<SectionDefinition>();
                    sections.Add(new SectionDefinition() {
                        ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_GENERAL,
                        PageTemplate = "Contenido",
                        Qualification = "lista",
                        Url = "/historico",
                        DefaultSection = true,
                    });
                    sections.Add(new SectionDefinition() {
                        ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_IMAGE_GALLERY,
                        PageTemplate = "Contenido_Imagen",
                        Qualification = "lista",
                        Url = "/multimedia/imagenes",
                        DefaultSection = true,
                    });
                    sections.Add(new SectionDefinition() {
                        ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_VIDEO_GALLERY,
                        PageTemplate = "Video",
                        Qualification = "lista",
                        Url = "/multimedia/videos",
                        DefaultSection = true,
                    });
                    sections.Add(new SectionDefinition() {
                        ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_TOON_GALLERY,
                        PageTemplate = "Contenido_Caricatura",
                        Qualification = "lista",
                        Url = "/opinion/caricaturas",
                        DefaultSection = true,
                    });
                    sections.Add(new SectionDefinition() {
                        ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_INFOGRAPHIC_GALLERY,
                        PageTemplate = "Infografía",
                        Qualification = "lista",
                        Url = "/multimedia/infografias",
                        DefaultSection = true,
                    });

                    using (StreamWriter sw = new StreamWriter(_sectionsJSONFile)) {
                        string jsonInfo = JsonConvert.SerializeObject(sections);
                        sw.Write(jsonInfo);
                    }
                }
                else {
                    try {
                        using (StreamReader sr = new StreamReader(_sectionsJSONFile)) {
                            String line = sr.ReadToEnd();
                            sections = JsonConvert.DeserializeObject<List<SectionDefinition>>(line);
                        }
                    }
                    catch (Exception ex) {
                        if (log.IsErrorEnabled) {
                            log.Error("Json file could not be read [" + _sectionsJSONFile + "]", ex);
                        }
                    }
                }
            }

            // Now it is time to decide how to initialize the 'cnts' private property.
            if (_resetCounters == 1) {
                cnts = new Counters();

                cnts.IterWebManifestFile = 0;
                cnts.Image = 1;
                cnts.Multimedia = 1;
                cnts.OldDoc = 1;
                cnts.PageContent = 1;
                InitFolders();
            }
            else {
                if (!File.Exists(_counterJSONFile)) {
                    cnts = new Counters();

                    cnts.IterWebManifestFile = 0;
                    cnts.Image = 1;
                    cnts.Multimedia = 1;
                    cnts.OldDoc = 1;
                    cnts.PageContent = 1;
                    InitFolders();
                }
                else {
                    try {
                        using (StreamReader sr = new StreamReader(_counterJSONFile)) {
                            String line = sr.ReadToEnd();

                            // if for some reason the file is empty then a new object setting is created.
                            if (line == "") {
                                cnts = new Counters();

                                cnts.IterWebManifestFile = 0;
                                cnts.Image = 1;
                                cnts.Multimedia = 1;
                                cnts.OldDoc = 1;
                                cnts.PageContent = 1;
                                InitFolders();
                            }
                            else {
                                cnts = JsonConvert.DeserializeObject<Counters>(line);
                            }
                        }
                    }
                    catch (Exception ex) {
                        if (log.IsErrorEnabled) {
                            log.Error("Json file could not be read [" + _counterJSONFile + "]", ex);
                        }
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("SetupEnvironment End");
            }
        }

        /// <summary>
        /// Create folders on demand.
        /// </summary>
        private void InitFolders() {
            if (log.IsDebugEnabled) {
                log.Debug("InitFolders start");
            }

            if (log.IsInfoEnabled) {
                log.Info("Clean up Working directory");
            }
            Console.WriteLine("Clean up Working directory ");
            if (Directory.Exists(_migrationStoreFolder)) {
                Directory.Delete(_migrationStoreFolder, true);
            }
            Directory.CreateDirectory(_migrationStoreFolder);

            if (Directory.Exists(_zipFolder)) {
                Directory.Delete(_zipFolder, true);
            }
            Directory.CreateDirectory(_zipFolder);
            if (log.IsDebugEnabled) {
                log.Debug("InitFolders start");
            }

        }

        /// <summary>
        /// Given a string replaces any HTML code to normal string, e.g., &quot; to ".
        /// NOTE: If source string is set to null, then no processing takes place
        /// and null is returned.
        /// </summary>
        /// <param name="text">Input source</param>
        /// <returns>Filtered source with HTML replacements.</returns>
        private string ReplaceHTMLCharacters(string text) {
            if (text == null) {
                return null;
            }

            string s = text.Replace("&quot;", "\"");
            return s;
        }

        /// <summary>
        /// Analyze the string looking for a set of characters that are
        /// deemed as invalid in XML contexts.
        /// </summary>
        /// <param name="contentText">The string to analyze</param>
        /// <returns>A new string with all characters in list removed</returns>
        private string RemoveInvalidXmlChars(string contentText) {
            contentText = contentText.Supress('\a');
            contentText = contentText.Supress((char)0x10);
            contentText = contentText.Supress((char)0x03);
            contentText = contentText.Supress((char)0x1F);
            contentText = contentText.Supress((char)0x1E);
            contentText = contentText.Supress((char)0x1C);
            contentText = contentText.Supress((char)0x0C);
            return contentText;
        }

        /// <summary>
        /// Fills up the SE4 Template FieldMapping
        /// </summary>
        private void BuildSe4TemplateFieldMapping() {
            if (log.IsDebugEnabled) log.Debug("BuildSe4TemplateFieldMapping start");

            se4TemplateFieldMapping = new Dictionary<string, List<ValidMapField>>();

            se4TemplateFieldMapping.Add("creacion_notaInterior100", FillTemplate001());
            se4TemplateFieldMapping.Add("Creacion_NotaInteriorEspecialesEC100A", FillTemplate002());
            se4TemplateFieldMapping.Add("creacion_notaInterior", FillTemplate003());
            se4TemplateFieldMapping.Add("creacion_notaInterior100M", FillTemplate004());
            //COQ: Jun.27/2014 If commented then all documents with this layout are discarded  -> se4TemplateFieldMapping.Add("creacion_notaInterior100MM", FillTemplate005());
            se4TemplateFieldMapping.Add("creacion_notaInteriorM", FillTemplate006());
            se4TemplateFieldMapping.Add("creacion_notaInteriorT", FillTemplate007());
            se4TemplateFieldMapping.Add("NotaInterior", FillTemplate008());
            se4TemplateFieldMapping.Add("NotaInteriorNew", FillTemplate009());
            se4TemplateFieldMapping.Add("NotaInteriorNavidad", FillTemplate010());
            //COQ: Jun.19/2014 If commented then all documents with this layout are discarded  -> se4TemplateFieldMapping.Add("creacion_minutoaminuto", FillTemplate011());
            se4TemplateFieldMapping.Add("creacion_galeria", FillTemplate012());
            se4TemplateFieldMapping.Add("creacion_infografias", FillTemplate013());
            se4TemplateFieldMapping.Add("creacion_video", FillTemplate014());
            //COQ: Jun.27/2014 If commented then all documents with this layout are discarded  -> se4TemplateFieldMapping.Add("creacion_audio", FillTemplate015());
            se4TemplateFieldMapping.Add("creacion_graficos", FillTemplate016());
            se4TemplateFieldMapping.Add("creacion_notaInteriorEditorial", FillTemplate017());

            if (log.IsDebugEnabled) log.Debug("BuildSe4TemplateFieldMapping end");
        }

        /// <summary>
        /// Valid list for template 'creacion_notaInterior100'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate001() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate001 start template 'creacion_notaInterior100'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "Image", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "Image", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "Set", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "Cutline", "text", "text", true));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "Image", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate001 end template 'creacion_notaInterior100'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'Creacion_NotaInteriorEspecialesEC100A'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate002() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate002 start template 'Creacion_NotaInteriorEspecialesEC100A'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "imagen_ITERWEB", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "imagen_ITERWEB", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "descripcionImagenNota_ITERWEB", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "credito_ITERWEB", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicioThumb", "imagen_MiniaturasEspListas", "thumb_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicioThumb", "media_img_MiniaturasEspListas", "thumb_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicioThumb", "descripcionMiniaturasEspListas", "thumbDescripcion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicioThumb", "fotoInicioThumb_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "FRAmpliarImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "FRDescAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descrpcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate002 end template 'Creacion_NotaInteriorEspecialesEC100A'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacon_notaInterior'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate003() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate003 start template 'creacion_notaInterior'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate003 end template 'creacion_notaInterior'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_notaInterior100M'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate004() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate004 start template 'creacion_notaInterior100M'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "Image", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "Image", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "Set", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "Cutline", "text", "text", true));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "Image", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate004 end template 'creacion_notaInterior100M'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_notaInterior100MM'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate005() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate005 start template 'creacon_notaInterior100MM'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "imagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "imagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "descripcionImagenNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "credito_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "medio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "titulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "entradilla_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "autor_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "ciudad_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "firma_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "FRAmpliarImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "FRDescAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "EDITORHTML_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate005 end template 'creacon_notaInterior100MM'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_notaInteriorM'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate006() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate006 start template 'creacion_notaInteriorM'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "texto_creditoFotoRelacionada", "FRLOV_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("cuerpo", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "html_cuerpoAyuda", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "BitTip", "BitTip_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "texto_CuerpoTip", "texto_CuerpoTip_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("bittip", "bittip_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloGaleria1", "tituloGaleria1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoGaleria1", "tipoGaleria1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaGaleria1", "rutaGaleria1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altGaleria1", "altGaleria1", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloGaleria2", "tituloGaleria2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoGaleria2", "tipoGaleria2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaGaleria2", "rutaGaleria2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altGaleria2", "altGaleria2", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate006 end template 'creacion_notaInteriorM'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_noteInteriorT'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate007() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate007 start template 'creacion_noteInteriorT'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRLOV_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate007 end template 'creacion_noteInteriorT'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'NotaInterior'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate008() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate008 start template 'NotaInterior'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo", "Headline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo_Interior", "Headline", "text", "text", false));
            validFieldList.Add(new ValidMapField("Titulo", "Headline", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Bullet", "Bullet_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EmailAutor", "EmailAutor_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("Autor", "Byline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Ciudad", "City", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Fecha", "Fecha_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("EntreTitulo", "EntreTitulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EditorHtml", "EditorHtml_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "TituloAyuda", "TituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "Titulor", "Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "EDITORHTML", "AyudaEditor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Ayuda", "Ayuda_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "BitTip", "BitTip_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "texto_CuerpoTip", "BitTipCuerpo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("bittip", "bittip_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "TituloMapa", "TituloMapa_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Mapa", "Mapa_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "MEDIA_IMG_Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "CreditoFoto", "FGCreditoFoto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "TEXTO_PieDeFoto", "FGPieFoto_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoGrande", "FotoGrande", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Video", "Video_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "TituloAudio", "Audio_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "RutaAudio", "RutaAudio_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Audio", "Audio_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "NombreOpcion", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "URL_RutaNota", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("NotaRelacionada", "NotaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "TituloGaleria", "GaleriaTituloGaleria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "CarpetaGaleria", "CarpetaGaleria_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Galeria", "Galeria_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Clasificacion", "Clasificacion_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("HoraHome", "HoraHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("VideoHome", "VideoHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "MEDIA_IMG_Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoHome", "FotoHome_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("CreditoFotoHome", "CreditoFotoHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("ReseniaHome", "ReseniaHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "TituloLinkHome", "TituloLinkHome_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "URL_RutalinkHome", "URL_RutalinkHome_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("LinkHome", "LinkHome_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome1", "TituloVideoHome1_ITEWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome1", "RutaVideoHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome2", "TituloVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome2", "RutaVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome3", "TituloVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome3", "RutaVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome4", "TituloVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome4", "RutaVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome1", "TituloAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome1", "RutaAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome2", "TituloAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome2", "RutaAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome3", "TituloAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome3", "RutaAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome4", "TituloAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome4", "RutaAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome1", "TituloNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome1", "URL_RutaNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome2", "TituloNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome2", "URL_RutaNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome3", "TituloNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome3", "URL_RutaNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome4", "TituloNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome4", "URL_RutaNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome1", "TituloNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome1", "URL_RutaNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome2", "TituloNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome2", "URL_RutaNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome3", "TituloNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome3", "URL_RutaNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome4", "TituloNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome4", "URL_RutaNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria1", "TituloGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria1", "CarpetaGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria2", "TituloGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria2", "CarpetaGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria3", "TituloGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria3", "CarpetaGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria4", "TituloGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria4", "CarpetaGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria5", "TituloGaleria5_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria5", "CarpetaGaleria5_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Relacionado", "Relacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("GraficoTip", "GraficoTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("BitTipLector", "BitTipLector_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("color", "color_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate008 end template 'NotaInterior'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'NotaInteriorNew'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate009() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate009 start template 'NotaInteriorNew'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo", "Headline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo_Interior", "Headline", "text", "text", false));
            validFieldList.Add(new ValidMapField("Titulo", "Headline", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Bullet", "Bullet_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EmailAutor", "EmailAutor_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("Autor", "Byline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Ciudad", "City", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Fecha", "Fecha_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("editorhtml", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("EntreTitulo", "EntreTitulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EditorHtml", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "TituloAyuda", "TituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "Titulor", "Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "EDITORHTML", "AyudaEditor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Ayuda", "Ayuda_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "BitTip", "BitTip_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "texto_CuerpoTip", "BitTipCuerpo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("bittip", "bittip_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "TituloMapa", "TituloMapa_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Mapa", "Mapa_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "MEDIA_IMG_Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "CreditoFoto", "FGCreditoFoto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "TEXTO_PieDeFoto", "FGPieFoto_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoGrande", "FotoGrande", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Video", "Video_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "TituloAudio", "Audio_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "RutaAudio", "RutaAudio_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Audio", "Audio_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "NombreOpcion", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "URL_RutaNota", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("NotaRelacionada", "NotaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "TituloGaleria", "GaleriaTituloGaleria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "CarpetaGaleria", "CarpetaGaleria_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Galeria", "Galeria_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulozonacomercial", "titulozonacomercial_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("zona", "URL_Rutalink", "URL_Rutalink_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("zona", "MEDIA_IMG_W86_H95_Imagen", "MEDIA_IMG_W86_H95_Imagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("zona", "zona_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("ReseniaHome", "ReseniaHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("Clasificacion", "Clasificacion_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("HoraHome", "HoraHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("VideoHome", "VideoHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "MEDIA_IMG_Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoHome", "FotoHome_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("CreditoFotoHome", "CreditoFotoHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "TituloLinkHome", "TituloLinkHome_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "URL_RutalinkHome", "URL_RutalinkHome_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("LinkHome", "LinkHome_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome1", "TituloVideoHome1_ITEWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome1", "RutaVideoHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome2", "TituloVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome2", "RutaVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome3", "TituloVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome3", "RutaVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome4", "TituloVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome4", "RutaVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome1", "TituloAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome1", "RutaAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome2", "TituloAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome2", "RutaAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome3", "TituloAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome3", "RutaAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome4", "TituloAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome4", "RutaAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome1", "TituloNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome1", "URL_RutaNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome2", "TituloNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome2", "URL_RutaNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome3", "TituloNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome3", "URL_RutaNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome4", "TituloNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome4", "URL_RutaNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome1", "TituloNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome1", "URL_RutaNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome2", "TituloNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome2", "URL_RutaNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome3", "TituloNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome3", "URL_RutaNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome4", "TituloNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome4", "URL_RutaNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria1", "TituloGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria1", "CarpetaGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria2", "TituloGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria2", "CarpetaGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria3", "TituloGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria3", "CarpetaGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria4", "TituloGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria4", "CarpetaGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria5", "TituloGaleria5_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria5", "CarpetaGaleria5_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Relacionado", "Relacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("GraficoTip", "GraficoTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("BitTipLector", "BitTipLector_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("color", "color_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate009 end template 'NotaInteriorNew'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'NotaInteriorNavidad'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate010() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate010 start template 'NotaInteriorNavidad'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo", "Headline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Titulo", "Titulo_Interior", "Headline", "text", "text", false));
            validFieldList.Add(new ValidMapField("Titulo", "Headline", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Bullet", "Bullet_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EmailAutor", "EmailAutor_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("Autor", "Byline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Ciudad", "City", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("Fecha", "Fecha_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("EntreTitulo", "EntreTitulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("EditorHtml", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "TituloAyuda", "Text", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "Titulo", "Text", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Ayuda", "EDITORHTML", "Text", "text", "text", true));
            validFieldList.Add(new ValidMapField("Ayuda", "Ayuda_ITERWEB", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "BitTip", "BitTip_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("bittip", "texto_CuerpoTip", "BitTipCuerpo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("bittip", "bittip_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "TituloMapa", "TituloMapa_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Mapa", "Mapa_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "MEDIA_IMG_Imagen", "FGImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "CreditoFoto", "FGCreditoFoto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoGrande", "TEXTO_PieDeFoto", "FGPieFoto_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoGrande", "FotoGrande", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("Video", "Video_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "TituloAudio", "Audio_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Audio", "RutaAudio", "RutaAudio_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Audio", "Audio_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "NombreOpcion", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("NotaRelacionada", "URL_RutaNota", "NRNombreOpcion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("NotaRelacionada", "NotaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "TituloGaleria", "GaleriaTituloGaleria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Galeria", "CarpetaGaleria", "CarpetaGaleria_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Galeria", "Galeria_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("Clasificacion", "Clasificacion_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("HoraHome", "HoraHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("VideoHome", "VideoHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("FotoHome", "MEDIA_IMG_Imagen", "FotoHomeImagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("FotoHome", "FotoHome_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("CreditoFotoHome", "CreditoFotoHome_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("ReseniaHome", "ReseniaHome_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "TituloLinkHome", "TituloLinkHome_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("LinkHome", "URL_RutalinkHome", "URL_RutalinkHome_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("LinkHome", "LinkHome_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome1", "TituloVideoHome1_ITEWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome1", "RutaVideoHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome2", "TituloVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome2", "RutaVideoHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome3", "TituloVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome3", "RutaVideoHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloVideoHome4", "TituloVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaVideoHome4", "RutaVideoHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome1", "TituloAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome1", "RutaAudioHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome2", "TituloAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome2", "RutaAudioHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome3", "TituloAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome3", "RutaAudioHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloAudioHome4", "TituloAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "RutaAudioHome4", "RutaAudioHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome1", "TituloNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome1", "URL_RutaNotaHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome2", "TituloNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome2", "URL_RutaNotaHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome3", "TituloNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome3", "URL_RutaNotaHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotaHome4", "TituloNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotaHome4", "URL_RutaNotaHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome1", "TituloNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome1", "URL_RutaNotatHome1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome2", "TituloNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome2", "URL_RutaNotatHome2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome3", "TituloNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome3", "URL_RutaNotatHome3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloNotatHome4", "TituloNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "URL_RutaNotatHome4", "URL_RutaNotatHome4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria1", "TituloGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria1", "CarpetaGaleria1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria2", "TituloGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria2", "CarpetaGaleria2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria3", "TituloGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria3", "CarpetaGaleria3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria4", "TituloGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria4", "CarpetaGaleria4_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "TituloGaleria5", "TituloGaleria5_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("Relacionado", "CarpetaGaleria5", "CarpetaGaleria5_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("Relacionado", "Relacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("GraficoTip", "GraficoTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("BitTipLector", "BitTipLector_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("color", "color_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate010 end template 'NotaInteriorNavidad'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_minutoaminuto'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate011() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate011 start template 'creacion_minutoaminuto'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "imagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "imagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "descripcionImagenNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "credito_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "medio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("titulo", "titulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "entradilla_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "autor_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "ciudad_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "firma_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("minutoaminuto", "IdEvento", "IdEvento_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("minutoaminuto", "texto_urlEventoExterno", "texto_urlEventoExterno_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("minutoaminuto", "minutoaminuto_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "FRAmpliarImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "FRDescAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "EDITORHTML_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate011 end template 'creacion_minutoaminuto'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_galeria'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate012() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate012 start template 'creacion_galeria'");
            }

            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreGaleria", "expando", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate012 end template 'creacion_galeria'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_infografias'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate013() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate013 start template 'creacion_infografias'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "image_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_ampliacion", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcion_ampliacion", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "creditoFotoRelacionada", "Cutline", "text", "text", true));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("categoria", "categoria_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloMasNoticias", "agregarModuloMasNoticias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloLosMas", "agregarModuloLosMas_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoCanalInfografia", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoCanalInfografia", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoCanalInfografia", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionCanalInfografia", "descripcionCanalInfografia_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate013 end template 'creacion_infografias'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_video'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate014() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate014 start template 'creacion_video'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("medioGaleria", "medioGaleria_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("categoria", "categoria_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate014 end template 'creacion_video'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_audio'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate015() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate015 start template 'creacion_audio'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldList.Add(new ValidMapField("medio", "medio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("categoria", "categoria_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("titulo", "titulo_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "entradilla_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "autor_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "ciudad_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "firma_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "EDITORHTML_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate015 end template 'creacion_audio'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_graficos'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate016() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate016 start template 'creacion_graficos'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloMasNoticias", "agregarModuloMasNoticias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloLosMas", "agregarModuloLosMas_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate016 end template 'creacion_graficos'");
            return validFieldList;
        }

        /// <summary>
        /// Valid list for template 'creacion_notaInteriorEditorial'
        /// </summary>
        /// <returns>List of valid Map fields</returns>
        private List<ValidMapField> FillTemplate017() {
            if (log.IsDebugEnabled) {
                log.Debug("FillTemplate017 start template 'creacion_notaInteriorEditorial'");
            }

            List<ValidMapFieldAttributes> validFieldAttributList = null;
            List<ValidMapField> validFieldList = new List<ValidMapField>();

            validFieldList.Add(new ValidMapField("CMP_NomObjetoSE", "CMP_NomObjetoSE_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("CMP_Id_ObjetoSE", "CMP_Id_ObjetoSE_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "imagen", "Image", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "media_img_imagen", "imagen_ITERWEB", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "descripcionImagenNota", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "credito", "Cutline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "videoPrincipal", "videoPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("medio", "texto_videoPrincipalExterno", "texto_videoPrincipal_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("medio", "Image", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("entradilla", "Lead", true, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "autor", "Byline", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "ciudad", "City", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "fecha", "fecha_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("firma", "emailAutor", "emailAutor_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("firma", "signature", true, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("anteTituloEditorial", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("titulo", "Headline", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("cuerpoPrimerParrafo", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("EDITORHTML", "Text", true, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarObjetosRelacionados", "agregarObjetosRelacionados_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "Text", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "Text", "text", "text", true));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "Text", "text", "text", true));
            validFieldList.Add(new ValidMapField("contexto", "context", true, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "latitud", "latitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "longitud", "longitud_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("mapa", "comentarioUbicacion", "comentarioUbicacion_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("mapa", "mapa_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionadaInterior", "agregarNotaRelacionadaInterior_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "imagen", "NRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "url_rutaNr", "NRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "LOV_Target", "NRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("notaRelacionada", "tituloNr", "NRTitulo_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("notaRelacionada", "notaRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "imagen", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_image", "FRImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcionImage", "FRDescImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "LOV_target", "FRLOV_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "url_rutaExterna", "FRURL_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "ampliarimagen", "FRAmpliarImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "image_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "media_img_ampliacion", "FRImgAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "descripcion_ampliacion", "FRDescAmpliacion_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoRelacionada", "creditoFotoRelacionada", "FRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoRelacionada", "fotoRelacionada_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "videoNota", "VRNota_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "texto_videoNotaExterno", "VRTexto_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("videoRelacionado", "creditoVideoRelacionado", "VRCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("videoRelacionado", "videoRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "rutaAudio", "ARRuta_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("audioRelacionado", "creditoAudioRelacionado", "ARCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("audioRelacionado", "audioRelacionado_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoVertical", "agregarMicroFormatoVertical_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo_general", "MFV_Titulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_titulo_general", "MFVImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "media_img_imagen", "MFVImagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "big", "MFVBig_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "titulo", "MFVTitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "subtitulo", "MFVSubtitulo_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "texto_resumen", "MFVTextoResumen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoVertical", "credito", "MFVCredito_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoVertical", "MicroFormatoVertical_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("EDITORHTML", "ED_IT", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarMicroFormatoHorizontal", "agregarMicroFormatoHorizontal_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo_general", "MFHTituloGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_titulo_general", "MFHImagenGeneral_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen1", "MFHImagen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big1", "MFHBig1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo1", "MFHTitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo1", "MFHSubtitulo1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen1", "MFHTextoResumen1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito1", "MFHCredito1_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen2", "MFHImagen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big2", "MFHBig2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo2", "MFHTitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo2", "MFHSubtitulo2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen2", "MFHTextoResumen2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito2", "MFHCredito2_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "media_img_imagen3", "MFHImagen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "big3", "MFHBig3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "titulo3", "MFHTitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "subtitulo3", "MFHSubtitulo3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "texto_resumen3", "MFHTextoResumen3_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("MicroFormatoHorizontal", "credito3", "MFHCredito3_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("MicroFormatoHorizontal", "MicroFormatoHorizontal_ITERWEB", false, "text", "text", validFieldAttributList));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyudaPrincipal", "CtxTituloAyudaPrincipal_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "tituloAyuda", "CtxTituloAyuda_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("contexto", "EDITORHTML", "CtxEditorHTML_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("contexto", "contexto_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("publicaTip", "publicaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloInicioTip", "tituloInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicioTip", "targetInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExternaInicioTip", "rutaExternaInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("nombreFotoInicioTip", "nombreFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("fotoInicioTip", "fotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("descripcionFotoInicioTip", "descripcionFotoInicioTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("preguntaTip", "preguntaTip_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag1", "tag1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag2", "tag2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag3", "tag3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag4", "tag4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tag5", "tag5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloRedes", "agregarModuloRedes_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloGalerias", "agregarModuloGalerias_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarModuloQueRicoMedellin", "agregarModuloQueRicoMedellin_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoInicio", "media_img_imagen", "finicioimagen_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoInicio", "fotoInicio_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionImagen", "descripcionImagen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("creditoInicio", "creditoInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoExterno", "videoExterno_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("videoInicio", "videoInicio_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "categoria", "categoria_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("categoriaHora", "hora", "hora_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("categoriaHora", "categoriaHora_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("tituloInicio", "tituloInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("resumen", "resumen_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaExterna", "rutaExterna_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetInicio", "targetInicio_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("claseDestacadoFoto2", "claseDestacadoFoto2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("agregarNotaRelacionada", "agregarNotaRelacionada_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr1", "tituloNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr1", "tipoNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr1", "rutaNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr1", "targetNr1_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr1", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr2", "tituloNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr2", "tipoNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr2", "rutaNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr2", "targetNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr2", "altNr2_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr3", "tituloNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr3", "tipoNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr3", "rutaNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr3", "targetNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr3", "altNr3_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr4", "tituloNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr4", "tipoNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr4", "rutaNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr4", "targetNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr4", "altNr4_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr5", "tituloNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr5", "tipoNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr5", "rutaNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr5", "targetNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr5", "altNr5_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr6", "tituloNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr6", "tipoNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr6", "rutaNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr6", "targetNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr6", "altNr6_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr7", "tituloNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr7", "tipoNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr7", "rutaNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr7", "targetNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr7", "altNr7_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tituloNr8", "tituloNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("tipoNr8", "tipoNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("rutaNr8", "rutaNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("targetNr8", "targetNr8_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("altNr8", "altNr8_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "imagenportada", "imagenportada_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoportada", "media_img_portada", "media_img_portada_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoportada", "fotoportada_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotoportada", "descripcionfotoportada_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "imagen", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotopanorama", "media_img_portada", "fotopanorama_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotopanorama", "fotopanorama_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionfotopanorama", "descripcionfotopanorama_ITERWEB", false, "text", "text", null));

            validFieldAttributList = new List<ValidMapFieldAttributes>();
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldAttributList.Add(new ValidMapFieldAttributes("fotoNoticiaClave", "media_img_imagen", "fotoNoticiaClave_ITERWEB", "text", "text", false));
            validFieldList.Add(new ValidMapField("fotoNoticiaClave", "fotoNoticiaClave_ITERWEB", false, "text", "text", validFieldAttributList));
            validFieldList.Add(new ValidMapField("descripcionnoticiaclave", "descripcionnoticiaclave_ITERWEB", false, "text", "text", null));
            validFieldList.Add(new ValidMapField("periodista", "periodista_ITERWEB", false, "text", "text", null));

            if (log.IsDebugEnabled) log.Debug("FillTemplate017 end template 'creacion_notaInteriorEditorial'");
            return validFieldList;
        }

        /// <summary>
        /// Given image, video, audio name in 'mediaName' that file is copied to 'documentPath'. 
        /// If resource is not found a log is written.
        /// </summary>
        /// <param name="mediaName">A file name part, e.g., 'pepito.jpg'</param>
        /// <param name="documentPath">Target folder to copy to </param>
        /// <param name="sourcePath">Source folder to copy from</param>
        private void CopyMedia(string mediaName, string documentPath, string sourcePath = "") {
            if (log.IsDebugEnabled) log.Debug("CopyMedia start");
            string sourceFile = "";
            string targetFile = "";

            if (sourcePath == "") {
                sourceFile = _se4MediaSourceFolder + @"\" + mediaName;
            }
            else {
                sourceFile = sourcePath + @"\" + mediaName;
            }
            if (!File.Exists(sourceFile)) {
                if (log.IsDebugEnabled) log.Debug("sourceFile=[" + sourceFile + "] does not exist");
            }
            else {
                targetFile = documentPath + @"\" + mediaName;
                File.Copy(sourceFile, targetFile, true);
            }
            if (log.IsDebugEnabled) log.Debug("CopyMedia end");
        }

        /// <summary>
        /// Given the XmlDocument, it is persisted to a file.
        /// </summary>
        /// <param name="fileName">Parent Location to save document</param>
        /// <param name="iterDoc">Information to save</param>
        private void SaveXmlDocument(string fileName, XmlDocument iterDoc) {
            if (log.IsDebugEnabled) log.Debug("SaveXmlDocument start");
            if (log.IsDebugEnabled) log.Debug("Using file=[" + fileName + "]");

            using (StreamWriter of = new StreamWriter(fileName)) {
                //if (_xmlBeautify == "yes") {
                //    of.Write(iterDoc.Beautify());
                //}
                //else {
                //    of.Write(iterDoc.InnerXml);
                //}
                iterDoc.Save(of);
            }
            if (log.IsDebugEnabled) log.Debug("SaveXmlDocument end");
        }

        /// <summary>
        /// Saves an old document structure to its XML file used in ITERWEB migration processing.
        /// </summary>
        /// <param name="contentPath">File System path to create the docuemnt</param>
        /// <param name="documentPath">File System path to create images</param>
        /// <param name="info">Document info</param>
        /// <param name="articleNum">Id for document to save</param>
        private void CreateDocumentAttributesToXml(string contentPath, string documentPath, Doc info, long articleNum) {
            if (log.IsDebugEnabled) log.Debug("CreateDocumentAttributesToXml start (Old Doc processing)");
            XmlCDataSection cdata = null;
            XmlDocument iterDoc = null;
            XmlElement item = null;
            XmlElement itemContent;
            string contentText;
            int i = 1;
            int itemNum = 1;
            string docGlobalRef = DocumentImageDir;
            iterDoc = new XmlDocument();
            XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement rootIter = iterDoc.CreateElement("root");
            iterDoc.PreserveWhitespace = false;
            iterDoc.AppendChild(rootIter);
            iterDoc.InsertBefore(iterDocDeclaration, rootIter);
            switch (info.TemplateType) {
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var bulletItem in info.Bullet) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", bulletItem.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = bulletItem.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }

                    itemNum = 1;
                    foreach (var pr in info.PhotoRelated) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pr.ImageName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "");
                        rootIter.AppendChild(item);

                        var imageName = docGlobalRef + ExtractImageNamePart(pr.ImageSrc);
                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", pr.FooterName);
                        itemChild.SetAttribute("type", "text");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = pr.Footer;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);
                        itemNum++;
                    }

                    itemNum = 1;
                    foreach (var rh in info.ReaderHelp) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", rh.NameTitle + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = rh.NameTitleContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", rh.NameText + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = rh.NameTextContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002:
                    // Process the NewsGrouper only list.
                    itemNum = 1;
                    foreach (var ng in info.NewsGrouper) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", ng.TitleName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = ng.TitleContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", ng.TextName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = ng.TextContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003:
                    // Process the CartoonGrouper only list.
                    itemNum = 1;
                    foreach (var cg in info.CartoonGrouper) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", cg.TitleName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = cg.TitleContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", cg.TextName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "");
                        rootIter.AppendChild(item);

                        var imageName = docGlobalRef + ExtractImageNamePart(cg.TextContent);
                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", cg.AuthorName); // Actually, Footer
                        itemChild.SetAttribute("type", "text");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = cg.AuthorContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.AuthorName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.CityName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.DisplayDateName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.DisplayDateText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var bulletItem in info.Bullet) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", bulletItem.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = bulletItem.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }

                    itemNum = 1;
                    foreach (var pr in info.PhotoRelated) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pr.ImageName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "");
                        rootIter.AppendChild(item);

                        var imageName = "";
                        itemContent = iterDoc.CreateElement("dynamic-content");
                        if (pr.ImageSrc != "") {
                            imageName = docGlobalRef + ExtractImageNamePart(pr.ImageSrc);
                        }

                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", pr.FooterName);
                        itemChild.SetAttribute("type", "text");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = pr.Footer;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);
                        itemNum++;
                    }

                    itemNum = 1;
                    foreach (var rh in info.ReaderHelp) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", rh.NameTitle + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = rh.NameTitleContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", rh.NameText + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = rh.NameTextContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var pg in info.PhotoGallery) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pg.PhotoBigName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        var imageName = docGlobalRef + ExtractImageNamePart(pg.PhotoBigContent);
                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pg.PhotoSmallName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        imageName = docGlobalRef + ExtractImageNamePart(pg.PhotoSmallContent);
                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pg.PhotoCreditName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = pg.PhotoCreditContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);

                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pg.PhotoFooterName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = pg.PhotoFooterContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_007:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.ImageOnlySet.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    var imageNameOnly = docGlobalRef + ExtractImageNamePart(info.ImageOnlySet.Content);
                    var itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "High");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Medium");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Low");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var bulletItem in info.Bullet) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", bulletItem.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = bulletItem.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_008:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var bulletItem in info.Bullet) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", bulletItem.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = bulletItem.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }

                    itemNum = 1;
                    foreach (var pr in info.PhotoRelated) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pr.ImageName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "");
                        rootIter.AppendChild(item);

                        var imageName = docGlobalRef + ExtractImageNamePart(pr.ImageSrc);
                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", pr.FooterName);
                        itemChild.SetAttribute("type", "text");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = pr.Footer;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);
                        itemNum++;
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_009:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var bulletItem in info.Bullet) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", bulletItem.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = bulletItem.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.AuthorName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.CityName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.DisplayDateName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.DisplayDateText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.ImageOnlySet.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    imageNameOnly = docGlobalRef + ExtractImageNamePart(info.ImageOnlySet.Content);
                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "High");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Medium");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Low");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);
                    break;

                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_011:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.AuthorName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.CityName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.DisplayDateName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.DisplayDateText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;

                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;

                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_013:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.AuthorName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.CityName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.DisplayDateName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.DisplayDateText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.ImageOnlySet.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    imageNameOnly = docGlobalRef + ExtractImageNamePart(info.ImageOnlySet.Content);
                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "High");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Medium");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);

                    itemChildOnly = iterDoc.CreateElement("dynamic-element");
                    itemChildOnly.SetAttribute("instance-id", "ec" + (i++));
                    itemChildOnly.SetAttribute("name", "Low");
                    itemChildOnly.SetAttribute("type", "document_library");
                    itemChildOnly.SetAttribute("index-type", "");
                    item.AppendChild(itemChildOnly);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = imageNameOnly;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    itemChildOnly.AppendChild(itemContent);
                    break;

                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_014:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.AuthorName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.CityName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.CreditText.DisplayDateName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.CreditText.DisplayDateText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;

                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_015:
                    itemNum = 1;
                    foreach (var tit in info.Title) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", tit.Name + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "text");
                        rootIter.AppendChild(item);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = tit.Content;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        item.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.DocumentText.Name);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    itemNum = 1;
                    foreach (var pr in info.PhotoFooterGallery.PhotoList) {
                        item = iterDoc.CreateElement("dynamic-element");
                        item.SetAttribute("instance-id", "ec" + (i++));
                        item.SetAttribute("name", pr.ImageName + itemNum);
                        item.SetAttribute("type", "text");
                        item.SetAttribute("index-type", "");
                        rootIter.AppendChild(item);

                        var imageName = docGlobalRef + ExtractImageNamePart(pr.ImageSrc);
                        var itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "High");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Medium");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);

                        itemChild = iterDoc.CreateElement("dynamic-element");
                        itemChild.SetAttribute("instance-id", "ec" + (i++));
                        itemChild.SetAttribute("name", "Low");
                        itemChild.SetAttribute("type", "document_library");
                        itemChild.SetAttribute("index-type", "");
                        item.AppendChild(itemChild);

                        itemContent = iterDoc.CreateElement("dynamic-content");
                        contentText = imageName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        itemContent.InnerXml = cdata.OuterXml;
                        itemChild.AppendChild(itemContent);
                        itemNum++;
                    }
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.PhotoFooterGallery.FooterTitleName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.PhotoFooterGallery.FoooterTitle;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);

                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", info.PhotoFooterGallery.FooterName);
                    item.SetAttribute("type", "text");
                    item.SetAttribute("index-type", "text");
                    rootIter.AppendChild(item);

                    itemContent = iterDoc.CreateElement("dynamic-content");
                    contentText = info.PhotoFooterGallery.Footer;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                    break;
            }
            SaveXmlDocument(contentPath + @"\" + articleNum + ".xml", iterDoc);
            if (info.TemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001) {
            }
            else {
                if (info.TemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002) {
                }
            }

            if (log.IsDebugEnabled) log.Debug("CreateDocumentAttributesToXml end (Old Doc processing)");
        }

        /// <summary>
        /// Retrieves document attributes and saves to disk
        /// </summary>
        /// <param name="contentPath">File System path to create the docuemnt</param>
        /// <param name="documentPath">File System path to create images</param>
        /// <param name="doc">Document info</param>
        /// <param name="attributeList">meta data elements associated with document</param>
        private void CreateDocumentAttributesToXml(string contentPath, string documentPath, IterwebMapInfo doc, List<SE4Attribute> attributeList) {
            if (log.IsDebugEnabled) log.Debug("CreateDocumentAttributesToXml start");
            XmlCDataSection cdata = null;
            XmlDocument iterDoc = null;
            XmlElement item = null;
            int i = 1;
            string name = "";
            string targetType = "";
            string targetIndexType = "";
            ValidMapField vf = null;
            List<ValidMapField> documentValidFields = se4TemplateFieldMapping[doc.Layout];

            iterDoc = new XmlDocument();
            XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement rootIter = iterDoc.CreateElement("root");
            iterDoc.PreserveWhitespace = false;
            iterDoc.AppendChild(rootIter);
            iterDoc.InsertBefore(iterDocDeclaration, rootIter);

            foreach (var it in attributeList) {
                vf = documentValidFields.Find(e => e.Source == it.ElementName && e.Use);
                if (vf.Attributes != null) {
                    ValidMapFieldAttributes attr = vf.Attributes.Find(e => e.SourceAttribute == it.AttributeName);
                    if (attr != null) {
                        name = attr.Target;
                        targetType = attr.TargetType;
                        targetIndexType = attr.TargetIndexType;
                    }
                    else {
                        name = vf.Target;
                        targetType = vf.TargetType;
                        targetIndexType = vf.TargetIndexType;
                    }
                }
                else {
                    name = vf.Target;
                    targetType = vf.TargetType;
                    targetIndexType = vf.TargetIndexType;
                }
                if (it.MultimediaAttribute == GlobalConstants.SE4ATTR_MULTIMEDIA_NONE) {
                    item = iterDoc.CreateElement("dynamic-element");
                    item.SetAttribute("instance-id", "ec" + (i++));
                    item.SetAttribute("name", name);
                    item.SetAttribute("type", targetType);
                    item.SetAttribute("index-type", targetIndexType);
                    rootIter.AppendChild(item);

                    XmlElement itemContent = iterDoc.CreateElement("dynamic-content");
                    string contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemContent.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemContent);
                }
                else {
                    switch (it.MultimediaAttribute) {
                        case GlobalConstants.SE4ATTR_MULTIMEDIA_IMAGE:
                            item = iterDoc.CreateElement("dynamic-element");
                            item.SetAttribute("instance-id", "ec" + (i++));
                            item.SetAttribute("name", name);
                            item.SetAttribute("type", targetType);
                            item.SetAttribute("index-type", targetIndexType);
                            rootIter.AppendChild(item);

                            var itemChild = iterDoc.CreateElement("dynamic-element");
                            itemChild.SetAttribute("instance-id", "ec" + (i++));
                            itemChild.SetAttribute("name", "High");
                            itemChild.SetAttribute("type", "document_library");
                            itemChild.SetAttribute("index-type", "");
                            item.AppendChild(itemChild);

                            XmlElement itemContent = iterDoc.CreateElement("dynamic-content");
                            string contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                            contentText = RemoveInvalidXmlChars(contentText);
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemContent.InnerXml = cdata.OuterXml;
                            itemChild.AppendChild(itemContent);

                            itemChild = iterDoc.CreateElement("dynamic-element");
                            itemChild.SetAttribute("instance-id", "ec" + (i++));
                            itemChild.SetAttribute("name", "Medium");
                            itemChild.SetAttribute("type", "document_library");
                            itemChild.SetAttribute("index-type", "");
                            item.AppendChild(itemChild);

                            itemContent = iterDoc.CreateElement("dynamic-content");
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemContent.InnerXml = cdata.OuterXml;
                            itemChild.AppendChild(itemContent);

                            itemChild = iterDoc.CreateElement("dynamic-element");
                            itemChild.SetAttribute("instance-id", "ec" + (i++));
                            itemChild.SetAttribute("name", "Low");
                            itemChild.SetAttribute("type", "document_library");
                            itemChild.SetAttribute("index-type", "");
                            item.AppendChild(itemChild);

                            itemContent = iterDoc.CreateElement("dynamic-content");
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemContent.InnerXml = cdata.OuterXml;
                            itemChild.AppendChild(itemContent);
                            break;
                        case GlobalConstants.SE4ATTR_MULTIMEDIA_VIDEO:
                            break;
                        case GlobalConstants.SE4ATTR_MULTIMEDIA_AUDIO:
                        default:
                            break;
                    }
                }
            }
            SaveXmlDocument(contentPath + @"\" + doc.IdObjetoSE + ".xml", iterDoc);
            if (log.IsDebugEnabled) log.Debug("CreateDocumentAttributesToXml end");
        }

        /// <summary>
        /// Inspects if there is an image in attr.
        /// </summary>
        /// <param name="attr">Data to work with</param>
        /// <param name="info">Info of document being processed</param>
        /// <param name="documentPath">Target resource directory</param>
        /// <returns>Filenane part</returns>
        private string ExtractMediaName(SE4Attribute attr, IterwebMapInfo info, string documentPath) {
            string s = "";
            if (log.IsDebugEnabled) log.Debug("Extracting mediaName Start ATTR");
            if (attr.Text != "") {
                s = attr.Text;
                var isJpg = s.Contains(".jpg") || s.Contains(".JPG");
                if (!isJpg) {
                    s += ".jpg";
                }
                s = ExtractImageNamePart(s);
                attr.Text = s = s.Supress('/').Supress('\\');
                s = CopyImageToZip(_se4MediaSourceFolder, documentPath, s, info);
                if (s != "") {
                    s = attr.Text = DocumentImageDir + s;
                    if (log.IsDebugEnabled) log.Debug("Media name found, set to [" + attr.Text + "]");
                }
            }
            if (log.IsDebugEnabled) log.Debug("Extracting mediaName End ATTR");
            return s;
        }

        /// <summary>
        /// This actually copies the image from source directory to target directory. Applies to Toon Directory images.
        /// </summary>
        /// <param name="srcDir">Image source folder to gather</param>
        /// <param name="targetDir">Image target folder to copy to</param>
        /// <param name="fileName">File name of image.</param>
        /// <param name="info">A toon directory info record</param>
        /// <returns>If 'fileName' has been changed due to non-ascii characters in it, it is returned like so, 
        /// otherwise, it is returned as such.</returns>
        private string CopyImageToZip(string srcDir, string targetDir, string fileName, ToonDirInfo info) {
            string line = "";
            string rslt = "";

            var sourceFile = srcDir + @"\" + fileName;
            rslt = fileName;
            if (log.IsDebugEnabled) {
                log.Debug("CopyImageToZip(string srcDir, string targetDir, string fileName, ToonDirInfo info) Start");
            }
            line = "Using parameters srcDir=[" + srcDir + "], targetDir=[" + targetDir + "], fileName=[" + fileName + "], info idsitemap=[" + info.Id + "]";
            if (log.IsDebugEnabled) {
                log.Debug(line);
            }
            if (!File.Exists(sourceFile)) {
                if (log.IsWarnEnabled) {
                    log.Warn("Document image for TOON id=[" + info.Id + "], sourceFile=[" + sourceFile + "] does not exist");
                }
                rslt = "";
            }
            else {
                rslt = ConfigureImageName(fileName, targetDir, srcDir);
            }
            if (log.IsDebugEnabled) {
                log.Debug("CopyImageToZip(string srcDir, string targetDir, string fileName, ToonDirInfo info) End");
            }
            return rslt;
        }

        /// <summary>
        /// This actually copies the image from source directory to target directory.
        /// </summary>
        /// <param name="srcDir">Image source folder to gather</param>
        /// <param name="targetDir">Image target folder to copy to</param>
        /// <param name="fileName">File name of image.</param>
        /// <param name="info">Info of document being processed</param>
        /// <param name="printToLog">True to print to log</param>
        /// <returns>If 'fileName' has been changed due to non-ascii characters in it, it is returned like so, 
        /// otherwise, it is returned as such.</returns>
        private string CopyImageToZip(string srcDir, string targetDir, string fileName, IterwebMapInfo info, bool printToLog = true) {
            string line = "";
            string rslt = "";
            var sourceFile = srcDir + @"\" + fileName;
            rslt = fileName;
            if (log.IsDebugEnabled) {
                log.Debug("CopyImageToZip(string srcDir, string targetDir, string fileName, IterwebMapInfo info) Start");
            }
            line = "Using parameters srcDir=[" + srcDir + "], targetDir=[" + targetDir + "], fileName=[" + fileName + "], info idsitemap=[" + info.IdSitemap + "]";
            if (log.IsDebugEnabled) {
                log.Debug(line);
            }
            if (!File.Exists(sourceFile)) {
                if (printToLog && log.IsWarnEnabled) {
                    log.Warn("Document image for idSitemap=[" + info.IdSitemap + "], sourceFile=[" + sourceFile + "] does not exist");
                }
                rslt = "";
            }
            else {
                rslt = ConfigureImageName(fileName, targetDir, srcDir);
            }
            if (log.IsDebugEnabled) {
                log.Debug("CopyImageToZip(string srcDir, string targetDir, string fileName, IterwebMapInfo info) End");
            }
            return rslt;
        }

        /// <summary>
        /// Given the id for document in SE4, it loads all of its attribute elements to further processing.
        /// </summary>
        /// <param name="p">id of document to retrieve</param>
        /// <param name="layout">Name of layout to check fields against</param>
        /// <returns>List of useful attributes for document</returns>
        private List<SE4Attribute> LoadSE4DocItems(long p, string layout) {
            if (log.IsDebugEnabled) log.Debug("LoadSE4DocItems start");
            string elementName = "";
            List<SE4Attribute> docAttrList = new List<SE4Attribute>();
            List<ValidMapField> documentValidFields = se4TemplateFieldMapping[layout];
            string sql = "select A.Orden, A.Elemento, A.OrdenAtributo, A.Atributo, A.Texto, A.CodContenidoLargo, B.ContenidoLargo " +
                         " from objetosecontenido A left join " +
                         "     contenidolargo    B on A.CodContenidoLargo = B.id_ContenidoLargo " +
                         " where Codobjetose = @idSE4 " +
                         "order by A.CodObjetoSE, A.orden, A.OrdenAtributo ";

            if (log.IsDebugEnabled) {
                log.Debug("Using sql=[" + sql + "]");
            }

            HandleDatabase hdb = new HandleDatabase(_connStr);
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@idSE4";
            param1.Value = p;
            param1.SqlDbType = SqlDbType.BigInt;

            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("LoadSE4DocItems");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param1);
            while (rdr.Read()) {
                elementName = rdr["Elemento"].ToString();
                if (documentValidFields.Exists(e => e.Source == elementName && e.Use)) {
                    int order = Convert.ToInt32(rdr["Orden"]);
                    int attributeOrder = Convert.ToInt32(rdr["OrdenAtributo"]);
                    string attributeName = rdr["Atributo"].ToString();
                    string text = rdr["Texto"].ToString();
                    long hugeContentCode = -1;
                    string hugeText = null;
                    SE4Attribute attr = null;

                    if (rdr["CodContenidoLargo"] != DBNull.Value) {
                        hugeContentCode = Convert.ToInt64(rdr["CodContenidoLargo"]);
                    }
                    if (rdr["ContenidoLargo"] != DBNull.Value) {
                        hugeText = rdr["ContenidoLargo"].ToString();
                    }

                    text = ReplaceHTMLCharacters(text);
                    hugeText = ReplaceHTMLCharacters(hugeText);

                    ValidMapField vmf = documentValidFields.Find(e => e.Source == elementName && e.Use);
                    if (vmf.Attributes != null) {
                        ValidMapFieldAttributes vmfa = vmf.Attributes.Find(e => e.Source == elementName && e.SourceAttribute == attributeName && e.Use);
                        if (vmfa == null) {
                            continue;
                        }
                    }
                    attr = new SE4Attribute(order, elementName, attributeOrder, attributeName, text, hugeContentCode, hugeText);
                    docAttrList.Add(attr);
                }
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) log.Debug("LoadSE4DocItems end");
            return docAttrList;
        }

        /// <summary>
        /// Create the image entry in XML for image dependency.
        /// </summary>
        /// <param name="iterDoc">Master XML manifest reference</param>
        /// <param name="iterPool">Node to append to</param>        
        /// <param name="imgName">Resource name</param>
        /// <param name="imgId">ID to identify the resource</param>
        private void CreateImageEntry(XmlDocument iterDoc, XmlElement iterPool, string imgName, string imgId) {
            if (log.IsDebugEnabled) {
                log.Debug("CreateImageEntry start");
                log.Debug("Processing image name =[" + imgName + "], imgId=[" + imgId + "]");
            }

            XmlElement param = null;
            string[] imgNameParts = imgName.Split('.');

            XmlElement itemImg = iterDoc.CreateElement("item");
            itemImg.SetAttribute("classname", "com.liferay.portlet.documentlibrary.model.DLFileEntry");
            itemImg.SetAttribute("globalid", imgId);
            itemImg.SetAttribute("groupid", "Global");
            itemImg.SetAttribute("operation", "create");

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "folder");
            param.InnerText = "&lt;![CDATA[0]]&gt;";
            itemImg.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "extension");
            param.InnerText = "&lt;![CDATA[" + imgNameParts[1] + "]]&gt;";
            itemImg.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "title");
            param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(imgName) + "]]&gt;"; ;
            itemImg.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "name");
            param.InnerText = "&lt;![CDATA[" + imgNameParts[0] + "]]&gt;"; ;
            itemImg.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "fileurl");
            param.InnerText = "&lt;![CDATA[" + DocumentImageDir + imgName + "]]&gt;"; ; ;
            itemImg.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "sourcename");
            param.InnerText = "&lt;![CDATA[" + imgName + "]]&gt;"; ;
            itemImg.AppendChild(param);

            iterPool.AppendChild(itemImg);
            if (log.IsDebugEnabled) log.Debug("CreateImageEntry end");
        }

        /// <summary>
        /// Create the page content entry in XML for PC dependency.
        /// </summary>
        /// <param name="iterDoc">Master XML manifest reference</param>
        /// <param name="iterPool">Node to append to</param>        
        /// <param name="pcId">ID to identify the resource</param>
        /// <param name="idSE4">Refers to the article being mapped.</param>
        private void CreatePageContentEntry(XmlDocument iterDoc, XmlElement iterPool, string pcId, string idSE4) {
            if (log.IsDebugEnabled) {
                log.Debug("CreatePageContentEntry start");
                log.Debug("Processing page content=[" + pcId + "]");
            }

            string pcIdModified = pcId.Replace("EC_PC", "EC_PAG");
            XmlElement param = null;
            XmlElement itemPC = iterDoc.CreateElement("item");
            itemPC.SetAttribute("classname", "com.protecmedia.iter.news.model.PageContent");
            itemPC.SetAttribute("operation", "create");
            itemPC.SetAttribute("globalid", pcId);
            itemPC.SetAttribute("groupid", _globalSiteName);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "orden");
            param.InnerText = "&lt;![CDATA[1]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "pagetemplate");
            param.InnerText = "&lt;![CDATA[]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "qualification");
            param.InnerText = "&lt;![CDATA[" + _qualification + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "datefrom");
            param.InnerText = "&lt;![CDATA[" + String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "article");
            param.InnerText = "&lt;![CDATA[" + idSE4 + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "layout");
            param.InnerText = "&lt;![CDATA[" + _layout + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "defaultSection");
            param.InnerText = "&lt;![CDATA[false]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "dateto");
            param.InnerText = "&lt;![CDATA[" + String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "type");
            param.InnerText = "&lt;![CDATA[STANDARD-ARTICLE]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "contentgroup");
            param.InnerText = "&lt;![CDATA[Global]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "pagecontentid");
            param.InnerText = "&lt;![CDATA[" + pcIdModified + "]]&gt;";
            itemPC.AppendChild(param);

            param = iterDoc.CreateElement("param");
            param.SetAttribute("name", "online");
            param.InnerText = "&lt;![CDATA[false]]&gt;";
            itemPC.AppendChild(param);

            iterPool.AppendChild(itemPC);
            if (log.IsDebugEnabled) {
                log.Debug("CreatePageContentEntry end");
            }
        }

        /// <summary>
        /// Updates old document Template Type based in the jSonContent. 
        /// NOTE: Used only once.
        /// </summary>
        protected void OldDocUpdateTemplateType() {
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("SiteMapUpdate");
            foreach (var item in _se4DocList) {
                Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(item.JsonContent);

                if (examinedDoc != null) {
                    int templateType = examinedDoc.TemplateType;
                    string sql = "Update sitemap set oldDocTemplateType = @templateType where idOld = @id";

                    SqlParameter param4 = new SqlParameter();
                    param4.ParameterName = "@templateType";
                    param4.Value = examinedDoc.TemplateType;
                    param4.SqlDbType = SqlDbType.Int;

                    SqlParameter param3 = new SqlParameter();
                    param3.ParameterName = "@id";
                    param3.Value = item.IDOldDoc;
                    param3.SqlDbType = SqlDbType.VarChar;
                    hdb.ExecSQLStmt(transaction, sql, param3, param4);
                }
            }
            transaction.Commit();
            hdb.Close();
        }

        /// <summary>
        /// Process packages for SE4 Documents (source come from database table).
        /// See notes (1) -> Output XML structure change.
        /// Here it is considered the 'OutputStructure' which dictates the final output XML.
        /// </summary>
        private void GeneratePackagesForSE4Docs() {
            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesForSE4Docs Start");
            }

            XmlDocument iterDoc = null;
            XmlElement iterPool = null;
            XmlElement iterList = null;
            XmlElement innerParam = null;
            XmlElement item = null;
            XmlElement param = null;
            XmlElement rootArticles = null;
            List<string> se4DocNonGenerated = new List<string>();
            List<IterwebMapInfo> processedIds = new List<IterwebMapInfo>();
            int i;
            string path = "";
            string iterwebManifest = "";
            string contentPath = "";
            string documentPath = "";
            string manifestFile = "";
            string zipFileName = "";
            string zipFileNameOnly = "";
            string logLine = "";
            string title = "";
            string lineInfo = "Generating for " + _se4DocList.Count + " documents";
            long numDocsWritten = 0L;
            long totalNumDocsWritten = 0L;

            if (_outputStructure == 2) {
                DocumentImageDir = "/bin/";
            }
            if (_se4DocList.Count == 0) {
                string msg = "No documents to package, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsInfoEnabled) {
                    log.Info(msg);
                }
                return;
            }
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            i = 1;
            foreach (var it in _se4DocList) {
                if (_updateCreateDate == 1) {
                    // Get File.ASP from URL create time as display document timestamp
                    string urlToUse = it.Url;
                    urlToUse = urlToUse.ReconfigureHostNameFrom(_knownDomainList, _replaceKnownDomainList);
                    it.UpdateDate = it.DisplayDate = ExtractOldDocFileDateTime(urlToUse);
                    UpdateOldDoc(it);
                }
                if (i == 1) {
                    // Create folder structure
                    iterwebManifest = Convert.ToString(cnts.IterWebManifestFile).PadLeft(10, '0') + "-pkse4docs";

                    logLine = "Package " + iterwebManifest;
                    if (log.IsInfoEnabled) log.Info(logLine);
                    Console.WriteLine(logLine);
                    path = _migrationStoreFolder + @"\" + iterwebManifest;
                    contentPath = path + @"\contents";
                    documentPath = path + @"\documents\Global\0";
                    manifestFile = path + @"\iter.xml";
                    zipFileNameOnly = iterwebManifest + ".zip";
                    zipFileName = _zipFolder + @"\" + zipFileNameOnly;

                    if (_outputStructure == 2) {
                        documentPath = path + @"\bin";
                    }
                    _docPath = documentPath;
                    Directory.CreateDirectory(path);
                    if (_outputStructure == 1) {
                        Directory.CreateDirectory(contentPath);
                    }
                    Directory.CreateDirectory(documentPath);
                    processedIds.Clear();
                    iterDoc = new XmlDocument();
                    if (_outputStructure == 1) {
                        XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        XmlElement rootIter = iterDoc.CreateElement("iter");
                        iterDoc.PreserveWhitespace = false;
                        rootIter.SetAttribute("scopegroupid", _globalSiteName);
                        rootIter.SetAttribute("maxwidth", _defaultImageMaxWidth);
                        rootIter.SetAttribute("maxheight", _defaultImageMaxHeight);
                        iterDoc.AppendChild(rootIter);
                        iterDoc.InsertBefore(iterDocDeclaration, rootIter);

                        iterList = iterDoc.CreateElement("list");
                        rootIter.AppendChild(iterList);
                    }
                    if (_outputStructure == 2) {
                        XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        rootArticles = iterDoc.CreateElement("articles");
                        iterDoc.PreserveWhitespace = false;
                        rootArticles.SetAttribute("maxwidth", _defaultImageMaxWidth);
                        rootArticles.SetAttribute("maxheight", _defaultImageMaxHeight);
                        iterDoc.AppendChild(rootArticles);
                        iterDoc.InsertBefore(iterDocDeclaration, rootArticles);
                    }
                }

                if (_outputStructure == 1) {
                    // If title is longer than the specified number of characters, then it is truncated                
                    title = it.SEName.LimitTo(_titleLimitTo);
                    iterPool = iterDoc.CreateElement("pool");
                    iterList.AppendChild(iterPool);

                    innerParam = null;
                    item = iterDoc.CreateElement("item");
                    item.SetAttribute("classname", "com.liferay.portlet.journal.model.JournalArticle");
                    item.SetAttribute("globalid", it.IDSE4);
                    item.SetAttribute("operation", "create");
                    iterPool.AppendChild(item);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "template");
                    param.InnerText = "&lt;![CDATA[" + _iterWebLayoutName + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "structure");
                    param.InnerText = "&lt;![CDATA[STANDARD-ARTICLE]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "articleid");
                    param.InnerText = "&lt;![CDATA[" + it.IDSE4ArticleId + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "indexable");
                    param.InnerText = "&lt;![CDATA[1]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "expirationdate");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "reviewdate");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "type");
                    param.InnerText = "&lt;![CDATA[general]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "url-title");
                    param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(title.HyphenLower(it.IdObjetoSE)) + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "legacy-url");
                    param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(it.UrlPath) + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "displaydate");
                    param.InnerText = "&lt;![CDATA[" + String.Format("{0:yyyy/MM/dd HH:mm:ss}", it.DisplayDate) + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "version");
                    param.InnerText = "&lt;![CDATA[1.0]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "title");
                    param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(title) + "]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "file");
                    param.InnerText = "&lt;![CDATA[/contents/" + it.IdObjetoSE + ".xml]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "description");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "small-image-url");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "small-image");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", "small-image-file");
                    param.InnerText = "&lt;![CDATA[]]&gt;";
                    item.AppendChild(param);

                    // Retrieve Document Attributes and filter to use those enabled to do so.
                    List<SE4Attribute> docSE4AttrList = LoadSE4DocItems(it.IdObjetoSE, it.Layout);

                    // Copy media to Zip Container.
                    foreach (var s in mediaNameOldList) {
                        var sourceFile = _se4MediaSourceFolder + @"\" + s;

                        if (!File.Exists(sourceFile)) {
                            if (log.IsErrorEnabled) {
                                log.Error("Document image for idSitemap=[" + it.IdSitemap + "], sourceFile=[" + sourceFile + "] does not exist");
                            }
                        }
                        else {
                            CopyMedia(s, documentPath);
                        }
                    }
                    try {
                        CreateDocumentAttributesToXml(contentPath, documentPath, it, docSE4AttrList);
                    }
                    catch (Exception e) {
                        var msg = "Document not generated (creating Document XML), an exception occured idSitemap=[" + it.IdSitemap + "]";
                        se4DocNonGenerated.Add(msg);
                        if (log.IsFatalEnabled) {
                            log.Fatal(msg, e);
                        }
                    }

                    // Create Dependencies
                    // 1) Images
                    foreach (var s in mediaNameOldList) {
                        string imgId = "EC_IMG_" + (cnts.Image++);
                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", imgId);
                        param.SetAttribute("type", "dependency");
                        item.AppendChild(param);

                        innerParam = iterDoc.CreateElement("param");
                        innerParam.SetAttribute("name", "classname");
                        innerParam.InnerText = "&lt;![CDATA[com.liferay.portlet.documentlibrary.model.DLFileEntry]]&gt;";
                        param.AppendChild(innerParam);

                        innerParam = iterDoc.CreateElement("param");
                        innerParam.SetAttribute("name", "groupname");
                        innerParam.InnerText = "&lt;![CDATA[Global]]&gt;";
                        param.AppendChild(innerParam);

                        try {
                            CreateImageEntry(iterDoc, iterPool, s, imgId);
                        }
                        catch (Exception e) {
                            var msg = "Document not generated (creating Image Entry), an exception occured idSitemap=[" + it.IdSitemap + "]";
                            se4DocNonGenerated.Add(msg);
                            if (log.IsFatalEnabled) {
                                log.Fatal("Document not generated, an exception occured, doc idSitemap=" + it.IdSitemap, e);
                            }
                        }
                    }

                    // 2) Page Content
                    string pcId = "EC_PC_" + (cnts.PageContent++);
                    param = iterDoc.CreateElement("param");
                    param.SetAttribute("name", pcId);
                    param.SetAttribute("type", "dependency");
                    item.AppendChild(param);

                    innerParam = iterDoc.CreateElement("param");
                    innerParam.SetAttribute("name", "classname");
                    innerParam.InnerText = "&lt;![CDATA[com.protecmedia.iter.news.model.PageContent]]&gt;";
                    param.AppendChild(innerParam);

                    innerParam = iterDoc.CreateElement("param");
                    innerParam.SetAttribute("name", "groupname");
                    innerParam.InnerText = "&lt;![CDATA[" + _globalSiteName + "]]&gt;";
                    param.AppendChild(innerParam);

                    CreatePageContentEntry(iterDoc, iterPool, pcId, it.IDSE4);
                }
                if (_outputStructure == 2) {
                    if (se4TemplateFieldMapping.ContainsKey(it.Layout)) {
                        // If title is longer than the specified number of characters, then it is truncated                
                        title = it.SEName.LimitTo(_titleLimitTo);
                        bool createDoc = true;
                        int discardedCode = 0;

                        // If document is 'creacion_galeria' and there is not 'pubinfo.txt' or Gallery directory then 
                        // document is marked as 
                        if (it.Layout == "creacion_galeria") {
                            createDoc = IsValidImageGallery(it, documentPath);
                            discardedCode = 16;
                        }
                        else {
                            // For other templatees let's find out if they are valid to be generated.
                            createDoc = IsValidDocument(it);
                            discardedCode = 15;
                        }
                        if (createDoc) {
                            CreateOutputArticleDocToXml(false, documentPath, iterDoc, rootArticles, it, title);
                            processedIds.Add(it);
                            numDocsWritten++;
                        }
                        else {
                            it.Processed = discardedCode;
                            processedIds.Add(it);
                            lineInfo = "Discarded document, not written to package idSitemap=[" + it.IdSitemap + "], processed=[" + discardedCode + "]";
                            Console.WriteLine(lineInfo);
                            if (log.IsWarnEnabled) {
                                log.Warn(lineInfo);
                            }
                        }
                    }
                }
                else {
                    processedIds.Add(it);
                    numDocsWritten++;
                }

                // Process next document in list.
                i++;
                if (i > _docLimit) {
                    // Saving file to disk and go on to next package.                    
                    SaveXmlDocument(manifestFile, iterDoc);
                    CompressToZip(path, zipFileName);
                    i = 1;
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                    MarkRecordsAsProcessed(processedIds, zipFileNameOnly);
                    logLine = "Package " + iterwebManifest + " -> Ids [" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    logLine = "Package " + iterwebManifest + " contains " + numDocsWritten + " documents.";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    totalNumDocsWritten += numDocsWritten;
                    numDocsWritten = 0L;
                    GC.Collect();
                }
            }
            if (iterDoc != null) {
                if (Directory.Exists(path)) {
                    SaveXmlDocument(manifestFile, iterDoc);
                    CompressToZip(path, zipFileName);
                    MarkRecordsAsProcessed(processedIds, zipFileNameOnly);
                    logLine = "Package " + iterwebManifest + " -> Ids [" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    logLine = "Package " + iterwebManifest + " contains " + numDocsWritten + " documents.";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    totalNumDocsWritten += numDocsWritten;
                    numDocsWritten = 0L;
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                GC.Collect();
            }

            if (se4DocNonGenerated.Count != 0) {
                Console.WriteLine();
                Console.WriteLine("SE4 Documents not generated");
                foreach (var s in se4DocNonGenerated) {
                    Console.WriteLine(s);
                    if (log.IsErrorEnabled) {
                        log.Error(s);
                    }
                }
            }
            lineInfo = "Generating for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            lineInfo = "Actually written documents to Package " + totalNumDocsWritten;
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesForSE4Docs End");
            }
        }

        /// <summary>
        /// Process packages for Toons found in a directory (source come from database table 'toondir').
        /// NOTE: Method only supports 'outputstructure' set to 2.
        /// </summary>
        private void GeneratePackagesToonDir() {
            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesToonDir Start");
            }
            string sourceImagePath = _se4MediaSourceFolder + @"\caricaturas2";
            XmlDocument iterDoc = null;
            XmlElement rootArticles = null;
            List<ToonDirInfo> processedIds = new List<ToonDirInfo>();
            int i;
            string path = "";
            string iterwebManifest = "";
            string contentPath = "";
            string documentPath = "";
            string manifestFile = "";
            string zipFileName = "";
            string zipFileNameOnly = "";
            string logLine = "";
            string lineInfo = "Generating for " + _toonList.Count + " documents";

            if (_outputStructure == 2) {
                DocumentImageDir = "/bin/";
            }
            if (_toonList.Count == 0) {
                string msg = "No documents to package, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsWarnEnabled) {
                    log.Warn(msg);
                }
                return;
            }
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            i = 1;
            foreach (var it in _toonList) {
                if (i == 1) {
                    // Create folder structure
                    iterwebManifest = Convert.ToString(cnts.IterWebManifestFile).PadLeft(10, '0') + "-pktoondir";

                    logLine = "Package " + iterwebManifest;
                    if (log.IsInfoEnabled) log.Info(logLine);
                    Console.WriteLine(logLine);
                    path = _migrationStoreFolder + @"\" + iterwebManifest;
                    contentPath = path + @"\contents";
                    documentPath = path + @"\bin";
                    manifestFile = path + @"\iter.xml";
                    zipFileNameOnly = iterwebManifest + ".zip";
                    zipFileName = _zipFolder + @"\" + zipFileNameOnly;

                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(documentPath);

                    processedIds.Clear();
                    iterDoc = new XmlDocument();

                    XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    rootArticles = iterDoc.CreateElement("articles");
                    iterDoc.PreserveWhitespace = false;
                    rootArticles.SetAttribute("maxwidth", _defaultImageMaxWidth);
                    rootArticles.SetAttribute("maxheight", _defaultImageMaxHeight);
                    iterDoc.AppendChild(rootArticles);
                    iterDoc.InsertBefore(iterDocDeclaration, rootArticles);
                }
                CreateOutputArticleToonDocToXml(iterDoc, rootArticles, it, sourceImagePath, documentPath);
                processedIds.Add(it);

                // Process next document in list.
                i++;
                if (i > _docLimit) {
                    // Saving file to disk and go on to next package.                    
                    SaveXmlDocument(manifestFile, iterDoc);
                    CompressToZip(path, zipFileName);
                    i = 1;
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                    MarkToonDirRecordsAsProcessed(processedIds, zipFileNameOnly);
                    GC.Collect();
                }
            }
            if (iterDoc != null) {
                if (Directory.Exists(path)) {
                    SaveXmlDocument(manifestFile, iterDoc);
                    CompressToZip(path, zipFileName);
                    MarkToonDirRecordsAsProcessed(processedIds, zipFileNameOnly);
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                GC.Collect();
            }
            lineInfo = "Generating for " + _toonList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesToonDir End");
            }
        }

        /// <summary>
        /// Marks records in list as being processed. Further iterations should not use that record again.
        /// </summary>
        /// <param name="processedIds">List of documents to update.</param>
        /// <param name="zipFileName">Indicates all these 'processedIds' are stored in this zip file.</param>
        private void MarkRecordsAsProcessed(List<IterwebMapInfo> processedIds, string zipFileName) {
            if (log.IsDebugEnabled) log.Debug("MarkRecordsAsProcessed start");
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("markRecordsAsProcessed");
            String sql = "update sitemap set processed = @processed, zipFilename = @zipFileName where idSitemap = @idSitemap";
            SqlParameter param1 = new SqlParameter();
            SqlParameter param2 = new SqlParameter();
            SqlParameter param3 = new SqlParameter();
            param1.ParameterName = "@idSitemap";
            param1.SqlDbType = SqlDbType.Int;
            param2.ParameterName = "@zipFileName";
            param2.SqlDbType = SqlDbType.VarChar;
            param3.ParameterName = "@processed";
            param3.SqlDbType = SqlDbType.Int;
            foreach (var it in processedIds) {
                param1.Value = it.IdSitemap;
                param2.Value = zipFileName;
                param3.Value = (it.Processed == 0 ? 1 : it.Processed);
                hdb.ExecSQLStmt(transaction, sql, param1, param2, param3);
            }
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) log.Debug("MarkRecordsAsProcessed end");
        }

        /// <summary>
        /// Marks records in list as being processed. Further iterations should not use that record again.
        /// </summary>
        /// <param name="processedIds">List of Documents to update.</param>
        /// <param name="zipFileName">Indicates all these 'processedIds' are stored in this zip file.</param>
        private void MarkToonDirRecordsAsProcessed(List<ToonDirInfo> processedIds, string zipFileName) {
            if (log.IsDebugEnabled) log.Debug("MarkToonDirRecordsAsProcessed start");
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("MarkToonDirRecordsAsProcessed");
            String sql = "update toondir set processed = @processed, zipFilename = @zipFileName where id = @id";
            SqlParameter param1 = new SqlParameter();
            SqlParameter param2 = new SqlParameter();
            SqlParameter param3 = new SqlParameter();
            param1.ParameterName = "@id";
            param1.SqlDbType = SqlDbType.Int;
            param2.ParameterName = "@zipFileName";
            param2.SqlDbType = SqlDbType.VarChar;
            param3.ParameterName = "@processed";
            param3.SqlDbType = SqlDbType.Int;
            foreach (var it in processedIds) {
                param1.Value = it.Id;
                param2.Value = zipFileName;
                param3.Value = (it.Processed == 0 ? 1 : it.Processed);
                hdb.ExecSQLStmt(transaction, sql, param1, param2, param3);
            }
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) log.Debug("MarkToonDirRecordsAsProcessed end");
        }

        /// <summary>
        /// Copies one image to the image list refeenced in manifest file
        /// </summary>
        /// <param name="documentPath">In XML reference path</param>
        /// <param name="articleNum">Article being processed</param>
        /// <param name="imgName">Image name in reference</param>
        private void AssignImageAttributeToList(string documentPath, long articleNum, string imgName) {
            var s = ExtractImageNamePart(imgName);
            var sourceFile = _se4MediaSourceFolder + @"\" + s;

            //mediaNameOldList.Add(s);
            if (!File.Exists(sourceFile)) {
                if (log.IsErrorEnabled) {
                    log.Error("Document image for id=[" + articleNum + "], sourceFile=[" + sourceFile + "] does not exist");
                }
            }
            else {
                CopyMedia(s, documentPath);
            }
        }

        /// <summary>
        /// Locates first CartoonGrouper file and extracts its modified date and sets it as the new CreateDate/ModifiedDate for 
        /// record.
        /// </summary>
        /// <remarks>Be specially cautious as the 'it' create/modify date is not persisted back to database.</remarks>
        /// <param name="it">Record to update</param>
        /// <param name="examinedDoc">Json Converted data.</param>
        private void UpdateOldDocToonCreateDate(IterwebMapInfo it, Doc examinedDoc) {
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDocToonCreateDate Start");
            }

            //Se4MediaSourceFolder
                
            string fileNamePath = "";
            foreach (var cg in examinedDoc.CartoonGrouper) {
                fileNamePath = _se4MediaSourceFolder + "\\" + ExtractImageNamePart(cg.TextContent);
                DateTime dt = new DateTime(1900, 01, 01); // If file does not exist, sets this date. Used for OLD docs scanning.
                if (File.Exists(fileNamePath)) {
                    dt = File.GetLastWriteTime(fileNamePath);                    
                }
                it.DisplayDate = it.UpdateDate = dt;
            }            
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDocToonCreateDate End");
            }
        }

        /// <summary>
        /// When a need to update the creation date for OLD documents, updates record like so.
        /// </summary>
        private void UpdateCreateDateForOldDocsOnly() {
            if (log.IsDebugEnabled) {
                log.Debug("UpdateCreateDateForOldDocsOnly Start");
            }
            if (_se4DocList.Count == 0) {
                string msg = "No documents to update, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsInfoEnabled) {
                    log.Info(msg);
                }
                if (log.IsDebugEnabled) {
                    log.Debug("UpdateCreateDateForOldDocsOnly End");
                }
                return;
            }

            string lineInfo = "Updating for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            foreach (var it in _se4DocList) {
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    // Get File.ASP from URL create time as display document timestamp
                    string urlToUse = it.Url;
                    urlToUse = urlToUse.ReconfigureHostNameFrom(_knownDomainList, _replaceKnownDomainList);
                    it.UpdateDate = it.DisplayDate = ExtractOldDocFileDateTime(urlToUse);
                    UpdateOldDoc(it);
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("UpdateCreateDateForOldDocsOnly End");
            }
        }

        /// <summary>
        /// Process packages for Old Documents (source come from database table).
        /// See notes (1) -> Output XML structure change.
        /// Here it is considered the 'OutputStructure' which dictates the final output XML.
        /// </summary>
        private void GeneratePackagesForOldDocs() {
            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesForOldDocs Start");
            }
            if (_se4DocList.Count == 0) {
                string msg = "No documents to package, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsInfoEnabled) {
                    log.Info(msg);
                }
                if (log.IsDebugEnabled) {
                    log.Debug("GeneratePackagesForOldDocs End");
                }
                return;
            }

            long numDocGenerated = 0L;
            XmlDocument iterDoc = null;
            XmlElement iterPool = null;
            XmlElement iterList = null;
            XmlElement param = null;
            XmlElement innerParam = null;
            XmlElement item = null;
            XmlElement rootArticles = null;
            List<string> se4DocNonGenerated = new List<string>();
            List<IterwebMapInfo> processedIds = new List<IterwebMapInfo>();
            int i;
            string path = "";
            string iterwebManifest = "";
            string contentPath = "";
            string documentPath = "";
            string manifestFile = "";
            string zipFileName = "";
            string zipFileNameOnly = "";
            string logLine = "";
            string lineInfo = "Generating for " + _se4DocList.Count + " documents";
            long numDocsWritten = 0L;
            long totalNumDocsWritten = 0L;
            bool createDoc = false;

            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            i = 1;
            foreach (var it in _se4DocList) {
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    createDoc = true;
                    if (_updateCreateDate == 1) {
                        // Get File.ASP from URL create time as display document timestamp
                        string urlToUse = it.Url;
                        urlToUse = urlToUse.ReconfigureHostNameFrom(_knownDomainList, _replaceKnownDomainList);
                        it.UpdateDate = it.DisplayDate = ExtractOldDocFileDateTime(urlToUse);
                        UpdateOldDoc(it);
                    }
                    numDocGenerated++;
                    if (i == 1) {
                        // Create folder structure
                        iterwebManifest = Convert.ToString(cnts.IterWebManifestFile).PadLeft(10, '0') + "-pkolddocs";

                        logLine = "Package " + iterwebManifest;
                        if (log.IsInfoEnabled) log.Info(logLine);
                        Console.WriteLine(logLine);
                        path = _migrationStoreFolder + @"\" + iterwebManifest;
                        contentPath = path + @"\contents";
                        documentPath = path + @"\documents\Global\0";
                        manifestFile = path + @"\iter.xml";
                        zipFileNameOnly = iterwebManifest + ".zip";
                        zipFileName = _zipFolder + @"\" + zipFileNameOnly;

                        if (_outputStructure == 2) {
                            documentPath = path + @"\bin";
                        }

                        Directory.CreateDirectory(path);
                        if (_outputStructure == 1) {
                            Directory.CreateDirectory(contentPath);
                        }
                        Directory.CreateDirectory(documentPath);
                        processedIds.Clear();
                        iterDoc = new XmlDocument();
                        if (_outputStructure == 1) {
                            XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                            XmlElement rootIter = iterDoc.CreateElement("iter");
                            iterDoc.PreserveWhitespace = false;
                            rootIter.SetAttribute("scopegroupid", _globalSiteName);
                            rootIter.SetAttribute("maxwidth", _defaultImageMaxWidth);
                            rootIter.SetAttribute("maxheight", _defaultImageMaxHeight);
                            iterDoc.AppendChild(rootIter);
                            iterDoc.InsertBefore(iterDocDeclaration, rootIter);

                            iterList = iterDoc.CreateElement("list");
                            rootIter.AppendChild(iterList);
                        }
                        if (_outputStructure == 2) {
                            XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                            rootArticles = iterDoc.CreateElement("articles");
                            iterDoc.PreserveWhitespace = false;
                            rootArticles.SetAttribute("maxwidth", _defaultImageMaxWidth);
                            rootArticles.SetAttribute("maxheight", _defaultImageMaxHeight);
                            iterDoc.AppendChild(rootArticles);
                            iterDoc.InsertBefore(iterDocDeclaration, rootArticles);
                        }
                    }

                    Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);
                    long articleNum = Convert.ToInt64(it.IDSE4ArticleId.Substring(3));
                    string title = "";

                    examinedDoc.SetMetadataNameToFinal();
                    if (examinedDoc.TemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003) {
                        title = examinedDoc.CartoonGrouper[0].TitleContent;
                    }
                    else {
                        title = examinedDoc.Title[0].Content;
                    }
                    if (title == "") {
                        title = it.UrlTitle;
                    }
                    if (_outputStructure == 1) {
                        // If title is longer than the specified number of characters, then it is truncated
                        title = title.LimitTo(_titleLimitTo);
                        iterPool = iterDoc.CreateElement("pool");
                        iterList.AppendChild(iterPool);

                        innerParam = null;
                        item = iterDoc.CreateElement("item");
                        item.SetAttribute("classname", "com.liferay.portlet.journal.model.JournalArticle");
                        item.SetAttribute("globalid", it.IDOldDoc);
                        item.SetAttribute("operation", "create");
                        iterPool.AppendChild(item);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "template");
                        param.InnerText = "&lt;![CDATA[" + _iterWebLayoutName + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "structure");
                        param.InnerText = "&lt;![CDATA[STANDARD-ARTICLE]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "articleid");
                        param.InnerText = "&lt;![CDATA[" + it.IDSE4ArticleId + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "indexable");
                        param.InnerText = "&lt;![CDATA[1]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "expirationdate");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "reviewdate");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "type");
                        param.InnerText = "&lt;![CDATA[general]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "url-title");
                        param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(title.HyphenLower(articleNum)) + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "legacy-url");
                        param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(it.UrlPath) + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "displaydate");
                        param.InnerText = "&lt;![CDATA[" + String.Format("{0:yyyy/MM/dd HH:mm:ss}", it.DisplayDate) + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "version");
                        param.InnerText = "&lt;![CDATA[1.0]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "title");
                        param.InnerText = "&lt;![CDATA[" + RemoveInvalidXmlChars(title) + "]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "file");
                        param.InnerText = "&lt;![CDATA[/contents/" + articleNum + ".xml]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "description");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "small-image-url");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "small-image");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", "small-image-file");
                        param.InnerText = "&lt;![CDATA[]]&gt;";
                        item.AppendChild(param);

                        foreach (var cg in examinedDoc.CartoonGrouper) {
                            AssignImageAttributeToList(documentPath, articleNum, cg.TextContent);
                        }
                        foreach (var pr in examinedDoc.PhotoRelated) {
                            AssignImageAttributeToList(documentPath, articleNum, pr.ImageSrc);
                        }
                        foreach (var pg in examinedDoc.PhotoGallery) {
                            AssignImageAttributeToList(documentPath, articleNum, pg.PhotoBigContent);
                            AssignImageAttributeToList(documentPath, articleNum, pg.PhotoSmallContent);
                        }
                        if (examinedDoc.ImageOnlySet.Content != "") {
                            AssignImageAttributeToList(documentPath, articleNum, examinedDoc.ImageOnlySet.Content);
                        }
                        foreach (var pfImage in examinedDoc.PhotoFooterGallery.PhotoList) {
                            AssignImageAttributeToList(documentPath, articleNum, pfImage.ImageSrc);
                        }
                    }
                    if (_outputStructure == 2) {
                        createDoc = IsValidOldDocument(examinedDoc, it);
                        if (createDoc) {
                            // If title is longer than the specified number of characters, then it is truncated
                            title = title.LimitTo(_titleLimitTo);
                            if (it.OldDocTemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003) {
                                UpdateOldDocToonCreateDate(it, examinedDoc);
                            }
                            CreateOutputArticleDocToXml(true, documentPath, iterDoc, rootArticles, it, title, examinedDoc, articleNum);
                        }
                    }

                    if (_outputStructure == 1) {
                        try {
                            CreateDocumentAttributesToXml(contentPath, documentPath, examinedDoc, articleNum);
                        }
                        catch (Exception e) {
                            var msg = "Document not generated (creating Document XML), an exception occured id=[" + articleNum + "]";
                            se4DocNonGenerated.Add(msg);
                            if (log.IsFatalEnabled) {
                                log.Fatal(msg, e);
                            }
                        }

                        // Create Dependencies
                        // 1) Images
                        foreach (var s in mediaNameOldList) {
                            string imgId = "EC_IMG_" + (cnts.Image++);
                            param = iterDoc.CreateElement("param");
                            param.SetAttribute("name", imgId);
                            param.SetAttribute("type", "dependency");
                            item.AppendChild(param);

                            innerParam = iterDoc.CreateElement("param");
                            innerParam.SetAttribute("name", "classname");
                            innerParam.InnerText = "&lt;![CDATA[com.liferay.portlet.documentlibrary.model.DLFileEntry]]&gt;";
                            param.AppendChild(innerParam);

                            innerParam = iterDoc.CreateElement("param");
                            innerParam.SetAttribute("name", "groupname");
                            innerParam.InnerText = "&lt;![CDATA[Global]]&gt;";
                            param.AppendChild(innerParam);

                            if (s != "") {
                                try {
                                    CreateImageEntry(iterDoc, iterPool, s, imgId);
                                }
                                catch (Exception e) {
                                    var msg = "Document not generated (creating Image Entry), an exception occured id=[" + articleNum + "]";
                                    se4DocNonGenerated.Add(msg);
                                    if (log.IsFatalEnabled) {
                                        log.Fatal("Document not generated, an exception occured, doc id=" + articleNum, e);
                                    }
                                }
                            }
                        }

                        // 2) Page Content
                        string pcId = "EC_PC_" + (cnts.PageContent++);
                        param = iterDoc.CreateElement("param");
                        param.SetAttribute("name", pcId);
                        param.SetAttribute("type", "dependency");
                        item.AppendChild(param);

                        innerParam = iterDoc.CreateElement("param");
                        innerParam.SetAttribute("name", "classname");
                        innerParam.InnerText = "&lt;![CDATA[com.protecmedia.iter.news.model.PageContent]]&gt;";
                        param.AppendChild(innerParam);

                        innerParam = iterDoc.CreateElement("param");
                        innerParam.SetAttribute("name", "groupname");
                        innerParam.InnerText = "&lt;![CDATA[" + _globalSiteName + "]]&gt;";
                        param.AppendChild(innerParam);

                        CreatePageContentEntry(iterDoc, iterPool, pcId, it.IDOldDoc);
                    }
                    processedIds.Add(it);
                    if (createDoc) {
                        numDocsWritten++;
                    }
                    else {
                        lineInfo = "Discarded document, not written to package idSitemap=[" + it.IdSitemap + "], processed=[" + 17 + "]";
                        Console.WriteLine(lineInfo);
                        if (log.IsWarnEnabled) {
                            log.Warn(lineInfo);
                        }
                    }

                    // Process next document in list.
                    i++;
                    if (i > _docLimit) {
                        // Saving file to disk and go on to next package.                    
                        SaveXmlDocument(manifestFile, iterDoc);
                        CompressToZip(path, zipFileName);
                        i = 1;
                        cnts.IterWebManifestFile++;
                        isIterWebManifestFileAffected = true;
                        MarkRecordsAsProcessed(processedIds, zipFileNameOnly);
                        logLine = "Package " + iterwebManifest + " -> Ids [" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                        Console.WriteLine(logLine);
                        if (log.IsWarnEnabled) {
                            log.Warn(logLine);
                        }
                        logLine = "Package " + iterwebManifest + " contains " + numDocsWritten + " documents.";
                        Console.WriteLine(logLine);
                        if (log.IsWarnEnabled) {
                            log.Warn(logLine);
                        }
                        totalNumDocsWritten += numDocsWritten;
                        numDocsWritten = 0L;
                        GC.Collect();
                    }
                }
            }
            if (iterDoc != null) {
                if (Directory.Exists(path)) {
                    SaveXmlDocument(manifestFile, iterDoc);
                    CompressToZip(path, zipFileName);
                    MarkRecordsAsProcessed(processedIds, zipFileNameOnly);
                    logLine = "Package " + iterwebManifest + " -> Ids [" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    logLine = "Package " + iterwebManifest + " contains " + numDocsWritten + " documents.";
                    Console.WriteLine(logLine);
                    if (log.IsWarnEnabled) {
                        log.Warn(logLine);
                    }
                    totalNumDocsWritten += numDocsWritten;
                    numDocsWritten = 0L;
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                GC.Collect();
            }
            if (se4DocNonGenerated.Count != 0) {
                Console.WriteLine();
                Console.WriteLine("SE4 Documents not generated");
                foreach (var s in se4DocNonGenerated) {
                    Console.WriteLine(s);
                    if (log.IsErrorEnabled) {
                        log.Error(s);
                    }
                }
            }
            lineInfo = "Generating for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);

            lineInfo = "Actually written documents to Package " + totalNumDocsWritten;
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);

            lineInfo = "Actually generated documents " + numDocGenerated;
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);

            if (log.IsDebugEnabled) {
                log.Debug("GeneratePackagesForOldDocs End");
            }
        }        

        /// <summary>
        /// If output structure is 2, it creates all of the xml elements to document.
        /// </summary>
        /// <param name="isOldDoc">OldDoc or SE4 doc</param>
        /// <param name="documentPath">Target directory for resources</param>
        /// <param name="iterDoc">XML main node</param>
        /// <param name="iterArticles">XML node to articles</param>
        /// <param name="it">Record info</param>
        /// <param name="title">Document title</param>
        /// <param name="examinedDoc">Old Doc under examination. Optional</param>
        /// <param name="articleNum">Number or article being processed. Optional.</param>
        private void CreateOutputArticleDocToXml(bool isOldDoc, string documentPath, XmlDocument iterDoc, XmlElement iterArticles, IterwebMapInfo it, string title, Doc examinedDoc = null, long articleNum = 0) {
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleDocToXml start");
            XmlElement iterArticle = iterDoc.CreateElement("article");
            XmlElement iterArticleMetadata = iterDoc.CreateElement("metadata");
            XmlElement iterArticleContent = iterDoc.CreateElement("content");

            XmlElement properties = iterDoc.CreateElement("properties");
            properties.SetAttribute("indexable", "1");
            properties.SetAttribute("urltitle", RemoveInvalidXmlChars(it.UrlPath.ExtractUrlTitle()));
            properties.SetAttribute("legacyurl", RemoveInvalidXmlChars(it.UrlPath));
            properties.SetAttribute("title", RemoveInvalidXmlChars(title));
            properties.SetAttribute("createdate", String.Format("{0:yyyy/MM/dd HH:mm:ss}", it.DisplayDate));
            properties.SetAttribute("modifieddate", String.Format("{0:yyyy/MM/dd HH:mm:ss}", it.UpdateDate));
            iterArticleMetadata.AppendChild(properties);

            iterArticle.AppendChild(iterArticleMetadata);
            iterArticle.AppendChild(iterArticleContent);

            if (isOldDoc) {
                iterArticle.SetAttribute("articleid", it.IDOldDoc);
            }
            else {
                iterArticle.SetAttribute("articleid", it.IDSE4ArticleId);
            }

            string filter = "";
            string filterEx = "";
            filter = GlobalConstants.ARTICLE_HISTORICAL_GENERAL;

            if (isOldDoc) {
                switch (examinedDoc.TemplateType) {
                    case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003:
                        filter = GlobalConstants.ARTICLE_HISTORICAL_TOON_GALLERY;
                        break;
                    default:
                        break;
                }
            }
            else {
                if (it.Layout == "creacion_infografias") {
                    filter = GlobalConstants.ARTICLE_HISTORICAL_GENERAL;
                }
                if (it.Layout == "creacion_galeria") {
                    filter = GlobalConstants.ARTICLE_HISTORICAL_IMAGE_GALLERY;
                }
                if (it.IdBrightCove != null) {
                    if (it.Layout == "creacion_video") {
                        filter = GlobalConstants.ARTICLE_HISTORICAL_VIDEO_GALLERY;
                    }
                    else {
                        if (it.Layout == "creacion_notaInterior100" || it.Layout == "creacion_notaInterior100M") {
                            filter = GlobalConstants.ARTICLE_HISTORICAL_GENERAL;
                            filterEx = GlobalConstants.ARTICLE_HISTORICAL_VIDEO_GALLERY;
                        }
                    }
                }
                if (it.IsInfographic == 1) {
                    filterEx = GlobalConstants.ARTICLE_HISTORICAL_INFOGRAPHIC_GALLERY;
                    filter = GlobalConstants.ARTICLE_HISTORICAL_GENERAL;
                }
            }
            CreateOutputArticleSectionsDocToXml(iterDoc, iterArticleMetadata, it.DisplayDate, filter, filterEx);
            if (isOldDoc) {
                CreateOutputArticleContentDocToXml(documentPath, iterDoc, iterArticleContent, it, examinedDoc, articleNum);
            }
            else {
                CreateOutputArticleCategoriesDocToXml(iterDoc, iterArticleMetadata, it.IdObjetoSE);

                // Retrieve Document Attributes and filter to use those enabled to do so.
                List<SE4Attribute> docSE4AttrList = LoadSE4DocItems(it.IdObjetoSE, it.Layout);
                CreateOutputArticleContentDocToXml(documentPath, iterDoc, iterArticleContent, it, docSE4AttrList);
            }
            iterArticles.AppendChild(iterArticle);
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleDocToXml end");
        }

        /// <summary>
        /// Write 'it' to document XML.
        /// </summary>
        /// <param name="iterDoc">XML document</param>
        /// <param name="iterArticles">XML articles node </param>        
        /// <param name="it">Node info</param>
        /// <param name="sourcePath">Resource source directory</param>
        /// <param name="documentPath">Resource target directory</param>
        /// <param name="documentPath"
        private void CreateOutputArticleToonDocToXml(XmlDocument iterDoc, XmlElement iterArticles, ToonDirInfo it, string sourcePath, string documentPath) {
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleToonDocToXml start");
            XmlElement iterArticle = iterDoc.CreateElement("article");
            XmlElement iterArticleMetadata = iterDoc.CreateElement("metadata");
            XmlElement iterArticleContent = iterDoc.CreateElement("content");
            DateTime toonDateToUse;

            if (it.UseComputedCreateDate == 1) {
                toonDateToUse = it.ComputedCreatedDate;
            }
            else {
                toonDateToUse = it.CreateDate;
            }

            string dtStr = String.Format("{0:dd/MM/yyyy}", toonDateToUse);
            string title = "Caricatura " + dtStr;
            string urlTitle = "Caricatura_" + dtStr;
            string legacyUrl = "";

            urlTitle = urlTitle.Replace('/', '_').ToLower().ToString();
            legacyUrl = "/Bancomedios/Imagenes/caricaturas2/" + it.FileName;

            XmlElement properties = iterDoc.CreateElement("properties");
            properties.SetAttribute("indexable", "1");
            properties.SetAttribute("urltitle", urlTitle);
            properties.SetAttribute("legacyurl", legacyUrl);
            properties.SetAttribute("title", title);
            properties.SetAttribute("createdate", String.Format("{0:yyyy/MM/dd HH:mm:ss}", toonDateToUse));
            properties.SetAttribute("modifieddate", String.Format("{0:yyyy/MM/dd HH:mm:ss}", toonDateToUse));
            iterArticleMetadata.AppendChild(properties);

            iterArticle.AppendChild(iterArticleMetadata);
            iterArticle.AppendChild(iterArticleContent);
            iterArticle.SetAttribute("articleid", it.IdArticle);

            string filter = "";
            filter = GlobalConstants.ARTICLE_HISTORICAL_TOON_GALLERY;
            CreateOutputArticleSectionsDocToXml(iterDoc, iterArticleMetadata, toonDateToUse, filter);

            var item = iterDoc.CreateElement("component");
            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_TOON);
            iterArticleContent.AppendChild(item);

            var itemChild = iterDoc.CreateElement("file");
            string fileName = it.FileName;
            fileName = CopyImageToZip(sourcePath, documentPath, fileName, it);
            itemChild.SetAttribute("path", DocumentImageDir + fileName);
            item.AppendChild(itemChild);
            iterArticles.AppendChild(iterArticle);
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleToonDocToXml end");
        }

        /// <summary>
        /// Appends document content if output is new Article XML format.
        /// </summary>
        /// <param name="documentPath">Resource target directory</param>
        /// <param name="iterDoc">XML Document</param>
        /// <param name="iterArticleContent">Xml Document Node</param>
        /// <param name="doc">Database object being processed</param>
        /// <param name="info">Doc under examination</param>
        /// <param name="articleNum">Num article being processed</param>
        private void CreateOutputArticleContentDocToXml(string documentPath, XmlDocument iterDoc, XmlElement iterArticleContent, IterwebMapInfo doc, Doc info, long articleNum) {
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocToXml start (Old Doc processing)");
            XmlElement item = null;
            XmlElement itemChild = null;
            XmlCDataSection cdata = null;
            string contentText = "";
            string docGlobalRef = "/bin/";
            string fileName = "";

            switch (info.TemplateType) {
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    if (info.Bullet.Count != 0) {
                        contentText = "";
                        contentText += "<ul>";
                        foreach (var bulletItem in info.Bullet) {
                            contentText += "<li>" + bulletItem.Content + "</li>";
                        }
                        contentText += "</ul>";
                    }
                    contentText += info.DocumentText.Content + "<br><br>";
                    foreach (var rh in info.ReaderHelp) {
                        contentText += "<b>" + rh.NameTitleContent + "</b><br>";
                        contentText += rh.NameTextContent + "<br>";
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    foreach (var pr in info.PhotoRelated) {
                        fileName = ExtractImageNamePart(pr.ImageSrc);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {
                            // An image file reference.
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                            iterArticleContent.AppendChild(item);


                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);

                            itemChild = iterDoc.CreateElement("component");
                            itemChild.SetAttribute("name", "Cutline");
                            contentText = pr.Footer;
                            contentText = RemoveInvalidXmlChars(contentText);
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemChild.InnerXml = cdata.OuterXml;
                            item.AppendChild(itemChild);
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    // Process the NewsGrouper only list.
                    contentText = "";
                    foreach (var ng in info.NewsGrouper) {
                        contentText += "<p>" + ng.TitleContent + "</p>";
                        contentText += "<p>" + ng.TextContent + "</p><hr>";
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003:
                    // Process the CartoonGrouper only list.
                    foreach (var cg in info.CartoonGrouper) {
                        item = iterDoc.CreateElement("component");
                        item.SetAttribute("name", "Headline");
                        item.SetAttribute("index", "1");
                        iterArticleContent.AppendChild(item);

                        contentText = cg.TitleContent;
                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        item.InnerXml = cdata.OuterXml;

                        fileName = ExtractImageNamePart(cg.TextContent);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {
                            // An image file reference.
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_TOON);
                            iterArticleContent.AppendChild(item);

                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);

                            itemChild = iterDoc.CreateElement("component");
                            itemChild.SetAttribute("name", "Cutline");
                            contentText = cg.AuthorContent;
                            contentText = RemoveInvalidXmlChars(contentText);
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemChild.InnerXml = cdata.OuterXml;
                            item.AppendChild(itemChild);
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = "";
                    contentText = info.DocumentText.Content + "<br><br>";
                    foreach (var rh in info.ReaderHelp) {
                        contentText += "<b>" + rh.NameTitleContent + "<b>";
                        contentText += rh.NameTextContent + "<br>";
                    }
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Byline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "City");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    if (info.Bullet.Count != 0) {
                        contentText = "<ul>";
                        foreach (var bulletItem in info.Bullet) {
                            contentText += "<li>" + bulletItem.Content + "</li>";
                        }
                        contentText += "</ul>";
                        item = iterDoc.CreateElement("component");
                        item.SetAttribute("name", "Lead");
                        item.SetAttribute("index", "1");
                        iterArticleContent.AppendChild(item);

                        contentText = RemoveInvalidXmlChars(contentText);
                        cdata = iterDoc.CreateCDataSection(contentText);
                        item.InnerXml = cdata.OuterXml;
                    }
                    foreach (var pr in info.PhotoRelated) {
                        // An image file reference.
                        if (pr.ImageSrc != "") {
                            fileName = ExtractImageNamePart(pr.ImageSrc);
                            fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                            if (fileName != "") {
                                item = iterDoc.CreateElement("component");
                                item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                                iterArticleContent.AppendChild(item);

                                contentText = docGlobalRef + fileName;
                                contentText = RemoveInvalidXmlChars(contentText);

                                itemChild = iterDoc.CreateElement("file");
                                itemChild.SetAttribute("path", contentText);
                                item.AppendChild(itemChild);

                                itemChild = iterDoc.CreateElement("component");
                                itemChild.SetAttribute("name", "Cutline");
                                contentText = pr.Footer;
                                contentText = RemoveInvalidXmlChars(contentText);
                                cdata = iterDoc.CreateCDataSection(contentText);
                                itemChild.InnerXml = cdata.OuterXml;
                                item.AppendChild(itemChild);
                            }
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    foreach (var pg in info.PhotoGallery) {
                        fileName = ExtractImageNamePart(pg.PhotoBigContent);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {
                            // An image file reference.
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                            iterArticleContent.AppendChild(item);

                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);

                            contentText = "";
                            contentText += pg.PhotoCreditContent;
                            if (pg.PhotoFooterContent != "") {
                                contentText += " - " + pg.PhotoFooterContent;
                            }
                            if (contentText != "") {
                                itemChild = iterDoc.CreateElement("component");
                                itemChild.SetAttribute("name", "Cutline");
                                contentText = RemoveInvalidXmlChars(contentText);
                                cdata = iterDoc.CreateCDataSection(contentText);
                                itemChild.InnerXml = cdata.OuterXml;
                                item.AppendChild(itemChild);
                            }
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    contentText = "";
                    if (info.Bullet.Count != 0) {
                        contentText += "<ul>";
                        foreach (var it in info.Bullet) {
                            contentText += "<li>" + it.Content + "</li>";
                        }
                        contentText += "</ul>";
                    }

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText += info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_007:
                    // COQ: Jul.01/2014 - All records discarded, not processed.
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_008:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    contentText = "";
                    if (info.Bullet.Count != 0) {
                        contentText += "<ul>";
                        foreach (var bulletItem in info.Bullet) {
                            contentText += "<li>" + bulletItem.Content + "</li>";
                        }
                        contentText += "</ul>";
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText += info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    foreach (var pr in info.PhotoRelated) {
                        fileName = ExtractImageNamePart(pr.ImageSrc);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {

                            // An image file reference.
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                            iterArticleContent.AppendChild(item);


                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);

                            itemChild = iterDoc.CreateElement("component");
                            itemChild.SetAttribute("name", "Cutline");
                            contentText = pr.Footer;
                            contentText = RemoveInvalidXmlChars(contentText);
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemChild.InnerXml = cdata.OuterXml;
                            item.AppendChild(itemChild);
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_009:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    contentText = "";
                    if (info.Bullet.Count != 0) {
                        contentText += "<ul>";
                        foreach (var bulletItem in info.Bullet) {
                            contentText += "<li>" + bulletItem.Content + "</li>";
                        }
                        contentText += "<ul>";
                    }

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText += info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Byline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "City");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    if (info.PhotoRelated.Count != 0) {
                        // An image file reference.
                        foreach (var pr in info.PhotoRelated) {
                            fileName = ExtractImageNamePart(pr.ImageSrc);
                            fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                            if (fileName != "") {

                                item = iterDoc.CreateElement("component");
                                item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                                iterArticleContent.AppendChild(item);

                                contentText = docGlobalRef + fileName;
                                contentText = RemoveInvalidXmlChars(contentText);
                                itemChild = iterDoc.CreateElement("file");
                                itemChild.SetAttribute("path", contentText);
                                item.AppendChild(itemChild);

                                itemChild = iterDoc.CreateElement("component");
                                itemChild.SetAttribute("name", "Cutline");
                                contentText = pr.Footer;
                                contentText = RemoveInvalidXmlChars(contentText);
                                cdata = iterDoc.CreateCDataSection(contentText);
                                itemChild.InnerXml = cdata.OuterXml;
                                item.AppendChild(itemChild);
                            }
                        }
                    }
                    if (info.ImageOnlySet.Content != "" && info.PhotoRelated.Count == 0) {
                        fileName = ExtractImageNamePart(info.ImageOnlySet.Content);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                            iterArticleContent.AppendChild(item);

                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);
                        }
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_011:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Byline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "City");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_013:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Byline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "City");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    // An image file reference.
                    fileName = ExtractImageNamePart(info.ImageOnlySet.Content);
                    fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                    if (fileName != "") {
                        item = iterDoc.CreateElement("component");
                        item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                        iterArticleContent.AppendChild(item);

                        contentText = docGlobalRef + fileName;
                        contentText = RemoveInvalidXmlChars(contentText);
                        itemChild = iterDoc.CreateElement("file");
                        itemChild.SetAttribute("path", contentText);
                        item.AppendChild(itemChild);
                    }
                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_014:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Byline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.AuthorText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "City");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.CreditText.CityText;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    break;
                case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_015:
                    contentText = "";
                    foreach (var tit in info.Title) {
                        contentText += tit.Content;
                    }

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Headline");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Text");
                    item.SetAttribute("index", "1");
                    iterArticleContent.AppendChild(item);

                    contentText = info.DocumentText.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    item.InnerXml = cdata.OuterXml;

                    foreach (var pr in info.PhotoFooterGallery.PhotoList) {
                        fileName = ExtractImageNamePart(pr.ImageSrc);
                        fileName = CopyImageToZip(_se4MediaSourceFolder, documentPath, fileName, doc);
                        if (fileName != "") {
                            // An image file reference.
                            item = iterDoc.CreateElement("component");
                            item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                            iterArticleContent.AppendChild(item);

                            contentText = docGlobalRef + fileName;
                            contentText = RemoveInvalidXmlChars(contentText);
                            itemChild = iterDoc.CreateElement("file");
                            itemChild.SetAttribute("path", contentText);
                            item.AppendChild(itemChild);

                            itemChild = iterDoc.CreateElement("component");
                            itemChild.SetAttribute("name", "Cutline");
                            contentText = "";
                            contentText += info.PhotoFooterGallery.FoooterTitle + " - " + info.PhotoFooterGallery.Footer;
                            contentText = RemoveInvalidXmlChars(contentText);
                            cdata = iterDoc.CreateCDataSection(contentText);
                            itemChild.InnerXml = cdata.OuterXml;
                            item.AppendChild(itemChild);
                        }
                    }
                    break;
            }
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocToXml end (Old Doc processing)");
        }

        /// <summary>
        /// Before trying to generate document to XML it is checked if content is to be generated.
        /// </summary>
        /// <param name="it">Document to process</param>
        /// <param name="threshold">How many characters to allow for document text. If it is set to -1 do not use it.</param>
        /// <returns>TRUE if document can be written to XML</returns>
        private bool IsValidDocument(IterwebMapInfo it, int threshold = -1) {
            if (it.Layout == "creacion_video") {
                return true;
            }

            Boolean rslt = false;
            string textData = "";

            // Retrieve Document Attributes and filter to use those enabled to do so.
            List<SE4Attribute> docSE4AttrList = LoadSE4DocItems(it.IdObjetoSE, it.Layout);

            var filteredSE4AttrList = (from a in docSE4AttrList
                                       where a.ElementName == "cuerpoPrimerParrafo" ||
                                        a.ElementName == "EDITORHTML" ||
                                        a.ElementName == "EditorHtml" ||
                                        a.ElementName == "editorhtml" ||
                                        a.ElementName == "cuerpo"
                                       select a
                            ).ToList<SE4Attribute>();
            foreach (var attr in filteredSE4AttrList) {
                if (attr != null) {
                    textData += (attr.HugeContentCode != -1 ? attr.HugeText : attr.Text);
                }
            }
            rslt = true; // Assume it can be written to XML.
            if (textData == "") {
                rslt = false;
            }
            else {
                if (threshold != -1) {
                    if (textData.Length <= threshold) {
                        rslt = false;
                    }
                }
            }
            return rslt;
        }

        /// <summary>
        /// Before trying to generate document to XML it is checked if content is to be generated.
        /// </summary>
        /// <param name="it">Document to process</param>
        /// <param name="docContent">Filled with the whole content being analyzed</param>
        /// <param name="threshold">How many characters to allow for document text. If it is set to -1 do not use it.</param>
        /// <returns>TRUE if document can be written to XML</returns>
        private bool IsValidDocument(IterwebMapInfo it, ref String docContent, int threshold = -1) {
            if (it.Layout == "creacion_video") {
                return true;
            }

            Boolean rslt = false;
            string textData = "";

            // Retrieve Document Attributes and filter to use those enabled to do so.
            List<SE4Attribute> docSE4AttrList = LoadSE4DocItems(it.IdObjetoSE, it.Layout);

            var filteredSE4AttrList = (from a in docSE4AttrList
                                       where a.ElementName == "cuerpoPrimerParrafo" ||
                                        a.ElementName == "EDITORHTML" ||
                                        a.ElementName == "EditorHtml" ||
                                        a.ElementName == "editorhtml" ||
                                        a.ElementName == "cuerpo"
                                       select a
                            ).ToList<SE4Attribute>();
            foreach (var attr in filteredSE4AttrList) {
                if (attr != null) {
                    textData += (attr.HugeContentCode != -1 ? attr.HugeText : attr.Text);
                }
            }
            docContent = textData;
            rslt = true; // Assume it can be written to XML.
            if (textData == "") {
                rslt = false;
            }
            else {
                if (threshold != -1) {
                    if (textData.Length <= threshold) {
                        rslt = false;
                    }
                }
            }
            return rslt;
        }



        /// <summary>
        /// Before trying to generate document to XML it is checked if content is to be generated.
        /// </summary>
        /// <param name="it">Document to process</param>
        /// <returns>TRUE if document can be written to XML</returns>
        private bool IsValidOldDocument(Doc docInfo, IterwebMapInfo it) {
            Boolean rslt = true;

            if (it.OldDocTemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003) {
                // Bypass check as content is already valid.
                return rslt;
            }
            else {
                switch (it.OldDocTemplateType) {
                    case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_002:
                        if (docInfo.NewsGrouper.Count == 0 && docInfo.DocumentText.Content == "") {
                            rslt = false;
                        }
                        break;
                    case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005:
                        if (docInfo.PhotoGallery.Count == 0 && docInfo.DocumentText.Content == "") {
                            rslt = false;
                        }
                        break;
                    default:
                        if (docInfo.DocumentText.Content == "") {
                            rslt = false;
                        }
                        break;
                }
            }

            if (!rslt) {
                it.Processed = 17;
            }
            return rslt;
        }
        /// <summary>
        /// Before attempting to generate the document to XML it is checked if image gallery is actually set.
        /// </summary>
        /// <param name="doc">Properties to doc to check</param>
        /// <param name="documentPath">ZIP path</param>
        /// <returns>True if valid.</returns>
        private bool IsValidImageGallery(IterwebMapInfo doc, string documentPath) {
            bool rslt = true;
            if (log.IsDebugEnabled) log.Debug("IsValidImageGallery Start");
            // Retrieve Document Attributes and filter to use those enabled to do so.
            List<SE4Attribute> docSE4AttrList = LoadSE4DocItems(doc.IdObjetoSE, doc.Layout);
            var node = (from a in docSE4AttrList
                        where a.ElementName == "nombreGaleria"
                        select a
                       ).FirstOrDefault<SE4Attribute>();
            if (node != null) {
                string contextText = (node.HugeContentCode != -1 ? node.HugeText : node.Text);
                if (contextText == "") {
                    string msg = "Doc idSitemap=[" + doc.IdSitemap + "] has not a gallery directory set appropriately, document not exported";
                    if (log.IsFatalEnabled) {
                        log.Fatal(msg);
                    }
                    Console.WriteLine(msg);
                    doc.Processed = 12;
                    rslt = false;
                }
                else {
                    string imageGalleryFolder = _se4MediaImageGallerySourceFolder;
                    imageGalleryFolder += @"\" + contextText;
                    if (Directory.Exists(imageGalleryFolder)) {
                        string pubInfoFileName = imageGalleryFolder + @"\Pubinfo.txt";
                        if (File.Exists(pubInfoFileName)) {
                            // First store in list all found images
                            imageGalleryTitle = "";
                            galleryImgList = new List<ImageOnly>();
                            string[] lines = File.ReadAllLines(pubInfoFileName);

                            // Grab Title
                            var imageGalleryHeadLine = lines[0].Split('|');
                            if (imageGalleryHeadLine.Length > 0) {
                                imageGalleryTitle = imageGalleryHeadLine[1];
                            }

                            for (int i = 1; i < lines.Length; i++) {
                                var elements = lines[i].Split('|');
                                string fileName = elements[0];

                                fileName = HttpUtility.HtmlDecode(fileName) + ".jpg";
                                fileName = CopyImageToZip(imageGalleryFolder, documentPath, fileName, doc, false);
                                if (fileName != "") {
                                    ImageOnly imgInfo = new ImageOnly();
                                    imgInfo.Name = fileName;
                                    imgInfo.Content = elements[1] + " - " + elements[2];
                                    galleryImgList.Add(imgInfo);
                                }
                            }
                            if (galleryImgList.Count == 0) {
                                doc.Processed = 11;
                                rslt = false;
                                if (log.IsErrorEnabled) {
                                    log.Error("Doc idSitemap=[" + doc.IdSitemap + "] does not contain any images SET to processed = 11");
                                }
                            }
                        }
                        else {
                            if (log.IsErrorEnabled) {
                                log.Error("Doc idSitemap=[" + doc.IdSitemap + "] does not contain 'PubInfo.txt' file SET to processed = 10");
                            }
                            doc.Processed = 10;
                            rslt = false;
                        }
                    }
                    else {
                        if (log.IsErrorEnabled) {
                            log.Error("Doc idSitemap=[" + doc.IdSitemap + "] does not contain 'PubInfo.txt' file SET to processed = 10");
                        }
                        doc.Processed = 10;
                        rslt = false;
                    }
                }
            }
            else {
                // Document was not actually found in DB.
                rslt = false;
            }
            if (log.IsDebugEnabled) log.Debug("IsValidImageGallery End");
            return rslt;
        }

        /// <summary>
        /// Evaluates a directory to gather the image gallery contents to XML.
        /// </summary>
        /// <param name="iterDoc">XML Document</param>        
        /// <param name="iterArticleContent">Xml Document Node</param>
        /// <param name="doc">SE4 related information</param>
        /// <param name="attributeList">List of attributes to work with</param>
        /// <param name="documentPath">Target resource directory</param>
        private void CreateOutputArticleContentDocImageGalleryToXml(XmlDocument iterDoc, XmlElement iterArticleContent, IterwebMapInfo doc, List<SE4Attribute> attributeList, string documentPath) {
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocImageGalleryToXml Start");
            if (galleryImgList.Count > 0) {
                XmlElement item = null;
                XmlElement itemChild = null;
                string contentText = "";
                XmlCDataSection cdata = null;

                item = iterDoc.CreateElement("component");
                item.SetAttribute("name", "Headline");
                item.SetAttribute("index", "1");
                iterArticleContent.AppendChild(item);

                contentText = RemoveInvalidXmlChars(imageGalleryTitle);
                cdata = iterDoc.CreateCDataSection(contentText);
                item.InnerXml = cdata.OuterXml;
                foreach (var it in galleryImgList) {
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                    iterArticleContent.AppendChild(item);

                    itemChild = iterDoc.CreateElement("file");
                    itemChild.SetAttribute("path", DocumentImageDir + it.Name);
                    item.AppendChild(itemChild);

                    itemChild = iterDoc.CreateElement("component");
                    itemChild.SetAttribute("name", "Cutline");
                    contentText = it.Content;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemChild.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemChild);
                }
                galleryImgList.Clear();
            }
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocImageGalleryToXml End");
        }

        /// <summary>
        /// Appends document content if output is new Article XML format.
        /// </summary>
        /// <param name="documentPath">Target resource directory</param>
        /// <param name="iterDoc">XML Document</param>        
        /// <param name="iterArticleContent">Xml Document Node</param>
        /// <param name="doc">SE4 related information</param>
        /// <param name="attributeList">List of attributes to work with</param>
        private void CreateOutputArticleContentDocToXml(string documentPath, XmlDocument iterDoc, XmlElement iterArticleContent, IterwebMapInfo doc, List<SE4Attribute> attributeList) {
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocToXml start (SE4 Doc processing)");
            XmlCDataSection cdata = null;
            XmlElement item = null;
            XmlElement itemChild = null;
            string contentText = "";
            string name = "";
            string targetType = "";
            string targetIndexType = "";
            ValidMapField vf = null;
            List<ValidMapField> documentValidFields = se4TemplateFieldMapping[doc.Layout];

            string mediaCutLine = "";
            string mediaRelatedImageCutLine = "";
            string mediaImageName = "";
            string mediaRelatedImageName = "";
            string textData = "";
            bool isInfographicSet = (doc.IsInfographic == 1);
            bool docHasZoomImage = false;

            // The logic behind inserting the idBrightCove value consists to prepare this value in database and
            // ask here for layout value and then if it is not null then create the componet to XML 
            // NOTE: Fields in layout with names 'videoPrincipal', and 'videoGaleria' cannot be loaded herein, that
            // is, they must be set to false in 'BuildSE4TemplateMapping()'.
            if (doc.Layout == "creacion_video" ||
                doc.Layout == "creacion_notaInterior100" ||
                doc.Layout == "creacion_minutoaminuto" ||
                doc.Layout == "creacion_notaInterior100M") {
                if (doc.IdBrightCove != null) {
                    item = iterDoc.CreateElement("component");
                    item.SetAttribute("name", "Multimedia_Brightcove");
                    item.SetAttribute("index", "0");
                    iterArticleContent.AppendChild(item);

                    cdata = iterDoc.CreateCDataSection(doc.IdBrightCove);
                    item.InnerXml = cdata.OuterXml;
                }
            }

            if (doc.Layout == "creacion_galeria") {
                // Document has been completed processed at this step.
                CreateOutputArticleContentDocImageGalleryToXml(iterDoc, iterArticleContent, doc, attributeList, documentPath);
                return;
            }

            // Exception to general loop next to this one. That is, that loop assumes all fields are exported one by one.
            // Special evaluation for fields.            
            foreach (var it in attributeList) {
                if (doc.Layout == "creacion_notaInteriorEditorial") {
                    ;
                }
                else {
                    // At this place the 'CuerpoPrimerParrafo' is concatenated to 'EDITORHTML', 'cuerpo', ..., fields.
                    if (it.ElementName == "cuerpoPrimerParrafo" ||
                        it.ElementName == "EDITORHTML" ||
                        it.ElementName == "EditorHtml" ||
                        it.ElementName == "editorhtml" ||
                        it.ElementName == "cuerpo"
                        ) {
                        contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                        contentText = RemoveInvalidXmlChars(contentText);
                        textData += contentText;
                    }
                }
                if (it.ElementName == "medio") {
                    switch (it.AttributeName) {
                        case "imagen":
                            if (mediaImageName == "") {
                                mediaImageName = ExtractMediaName(it, doc, documentPath);
                            }
                            break;
                        case "media_img_imagen":
                            if (mediaImageName == "") {
                                mediaImageName = ExtractMediaName(it, doc, documentPath);
                            }
                            break;
                        case "image_ampliacion":
                            if (mediaImageName == "") {
                                mediaImageName = ExtractMediaName(it, doc, documentPath);
                            }
                            break;
                        case "media_img_ampliacion":
                            if (mediaImageName == "") {
                                mediaImageName = ExtractMediaName(it, doc, documentPath);
                            }
                            break;
                        case "descripcion_ampliacion":
                            contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                            contentText = RemoveInvalidXmlChars(contentText);
                            if (mediaCutLine == "") {
                                mediaCutLine += contentText;
                            }
                            else {
                                mediaCutLine += " |" + contentText;
                            }
                            break;
                        case "descripcionImagenNota":
                            contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                            contentText = RemoveInvalidXmlChars(contentText);
                            if (mediaCutLine == "") {
                                mediaCutLine += contentText;
                            }
                            else {
                                mediaCutLine += " |" + contentText;
                            }
                            break;
                        case "credito":
                            contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                            contentText = RemoveInvalidXmlChars(contentText);
                            if (mediaCutLine == "") {
                                mediaCutLine += contentText;
                            }
                            else {
                                mediaCutLine += " | " + contentText;
                            }
                            break;
                    }
                }
                if (isInfographicSet) {
                    if (it.ElementName == "fotoRelacionada" && it.AttributeName == "ampliarimagen") {
                        contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                        contentText = RemoveInvalidXmlChars(contentText);
                        if (contentText != "") {
                            docHasZoomImage = true;
                        }
                    }
                    if (it.ElementName == "fotoRelacionada" && docHasZoomImage) {
                        switch (it.AttributeName) {
                            case "image_ampliacion":
                                if (mediaRelatedImageName == "") {
                                    mediaRelatedImageName = ExtractMediaName(it, doc, documentPath);
                                }
                                break;
                            case "media_img_ampliacion":
                                if (mediaRelatedImageName == "") {
                                    mediaRelatedImageName = ExtractMediaName(it, doc, documentPath);
                                }
                                break;
                            case "descripcion_ampliacion":
                                contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                                contentText = RemoveInvalidXmlChars(contentText);
                                if (mediaRelatedImageCutLine == "") {
                                    mediaRelatedImageCutLine += contentText;
                                }
                                else {
                                    mediaRelatedImageCutLine += " |" + contentText;
                                }
                                break;
                            case "creditoFotoRelacionada":
                                contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                                contentText = RemoveInvalidXmlChars(contentText);
                                if (mediaRelatedImageCutLine == "") {
                                    mediaRelatedImageCutLine += contentText;
                                }
                                else {
                                    mediaRelatedImageCutLine += " |" + contentText;
                                }
                                break;
                        }
                    }
                }
            }
            if (mediaImageName != "") {
                item = iterDoc.CreateElement("component");
                if (doc.Layout == "creacion_infografias") {
                    item.SetAttribute("name", GlobalConstants.IMAGE_NAME_INPHOGRAFIC);
                }
                else {
                    item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                }
                iterArticleContent.AppendChild(item);

                itemChild = iterDoc.CreateElement("file");
                itemChild.SetAttribute("path", mediaImageName);
                item.AppendChild(itemChild);

                if (mediaCutLine != "") {
                    itemChild = iterDoc.CreateElement("component");
                    itemChild.SetAttribute("name", "Cutline");
                    contentText = mediaCutLine;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemChild.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemChild);
                }
            }
            if (mediaRelatedImageName != "") {
                item = iterDoc.CreateElement("component");
                if (docHasZoomImage) {
                    item.SetAttribute("name", GlobalConstants.IMAGE_NAME_INPHOGRAFIC);
                }
                else {
                    item.SetAttribute("name", GlobalConstants.IMAGE_NAME_GENERAL);
                }
                iterArticleContent.AppendChild(item);

                itemChild = iterDoc.CreateElement("file");
                itemChild.SetAttribute("path", mediaRelatedImageName);
                item.AppendChild(itemChild);

                if (mediaRelatedImageCutLine != "") {
                    itemChild = iterDoc.CreateElement("component");
                    itemChild.SetAttribute("name", "Cutline");
                    contentText = mediaRelatedImageCutLine;
                    contentText = RemoveInvalidXmlChars(contentText);
                    cdata = iterDoc.CreateCDataSection(contentText);
                    itemChild.InnerXml = cdata.OuterXml;
                    item.AppendChild(itemChild);
                }
            }

            if (doc.Layout == "creacion_video") {
                // Invalidates 'document content (text)' gathered so far
                // Special case. When we are in 'creacion_video' we put 'entradilla' (lead) concatenated to 'content' (text).
                var filteredSE4AttrList = (from a in attributeList
                                           where a.ElementName == "entradilla"
                                           select a
                            ).ToList<SE4Attribute>().FirstOrDefault<SE4Attribute>();
                if (filteredSE4AttrList != null) {
                    contentText = (filteredSE4AttrList.HugeContentCode != -1 ? filteredSE4AttrList.HugeText : filteredSE4AttrList.Text);
                    contentText = RemoveInvalidXmlChars(contentText);

                    // First lead then text to final text
                    textData = contentText + " " + textData;
                    if (textData != "") {
                        textData = textData.Trim();
                    }
                }
            }
            if (doc.Layout == "creacion_notaInteriorEditorial") {
                // Invalidates 'document content (text)' gathered so far
                var firstParagraphField = (from a in attributeList
                                           where a.ElementName == "cuerpoPrimerParrafo"
                                           select a
                                          ).ToList<SE4Attribute>().FirstOrDefault<SE4Attribute>();
                var txtField = (from a in attributeList
                                where a.ElementName == "EDITORHTML"
                                select a
                               ).ToList<SE4Attribute>().FirstOrDefault<SE4Attribute>();
                var contextFieldList = (from a in attributeList
                                        where a.ElementName == "contexto"
                                        select a
                                       ).ToList<SE4Attribute>();
                contentText = textData = "";
                var txtFirstP = (firstParagraphField.HugeContentCode != -1 ? firstParagraphField.HugeText : firstParagraphField.Text);
                txtFirstP = RemoveInvalidXmlChars(contentText);

                contentText = (txtField.HugeContentCode != -1 ? txtField.HugeText : txtField.Text);
                contentText += txtFirstP;
                contentText = RemoveInvalidXmlChars(contentText);

                textData += "<div class='detalle_editorial'>" + contentText + "</div>";

                contentText = "<p><b>Contraposición</b></p>";
                foreach (var attr in contextFieldList) {
                    if (attr.AttributeName == "tituloAyuda" || attr.AttributeName == "EDITORHTML") {
                        contentText += (attr.HugeContentCode != -1 ? attr.HugeText : attr.Text);
                    }
                }
                contentText = RemoveInvalidXmlChars(contentText);
                textData += "<div class='detalle_contraposicion'>" + contentText + "</div>";
            }
            if (doc.Layout == "NotaInteriorNavidad") {
                // Invalidates 'document content (text)' gathered so far                
                var txtField = (from a in attributeList
                                where a.ElementName == "EDITORHTML"
                                select a
                               ).ToList<SE4Attribute>().FirstOrDefault<SE4Attribute>();
                var helpFieldList = (from a in attributeList
                                     where a.ElementName == "Ayuda"
                                     select a
                                    ).ToList<SE4Attribute>();
                contentText = textData = "";
                contentText = (txtField.HugeContentCode != -1 ? txtField.HugeText : txtField.Text);
                foreach (var attr in helpFieldList) {
                    contentText += "<p>" + (attr.HugeContentCode != -1 ? attr.HugeText : attr.Text) + "</p>";
                }
                contentText = RemoveInvalidXmlChars(contentText);
                textData = contentText;

            }
            if (textData != "") {
                item = iterDoc.CreateElement("component");
                item.SetAttribute("name", "Text");
                item.SetAttribute("index", "1");
                iterArticleContent.AppendChild(item);

                cdata = iterDoc.CreateCDataSection(textData);
                item.InnerXml = cdata.OuterXml;
            }

            // Now other fields
            foreach (var it in attributeList) {
                // Discard already processed elements.
                if (it.ElementName == "medio" ||
                    it.ElementName == "cuerpoPrimerParrafo" ||
                    it.ElementName == "EDITORHTML" ||
                    it.ElementName == "EditorHtml" ||
                    it.ElementName == "editorhtml" ||
                    it.ElementName == "cuerpo" ||
                    it.ElementName == "nombreGaleria" ||
                    it.ElementName == "anteTituloEditorial" ||
                    it.ElementName == "contexto" ||
                    it.ElementName == "fotoRelacionada" ||
                    it.ElementName == "Ayuda"
                    ) {
                    continue;
                }
                vf = documentValidFields.Find(e => e.Source == it.ElementName && e.Use);
                if (vf.Attributes != null) {
                    ValidMapFieldAttributes attr = vf.Attributes.Find(e => e.SourceAttribute == it.AttributeName);
                    if (attr != null) {
                        name = attr.Target;
                        targetType = attr.TargetType;
                        targetIndexType = attr.TargetIndexType;
                    }
                    else {
                        name = vf.Target;
                        targetType = vf.TargetType;
                        targetIndexType = vf.TargetIndexType;
                    }
                }
                else {
                    name = vf.Target;
                    targetType = vf.TargetType;
                    targetIndexType = vf.TargetIndexType;
                }

                if (it.MultimediaAttribute == GlobalConstants.SE4ATTR_MULTIMEDIA_NONE) {
                    contentText = (it.HugeContentCode != -1 ? it.HugeText : it.Text);
                    contentText = RemoveInvalidXmlChars(contentText);

                    if (contentText != "") {
                        item = iterDoc.CreateElement("component");
                        item.SetAttribute("name", name);
                        item.SetAttribute("index", "1");
                        iterArticleContent.AppendChild(item);

                        cdata = iterDoc.CreateCDataSection(contentText);
                        item.InnerXml = cdata.OuterXml;
                    }
                }
            }
            if (log.IsDebugEnabled) log.Debug("CreateOutputArticleContentDocToXml end (SE4 Doc processing)");
        }

        /// <summary>
        /// Given 'idDoc' retrieves vocabularies if set for it.
        /// </summary>
        /// <param name="idDoc">Id of document to retrieve.</param>
        /// <returns></returns>
        private List<VocabularyDefinition> RetrieveVocabulariesForDoc(long idDoc) {
            List<VocabularyDefinition> rslt = new List<VocabularyDefinition>();
            string sql = "";
            if (log.IsDebugEnabled) {
                log.Debug("RetrieveVocabulariesForDoc Start");
            }
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("RetrieveVocabulariesForDoc");
            sql += "select distinct B.Description CatalogName,  C.CategoryPath, C.Description CategoryName ";
            sql += "from ArticleSectionCatalogCategory A                                inner join ";
            sql += "     catalognames                  B on A.idCatalog = B.idCatalog   inner join ";
            sql += "     Categories                    C on A.idCategory = C.idCategory inner join ";
            sql += "     articlecategories             D on A.idSection = D.idSeccion   inner join ";
            sql += "     ArticleSectionCategory        E on A.idSection = E.idSeccion ";
            sql += "where E.usar = 1 and D.id_ObjetoSE = @iddoc ";
            sql += "order by CatalogName, CategoryName ";

            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@iddoc";
            param1.SqlDbType = SqlDbType.Int;
            param1.Value = idDoc;
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param1);
            if (rdr.HasRows) {
                int i = 0;
                String oldCatalogName = "";
                String catalogName = "";
                String categoryName = "";
                String categoryNamePath = "";
                VocabularyDefinition vd = null;
                while (rdr.Read()) {
                    catalogName = rdr["CatalogName"].ToString();
                    categoryName = rdr["CategoryName"].ToString();
                    categoryNamePath = rdr["CategoryPath"].ToString();
                    if (oldCatalogName != catalogName) {
                        vd = new VocabularyDefinition();
                        vd.Categories = new List<CategoryDefinition>();
                        vd.ApplyTo = GlobalConstants.ARTICLE_HISTORICAL_GENERAL;
                        oldCatalogName = vd.Name = catalogName.Trim();
                        rslt.Add(vd);
                        i = 0;
                    }
                    CategoryDefinition cd = new CategoryDefinition();
                    cd.Name = categoryName.Trim();
                    cd.NamePath = categoryNamePath.Trim();
                    cd.SetAttribute = 1;
                    cd.Main = 0;
                    if (i == 0) {
                        cd.Main = 1;
                    }
                    vd.Categories.Add(cd);
                    i++;
                }
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("RetrieveVocabulariesForDoc End");
            }
            return rslt;
        }

        /// <summary>
        /// Appends categories to the article output format.
        /// </summary>
        /// <param name="iterDoc">XML Document</param>
        /// <param name="iterArticle">XMl Document Node</param>
        /// <param name="filter">Filter records from sections memory list</param>
        private void CreateOutputArticleCategoriesDocToXml(XmlDocument iterDoc, XmlElement iterArticle, long idDoc) {
            if (log.IsDebugEnabled) {
                log.Debug("CreateOutputArticleCategoriesDocToXml Start");
            }

            var applyCategories = RetrieveVocabulariesForDoc(idDoc);
            if (applyCategories != null && applyCategories.Count > 0) {
                XmlElement category = null;
                XmlElement categories = iterDoc.CreateElement("categories");
                foreach (var s in applyCategories) {
                    XmlElement vocabulary = iterDoc.CreateElement("vocabulary");
                    vocabulary.SetAttribute("name", s.Name);
                    foreach (var c in s.Categories) {
                        if (c.NamePath == "") {
                            category = iterDoc.CreateElement("category");
                            category.SetAttribute("name", c.Name);
                            category.SetAttribute("main", c.Main.ToString());
                            category.SetAttribute("set", c.SetAttribute.ToString());
                            vocabulary.AppendChild(category);
                        }
                        else {
                            XmlElement categoryParent = iterDoc.CreateElement("category");
                            vocabulary.AppendChild(categoryParent);
                            String[] cpath = c.NamePath.Split(',');
                            for (int j = 0; j < cpath.Length; j++) {
                                String citem = cpath[j].Trim();
                                if (j == 0) {
                                    categoryParent.SetAttribute("set", "0");
                                    categoryParent.SetAttribute("name", citem);
                                }
                                else {
                                    category = iterDoc.CreateElement("category");
                                    category.SetAttribute("set", "0");
                                    category.SetAttribute("name", citem);
                                    categoryParent.AppendChild(category);
                                    categoryParent = category;
                                }
                            }
                            category = iterDoc.CreateElement("category");
                            category.SetAttribute("name", c.Name);
                            category.SetAttribute("main", c.Main.ToString());
                            category.SetAttribute("set", c.SetAttribute.ToString());
                            categoryParent.AppendChild(category);
                        }

                    }
                    categories.AppendChild(vocabulary);
                }
                iterArticle.AppendChild(categories);
            }
            if (log.IsDebugEnabled) {
                log.Debug("CreateOutputArticleCategoriesDocToXml End");
            }
        }

        /// <summary>
        /// Appends sections to the article output format.
        /// It allows to define up to two sections per document and it is controlled by the parameters
        /// 'filter' and 'filterEx'. By default 'filterEx' is set to empty string meaning that no extended section is set, that
        /// is, at least one section is set, but if 'filterEx' is not empty, then a second section is added.
        /// </summary>
        /// <param name="iterDoc">XML Document</param>
        /// <param name="iterArticle">XMl Document Node</param>
        /// <param name="filter">Filter records from sections memory list</param>
        /// <param name="filterEx">Filter records from sections memory list (if one is found with
        /// this criteria, it is set to not default section and should appear in XML as second)</param>
        /// <param name="dt">To determine date validity for section</param>
        private void CreateOutputArticleSectionsDocToXml(XmlDocument iterDoc, XmlElement iterArticle, DateTime dt, string filter, string filterEx = "") {
            if (log.IsDebugEnabled) {
                log.Debug("CreateOutputArticleSectionsDocToXml Start");
            }

            var applySections = (from d in sections
                                 where d.ApplyTo == filter
                                 select d
                  );

            XmlElement secs = iterDoc.CreateElement("sections");
            foreach (var sec in applySections) {
                XmlElement s = iterDoc.CreateElement("section");
                s.SetAttribute("pagetemplate", sec.PageTemplate);
                s.SetAttribute("qualification", sec.Qualification);
                s.SetAttribute("url", sec.Url);

                string defSectionVal = (sec.DefaultSection) ? "true" : "false";
                s.SetAttribute("defaultSection", defSectionVal);
                s.SetAttribute("datefrom", String.Format("{0:yyyy/MM/dd HH:mm:ss}", dt));
                s.SetAttribute("dateto", String.Format("{0:yyyy/MM/dd HH:mm:ss}", dt.AddDays(1)));
                secs.AppendChild(s);
            }
            if (filterEx != "") {
                var applySectionsEx = (from d in sections
                                       where d.ApplyTo == filterEx
                                       select d
                  );
                foreach (var sec in applySectionsEx) {
                    XmlElement s = iterDoc.CreateElement("section");
                    s.SetAttribute("pagetemplate", sec.PageTemplate);
                    s.SetAttribute("qualification", sec.Qualification);
                    s.SetAttribute("url", sec.Url);

                    string defSectionVal = "false";
                    s.SetAttribute("defaultSection", defSectionVal);
                    s.SetAttribute("datefrom", String.Format("{0:yyyy/MM/dd HH:mm:ss}", dt));
                    s.SetAttribute("dateto", String.Format("{0:yyyy/MM/dd HH:mm:ss}", dt.AddDays(1)));
                    secs.AppendChild(s);
                }
            }
            iterArticle.AppendChild(secs);
            if (log.IsDebugEnabled) {
                log.Debug("CreateOutputArticleSectionsDocToXml End");
            }
        }

        /// <summary>
        /// Used to extract the image name part out of a given URL or filename.
        /// </summary>
        /// <param name="p">Source to get image name part</param>
        /// <returns>Only the image name with the .jpg extension (SE4  
        /// uses only this extension </returns>
        private string ExtractImageNamePart(string p) {
            string rslt = "";
            string sTmp = p.ToLower();
            if (sTmp.Contains("bancomedios")) {
                var a = p.Split('/');
                rslt = a[a.Length - 1];
            }
            else {
                rslt = p;
            }
            rslt = rslt.Supress('/').Supress('\\');
            return rslt;
        }

        /// <summary>
        /// It may happen that old documents are loaded in memory but it some documents don't have title, thus they must be marked as invalid.
        /// They are logged in app logger as 'Document without title set, warning, review'. It this has happened
        /// it means a scanning condition is invalid.
        /// </summary>
        private void PostValidateOldDocuments() {
            if (log.IsDebugEnabled) {
                log.Debug("PostValidateOldDocuments Start");
            }
            string lineInfo = "PostValidateOldDocuments for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsInfoEnabled) log.Info(lineInfo);
            int numDocument = 1;
            int numInvalidPostDocs = 0;

            foreach (var it in _se4DocList) {
                Console.Write("\rEvaluating " + it.IDOldDoc + " Num document: " + (numDocument++));
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {

                    Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);
                    string title = "";

                    try {
                        if (examinedDoc.TemplateType == TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003) {
                            title = examinedDoc.CartoonGrouper[0].TitleContent;
                        }
                        else {
                            title = examinedDoc.Title[0].Content;
                        }
                    }
                    catch (Exception ex) {
                        lineInfo = "Document without title set, warning, review " + it.IDOldDoc;
                        Console.Write("\r");
                        Console.WriteLine(lineInfo.PadRight(30, '.'));
                        if (log.IsErrorEnabled) {
                            log.Error(lineInfo, ex);
                        }
                        it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_POST_VALIDATE_NO_TITLE;
                        it.OldDocTemplateType = TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_UNASSIGNED;
                        it.JsonContent = "";
                        UpdateOldDoc(it);
                        numInvalidPostDocs++;
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Invalid number of post scanned document processing " + numInvalidPostDocs);
            if (log.IsDebugEnabled) {
                log.Debug("PostValidateOldDocuments End");
            }
        }

        /// <summary>
        /// Exec an SQL statement.
        /// </summary>
        private void ExecuteSqlStmt() {
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmt Start");
            }
            if (_sqlExecStmt != "") {
                if (log.IsWarnEnabled) {
                    log.Warn("Using SQL=[" + _sqlExecStmt + "]");
                }
                HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
                hdb.Open();
                SqlTransaction transaction = hdb.BeginTransaction("ExecuteSqlStmt");
                hdb.ExecSQLStmt(transaction, _sqlExecStmt);
                transaction.Commit();
                hdb.Close();
            }
            else {
                var s = "No SQL statement to execute";
                Console.WriteLine(s);
                if (log.IsInfoEnabled) {
                    log.Info(s);
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmt End");
            }
        }

        /// <summary>
        /// Exec an SQL statement in batch supplied by sql parameter. SQL must end with semi-colon. SQL msut not contain any commentary in it.        
        /// </summary>
        /// <param name="sqlBatchProvided">In memory SQL to act upon.</param>
        private void ExecuteSqlStmtBatch(string sqlBatchSupplied) {
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmtBatch(string) Start");
            }
            String[] readSQLStmts = null;
            String line = "";
            line = "Executing SQL Commands from  [" + sqlBatchSupplied + "]";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            line = "Executing statements";
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            readSQLStmts = sqlBatchSupplied.Split(';');
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("ExecuteSqlStmtBatch1");
            try {
                String sqlExec = "";
                foreach (string sql in readSQLStmts) {
                    if (sql != "") {
                        sqlExec = sql;
                        if (log.IsWarnEnabled) {
                            log.Warn("Executing SQL=[" + sqlExec + "]");
                        }
                        hdb.ExecSQLStmt(transaction, sqlExec);
                        sqlExec = "";
                    }
                }
                transaction.Commit();
            }
            catch (Exception e) {
                Console.WriteLine("Exception caught (ExecuteSqlStmtBatch). See log for details");
                line = "Processing aborted by Exception raised ";
                Console.WriteLine(line);
                if (log.IsErrorEnabled) {
                    log.Error(line);
                    log.Error(e.Message);
                    log.Error(e.StackTrace);
                }
                transaction.Rollback();
            }
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmtBatch(string) End");
            }

        }

        /// <summary>
        /// Exec an SQL statement in batch.
        /// NOTE: Reads a file from 'SQLBatchFile' configuration parameter and assumes a true SQL statement 
        /// (which ends with semicolon) is read and executed accordingly and Batch file must be UTF-8 encoded.
        /// SQL must end with semi-colon. SQL msut not contain any commentary in it.
        /// </summary>
        private void ExecuteSqlStmtBatch() {
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmtBatch Start");
            }
            String[] readSQLStmts = null;
            String line = "";
            line = "Executing SQL Commands from file [" + _sqlBatchFile + "]";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            if (!File.Exists(_sqlBatchFile)) {
                line = "Batch file [" + _sqlBatchFile + "] does not exist";
                Console.WriteLine(line);
                if (log.IsErrorEnabled) {
                    log.Error(line);
                }
            }
            else {
                readSQLStmts = File.ReadAllLines(_sqlBatchFile, Encoding.UTF8);
                if (readSQLStmts.Length == 0) {
                    line = "SQL Batch File is empty";
                    Console.WriteLine(line);
                    if (log.IsErrorEnabled) {
                        log.Error(line);
                    }
                }
                else {
                    line = "Executing statements";
                    if (log.IsInfoEnabled) {
                        log.Info(line);
                    }
                    HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
                    hdb.Open();
                    SqlTransaction transaction = hdb.BeginTransaction("ExecuteSqlStmtBatch");
                    try {
                        bool trueSqlStmt = false;
                        String sqlExec = "";
                        foreach (string sql in readSQLStmts) {
                            if (sql != "") {
                                if (sql.Contains(';')) {
                                    trueSqlStmt = true;
                                }
                                sqlExec += sql + " ";
                                if (trueSqlStmt) {
                                    if (log.IsWarnEnabled) {
                                        log.Warn("Executing SQL=[" + sqlExec + "]");
                                    }
                                    hdb.ExecSQLStmt(transaction, sqlExec);
                                    trueSqlStmt = false;
                                    sqlExec = "";
                                }

                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Exception caught (ExecuteSqlStmtBatch). See log for details");
                        line = "Processing aborted by Exception raised ";
                        Console.WriteLine(line);
                        if (log.IsErrorEnabled) {
                            log.Error(line);
                            log.Error(e.Message);
                            log.Error(e.StackTrace);
                        }
                        transaction.Rollback();
                    }
                    hdb.Close();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSqlStmtBatch End");
            }
        }

        /// <summary>
        /// Executes an SQL statement ant its results are saved in a text file for inspection.
        /// </summary>
        private void ExportSQLToCSV() {
            if (log.IsDebugEnabled) {
                log.Debug("ExportSQLToCSV Start");
            }
            if (_sqlExecStmt != "") {
                if (log.IsInfoEnabled) {
                    log.Info("Using SQL=[" + _sqlExecStmt + "]");
                }
                HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
                hdb.Open();
                SqlTransaction transaction = hdb.BeginTransaction("ExportSQLToCSV");
                SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, _sqlExecStmt);
                List<String> csvLines = rdr.ToCSV(true, ",");

                // Let's save to disk
                using (StreamWriter sw = new StreamWriter(_csvExportFile)) {
                    foreach (var line in csvLines) {
                        sw.WriteLine(line);
                    }
                }
                rdr.Close();
                transaction.Commit();
                hdb.Close();
            }
            else {
                var s = "No SQL statement to execute";
                Console.WriteLine(s);
                if (log.IsInfoEnabled) {
                    log.Info(s);
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("ExportSQLToCSV End");
            }
        }

        /// <summary>
        /// Executes an SQL statement ant its results are saved in a text file for inspection. It uses
        /// a file on disk to read the valid statement, using key 'SQLBatchFile'. NOTE: 'SQLBatchFile' file
        /// must contain one and only one SQL Select statement to use and it must be UTF-8 encoded.
        /// </summary>
        private void ExportSQLToCSVWithSQLInFile() {
            if (log.IsDebugEnabled) {
                log.Debug("ExportSQLToCSVWithSQLInFile Start");
            }
            String[] readSQLStmts = null;
            String line = "";
            line = "Executing SQL Commands from file [" + _sqlBatchFile + "]";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            if (!File.Exists(_sqlBatchFile)) {
                line = "Batch file [" + _sqlBatchFile + "] does not exist";
                Console.WriteLine(line);
                if (log.IsErrorEnabled) {
                    log.Error(line);
                }
            }
            else {
                readSQLStmts = File.ReadAllLines(_sqlBatchFile, Encoding.UTF8);
                if (readSQLStmts.Length == 0) {
                    line = "SQL File is empty";
                    Console.WriteLine(line);
                    if (log.IsErrorEnabled) {
                        log.Error(line);
                    }
                }
                else {
                    line = "Executing statement from file.";
                    if (log.IsInfoEnabled) {
                        log.Info(line);
                    }
                    bool trueSqlStmt = false;
                    String sqlExec = "";
                    foreach (string sql in readSQLStmts) {
                        if (sql != "") {
                            if (sql.Contains(';')) {
                                trueSqlStmt = true;
                            }
                            sqlExec += sql + " ";
                            if (trueSqlStmt) {
                                break;
                            }
                        }
                    }
                    if (log.IsWarnEnabled) {
                        log.Warn("Using SQL=[" + sqlExec + "]");
                    }
                    HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
                    hdb.Open();
                    SqlTransaction transaction = hdb.BeginTransaction("ExportSQLToCSVWithSQLInFile");
                    SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sqlExec);
                    List<String> csvLines = rdr.ToCSV(true, ",");

                    // Let's save to disk
                    using (StreamWriter sw = new StreamWriter(_csvExportFile)) {
                        foreach (var csvLine in csvLines) {
                            sw.WriteLine(csvLine);
                        }
                    }
                    rdr.Close();
                    transaction.Commit();
                    hdb.Close();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("ExportSQLToCSVWithSQLInFile End");
            }
        }


        /// <summary>
        /// Stores in 'InvalidUrls' database table as reference.
        /// </summary>        
        /// <param name="idSitemap">Sitemap record to use as reference</param>
        /// <param name="url">Invalid url</param>
        /// <param name="p">kind of record, 1: SE4 doc, 2: Old doc</param>
        private void SaveToInvalidUrlTable(long idSitemap, string url, string urlPath, int p) {
            if (log.IsDebugEnabled) log.Debug("SaveToInvalidUrlTable Start");
            string sqlToUse = "insert into invalidurls(id, url, urlencoded, kind) values (@sitemap, @url, @urlencoded, @kind) ";
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@sitemap";
            param1.Value = idSitemap;
            param1.SqlDbType = SqlDbType.Int;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@url";
            param2.Value = url;
            param2.SqlDbType = SqlDbType.VarChar;

            SqlParameter param3 = new SqlParameter();
            param3.ParameterName = "@kind";
            param3.Value = p;
            param3.SqlDbType = SqlDbType.Int;

            SqlParameter param4 = new SqlParameter();
            param4.ParameterName = "@urlencoded";
            param4.Value = HttpUtility.UrlEncode(urlPath);
            param4.SqlDbType = SqlDbType.VarChar;

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("SaveToInvalidUrlTable");
            hdb.ExecSQLStmt(transaction, sqlToUse, param1, param2, param3, param4);
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) log.Debug("SaveToInvalidUrlTable End");
        }

        /// <summary>
        /// Gets all useful records and validates that their URLs don't contain non-ascii characters, if one is found
        /// it is reported to both screen and log.
        /// </summary>
        private void ValidateURLChars() {
            if (log.IsDebugEnabled) {
                log.Debug("ValidateURLChars Start");
            }
            long numInvalidSE4Docs = 0;
            long numInvalidOldDocs = 0;
            string msg = "";
            string loadOldDocuments = "";
            string loadSE4Documents = "";
            string se4LayoutList = _se4LayoutToFilter;
            string layoutFilter = "";

            loadOldDocuments = "select idSitemap, url, urlPath from sitemap " +
                "where idOld is not null and idSE4 is null and oldDocStatus = 2 and url is not null";
            loadSE4Documents = "select idSitemap, url, urlPath from sitemap " +
                "where idSE4 is not null  and url is not null @layoutfilter@ and urlParameters = '' and processed = 0 ";

            if (se4LayoutList == "") {
                layoutFilter = "";
            }
            else {
                layoutFilter = "and layout in (@se4layoutlist@) ";
                layoutFilter = layoutFilter.Replace("@se4layoutlist@", se4LayoutList);
            }
            loadSE4Documents = loadSE4Documents.Replace("@layoutfilter@", layoutFilter);

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("DeleteInvalidUrls");

            hdb.ExecSQLStmt(transaction, "delete from invalidurls");
            transaction.Commit();

            transaction = hdb.BeginTransaction("ValidateURLChars");

            // 1. Loads valid SE4Documents to validate their URLs
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, loadSE4Documents);
            numInvalidSE4Docs = 0;
            while (rdr.Read()) {
                string url = rdr["url"].ToString();
                string urlPath = rdr["urlPath"].ToString();
                long idSitemap = Convert.ToInt32(rdr["idSitemap"].ToString());
                if (!url.IsUrlWithValidChars()) {
                    msg = "SE4Doc Url at idSitemap=[" + idSitemap + "] is not ASCII conformant. URL=[" + url + "]";
                    Console.WriteLine(msg);
                    if (log.IsInfoEnabled) {
                        log.Info(msg);
                    }
                    numInvalidSE4Docs++;
                    SaveToInvalidUrlTable(idSitemap, url, urlPath, 1);
                }
            }
            rdr.Close();

            // 2. Loads Old documents to validate their URLs.
            rdr = hdb.ExecSelectSQLStmtAsReader(transaction, loadOldDocuments);
            numInvalidOldDocs = 0;
            while (rdr.Read()) {
                string url = rdr["url"].ToString();
                string urlPath = rdr["urlPath"].ToString();
                long idSitemap = Convert.ToInt32(rdr["idSitemap"].ToString());
                if (!url.IsUrlWithValidChars()) {
                    msg = "Old DOC Url at idSitemap=[" + idSitemap + "] is not ASCII conformant. URL=[" + url + "]";
                    Console.WriteLine(msg);
                    if (log.IsInfoEnabled) {
                        log.Info(msg);
                    }
                    numInvalidOldDocs++;
                    SaveToInvalidUrlTable(idSitemap, url, urlPath, 2);
                }
            }

            msg = "There are [" + numInvalidSE4Docs + "] invalid URls in SE4 Docs";
            Console.WriteLine(msg);
            if (log.IsInfoEnabled) {
                log.Info(msg);
            }

            msg = "There are [" + numInvalidOldDocs + "] invalid URls in Old Docs";
            Console.WriteLine(msg);
            if (log.IsInfoEnabled) {
                log.Info(msg);
            }

            rdr.Close();
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("ValidateURLChars End");
            }
        }

        /// <summary>
        /// Invokes sp 'sp_syncmigrationdb' to update index table used as the migration-processing-control data.
        /// </summary>
        private void SynchronizeDBData() {
            if (log.IsDebugEnabled) {
                log.Debug("SynchronizeDBData Start");
            }
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("SynchronizeDBData");
            hdb.ExecSQLStmt(transaction, "execute sp_syncmigrationdb");
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("SynchronizeDBData End");
            }
        }

        /// <summary>
        /// Appends to XML a user field node.
        /// </summary>
        /// <param name="iterDoc">Master XML document.</param>
        /// <param name="userFields">User field node parent</param>
        /// <param name="fieldName">Name of field to use</param>
        /// <param name="contentText">Valud of field to use</param>
        private void UserAddFieldToXml(XmlDocument iterDoc, XmlElement userFields, string fieldName, string contentText) {
            string data = contentText;

            if (data != null) {
                data = data.Trim();
            }
            XmlElement item = iterDoc.CreateElement(fieldName);
            XmlCDataSection cdata = iterDoc.CreateCDataSection(data);
            item.InnerXml = cdata.OuterXml;
            userFields.AppendChild(item);
        }

        /// <summary>
        /// Appends to XML a user field node. (Optional Data).
        /// </summary>
        /// <param name="iterDoc">Master XML document.</param>
        /// <param name="userOptional">User field node parent</param>
        /// <param name="fieldName">Name of field to use</param>
        /// <param name="contentText">Valud of field to use</param>
        private void UserAddOptionalFieldToXml(XmlDocument iterDoc, XmlElement userOptional, string fieldName, string contentText) {
            string data = contentText;

            if (data != null){
                data = data.Trim();
            }
            XmlElement item = null;
            XmlElement child = null;
            XmlCDataSection cdata = null;
            item = iterDoc.CreateElement("i");
            item.SetAttribute("n", fieldName);
            child = iterDoc.CreateElement("v");
            item.AppendChild(child);
            cdata = iterDoc.CreateCDataSection(contentText);
            child.InnerXml = cdata.OuterXml;
            userOptional.AppendChild(item);
        }

        /// <summary>
        /// All current comment-user that exist in EC are generated in a ZIP to be imported by the Iterweb system.
        /// </summary>
        private void ExportUser() {
            // COQ Nov.06/2014
            // Due to time constraints a modification needs to be made here that involves spliting the users generated
            // in chunks of 10000 records and file not being zipped.
            string sql = "sp_retrieveusers";
            XmlDocument iterDoc = null;
            XmlElement rootUsers = null;
            string iterwebManifest = "";
            string manifestFile = "";
            string logLine = "";
            long counter = 0;
            int i = 1;
            int numRecsPerFile = 10000;

            if (log.IsDebugEnabled) {
                log.Debug("UserExport Start");
            }

            // Create folder structure            
            iterwebManifest = "pkusers";
            logLine = "Package " + iterwebManifest;
            if (log.IsInfoEnabled) log.Info(logLine);
            Console.WriteLine(logLine);

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("UserExport");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
            i = 0;
            int j = 1;
            while (rdr.Read()) {
                counter++;
                Console.Write("\rProcessing record " + counter);

                if (i == 0L) {
                    //iterwebManifest = Convert.ToString(cnts.IterWebManifestFile).PadLeft(10, '0') + "-pkusers.xml";
                    iterwebManifest = j + ".xml";
                    manifestFile = _zipFolder + @"\" + iterwebManifest;
                    iterDoc = new XmlDocument();
                    XmlDeclaration iterDocDeclaration = iterDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    rootUsers = iterDoc.CreateElement("us");
                    iterDoc.PreserveWhitespace = false;
                    iterDoc.AppendChild(rootUsers);
                    iterDoc.InsertBefore(iterDocDeclaration, rootUsers);
                }

                XmlElement user = iterDoc.CreateElement("u");
                XmlElement userFields = iterDoc.CreateElement("f");
                XmlElement userOptional = iterDoc.CreateElement("o");
                rootUsers.AppendChild(user);
                user.AppendChild(userFields);
                user.AppendChild(userOptional);

                UserAddFieldToXml(iterDoc, userFields, "aboid", rdr["Email"].ToString());
                UserAddFieldToXml(iterDoc, userFields, "pwd", rdr["Clave"].ToString());
                UserAddFieldToXml(iterDoc, userFields, "usrname", rdr["Usuario"].ToString());
                UserAddFieldToXml(iterDoc, userFields, "email", rdr["Email"].ToString());
                UserAddFieldToXml(iterDoc, userFields, "firstname", rdr["Nombres"].ToString());
                //UserAddFieldToXml(iterDoc, userFields, "lastname", rdr["Apellidos"].ToString());

                //UserAddOptionalFieldToXml(iterDoc, userOptional, "Nombre de pila", rdr["Nombres"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Apellidos", rdr["Apellidos"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Tipo de Documento", rdr["DescIdentificacion"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Número de documento", rdr["NumIdentificacion"].ToString());

                var birthDate = rdr["FechaNacimiento"];
                string birthDateFmt = "";
                if (birthDate == DBNull.Value) {
                    birthDateFmt = "";
                }
                else {
                    birthDateFmt = String.Format("{0:dd/MM/yyyy}", birthDate);
                }
                               
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Fecha Nacimiento", birthDateFmt);
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Género", rdr["DescSexo"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Móvil", rdr["Movil"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Teléfono Fijo", rdr["Telefono"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Dirección", rdr["Direccion"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "País de residencia", rdr["DescPais"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Nivel de educación", rdr["Estudios"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Departamento", rdr["DescDepto"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Ocupacion", rdr["Ocupacion"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Ciudad", rdr["DescCiudad"].ToString());

                String mapActive = "";
                var a = (Boolean)rdr["Activo"];
                if (a) {
                    mapActive = "1";
                }
                else {
                    mapActive = "0";
                }
                UserAddOptionalFieldToXml(iterDoc, userOptional, "He leído y acepto los Términos y Condiciones", mapActive);
                UserAddOptionalFieldToXml(iterDoc, userOptional, "Recibir titulares diarios", rdr["AceptaTitulares"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "He leído y acepto el Uso de mi información personal", rdr["AceptaOfertas"].ToString());
                UserAddOptionalFieldToXml(iterDoc, userOptional, "He leído y acepto el Uso de mi información por terceros", rdr["AceptaOfertasTerceros"].ToString());

                i++;
                if (i >= numRecsPerFile) {
                    SaveXmlDocument(manifestFile, iterDoc);
                    i = 0;
                    j++;
                    isIterWebManifestFileAffected = true;
                    GC.Collect();
                }
            }
            if (iterDoc != null) {
                SaveXmlDocument(manifestFile, iterDoc);
                cnts.IterWebManifestFile++;
                isIterWebManifestFileAffected = true;
                GC.Collect();
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            ExecuteSqlStmtBatch("update users set processed = 1 where processed = 0;");

            string s = counter + " users exported";
            Console.WriteLine();
            Console.WriteLine(s);
            if (log.IsInfoEnabled) log.Info(s);
            if (log.IsDebugEnabled) {
                log.Debug("UserExport End");
            }
        }

        /// <summary>
        /// Used to generate value for field UrlTitle in sitemap table.
        /// </summary>
        private void ComputeUrlTitle() {
            if (log.IsDebugEnabled) {
                log.Debug("ComputeUrlTitle Start");
            }
            long counter = 0;
            string line = "ComputeUrlTitle Init";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            Dictionary<long, String> urlTitleList = new Dictionary<long, String>();
            string sql = "select idSitemap, urlPath from sitemap where urlTitle is null";
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("ComputeUrlTitle");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
            while (rdr.Read()) {
                counter++;
                Console.Write(counter + "\r");
                var urlPath = rdr["urlPath"].ToString();
                var id = Convert.ToInt32(rdr["idSitemap"].ToString());
                if (urlPath != "") {
                    urlPath = urlPath.ExtractUrlTitle();
                    urlTitleList.Add(id, urlPath);
                }
            }
            Console.WriteLine();
            rdr.Close();
            transaction.Commit();

            line = "ComputeUrlTitle Updating database";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            long counter1 = 0;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@urltitle";
            param1.SqlDbType = SqlDbType.VarChar;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@id";
            param2.SqlDbType = SqlDbType.Int;
            transaction = hdb.BeginTransaction("ComputeUrlTitleUpdate");
            foreach (var pair in urlTitleList) {
                counter1++;
                Console.Write(counter1 + "\r");
                sql = "update sitemap set urltitle = @urltitle where idsitemap = @id";
                param1.Value = pair.Value;
                param2.Value = pair.Key;
                hdb.ExecSQLStmt(transaction, sql, param1, param2);
            }
            transaction.Commit();
            hdb.Close();
            Console.WriteLine();
            line = "ComputeUrlTitle processed " + counter + " items";
            Console.WriteLine(line);
            if (log.IsInfoEnabled) {
                log.Info(line);
            }
            if (log.IsDebugEnabled) {
                log.Debug("ComputeUrlTitle End");
            }
        }

        /// <summary>
        /// Loads toon info not actually being processed.
        /// </summary>
        private void LoadToonInfo() {
            string sql = "";

            sql = "select * from toondir where processed = 0 order by ComputedCreatedDate desc ";
            if (log.IsDebugEnabled) {
                log.Debug("LoadToonInfo Start");
            }
            _toonList.Clear();

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("LoadToonInfo");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
            while (rdr.Read()) {
                ToonDirInfo toonInfo = new ToonDirInfo();
                toonInfo.IdArticle = rdr["idArticle"].ToString();
                toonInfo.FileName = rdr["filename"].ToString();
				
                var dtCreate = Convert.ToDateTime(rdr["CreateDate"]);
                var dtUpdate = Convert.ToDateTime(rdr["UpdateDate"]);
                var dtComputeCreatedDate = Convert.ToDateTime(rdr["ComputedCreatedDate"]);

                toonInfo.CreateDate = dtCreate;
                toonInfo.UpdateDate = dtUpdate;
                toonInfo.Processed = Convert.ToInt32(rdr["processed"].ToString());
                toonInfo.Id = Convert.ToInt32(rdr["id"].ToString());
                toonInfo.ComputedCreatedDate = dtComputeCreatedDate;
                toonInfo.UseComputedCreateDate = (int)rdr["UseComputedCreateDate"];
                _toonList.Add(toonInfo);
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("LoadToonInfo End");
            }
        }

        /// <summary>
        /// Check if table 'toondirtmp' has newer records to insert into 'toondir'
        /// </summary>
        private void SyncToonInfo() {
            string sql = "";
            if (log.IsDebugEnabled) {
                log.Debug("SyncToonInfo Start");
            }
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("SyncToonInfo");
            sql += "insert into toondir(idArticle, fromdir, filename, dateprocessed, createdate, updatedate, processed) ";
            sql += "select idArticle, fromdir, filename, dateprocessed, createdate, updatedate, processed ";
            sql += "from toondirtmp a ";
            sql += "where filename not in (select filename from toondir) ";
            hdb.ExecSQLStmt(transaction, sql);

            sql = "update toondir set idArticle = 'EC_' + cast((id + 6000000) as varchar(255))";
            hdb.ExecSQLStmt(transaction, sql);
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("SyncToonInfo End");
            }
        }

        /// <summary>
        /// Load directory into temporary table in order to check if new files exist
        /// (files are sync'ed in database.
        /// </summary>
        private void LoadToonsIntoTemp() {
            String folderPath = _se4MediaSourceFolder + @"\Caricaturas2";
            if (log.IsDebugEnabled) {
                log.Debug("LoadToonsIntoTemp Start");
            }
            string sql = "insert into toondirtmp(fromdir, filename, dateProcessed, CreateDate, UpdateDate, processed, idArticle) values(@fromdir, @filename, @dateProcessed, @CreateDate, @UpdateDate, @processed, @idArticle)";
            var files = from file in Directory.GetFiles(folderPath, "caric*_G.jpg")
                        orderby file descending
                        select file;
            long sequence = 6000000L;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@fromdir";
            param1.SqlDbType = SqlDbType.VarChar;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@filename";
            param2.SqlDbType = SqlDbType.VarChar;

            SqlParameter param3 = new SqlParameter();
            param3.ParameterName = "@dateProcessed";
            param3.SqlDbType = SqlDbType.DateTime;

            SqlParameter param4 = new SqlParameter();
            param4.ParameterName = "@CreateDate";
            param4.SqlDbType = SqlDbType.DateTime;

            SqlParameter param5 = new SqlParameter();
            param5.ParameterName = "@UpdateDate";
            param5.SqlDbType = SqlDbType.DateTime;

            SqlParameter param6 = new SqlParameter();
            param6.ParameterName = "@processed";
            param6.SqlDbType = SqlDbType.Int;

            SqlParameter param7 = new SqlParameter();
            param7.ParameterName = "@idArticle";
            param7.SqlDbType = SqlDbType.VarChar;

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("LoadToonsIntoTemp");
            hdb.ExecSQLStmt(transaction, "delete from toondirtmp");
            foreach (var file in files) {
                param1.Value = folderPath;
                param2.Value = Path.GetFileName(file);
                param3.Value = DateTime.Now;
                param4.Value = param5.Value = File.GetLastWriteTime(file);
                param6.Value = 0;
                param7.Value = "EC_" + sequence++;
                hdb.ExecSQLStmt(transaction, sql, param1, param2, param3, param4, param5, param6, param7);
            }
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("LoadToonsIntoTemp End");
            }
        }

        /// <summary>
        /// Main process scanning for toons.
        /// </summary>
        private void ScanToons() {
            if (log.IsDebugEnabled) {
                log.Debug("ScanToons Start");
            }
            LoadToonsIntoTemp();
            SyncToonInfo();
            LoadToonInfo();
            if (log.IsDebugEnabled) {
                log.Debug("ScanToons End");
            }
        }

        /// <summary>
        /// Save counters for next iteration.
        /// </summary>
        public void SaveCounters() {
            // Saving counter object for next iteration
            try {
                using (StreamWriter sw = new StreamWriter(_counterJSONFile)) {
                    if (isIterWebManifestFileAffected) {
                        cnts.IterWebManifestFile = cnts.IterWebManifestFile - 1;
                    }
                    if (cnts.IterWebManifestFile < 0) {
                        cnts.IterWebManifestFile = 0;
                    }
                    string jsonInfo = JsonConvert.SerializeObject(cnts);
                    sw.Write(jsonInfo);
                }
            }
            catch (Exception ex) {
                if (log.IsErrorEnabled) {
                    log.Error("Json file could not be written [" + _counterJSONFile + "]", ex);
                }
            }
        }

        /// <summary>
        /// When given an image file name it cuts to a specified len, supress invalid characters and transforms 
        /// the original file name to a new name in lowercase. It copies image files from source to target directories.
        /// </summary>
        /// <param name="fileName">Input</param>
        /// <param name="targetDir">Directory to copy</param>
        /// <param name="srcDir"></param>
        /// <returns></returns>
        public String ConfigureImageName(String fileName, String targetDir, String srcDir) {
            if (log.IsDebugEnabled) log.Debug("ConfigureImageName start");
            String rslt = "";
            string s = fileName;
            var srcFilename = targetDir + @"\" + fileName;
            var destFilename = targetDir + @"\" + s;

            CopyMedia(fileName, targetDir, srcDir);

            // If file is 0Bytes, image should be discarded
            FileInfo f = new FileInfo(destFilename);
            long fileLen = f.Length;
            if (fileLen == 0L) {
                File.Delete(destFilename);
                s = "";
                rslt = s;
                if (log.IsDebugEnabled) log.Debug("ConfigureImageName end");
                return rslt;
            }

            StringUtils.IsStrWithNonAsciiChars(ref s);
            if (s.Length >= GlobalConstants.LIMIT_IMAGE_NAME) {
                s = s.LimitImageNameTo(GlobalConstants.LIMIT_IMAGE_NAME, GlobalConstants.EXT_JPG);
            }

            // Now let's put destFileName to lower case
            s = s.ToLower();
            destFilename = targetDir + @"\" + s;
            try {
                File.Move(srcFilename, destFilename);
            }
            catch (Exception) {
                if (fileName != s) {
                    Console.Write("File To remove " + srcFilename);
                    File.Delete(srcFilename);
                }
            }
            rslt = s;
            if (log.IsDebugEnabled) log.Debug("ConfigureImageName end");
            return rslt;
        }

        /// <summary>
        /// The only to do here is to display in log and screen those docs that have duplicate Title.
        /// </summary>
        /// <param name="templateType"></param>
        public void OldDocValidateTemplateDuplicateTitleReportOnly(int templateType) {
            if (log.IsDebugEnabled) log.Debug("OldDocValidateTemplateDuplicateTitleReportOnly start");

            string liner = "Executed OldDocValidateTemplateDuplicateTitleReportOnly() ";
            if (log.IsWarnEnabled) log.Warn(liner);
            Console.WriteLine(liner);

            List<IterwebMapInfo> processedIds = new List<IterwebMapInfo>();
            _se4DocList.Clear();
            Console.WriteLine("Loading documents from database");
            if (log.IsInfoEnabled) {
                log.Info("Loading documents from database");
            }
            LoadOldDocuments();

            liner = "Documents loaded=[" + _se4DocList.Count + "]";
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            foreach (var it in _se4DocList) {
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    if (it.OldDocTemplateType != templateType) {
                        continue;
                    }
                    Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);
                    if (examinedDoc.Title.Count > 1) {
                        int i = 1;
                        foreach (var tit in examinedDoc.Title) {
                            liner = "id=[" + it.IdSitemap + "], i=[" + i + "], tit=[" + tit.Content + "]";
                            if (log.IsWarnEnabled) log.Warn(liner);
                            i++;
                        }
                        processedIds.Add(it);
                    }
                }
            }
            liner = "Duplicates set to " + processedIds.Count;
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            liner = "Duplicated Title ids[" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            if (log.IsDebugEnabled) log.Debug("OldDocValidateTemplateDuplicateTitleReportOnly end");
        }

        /// <summary>
        /// The only to do here is to display in log and screen those docs that have duplicate Title.
        /// </summary>
        /// <param name="templateType">Old Doc template to analyze</param>
        public void OldDocValidateTemplateDuplicateTitleFixOnly(int templateType) {
            if (log.IsDebugEnabled) log.Debug("OldDocValidateTemplateDuplicateTitleFixOnly start");

            string liner = "Executed OldDocValidateTemplateDuplicateTitleFixOnly() ";
            if (log.IsWarnEnabled) log.Warn(liner);
            Console.WriteLine(liner);

            List<IterwebMapInfo> processedIds = new List<IterwebMapInfo>();
            _se4DocList.Clear();
            Console.WriteLine("Loading documents from database");
            if (log.IsInfoEnabled) {
                log.Info("Loading documents from database");
            }
            LoadOldDocuments();

            liner = "Documents loaded=[" + _se4DocList.Count + "]";
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            foreach (var it in _se4DocList) {
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    if (it.OldDocTemplateType != templateType) {
                        continue;
                    }
                    Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);
                    if (examinedDoc.Title.Count > 1) {
                        string data = examinedDoc.Title[1].Content;
                        examinedDoc.Title.Clear();

                        Title tit = new Title();
                        tit.Name = "Titulo_ITERWEB";
                        tit.Content = data;
                        examinedDoc.Title.Add(tit);

                        string json = JsonConvert.SerializeObject(examinedDoc);

                        it.JsonContent = json;
                        UpdateOldDoc(it);
                        processedIds.Add(it);
                    }
                }
            }
            liner = "Duplicates set to " + processedIds.Count;
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            liner = "Duplicated Title ids[" + processedIds.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
            Console.WriteLine(liner);
            if (log.IsWarnEnabled) log.Warn(liner);
            if (log.IsDebugEnabled) log.Debug("OldDocValidateTemplateDuplicateTitleFixOnly end");
        }

        /// <summary>
        /// It scans all valid documents (those with 'oldDocStatus' set to 2). Then puts for each 'oldDocTemplateType' a list of idSitemaps
        /// that tells which documents have less than 'threshold' size for its content.
        /// </summary>
        /// <param name="threshold">Document content threshold</param>
        private void ScanOldDocsContentSize(int threshold) {
            if (log.IsDebugEnabled) log.Debug("ScanOldDocsContentSize start");

            string lineInfo = "ScanOldDocsContentSize for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            int numDocument = 1;
            int invalidDocContentSize = 0;
            Dictionary<int, List<IterwebMapInfo>> templateTypeDictionary = new Dictionary<int, List<IterwebMapInfo>>();

            foreach (var it in _se4DocList) {
                Console.Write("\rEvaluating " + it.IDOldDoc + " Num document: " + (numDocument++));
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_SUCCESS) {
                    Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);
                    bool validTemplateNum = (examinedDoc.TemplateType != TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_003 &&
                                            examinedDoc.TemplateType != TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005 &&
                                            examinedDoc.TemplateType != TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_004 &&
                                            examinedDoc.TemplateType != TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006);
                    if (validTemplateNum) {
                        String docContent = examinedDoc.DocumentText.Content;
                        if (docContent.Length <= threshold) {
                            invalidDocContentSize++;
                            if (!templateTypeDictionary.ContainsKey(examinedDoc.TemplateType)) {
                                List<IterwebMapInfo> ids = new List<IterwebMapInfo>();
                                ids.Add(it);
                                templateTypeDictionary.Add(examinedDoc.TemplateType, ids);
                            }
                            else {
                                var keyList = templateTypeDictionary[examinedDoc.TemplateType];
                                keyList.Add(it);
                            }
                            LogToDocSizeTable(it.IdSitemap, it.IDSE4ArticleId, it.IDOldDoc, it.OldDocTemplateType.ToString(), threshold, docContent);
                        }
                    }
                }
            }
            Console.WriteLine();
            foreach (var pair in templateTypeDictionary) {
                lineInfo = "Results for TemplateType = " + pair.Key + " holding " + pair.Value.Count + " items with Ids [" + pair.Value.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                Console.WriteLine(lineInfo);
                if (log.IsWarnEnabled) log.Warn(lineInfo);
            }
            lineInfo = "There are " + invalidDocContentSize + " documents which threshold is less than " + threshold;
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            if (log.IsDebugEnabled) log.Debug("ScanOldDocsContentSize end");
        }

        /// <summary>
        /// It scans all valid documents. Then puts for each 'layout' a list of idSitemaps
        /// that tells which documents have less than 'threshold' size for its content.
        /// </summary>
        /// <param name="threshold">Document content threshold</param>
        private void ScanSE4DocsContentSize(int threshold) {
            if (log.IsDebugEnabled) log.Debug("ScanSE4DocsContentSize begin");
            string lineInfo = "ScanSE4DocsContentSize for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            int numDocument = 1;
            int invalidDocContentSize = 0;
            Dictionary<String, List<IterwebMapInfo>> layoutDictionary = new Dictionary<String, List<IterwebMapInfo>>();

            foreach (var it in _se4DocList) {
                Console.Write("\rEvaluating " + it.IDSE4ArticleId + " Num document: " + (numDocument++));

                if (it.Layout != "creacion_galeria") {
                    String docContent = "";
                    bool isValidDoc = IsValidDocument(it, ref docContent, GlobalConstants.SE4_DOC_CONTENT_THRESHOLD);
                    if (!isValidDoc) {
                        invalidDocContentSize++;
                        if (!layoutDictionary.ContainsKey(it.Layout)) {
                            List<IterwebMapInfo> ids = new List<IterwebMapInfo>();
                            ids.Add(it);
                            layoutDictionary.Add(it.Layout, ids);
                        }
                        else {
                            var keyList = layoutDictionary[it.Layout];
                            keyList.Add(it);
                        }
                        LogToDocSizeTable(it.IdSitemap, it.IDSE4ArticleId, it.IDOldDoc, it.Layout, threshold, docContent);
                    }
                }
            }
            Console.WriteLine();
            foreach (var pair in layoutDictionary) {
                lineInfo = "Results for Layout [" + pair.Key + "] holding " + pair.Value.Count + " items with Ids [" + pair.Value.Select(id => id.IdSitemap).ToList().ToStringDelimited(",") + "]";
                Console.WriteLine(lineInfo);
                if (log.IsWarnEnabled) log.Warn(lineInfo);
            }
            lineInfo = "There are " + invalidDocContentSize + " documents which threshold is less than " + threshold;
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            if (log.IsDebugEnabled) log.Debug("ScanSE4DocsContentSize end");
        }

        /// <summary>
        /// Scans old documents trying to infer if BancoConocimiento/Bancomedios is referenced in text
        /// </summary>
        private void ScanOldDocsLookupFor() {
            if (log.IsDebugEnabled) log.Debug("ScanOldDocsLookupFor start");
            string lineInfo = "ScanSE4DocsContentSize for " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsWarnEnabled) log.Warn(lineInfo);
            int numDocument = 1;
            string contentTxt = "";

            foreach (var it in _se4DocList) {
                Console.Write("\rEvaluating " + it.IDOldDoc + " Num document: " + (numDocument++));
                contentTxt = "";
                Doc examinedDoc = JsonConvert.DeserializeObject<Doc>(it.JsonContent);

                if (examinedDoc != null) {
                    switch (it.OldDocTemplateType) {
                        case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_001:
                            foreach (var bt in examinedDoc.Bullet) {
                                contentTxt += bt.Content;
                            }
                            contentTxt += examinedDoc.DocumentText.Content + "<br><br>";
                            foreach (var rh in examinedDoc.ReaderHelp) {                                
                                    contentTxt += "<b>" + rh.NameTitleContent + "</b><br>";
                                contentTxt += rh.NameTextContent + "<br>";
                            }
                            break;
                        case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_005:
                            contentTxt = examinedDoc.DocumentText.Content;
                            break;
                        case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_006:
                            contentTxt += examinedDoc.DocumentText.Content;
                            foreach (var bt in examinedDoc.Bullet) {
                                contentTxt += bt.Content;
                            }
                            break;
                        case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_010:
                             contentTxt += examinedDoc.DocumentText.Content;                            
                            break;
                        case TemplateTypeCode.OLD_DOC_TEMPLATE_TYPE_012:
                            contentTxt += examinedDoc.DocumentText.Content;
                            break;
                    }
                    contentTxt = contentTxt.ToLower();
                    if (contentTxt.Contains("bancoconocimiento") || contentTxt.Contains("bancomedios")) {
                        lineInfo = "Document " + it.IdSitemap + " contains one or BancoConocimiento/bancomedios in text";
                        if (log.IsInfoEnabled) log.Info(lineInfo);
                    }
                }
            }
            if (log.IsDebugEnabled) log.Debug("ScanOldDocsLookupFor end");    
        }

        /// <summary>
        /// This is the main driving process to generate 
        /// SE4 documents in Iterweb xml package format suitable for importing
        /// in the new CMS implementation (Iterweb using Liferay).
        /// Here it is considered the 'OutputStructure' which dictates the final output XML.
        /// Actually, 'OutputStructure' is considered in all Generate methods.
        /// </summary>
        public void Execute() {
            if (log.IsDebugEnabled) {
                log.Debug("Execute Start");
            }

            // 1. Initialization
            //---------------------------------------------------------------------------------------------------                        

            // 2. Processing old documents (those found in ASP files but not in databse).
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_ALL ||
                _docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_OLD_ONLY) {
                if (log.IsInfoEnabled) {
                    log.Info("Scanning old documents");
                }

                Console.WriteLine();
                Console.WriteLine("Scanning old documents");
                if (!_isOldDocsLoaded) {
                    _se4DocList.Clear();

                    Console.WriteLine("Loading documents from database");
                    if (log.IsInfoEnabled) {
                        log.Info("Loading documents from database");
                    }
                    LoadOldDocuments();
                }
                Console.WriteLine();
                Console.WriteLine("Scanning old documents and saved as json");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning old documents and saved as json");
                }
                CheckOldDocumentsToJSon();
                PostValidateOldDocuments();
                Console.WriteLine("Generating old documents packages");
                if (log.IsInfoEnabled) {
                    log.Info("Generating old documents packages");
                }
                if (_se4DocList.Count > 0) {
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                GeneratePackagesForOldDocs();
            }

            // 3. Processing SE4 documents (those found in Database table).
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_ALL ||
                _docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_SE4_ONLY) {
                if (log.IsInfoEnabled) {
                    log.Info("Scanning SE4 documents");
                }
                Console.WriteLine("Scanning SE4 documents");
                if (log.IsInfoEnabled) {
                    log.Info("Preparing Data to generate");
                }
                Console.WriteLine("Preparing Data to generate");
                _se4DocList.Clear();
                BuildSe4TemplateFieldMapping();
                LoadSE4Documents();
                Console.WriteLine();
                Console.WriteLine("Generating SE4 documents packages");
                if (log.IsInfoEnabled) {
                    log.Info("Generating SE4 documents packages");
                }
                if (_se4DocList.Count > 0) {
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                GeneratePackagesForSE4Docs();
            }

            // 4. Processing Toon documents from directory).
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_ALL ||
                _docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_TOON_FROM_DIR) {
                if (log.IsInfoEnabled) {
                    log.Info("Processing toon documents from directory");
                }

                if (log.IsInfoEnabled) {
                    log.Info("Loading into control database");
                }
                Console.WriteLine("Scanning Toons");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning Toons");
                }

                if (_byPassToonCreation == 1) {
                    var s = "Bypassing toon scanning";
                    if (log.IsInfoEnabled) {
                        log.Info(s);
                    }
                    Console.WriteLine(s);
                    LoadToonInfo();
                }
                else {
                    ScanToons();
                }
                Console.WriteLine();
                Console.WriteLine("Generate Toon packages");
                if (log.IsInfoEnabled) {
                    log.Info("Generate Toon packages");
                }
                if (_toonList.Count > 0) {
                    cnts.IterWebManifestFile++;
                    isIterWebManifestFileAffected = true;
                }
                else {
                    Console.WriteLine("Nothing to do");
                    if (log.IsInfoEnabled) {
                        log.Info("Nothing to do");
                    }
                }
                GeneratePackagesToonDir();
                Console.WriteLine("Toons processing ends.");
                if (log.IsInfoEnabled) {
                    log.Info("Toons processing ends.");
                }
            }

            // 5. Sync database.
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_SYNC_DB) {
                Console.WriteLine("Synchronizing db index");
                if (log.IsInfoEnabled) {
                    log.Info("Synchronizing db index");
                }
                SynchronizeDBData();
                Console.WriteLine("SynchronizeDBData finished.");
                if (log.IsInfoEnabled) {
                    log.Info("SynchronizeDBData finished.");
                }
            }

            // 6. Execute SQLExecStmt command
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_EXEC_SQLEXECSTMT) {
                Console.WriteLine("Exec SQL Statement");
                if (log.IsInfoEnabled) {
                    log.Info("Exec SQL Statement");
                }
                ExecuteSqlStmt();
                Console.WriteLine("Exec SQL Statement finished.");
                if (log.IsInfoEnabled) {
                    log.Info("Exec SQL Statement finished");
                }
            }

            // 7. Validate URL Chars conforms to ASCII
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_VALIDATE_URL_CHARS) {
                Console.WriteLine("Validate URL Chars");
                if (log.IsInfoEnabled) {
                    log.Info("Validate URL Chars");
                }
                ValidateURLChars();
                Console.WriteLine("Validate URL Chars finished.");
                if (log.IsInfoEnabled) {
                    log.Info("Validate URL Chars finished");
                }
            }

            // 8. User export.
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_USER_EXPORT) {
                Console.WriteLine("User export");
                if (log.IsInfoEnabled) {
                    log.Info("User export");
                }
                ExportUser();
                Console.WriteLine("User export finished.");
                if (log.IsInfoEnabled) {
                    log.Info("User export finished");
                }
            }

            // 9. Compute URL Title
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_COMPUTE_URLTITLE) {
                Console.WriteLine("Compute URLTitle");
                if (log.IsInfoEnabled) {
                    log.Info("Compute URLTitle");
                }
                ComputeUrlTitle();
                Console.WriteLine("Compute URLTitle");
                if (log.IsInfoEnabled) {
                    log.Info("Compute URLTitle");
                }
            }

            // 10. Update CreateDate for Old Docs Only
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_UPDATE_CREATE_DATE_OLD_DOCS_ONLY) {
                Console.WriteLine("Update CreateDate for Old Docs Only");
                if (log.IsInfoEnabled) {
                    log.Info("Update CreateDate for Old Docs Only");
                }
                if (log.IsInfoEnabled) {
                    log.Info("Scanning old documents");
                }

                Console.WriteLine();
                Console.WriteLine("Scanning old documents");
                if (!_isOldDocsLoaded) {
                    _se4DocList.Clear();

                    Console.WriteLine("Loading documents from database");
                    if (log.IsInfoEnabled) {
                        log.Info("Loading documents from database");
                    }
                    LoadOldDocuments();
                }
                Console.WriteLine();
                Console.WriteLine("Scanning old documents and saved as json");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning old documents and saved as json");
                }
                CheckOldDocumentsToJSon();
                PostValidateOldDocuments();
                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                // COQ: Sep.19/2014 --> UpdateCreateDateForOldDocsOnly();
            }

            // 11. SQL to CSV file generation.
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_UPDATE_CREATE_SQL_TO_CSV) {
                Console.WriteLine("SQL to CSV file generation.");
                if (log.IsInfoEnabled) {
                    log.Info("SQL to CSV file generation.");
                }

                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                ExportSQLToCSV();
            }

            // 12. SQL Batch Execute (reads a text file executing each line as an SQL statement. 
            // NOTE: A full transaction is commited. If no errors are caught then a COMMIT is done, else a ROLLBACK.
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_EXEC_SQL_BATCH) {
                Console.WriteLine("SQL Batch Execute.");
                if (log.IsInfoEnabled) {
                    log.Info("SQL Batch Execute.");
                }
                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                ExecuteSqlStmtBatch();
                Console.WriteLine("End Processing");
                if (log.IsInfoEnabled) {
                    log.Info("End Processing");
                }
            }

            // 13. SQL to CSV file generation. NOTE: SQL Statement is stored in file using key 'SQLBatchFile'.
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_UPDATE_CREATE_SQL_TO_CSV_SQL_IN_FILE) {
                Console.WriteLine("SQL to CSV file generation with 'SQLBatchFile' key.");
                if (log.IsInfoEnabled) {
                    log.Info("SQL to CSV file generation with 'SQLBatchFile' key.");
                }

                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                ExportSQLToCSVWithSQLInFile();
            }

            // 14. Analyze Old Doc Content Size
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_ANALYZE_OLDOCS_CONTENT_SIZE) {
                Console.WriteLine("Analyze Old Doc Content Size");
                if (log.IsInfoEnabled) {
                    log.Info("Analyze Old Doc Content Size");
                }

                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                Console.WriteLine("Scanning old documents");
                if (!_isOldDocsLoaded) {
                    _se4DocList.Clear();

                    Console.WriteLine("Loading documents from database");
                    if (log.IsInfoEnabled) {
                        log.Info("Loading documents from database");
                    }
                    LoadOldDocuments();
                }
                Console.WriteLine();
                Console.WriteLine("Scanning old documents for content size");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning old documents for content size");
                }
                ScanOldDocsContentSize(GlobalConstants.OLD_DOC_CONTENT_THRESHOLD);
            }

            // 15. Analyze SE4 Doc Content Size
            //---------------------------------------------------------------------------------------------------
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_ANALYZE_SE4DOCS_CONTENT_SIZE) {
                Console.WriteLine("Analyze SE4 Doc Content Size");
                if (log.IsInfoEnabled) {
                    log.Info("Analyze SE4 Doc Content Size");
                }

                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }
                Console.WriteLine("Scanning SE4 documents");
                if (!_isOldDocsLoaded) {
                    _se4DocList.Clear();

                    Console.WriteLine("Loading documents from database");
                    if (log.IsInfoEnabled) {
                        log.Info("Loading documents from database");
                    }
                    _se4DocList.Clear();
                    BuildSe4TemplateFieldMapping();
                    LoadSE4Documents();
                }
                Console.WriteLine();
                Console.WriteLine("Scanning SE4 documents for content size");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning SE4 documents for content size");
                }
                ScanSE4DocsContentSize(GlobalConstants.SE4_DOC_CONTENT_THRESHOLD);
            }

            // 16. Lookup BancoConocimiento in text
            if (_docProcessingMode == GlobalConstants.DOC_PROCESSING_MODE_LOOKUP_OLD_DOCS_BANCOCONOCIMIENTO_IN_TEXT) {
                Console.WriteLine("Analyze OldDocs BancoConocimiento in text");
                if (log.IsInfoEnabled) {
                    log.Info("Analyze OldDocs BancoConocimiento in text");
                }

                Console.WriteLine("Processing");
                if (log.IsInfoEnabled) {
                    log.Info("Processing");
                }                
                Console.WriteLine("Scanning old documents");
                if (!_isOldDocsLoaded) {
                    _se4DocList.Clear();

                    Console.WriteLine("Loading documents from database");
                    if (log.IsInfoEnabled) {
                        log.Info("Loading documents from database");
                    }
                    LoadOldDocuments();
                }
                Console.WriteLine();
                Console.WriteLine("Scanning Old Docs for Bancoconocimiento/BancoMedios in text");
                if (log.IsInfoEnabled) {
                    log.Info("Scanning Old Docs for Bancoconocimiento/BancoMedios in text");
                }
                ScanOldDocsLookupFor();
            }

            // 16. End.
            if (log.IsDebugEnabled) log.Debug("Execute End");
        }        
    }
}