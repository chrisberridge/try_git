/*==========================================================================*/
/* Source File:   PROGRAM.CS                                                */
/* Description:   Xml Parsing for Sitemap files Standalone app.             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.19/2013                                               */
/* Last Modified: May.15/2014                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Jul.19/2013 COQ File created.
============================================================================*/

using EC.Utils;
using EC.Utils.Domain;
using System;
using System.Collections.Generic;

namespace SitemapConsole {
    /// <summary>
    /// Compiles a series of Sitemap files and stores a result in Microsoft SQL Server database.
    /// </summary>
    public class AnalyseSitemap {
        /// <summary>
        /// Site map list to analyze.
        /// </summary>
        private List<SiteMapInfo> siteMapList;

        /// <summary>
        /// Fills up the Sitemap list.
        /// </summary>
        protected void FillSiteMapList() {
            string basePath = @"d:\Temp\Trash\T12-SEO Sitemaps\";
            string filter = "";
            siteMapList = new List<SiteMapInfo>();
            siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00007.xml", "sitemap-sitio-00007.xml", 1, filter));
            siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00006.xml", "sitemap-sitio-00006.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-video-00001.xml", "sitemap-video-00001.xml", 3, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00005.xml", "sitemap-sitio-00005.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00004.xml", "sitemap-sitio-00004.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00003.xml", "sitemap-sitio-00003.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00002.xml", "sitemap-sitio-00002.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-sitio-00001.xml", "sitemap-sitio-00001.xml", 1, filter));            
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap.xml", "sitemap.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-1.xml", "sitemap-1.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-10.xml", "sitemap-10.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-100.xml", "sitemap-100.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-2.xml", "sitemap-2.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-3.xml", "sitemap-3.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-4.xml", "sitemap-4.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-41.xml", "sitemap-41.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-42.xml", "sitemap-42.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-5.xml", "sitemap-5.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-52.xml", "sitemap-52.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-6.xml", "sitemap-6.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-61.xml", "sitemap-61.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-62.xml", "sitemap-62.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-71.xml", "sitemap-71.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-72.xml", "sitemap-72.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-9.xml", "sitemap-9.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-news.xml", "sitemap-news.xml", 2, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-secciones.xml", "sitemap-secciones.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-7.xml", "sitemap-7.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-81.xml", "sitemap-81.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-8.xml", "sitemap-8.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-82.xml", "sitemap-82.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-92.xml", "sitemap-92.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-91.xml", "sitemap-91.xml", 1, filter));
            //siteMapList.Add(new SiteMapInfo(basePath + "sitemap-51.xml", "sitemap-51.xml", 1, filter));
        }

        /// <summary>
        /// Iterates over the map list to load all of the sitemap files the loc or URL tag.
        /// </summary>
        public void ProcessMapList() {
            FillSiteMapList();
            SitemapXmlParsing xp = new SitemapXmlParsing();

            DateTime dtStartGlobal, dtEndGlobal;
            DateTime dtStart, dtEnd;
            List<string> info;
            var i = 1;

            dtStartGlobal = DateTime.Now;
            foreach (var smi in this.siteMapList) {
                xp.Source = smi.Source;
                xp.FileName = smi.FileName;
                xp.Filter = smi.Filter;
                xp.SitemapType = smi.Type;
                Console.Write(i + " File ");
                Console.Write("[" + xp.Source + "] ");
                dtStart = DateTime.Now;
                try {
                    info = xp.ParseSitemapFile();
                    xp.SaveToDatabase(info);
                    Console.Write("With " + info.Count + " items. ");
                    Console.Write("OK ");
                }
                catch (Exception ex) {
                    Console.WriteLine("Failed [" + ex.Message + "]");
                    Console.WriteLine("Source [" + ex.Source + "]");
                    Console.WriteLine("Stacktrace [" + ex.StackTrace + "]");
                }
                dtEnd = DateTime.Now;
                Console.WriteLine("Elapsed time: " + (dtEnd - dtStart));
                i++;
            }
            dtEndGlobal = DateTime.Now;
            Console.WriteLine("All files processed in " + (dtEndGlobal - dtStartGlobal));
        }
    }

    /// <summary>
    /// Main entry point to executable
    /// </summary>
    class Program {
        static void Main(string[] args) {
            //string theUrl = "http://www.elcolombiano.com/BancoConocimiento/¡/¡dejese_ganar_¿le_diria_usted_a_uno_de_sus_deportistas/¡dejese_ganar_¿le_diria_usted_a_uno_de_sus_deportistas.asp";
            //Uri uri = new Uri(theUrl);
            //Console.WriteLine("theURL=[" + theUrl + "]");
            //Console.WriteLine("uri=[" + uri.ToString() + "]");
            //Console.WriteLine("Host=[" + uri.Host + "]");
            //Console.WriteLine("HostTypeName=[" + uri.HostNameType + "]");
            //Console.WriteLine("AbsolutePath=[" + uri.AbsolutePath + "]");
            //Console.WriteLine("Query=[" + uri.Query + "]");
            //Console.WriteLine("LocalPath=[" + uri.LocalPath + "]");

            AnalyseSitemap analyseSiteMap = new AnalyseSitemap();
            Console.WriteLine("Start");
            analyseSiteMap.ProcessMapList();
            Console.WriteLine("End!");
            Console.WriteLine("Enjoy!");
        }
    }
}
