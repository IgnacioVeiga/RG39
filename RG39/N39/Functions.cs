using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace RG39.N39
{
    public static class FunctionsN39
    {

        public static void InicializarRuta(TabItem pAjustes, Label rutaCargada, Label programasDisponibles, ListView listaProgramas)
        {
            // obtiene la ruta del escritorio del usuario en sesion
            MainWindow.ruta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (string.IsNullOrWhiteSpace(MainWindow.ruta))
            {
                string mensaje = "No se pudo encontrar la ruta de tu escritorio.\nPor favor, elige donde escanear tu lista de juegos.";
                AvisoDirectorioNoUtil(mensaje, pAjustes);
            }
            else
            {
                rutaCargada.Content = "Ruta cargada: " + MainWindow.ruta;
                CargarArchivosEnRuta(MainWindow.ruta, pAjustes, rutaCargada, programasDisponibles, listaProgramas);
            }
        }

        public static void AvisoDirectorioNoUtil(string mensaje, TabItem pAjustes)
        {
            if (MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                pAjustes.IsSelected = true;
                
                // TODO: abrir el selector de carpetas.
            }
        }


        // TODO: cambiar la firma de esta funcion
        // Por ahora retorna un booleano que indica si la operacion fue exitosa
        // En ciertos casos necesito saber esto ultimo para mostrar o no un mensaje
        public static bool CargarArchivosEnRuta(string pRuta, TabItem pAjustes, Label rutaCargada, Label programasDisponibles, ListView listaProgramas)
        {
            if (string.IsNullOrWhiteSpace(pRuta))
            {
                string mensaje = "La ruta no existe, seleccione una correcta por favor.";
                AvisoDirectorioNoUtil(mensaje, pAjustes);
                return false;
            }

            string[] filePath = Directory.GetFiles(pRuta);
            IEnumerable<string> archivosFiltrados = filePath.Where(f => f.Remove(0, f.Length - 4) == ".lnk" || f.Remove(0, f.Length - 4) == ".url" || f.Remove(0, f.Length - 4) == ".exe");
            if (archivosFiltrados == null || !archivosFiltrados.Any())
            {
                string mensaje = "En:\n" + MainWindow.ruta + "\nNo hay videojuegos compatibles.\n" + MainWindow.formatosCompatibles;
                AvisoDirectorioNoUtil(mensaje, pAjustes);
                return false;
            }

            // "ruta" es la variable global y "pRuta" un parametro
            // "ruta" de ser posible nunca debe ser nula o vacia (salvo al inicializar el programa)
            MainWindow.ruta = pRuta;
            rutaCargada.Content = "Ruta cargada: " + MainWindow.ruta;
            MainWindow.archivos.Clear();

            foreach (string item in archivosFiltrados)
            {
                FileN39 archivo = new FileN39
                {
                    Id = MainWindow.archivos.Count + 1,
                    Active = true,
                    FilePath = item,
                    Path = MainWindow.ruta
                };
                MainWindow.archivos.Add(archivo);
            }

            programasDisponibles.Content = "Archivos en la lista: " + MainWindow.archivos.Count.ToString();
            listaProgramas.Items.Clear();

            foreach (var archivo in MainWindow.archivos)
            {
                listaProgramas.Items.Add(new FileN39
                {
                    Id = archivo.Id,
                    Active = archivo.Active,
                    FilePath = archivo.FilePath,
                    Path = archivo.Path,
                    FileName = archivo.FileName
                });

            }

            return true;
        }

        public static void LeerDatos(CheckBox preguntar)
        {
            if (File.Exists(@".\config.xml"))
            {
                try
                {
                    XmlReader config = XmlReader.Create("config.xml");
                    config.ReadStartElement("Configuracion");
                    preguntar.IsChecked = Convert.ToBoolean(config.ReadElementContentAsString());
                    MainWindow.ruta = config.ReadElementContentAsString();
                    config.Close();
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (string.IsNullOrWhiteSpace(MainWindow.ruta))
            {
                // obtiene la ruta del escritorio del usuario en sesion
                MainWindow.ruta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }


        public static void EjecutarJuego(ProcessStartInfo info, ListView listaProgramas, Label programasDisponibles, TabItem pAjustes)
        {
            try
            {
                Process.Start(info);
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                string mensaje = "El juego que iba a ejecutar ya no se encuentra disponible";
                MainWindow.archivos.Clear();
                listaProgramas.Items.Clear();
                programasDisponibles.Content = "Archivos en la lista: " + MainWindow.archivos.Count.ToString();
                AvisoDirectorioNoUtil(mensaje, pAjustes);
            }
        }


    }
}
