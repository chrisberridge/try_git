-- Author:   Carlos Adolfo Ortiz Q.
-- Date:     Mar.23/2013
-- Modified: Jun.09/2014

CREATE TABLE sitemap(
	url varchar(512) NULL,
	urlHost varchar(255) NULL,
	urlPath varchar(255) NULL,
	urlParameters varchar(255) NULL,
	source varchar(255) NULL,
	filename varchar(255) NULL,
	dateProcessed datetime NULL,
	sitemapType int NULL,
	filter varchar(255) NULL,
	idSE4 varchar(20) NULL,
	idOld varchar(20) NULL,
	idObjetoSE int NULL,
	Layout varchar(100) NULL,
	CreateDate datetime NULL,
	UpdateDate datetime NULL,
	Reads int NULL,
	ObjetoSEName varchar(255) NULL,
	idSE4ArticleId varchar(20) NULL,
	iterwebtemplate varchar(100) NULL,
	idMultimediaType smallint NULL,
	idBrightCove varchar(30) NULL,
	thBrightcove varchar(400) NULL,
	imgBrightCove varchar(400) NULL,
	strUrlMp4BightCove varchar(400) NULL,
	dtmBrightCove datetime NULL,
	jsonContent text NULL,
	oldDocStatus int NULL,
	oldDocTemplateType int NULL,
	idSitemap int IDENTITY(1,1) NOT NULL
);

alter table sitemap add processed int not null default 0;
alter table sitemap add idObjetoNoSE int null;
alter table sitemap add zipFileName varchar(255) null;
commit

CREATE NONCLUSTERED INDEX IX_FILENAME ON sitemap
(
	filename ASC
);

CREATE NONCLUSTERED INDEX IX_URL ON sitemap
(
	url ASC
);

CREATE NONCLUSTERED INDEX IX_URLPATH on sitemap
(
   urlPath ASC
);

create NONCLUSTERED INDEX IX_IDOLD on sitemap
(
   idOld ASC
);


CREATE TABLE toondir(
	{id] [int] IDENTITY(1,1) NOT NULL,
	[idArticle] [varchar](50) NULL,
	[fromdir] [varchar](255) NULL,
	[filename] [varchar](255) NULL,
	[dateProcessed] [datetime] NULL,
	[CreateDate] [datetime] NULL,
	[UpdateDate] [datetime] NULL,
	[processed] [int] NOT NULL,
	[zipFileName] [varchar](255) NULL,
 CONSTRAINT [PK_toondir] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
);

create table toondirtmp
(  
  fromdir varchar(255),
  filename varchar(255),
  dateProcessed datetime NULL,  
  CreateDate datetime NULL,
  UpdateDate datetime NULL,
  processed int NOT NULL,
  zipFileName varchar(255) NULL,
  idArticle varchar(50) NULL
);

create table articlecategories
(
  id_ObjetoSE int not null,
  idSeccion int not null,
  vocabulario varchar(255),
  categoria varchar(255),
  usar int
);

-- To populate from existing data
insert into sitemap(
url,urlHost, urlPath, urlParameters, source,
	filename, dateProcessed, sitemapType,
	filter,	idSE4, idOld, idObjetoSE, Layout,
	CreateDate,	UpdateDate,	Reads,
	ObjetoSEName, idSE4ArticleId,
	iterwebtemplate, idMultimediaType,
	idBrightCove, thBrightcove,	imgBrightCove, 
	strUrlMp4BightCove, dtmBrightCove,
	jsonContent, oldDocStatus,
	oldDocTemplateType, processed
	)
select url,urlHost, urlPath, urlParameters, source,
	filename, dateProcessed, sitemapType,
	filter,	idSE4, idOld, idObjetoSE, Layout,
	CreateDate,	UpdateDate,	Reads,
	ObjetoSEName, idSE4ArticleId,
	iterwebtemplate, idMultimediaType,
	idBrightCove, thBrightcove,	imgBrightCove, 
	strUrlMp4BightCove, dtmBrightCove,
	jsonContent, oldDocStatus,
	oldDocTemplateType, processed
from salaedicion4..sitemap

-----------------------------------------------------------------------------------------------------------------------------
-- Aug.23/2013
-- Last update: Jul.04/2014
-- Execute all statements below in sequence delimited by commit statement
-- NOTE: Use the BEGIN/END pair to execute what is needed to have sitemap updated.
-- BEGIN
-----------------------------------------------------------------------------------------------------------------------------

-- Execute only once if sitemap is new-installed in database
update sitemap
set idObjetoSE = B.id_ObjetoSE,
    ObjetoSEName = B.NomObjetoSE,
    Layout = B.Plantilla,
    CreateDate = B.FechaCreacion,
    UpdateDate = B.FechaModificacion,
    Reads = B.Lecturas

