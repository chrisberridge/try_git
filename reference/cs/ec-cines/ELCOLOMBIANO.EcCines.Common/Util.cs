/*==========================================================================*/
/* Source File:   UTIL.CS                                                   */
/* Description:   Helper class with utility methods.                        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Common {
    public static class Util {
        public static string getNombreDiaEspañol(string ingles) {
            string nombre = null;
            switch (ingles) {
                case "Monday":
                    nombre = "Lunes";
                    break;
                case "Tuesday":
                    nombre = "Martes";
                    break;
                case "Wednesday":
                    nombre = "Miércoles";
                    break;
                case "Thursday":
                    nombre = "Jueves";
                    break;
                case "Friday":
                    nombre = "Viernes";
                    break;
                case "Saturday":
                    nombre = "Sábado";
                    break;
                case "Sunday":
                    nombre = "Domingo";
                    break;
                default:
                    nombre = ingles;
                    break;
            }
            return nombre;
        }

        public static int getNumeroDia(string ingles) {
            int numero = 0;
            switch (ingles) {
                case "Monday":
                    numero = 1;
                    break;
                case "Tuesday":
                    numero = 2;
                    break;
                case "Wednesday":
                    numero = 3;
                    break;
                case "Thursday":
                    numero = 4;
                    break;
                case "Friday":
                    numero = 5;
                    break;
                case "Saturday":
                    numero = 6;
                    break;
                case "Sunday":
                    numero = 7;
                    break;
            }
            return numero;
        }
    }
}
