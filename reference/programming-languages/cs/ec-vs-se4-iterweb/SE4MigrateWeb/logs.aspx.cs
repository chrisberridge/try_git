/*==========================================================================*/
/* Source File:   LOGS.ASP.CS                                               */
/* Description:   Logs Page                                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.11/2014                                               */
/* Last Modified: Jul.11/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jul.11/2014 COQ File created.
============================================================================*/

using EC.SE4Migrate.Web.Utils;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace EC.SE4Migrate.Web {
    /// <summary>
    /// Logs Page
    /// </summary>
    public partial class logs : MigrationAbstract {
        /// <summary>
        /// Load page contents.
        /// </summary>
        /// <param name="sender">Object reference</param>
        /// <param name="e">Parameters to object reference</param>
        protected void Page_Load(object sender, EventArgs e) {
            _exeLaunchPath = RetrieveAppSetting("ExeLaunchPath");
            _logPath = RetrieveAppSetting("LogPath");
            if (!Page.IsPostBack) {
                // populate the dropdownlist                
                String sMappedPath = _logPath;

                var fqFilenames = new List<String>(Directory.GetFiles(sMappedPath));
                var filenames = fqFilenames.ConvertAll((s) => { return s.Replace(sMappedPath + "\\", ""); });

                ErrorMessage.InnerHtml = "";

                FileListView.DataSource = filenames;
                FileListView.DataBind();
            }
        }

        /// <summary>
        /// Zip files and send to web browser client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnGoClick(Object sender, EventArgs e) {
            _exeLaunchPath = RetrieveAppSetting("ExeLaunchPath");
            _logPath = RetrieveAppSetting("LogPath");
            ErrorMessage.InnerHtml = "";   // debugging only
            var filesToInclude = new System.Collections.Generic.List<String>();
            String sMappedPath = _logPath;
            var source = FileListView.DataKeys as DataKeyArray;

            foreach (var item in FileListView.Items) {
                CheckBox chkbox = item.FindControl("include") as CheckBox;
                Label lbl = item.FindControl("label") as Label;

                if (chkbox != null && lbl != null) {
                    if (chkbox.Checked) {
                        ErrorMessage.InnerHtml += String.Format("adding file: {0}<br/>\n", lbl.Text);
                        filesToInclude.Add(Path.Combine(sMappedPath, lbl.Text));
                    }
                }
            }

            if (filesToInclude.Count == 0) {
                ErrorMessage.InnerHtml += "You did not select any files?<br/>\n";
            }
            else {
                Response.Clear();
                Response.BufferOutput = false;

                System.Web.HttpContext c = HttpContext.Current;               
                string archiveName = String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "inline; filename=\"" + archiveName + "\"");

                // In some cases, saving a zip directly to Response.OutputStream can
                // present problems for the unzipper, especially on Macintosh.
                // To workaround that, you can save to a MemoryStream, then copy to
                // the Response.OutputStream.
                using (var ms = new MemoryStream()) {
                    using (ZipFile zip = new ZipFile()) {                        
                        if (!String.IsNullOrEmpty(tbPassword.Text)) {
                            zip.Password = tbPassword.Text;                            
                        }                       
                        zip.AddFiles(filesToInclude, "files");
                        zip.Save(ms);
                    }
                    // copy the memory stream to the Response.OutputStream
                    ms.Position = 0;
                    var b = new byte[1024];
                    int n;
                    while ((n = ms.Read(b, 0, b.Length)) > 0)
                        Response.OutputStream.Write(b, 0, n);
                }
                Response.Close();
            }
        }
    }
}