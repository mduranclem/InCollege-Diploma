using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json; // Necesario para convertir objetos a texto y viceversa

namespace InCollege.Datos
{
    public class Repositorio
    {
        // Nombre del archivo donde se guardarán los datos físicamente
        private string rutaArchivo = "base_de_datos_v2.json";

        /// <summary>
        /// Guarda cualquier lista de objetos en un archivo JSON.
        /// </summary>
        /// <typeparam name="T">El tipo de dato (ej. Usuario, Trabajo)</typeparam>
        /// <param name="listaDatos">La lista completa a guardar</param>
        public void GuardarDatos<T>(List<T> listaDatos)
        {
            // 1. Configuramos para que el JSON sea legible (con sangría)
            var opciones = new JsonSerializerOptions { WriteIndented = true };

            // 2. Convertimos la lista de objetos a texto (String)
            string jsonString = JsonSerializer.Serialize(listaDatos, opciones);

            // 3. Escribimos ese texto en el archivo (sobrescribe lo anterior)
            File.WriteAllText(rutaArchivo, jsonString);
        }

        /// <summary>
        /// Lee el archivo y devuelve la lista de objetos.
        /// </summary>
        public List<T> CargarDatos<T>()
        {
            // 1. Si el archivo no existe, devolvemos una lista vacía nueva
            if (!File.Exists(rutaArchivo))
            {
                return new List<T>();
            }

            // 2. Leemos todo el texto del archivo
            string jsonString = File.ReadAllText(rutaArchivo);

            // 3. Si el archivo está vacío, devolvemos lista vacía
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<T>();
            }

            // 4. Convertimos el texto de vuelta a objetos de C#
            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }
    }
}

