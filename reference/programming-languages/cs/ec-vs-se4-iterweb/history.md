File:     history.md
Author:   Carlos Adolfo Ortiz Q
Date:     Sep.03/2013
Modified: Nov.24/2014

Nov.24/2014 V1.8.110.380 Tag V1.8.110.380-Nov.24/2014
- Adds minor bug fixes.

Nov.21/2014 V1.8.100.370 Tag V1.8.100.370-Nov.21/2014
- Fixes the way toons are generated because its filename format inferred prior to this version assumed all files were to be uploads but
  it in fact was uploading duplicated toons. Now a correct filename format was used.

Nov.12/2014 V1.6.75.340 Tag V1.6.75.340-Nov.12/2014
- Old Docs correction 

Nov.05/2014 V1.5.60.320 Tag V1.5.60.320-Nov.05/2014
- Minor bug fixes
- Export user record logging

Oct.29/2014 V1.5.50.310 Tag V1.5.50.310-Oct.29/2014
- Adds changes to the way various SE4 docs template types are interpreted.
- Adds document content size inspector.

Oct.15/2014 V1.4.35.290 Tag V1.4.35.290-Oct.15/2014
- Corrects sereral bugs found.

Oct.10/1024 V1.4.30.260 Tag V1.4.30.260-Oct.10/2014
- Changes the way sections are being sent to the XML. This method CreateOutputArticleSectionsDocToXml changes 
  signature and adds a 'filterEx' parameter to indicate one more section is to be added and this one is not
  set to the default one.

Oct.09/2014 V1.4.25.255 Tag V1.4.25.255-Oct.09/2014
- Bug fixes

Oct.06/2014 V1.4.20.230 Tag V1.4.20.230-Oct.06/2014
- Logging is extended with messages about actually written records to XML.
- Processing refinement.

Sep.23/2014 V1.3.10.185 Tag V1.3.10.185-Sep.23/2014
- Corrects a lot of bugs to refine processing.

Jul.31/2014 V1.3.10.60 Tag V1.3.10.60-Jul.31/2014
- Prints number of records processed to WARN log mode.

Jul.30/2014 V1.3.10.55 Tag V1.3.10.55-Jul.30/2014
- Implements when an image is not referenced in non-SE4 documents then that image is not exported to XML file.

Jul.29/2014 V1.3.10.50 Tag V1.3.10.50-Jul.29/2014
- Implements when in an image gallery document there is no 'pubinfo.txt' or gallery dir is not existent then the document is 
  marked as processed with 10
  and a WARNING is logged and document is not written to XML file.
  If an image from gallery is not found, then that image is not referenced in XML file, but if all images are
  not found at all, then the whole document is not exported to XML file. A WARNING is logged and document is marked as 
  processed with 11 and with 12 if document is in SE4 database but its attribute 'nombreGaleria' is empty.
- Implements when an image is not referenced in SE4 Documents documents then that image is not exported to XML file.
- Changed List<long> processedIds = new List<long>() in SE4DocMigrate to accommodate a broader scope.

Jul.28/2014 V1.3.10.45 Tag V1.3.10.45-Jul.28/2014
- Reorganizes the way reporting is logged.
- Fixes an exception that halts execution in production, in image gallery mode, if pubinfo.txt does not exist the exception is raised.
- Fixes the way vocabulary and category are retrieved from database by committing to new tables and data.

Jul.24/2104 V1.3.10.35 Tag V1.3.10.35-Jul.24/2014
- Fixes when a file with name like 'josé-salgar-el-espectador-640x280-21072013.jpg' (NOTE: uses é) it its renamed to
  jose-salgar-el-espectador-640x280-21072013.jpg but if it is referenced again it is considered an exception like 
  'Cannot create a file that already exists', this happens because File.Move is used to rename a file.
- Corrects a bug when a command other than generating packages is issued it decreases number for 'IterWebManifestFile' in Counter object.
- Changes path for CSVExportFile file name to be other that Log files.
- If file name for image is longer than 75, then characters are removed, e.g., if filename is
  'carlos-mario-oquendo-mariana-pajon-miguel-calixto-640x280-07072014-Cortesia.jpg', which is 80 characters long
  then it is changed to 'carlos-mario-oquendo-mariana-pajon-miguel-calixto-640x280-07072014-Co.jpg'
- Adds to log the ids used to generate for any package to track errors.