--select B.id_ObjetoSE, B.NomObjetoSE, B.Plantilla, B.FechaCreacion, B.FechaModificacion, B.Lecturas
from sitemap A inner join salaedicion4..ObjetoSE B
on A.URLPath = '/'+B.URL
commit

-- Execute only once if sitemap is new-installed in database
update sitemap
set idSE4 = 'EC_ART_' + cast(idObjetoSE as Varchar(255)), 
    idSE4ArticleId = 'EC_' + cast(idObjetoSE as Varchar(255)),
    iterwebtemplate = 'PLANTILLA-CONTENIDOS-HISTORICO'
commit;

-- Insert all of ObjetoSE that are not in Sitemap (Sync)
insert into sitemap(url, urlHost, urlPath, dateProcessed, sitemapType, idSE4, idObjetoSE, Layout, CreateDate, UpdateDate, Reads, 
            ObjetoSEName, idSE4ArticleId, iterwebtemplate)
select 'http://www.elcolombiano.com/' + url as TheURL, 'www.elcolombiano.com' as TheUrlHost, '/' + url as TheUrlPath,
       CURRENT_TIMESTAMP as TheDateProcessed, 4 as theSitemapType, 'EC_ART_' + cast(Id_ObjetoSE as varchar(255)) as TheIdSE4, Id_ObjetoSE, Plantilla, 
	   FechaCreacion, FechaModificacion, Lecturas, NomObjetoSE,
	   'EC_' + cast(Id_ObjetoSE as Varchar(255)) as TheIdSE4ArticleId, 'PLANTILLA-CONTENIDOS-HISTORICO' as TheIterwebtemplate
from salaedicion4..ObjetoSE 
where Id_ObjetoSE not in (select distinct idObjetoSE from sitemap where idObjetoSE is not null);
commit

update sitemap
set esInfografia = 0
where esInfografia is null
commit

-- Must always execute (sync)
select '/BancoConocimiento/' + substring(strNombreArchivo, 1,1) + '/' + strNombreArchivo + '/' + strNombreArchivo + '.asp' url,
       idTipoArchivo, dtmFecha, 
	   smVideoGenerado, idBrightCove, thBrightcove,
	   imgBrightCove, strUrlMp4BrightCove, dtmBrightCove
into #t
from salaedicion4..tblMultimedia
where idTipoArchivo <> 4

 -- tblTipoArchivo
--1 VIDEO
--2 AUDIO
--3 GALERIA
--4 NO APLICA

-- Must always execute (sync)
update sitemap
set idMultimediaType = B.idTipoArchivo,
    idBrightCove = B.idBrightCove,
	thBrightcove = B.thBrightcove,
	imgBrightCove = B.imgBrightCove, 
	strUrlMp4BightCove = B.strUrlMp4BrightCove, 
	dtmBrightCove = B.dtmBrightCove
from sitemap A with (index(IX_URLPATH)) inner join
     #t B on A.urlPath = B.url
commit

-- Must always execute (sync). It looks for BrightCove videos in Objetose
drop table #t
select B.id_ObjetoSE, B.NomObjetoSE, B.plantilla, B.URL, B.FechaCreacion, A.Texto,
       C.idTipoArchivo, C.dtmFecha,  C.smVideoGenerado, C.idBrightCove, C.thBrightcove,
       C.imgBrightCove, C.strUrlMp4BrightCove, C.dtmBrightCove
into #t
from salaedicion4..objetoSEContenido A inner join salaedicion4..objetose B on A.CodObjetoSE = B.Id_ObjetoSe inner join
     salaedicion4..tblMultimedia C on A.texto = C.strNombreArchivo 
where A.Atributo in ('videoPrincipal', 'videoGaleria') and A.Texto is not null and A.Texto <> '' --and B.plantilla = 'creacion_notainterior100'
and C.idBrightCove is not null
commit

update protecimport..sitemap
set idMultimediaType = B.idTipoArchivo,
    idBrightCove = B.idBrightCove,
	thBrightcove = B.thBrightcove,
	imgBrightCove = B.imgBrightCove, 
	strUrlMp4BightCove = B.strUrlMp4BrightCove, 
	dtmBrightCove = B.dtmBrightCove
from protecimport..sitemap A inner join #t B on A.idObjetoSE = B.id_objetoSE
where A.idBrightCove is null
commit

-- Create IDS for Old Documents (executed only once).
select A.idSitemap + 4000000 ID, A.idOld, A.idSE4ArticleId, A.sitemapType, A.Url into #t2 
from sitemap A where idObjetoSE is null
commit

-- Create IDS for Old Documents (executed only once).
update #t2
set idOld =  'EC_ART_OLD_' + Cast(ID as Varchar(40)),
    idSE4ArticleId = 'EC_' + Cast(ID as varchar(40)),
	sitemapType = 5 -- Not in SE4 or Sitemaps
commit

