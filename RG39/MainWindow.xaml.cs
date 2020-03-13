using JuegoAleatorio.JuegoAleatorioDTOs;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace RG39
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ArchivoDTO> archivos = new List<ArchivoDTO>();
        public string ruta = "";
        public string formatosCompatibles = "(Formatos compatibles: \".lnk\", \".url\" o \".exe\")";

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LeerDatos();
                InicializarRuta();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ejecutar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ruta)) // si la ruta no está vacia o nula
                {
                    if (archivos.Count > 0) // si existen archivos
                    {
                        Random aleatorio = new Random();
                        ProcessStartInfo info = new ProcessStartInfo();
                        int num = aleatorio.Next(1, archivos.Count + 1);
                        info.UseShellExecute = true;
                        info.WorkingDirectory = ruta;
                        ArchivoDTO archivo = archivos.FirstOrDefault(a => a.Id == num && a.Activo); // Busca el archivo con el id aleatorio generado y en estado "activo"
                        if (archivo != null)
                        {
                            info.FileName = archivo.RNAF.Remove(0, ruta.Length + 1);
                            if (preguntar.IsChecked == true)
                            {
                                bool repetir = false;
                                do
                                {
                                    MessageBoxResult respuesta = MessageBox.Show("Iniciar \"" + info.FileName.ToString().Remove(info.FileName.Length - 4, 4) + "\"?", "Confirmar", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                    if (respuesta == MessageBoxResult.Yes)
                                    {
                                        repetir = false;
                                        EjecutarJuego(info);
                                    }
                                    else if (respuesta == MessageBoxResult.No)
                                    {
                                        repetir = true;
                                        aleatorio = new Random();
                                        num = aleatorio.Next(1, archivos.Count + 1);
                                        archivo = archivos.FirstOrDefault(a => a.Id == num && a.Activo); // Busca el archivo con el id aleatorio generado y en estado "activo"
                                        info.FileName = archivo.RNAF.Remove(0, ruta.Length + 1);
                                    }
                                    else
                                    {
                                        repetir = false;
                                    }
                                } while (repetir);
                            }
                            else
                            {
                                EjecutarJuego(info);
                            }
                        }
                        else
                        {
                            string mensaje = "El archivo ejecutar no está disponible, se realizará el listado de nuevo.";
                            MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Warning);
                            CargarArchivosEnRuta(ruta, false); // false = no fue accionado desde botón, automatico
                        }
                    }
                    else
                    {
                        string mensaje = "En esta ruta no se tiene ningun archivo compatible \n" + formatosCompatibles;
                        AvisoDirectorioNoUtil(mensaje);
                    }
                }
                else
                {
                    string mensaje = "La ruta no existe, selecciona una correcta por favor.";
                    AvisoDirectorioNoUtil(mensaje);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cambiarRuta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    AllowNonFileSystemItems = true,
                    IsFolderPicker = true,
                    Title = "Seleccionar ruta"
                };
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    CargarArchivosEnRuta(dialog.FileName, true); // true = accionado desde botón
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void guardarConfig_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@".\config.xml"))
            {
                File.Delete(@".\config.xml");
            }
            try
            {
                XmlWriter config = XmlWriter.Create("config.xml");
                config.WriteStartElement("Configuracion");
                config.WriteElementString("PreguntarJuego", preguntar.IsChecked.ToString());
                config.WriteElementString("Ruta", ruta);
                config.WriteEndElement();
                config.Close();
                MessageBox.Show("Exito al guardar!", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void borrarConfig_Click_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@".\config.xml"))
            {
                try
                {
                    File.Delete(@".\config.xml");
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            try
            {
                ruta = "";
                rutaCargada.Content = "Ruta cargada: " + ruta;
                archivos.Clear();
                if (MessageBox.Show("Exito al borrar!", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    InicializarRuta();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void refrescar_Click_1(object sender, RoutedEventArgs e)
        {
            CargarArchivosEnRuta(ruta, true); // true = accionado desde botón
        }

        private void activo_Click(object sender, RoutedEventArgs e)
        {
            // buscar archivo por su ID y cambiar el estado de Activo
        }

        // #region reutilizables
        public void CargarArchivosEnRuta(string pRuta, bool accionadoDesdeBoton)
        {
            if (!string.IsNullOrWhiteSpace(pRuta))
            {
                string[] rnaf = Directory.GetFiles(pRuta); // RNAF sigifica "Ruta Nombre de Archivo y Formato"
                if (rnaf.Length > 0) // verifico que la carpeta no esté vacia
                {
                    IEnumerable<string> archivosFiltrados = rnaf.Where(f => f.Remove(0, f.Length - 4) == ".lnk" || f.Remove(0, f.Length - 4) == ".url" || f.Remove(0, f.Length - 4) == ".exe");
                    if (archivosFiltrados != null && archivosFiltrados.Any())
                    {
                        ruta = pRuta;
                        rutaCargada.Content = "Ruta cargada: " + ruta;
                        archivos.Clear();
                        foreach (string item in archivosFiltrados)
                        {
                            ArchivoDTO archivo = new ArchivoDTO
                            {
                                Id = archivos.Count + 1,
                                Activo = true,
                                RNAF = item,
                                Ruta = ruta
                            };
                            archivos.Add(archivo);
                        }
                        programasDisponibles.Content = "Archivos en la lista: " + archivos.Count.ToString();
                        listaProgramas.Items.Clear();
                        foreach (var archivo in archivos)
                        {
                            listaProgramas.Items.Add(new ArchivoDTO
                            {
                                Id = archivo.Id,
                                Activo = archivo.Activo,
                                RNAF = archivo.RNAF,
                                Ruta = archivo.Ruta,
                                NombreArchivo = archivo.NombreArchivo
                            });

                        }
                        if (accionadoDesdeBoton)
                        {
                            MessageBox.Show("Cantidad de archivos compatibles: " + archivos.Count.ToString(), "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        string mensaje = "En:\n" + ruta + "\nNo hay videojuegos compatibles.\n" + formatosCompatibles;
                        AvisoDirectorioNoUtil(mensaje);
                    }
                }
                else
                {
                    string mensaje = "En:\n" + ruta + "\nNo hay videojuegos compatibles.\n" + formatosCompatibles;
                    AvisoDirectorioNoUtil(mensaje);
                }
            }
            else
            {
                string mensaje = "La ruta no existe, seleccione una correcta por favor.";
                AvisoDirectorioNoUtil(mensaje);
            }
        }

        public void AvisoDirectorioNoUtil(string mensaje)
        {
            if (MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                pAjustes.IsSelected = true;
            }
        }

        public void InicializarRuta()
        {
            ruta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // obtiene la ruta del escritorio del usuario en sesion
            if (!string.IsNullOrWhiteSpace(ruta))
            {
                rutaCargada.Content = "Ruta cargada: " + ruta;
                CargarArchivosEnRuta(ruta, false); // false = no fue accionado desde botón, automatico
            }
            else
            {
                string mensaje = "No se pudo encontrar la ruta de tu escritorio.\nPor favor, elige donde escanear tu lista de juegos.";
                AvisoDirectorioNoUtil(mensaje);
            }
        }

        public void LeerDatos()
        {
            if (File.Exists(@".\config.xml"))
            {
                try
                {
                    XmlReader config = XmlReader.Create("config.xml");
                    config.ReadStartElement("Configuracion");
                    preguntar.IsChecked = Convert.ToBoolean(config.ReadElementContentAsString());
                    ruta = config.ReadElementContentAsString();
                    config.Close();
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (string.IsNullOrWhiteSpace(ruta))
            {
                ruta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // obtiene la ruta del escritorio del usuario en sesion
            }
        }

        public void EjecutarJuego(ProcessStartInfo info)
        {
            try
            {
                Process.Start(info);
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                string mensaje = "El juego que iba a ejecutar ya no se encuentra disponible";
                archivos.Clear();
                listaProgramas.Items.Clear();
                programasDisponibles.Content = "Archivos en la lista: " + archivos.Count.ToString();
                AvisoDirectorioNoUtil(mensaje);
            }
        }
        // #endregion reutilizables
    }
}