Jul.22/2014 V1.3.10.30 Tag V1.3.10.30-Jul.22/2014
- Adds new command Update Create Date Old Docs Only to normalize old docs create date and update date.
- Adds new command to export a SQLDataReader to a CSV file using an extension method.
- Adds file cmdparams.properties at home directory for reference.
- Adds CSVExportFile configuration setting.
- Removes PrintVersion from being taken from configuration file and prints the one burned in EXE.
- Changes to version in AssemblyInfo.cs

Jul.18/2014 V1.3.10.20 Tag V1.3.10.20-Jul.18/2014
- Adds new command to update urlTitle value.

Jul.17/2014 V1.3.10.15 Tag V1.3.10.15-Jul.17/2004
- By a mistake, the V1.2.19.25 Jun.12/2014 was not generated and it cannot be generated so far.
- Renames 'SE4DocMigrate.GeneratePackages()' to a 'SE4DocMigrate.Execute()' as the application objective has changed from being exclusively
  to generate packages to a more general approach.
- Adds  new commands. NOTE: Commands are parameterized using property 'DocumentProcessingMode', it is not a suitable name but was
  named as such as the program mission was solely to generate packages, now it changes.
- Now application version label is extracted from configuration property file  
- Support user export. (Commentary user list).

Jun.12/2014 V1.2.19.25 Tag V1.2.19.25-Jun.12/2014
- Changes database connection processing. It uses a connection to connect exclusively to 'sitemap' table
  (This table is the processing control data).

May.21/2014 V1.2.19.15 Tag V1.2.19.15-May.21/2014
- Implements the document processed logic to just use records in a loading document phase for those documeents
  which have not been processed. 
- Adds source directory removal to avoid  disk space consumption.
-  
May.14/2014 V1.2.18.20 Tag V1.2.18.20-May.14/2014
- Adds a new way to output XML formats for iterweb article migration.

May.02/2014 V1.2.17.95 Tag V1.2.17.95-May.02/2014
- Version preparation for output XML change. That is, ProtectMedia in its Iterweb current version changes the structure for importing
  old documents to the Iterweb system. Thus, if you need old structure version, use Tag V1.2.17.90-Nov.29/2013.
  Output generation is contained in file SE4DOCMIGRATE.CS in methods 
  GeneratePackagesForOldDocs and GeneratePackagesForSE4Docs

Nov.29/2013 V1.2.17.90 Tag V1.2.17.90-Nov.29/2013
- Adds new logic to map more articles into existing templates.

Nov.28/2013 V1.2.16.85 Tag V1.2.16.85-Nov.28/2013
- New template added and analyze other ways to map more documents, in this case over existing ones.

Nov.19/2013 V1.2.12.65 Tag V1.2.12.65-Nov.19/2013
- Revised functionality for domain scanning.
- Now it can handle multiple named domains in URLs, such as 
  "www.elcolombiano.biz"
  "www.elcolombiano.co"
  "www.elcolombiano.com"
  "www.elcolombiano.com.co"
  "www.elcolombiano.tv"

Nov.12/2013 V1.2.8.40 Tag V1.2.8.40-Nov.12/2013
- Serious bug when processing Template 001

Nov.06/2013 V1.2.5.20 Tag V1.2.5.20-Nov.06/2013
- Two new templates added. Test case harness verified.

Oct.31/2013 V1.2.1.1 Tag  V1.2.1.1-Oct.31/2013
- As new templates were added it was necessary to have the test deck up and running again.
  Test deck data is first introduced in this release and saved in file 'README.md'.

Oct.24/2013 V1.1.7.1 Tag  V1.1.7.1-Oct.24/2013
- Minor fixes, some bug fixes and new version release

Oct.15/2013 V1.1.1.1 Tag  V1.1.1.1-Oct.15/2013
- Changed the final naming for Old document metadata.

Oct.11/2013 V1.0.10.60 Tag V1.0.10.60-Oct.11/2013
 - New master release

Oct.08/2013 V1.0.8.30  Tag: V1.0.8.30-Oct.08/2013
- Bug fixes.

Sep.25/2013 V1.0.6.10  Tag: V1.0.6.10-Sep.27/2013
- Two New template addition and fixing the interpretation for News Group template.

Sep.23/2013 V1.0.5.3  Tag: V1.0.5.3-Sep.23/2013
- Bug fixes.

Sep.20/2013 V1.0.5.2  Tag: V1.0.5.2-Sep.20/2013
- The application is more stable, although it requires more testing.

Sep.17/2013 V1.0.3.100 Tag: V1.0.3.100-Sep.17/2013
- All documents in SE4 are commited to Iterweb XML.
- New document scanning (old document logic).

Sep.03/2013 V1.0.0.0 Tag V1.0.0.0-Sep.03/2013
- Initial Commit