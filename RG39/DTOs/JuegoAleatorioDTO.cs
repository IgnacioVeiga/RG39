namespace JuegoAleatorio.JuegoAleatorioDTOs
{
    public class ArchivoDTO
    {
        public int Id { get; set; } // Para identificar a cada uno
        public bool Activo { get; set; } // Para filtrar programas de la lista

        public string RNAF { get; set; } // Sigifica "Ruta Nombre de Archivo y Formato"
        public string Ruta { get; set; } // Se carga a mano al crear el objeto
        public string Formato => RNAF.Remove(0, RNAF.Length - 4);
        private string nombreArchivo;
        public string NombreArchivo
        {
            get
            {
                nombreArchivo = RNAF.Remove(0, Ruta.Length + 1);
                nombreArchivo = nombreArchivo.Remove(nombreArchivo.Length - 4);
                return nombreArchivo;
            }
            set
            {
                nombreArchivo = value;
            }
        }
    }
}
