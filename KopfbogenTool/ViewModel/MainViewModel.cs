using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KopfbogenTool.Basics;
using System.Diagnostics;
using System.IO;
using KopfbogenTool.Service;

namespace KopfbogenTool.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel( IDialogService aDialogs, IPdfService aPdf )
        {
            mDialogs = aDialogs;
            mPdf = aPdf;

            OpenFileCommand = new RelayCommand( OpenFile );
        }

        private void OpenFile()
        {
            string theFileName = mDialogs.ShowFileOpenDialog( "PDF-Datei aussuchen, auf die der Kopfbogen angewendet werden soll", "*.pdf" );

            if( !String.IsNullOrWhiteSpace( theFileName ) )
            {
                ProcessPdf( theFileName );
            }            
        }

        public void ProcessPdf( string aFileName )
        {
            var theResult = mPdf.SetBackgroundFirstPage( aFileName );
            if( theResult != null )
            {
                ShowErrorMessage( theResult );
                return;
            }

            Process.Start( aFileName );
            App.Current.Shutdown();
        }

        public void ShowErrorMessage( string aMessage )
        {
            mDialogs.ShowMessageDialog( "Fehler", aMessage );
        }

        public RelayCommand OpenFileCommand { get; private set; }

        private readonly IDialogService mDialogs;
        private readonly IPdfService mPdf;
    }
}