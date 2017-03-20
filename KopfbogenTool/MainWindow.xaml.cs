using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using KopfbogenTool.ViewModel;
using System.IO;

namespace KopfbogenTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            // need to register ourselves as default MetroWindow for dialogs
            SimpleIoc.Default.Register<MetroWindow>(() => this);

            InitializeComponent();
        }

        private void Border_Drop( object sender, DragEventArgs e )
        {
            var theVM = DataContext as MainViewModel;

            if( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] theFiles = (string[]) e.Data.GetData( DataFormats.FileDrop );
                string theFirst = theFiles[ 0 ];

                if( Path.GetExtension( theFirst ).ToLower() == ".pdf" )
                {
                    theVM.ProcessPdf( theFirst );
                }
                else
                {
                    theVM.ShowErrorMessage( "Nur mit PDF-Dateien möglich." );
                }                
            }
            else
            {
                theVM.ShowErrorMessage( "Funktioniert nur mit Dateien." );
            }
        }
    }
}
