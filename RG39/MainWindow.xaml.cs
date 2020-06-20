using RG39.N39;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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
        public static List<FileN39> archivos = new List<FileN39>();
        public static string ruta = "";
        public static string formatosCompatibles = "(Formatos compatibles: \".lnk\", \".url\" o \".exe\")";

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                FunctionsN39.LeerDatos(preguntar);
                FunctionsN39.InicializarRuta(pAjustes, rutaCargada, programasDisponibles, listaProgramas);
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
                if (string.IsNullOrWhiteSpace(ruta))
                {
                    string mensaje = "La ruta no existe, selecciona una correcta por favor.";
                    FunctionsN39.AvisoDirectorioNoUtil(mensaje, pAjustes);
                }
                else
                {
                    if (archivos.Count <= 0)
                    {
                        string mensaje = "En esta ruta no se tiene ningun archivo compatible \n" + formatosCompatibles;
                        FunctionsN39.AvisoDirectorioNoUtil(mensaje, pAjustes);
                    }
                    else
                    {
                        Random aleatorio = new Random();
                        ProcessStartInfo info = new ProcessStartInfo();
                        int num = aleatorio.Next(1, archivos.Count + 1);
                        info.UseShellExecute = true;
                        info.WorkingDirectory = ruta;

                        // Busca el archivo con el id aleatorio generado y en estado "activo"
                        FileN39 archivo = archivos.FirstOrDefault(a => a.Id == num && a.Active);
                        if (archivo == null)
                        {
                            string mensaje = "El archivo ejecutar no está disponible, se realizará el listado de nuevo.";
                            MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Warning);
                            FunctionsN39.CargarArchivosEnRuta(ruta, pAjustes, rutaCargada, programasDisponibles, listaProgramas);
                        }
                        else
                        {
                            info.FileName = archivo.FilePath.Remove(0, ruta.Length + 1);
                            if (preguntar.IsChecked == true)
                            {
                                bool repetir = false;
                                do
                                {
                                    MessageBoxResult respuesta = MessageBox.Show("Iniciar \"" + info.FileName.ToString().Remove(info.FileName.Length - 4, 4) + "\"?", "Confirmar", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                    if (respuesta == MessageBoxResult.Yes)
                                    {
                                        repetir = false;
                                        FunctionsN39.EjecutarJuego(info, listaProgramas, programasDisponibles, pAjustes);
                                    }
                                    else if (respuesta == MessageBoxResult.No)
                                    {
                                        repetir = true;
                                        aleatorio = new Random();
                                        num = aleatorio.Next(1, archivos.Count + 1);

                                        // Busca el archivo con el id aleatorio generado y en estado "activo"
                                        archivo = archivos.FirstOrDefault(a => a.Id == num && a.Active);
                                        info.FileName = archivo.FilePath.Remove(0, ruta.Length + 1);
                                    }
                                    else
                                    {
                                        repetir = false;
                                    }
                                } while (repetir);
                            }
                            else
                            {
                                FunctionsN39.EjecutarJuego(info, listaProgramas, programasDisponibles, pAjustes);
                            }
                        }
                    }
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
                    if(FunctionsN39.CargarArchivosEnRuta(dialog.FileName, pAjustes, rutaCargada, programasDisponibles, listaProgramas))
                        MessageBox.Show("Cantidad de archivos compatibles: " + archivos.Count.ToString(), "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void guardarConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(@".\config.xml"))
                    File.Delete(@".\config.xml");
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
            try
            {
                if (File.Exists(@".\config.xml"))
                    File.Delete(@".\config.xml");

                ruta = "";
                rutaCargada.Content = "Ruta cargada: " + ruta;
                archivos.Clear();

                if (MessageBox.Show("Exito al borrar!", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    FunctionsN39.InicializarRuta(pAjustes, rutaCargada, programasDisponibles, listaProgramas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void refrescar_Click_1(object sender, RoutedEventArgs e)
        {
            if(FunctionsN39.CargarArchivosEnRuta(ruta, pAjustes, rutaCargada, programasDisponibles, listaProgramas))
                MessageBox.Show("Cantidad de archivos compatibles: " + archivos.Count.ToString(), "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void activo_Click(object sender, RoutedEventArgs e)
        {
            // buscar archivo por su ID y cambiar el estado de Activo
        }

    }
}
