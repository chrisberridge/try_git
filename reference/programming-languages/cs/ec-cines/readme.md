ECCines
=======

V1.0.0.30-May.05/2015
V1.0.0.25-Apr.15/2015 (RELEASE)

Sitios para la administración de cartelera de Cine EL COLOMBIANO
Debe tener en cuenta lo siguiente:

ECCines.Admin es el sitio administrativo y solo debe ser accesible por la red interna de EL COLOMBIANO.
EcCines es el sitio público.

Siempre que vaya a actualizar el sitio debe realizar una publicación (despliegue en las carpetas indicadas) y de ahí debe 
coger los contenidos para paso a producción.

Required Components
===================

Javascript
----------
jquery-2.1.3.js --> jQuery Version 2.1.3
jquery-ui-1.11.3.js -> jQuery UI Version 1.11.3
jquery.maskedinput-1.4.1.js -> jQuery MaskedInput V1.4.1 (https://github.com/digitalBush/jquery.maskedinput)
iframeResizer.contentWindow.min.js -> jQuery iFrameResizer V2.8.4 (https://github.com/davidjbradshaw/iframe-resizer)

C# Retrieved via NuGET
- JSon.NET V6.0.8

A continuación van los archivos y carpetas que deben ir en los sitios de producción

ECCines (sitio público) con URL=http://cines.elcolombiano.com --> página por defecto es 'readjson.aspx'
========================================================================================================
/Content --> Van todos los archivos y subcarpeta aquí.
/css --> Van todos los archivos y subcarpeta aquí.
/images --> Van todos los archivos y subcarpeta aquí.
/recursos --> Esta carpeta debe existir junto con 'json' y 'poster'.
/Scripts --> Van todos los archivos y subcarpeta aquí.
ecmoviesearch.aspx
readjson.aspx
showmoviedetail.aspx
Web.config

NOTA: Las siguientes páginas son solo de uso interno y no deben desplegarse.
showmoviebillboardwidget.html (no se debe desplegar, es de uso interno.)

ECCines.Admin (sitio administrativo) con URL=http://cines.elcolombiano.com/admin --> página por defecto 'Default.aspx'.
=====================================================================================================================
/Content --> Van todos los archivos y subcarpeta aquí.
/css --> Van todos los archivos y subcarpeta aquí.
/images --> Van todos los archivos y subcarpeta aquí.
/recursos --> Esta carpeta debe existir junto con 'json' y 'poster'.
/Scripts --> Van todos los archivos y subcarpeta aquí.
cine.aspx
CinesHorarios.aspx
Default.aspx
entidad.aspx
Horario.aspx
Index.aspx
moviedetailiframe.aspx 
teatro.aspx
showmoviedetail.aspx

NOTA: Las siguientes páginas son solo de uso interno y no deben desplegarse.
ecmovies.aspx (No se debe desplegar, es de uso interno.)
showmoviewidget.aspx (No se debe desplegar, es de uso interno.)
