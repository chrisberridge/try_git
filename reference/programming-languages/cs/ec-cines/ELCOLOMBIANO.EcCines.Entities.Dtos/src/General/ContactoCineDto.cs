/*==========================================================================*/
/* Source File:   CONTACTOCINEDTO.CS                                        */
/* Description:   Data transfer object                                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Apr.24/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    /// <summary>
    /// Data Transfer Object
    /// </summary>
    public class ContactoCineDto : AbstractCommonDomain {
        public int idContacto { get; set; }
        public int idCine { get; set; }
        public string cedulaContacto { get; set; }
        public string nombreContacto { get; set; }
        public string cargoContacto { get; set; }
        public string telefono1Contacto { get; set; }
        public string telefono2Contacto { get; set; }
    }
}