-- Create IDS for Old Documents (executed only once).
update sitemap
set idOld = 'EC_ART_OLD_' + Cast(A.ID as Varchar(40)),
    idSE4ArticleId = 'EC_' + Cast(A.ID as varchar(40)),
	sitemapType = 5 -- Not in SE4 or Sitemaps
--select *
from #t2 A inner join sitemap B on A.url = B.url
commit

-- Executed only once.
update sitemap
set urlParameters = ''
where urlParameters is null;
commit

-- Check unicity
select idSE4ArticleId
	from (
		select idSE4ArticleId, count(1) cnt
		from sitemap
		where urlParameters is not null and urlParameters = ''
		group by idSE4ArticleId
		having count(1) > 1
	) A
	
select *
from sitemap 
where createdate is null	

-- Execute the following to sync NoticiaClaveSeccion documents
insert into articleCategories(id_ObjetoSE, idSeccion)
select A.Id_ObjetoSE, A.idSeccion
from Salaedicion4..NoticiaClaveSeccion A left join
     articlecategories B on A.id_ObjetoSE = B.id_ObjetoSE and A.idSeccion = B.idSeccion
where B.idSeccion is null
commit

delete articlecategories
from articlecategories A inner join (
	select B.Id_ObjetoSE, B.idSeccion
	from Salaedicion4..NoticiaClaveSeccion A right join
		 articlecategories B on A.id_ObjetoSE = B.id_ObjetoSE and A.idSeccion = B.idSeccion
	where A.idSeccion is null
) B on A.id_ObjetoSE = B.id_ObjetoSE and A.idSeccion = B.idSeccion
commit

update articlecategories
set vocabulario = 'Agregadoras'
where vocabulario is null
commit

insert into ArticleSectionCategory(idSeccion, categoria, categoriaAlterno)
select A.Id_Magazin, A.NomMagazin, ''
from SalaEdicion4..Magazin A
where A.Id_Magazin in (
	select distinct A.idSeccion
	from articlecategories      A left join 
		 ArticleSectionCategory B on A.idSeccion = B.idSeccion
	where B.idSeccion is null
    ) 

	
-- Execute this one to update infographic notes
update sitemap
set esInfografia = 1
from sitemap A inner join
     articleMigrateInfographic B on A.idObjetoSE = B.id_objetose

-- Update metadata for Agregadora
update ArticleSectionCategory
set categoriaAlterno = A.Metadato,
    usar = A.usar
from agregadoras A inner join ArticleSectionCategory B on A.id = B.idSeccion

update sitemap
set urlParameters = ''
where urlParameters is null
commit
-- Jul.02/2014,  Executed only once
---- The following is to have an inventory of articles which uses 'creacion_notainterior100' and 'creacion_notainterior100M'
---- that must be in the Infographic historic catalog.
select id_objetose, NomObjetoSE, Plantilla, FechaCreacion into ArticleMigrateInfographic
from Salaedicion4..ObjetoSE 
where Id_ObjetoSE in
(
290407,281365,279423,276914,254695,236618,254718,258119,258105,237669,219162,241874,233799,268997,271126,273203,
213283,281296,266948,288516,258088,271525,281191,243758,263774,284299,265047,211901,277553,297540,278844,231064,
249925,249966,228131,291351,295450,290326,250901,293916,229928,233651,243790,227235,241596,248365,183479,269751,
240165,287349,297468,291360,293412,258572,294577,293099,295919,254407,259302,252060,291364,216919,254719,259317,
291194,255038,296817,276661,273182,262745,286576,186938,228452,262651,255049,255021,294245,277061,294720,295082,
241498,246875,290475,268028,252609,295964
)
--Jul.07/2014 -> 255038 will not migrate, it will be considered as manual.
-- From this point table ArticleMigrateInfographic must be synced.

-- Jul.03/2014
-- Validate Createdate lower than Updatedate
select idObjetoSE, createDate, UpdateDate
from sitemap where createDate > UpdateDate
commit

-- Fix dates
update sitemap
set updateDate = createDate
from sitemap where createDate > UpdateDate
commit


-----------------------------------------------------------------------------------------------------------------------------
-- END
-----------------------------------------------------------------------------------------------------------------------------



-------------
-- Other reports
-- Some statistics about records
-- SE4 docs
select createDate, DAY(CreateDate) dd, MONTH(CreateDate) mm, Year(CreateDate) yyyy into #t
from sitemap
where idSE4 is not null  and url is not null
commit

select yyyy, mm, count(1)
from #t
group by yyyy, mm
order by yyyy, mm
commit

-- non-SE4 docs
select createDate, DAY(CreateDate) dd, MONTH(CreateDate) mm, Year(CreateDate) yyyy into #t1
from sitemap
where idOld is not null and idSE4 is null
commit

select yyyy, mm, count(1)
from #t1
group by yyyy, mm
order by yyyy, mm
commit

