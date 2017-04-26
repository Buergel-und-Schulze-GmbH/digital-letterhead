using System;
using System.Threading.Tasks;
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

            OpenFileCommand = new RelayCommand( OpenFile, () => !IsBusy );
        }

        private void OpenFile()
        {
            string theFileName = mDialogs.ShowFileOpenDialog( "PDF-Datei aussuchen, auf die der Kopfbogen angewendet werden soll", "*.pdf" );

            if( !String.IsNullOrWhiteSpace( theFileName ) )
            {
                ProcessPdf( theFileName );
            }            
        }

        public async void ProcessPdf( string aFileName )
        {
            if( IsBusy )
            {
                return;
            }

            IsBusy = true;

            ProgressStatus = String.Format( @"Füge Kopfbogen hinzu für die Datei '{0}'...", Path.GetFileName( aFileName ) );

            await Task.Delay( 250 ); // Just to allow user to see progress

            var theResult = await Task.Run( () => mPdf.SetBackgroundFirstPage( aFileName ) );
            if( theResult != null )
            {
                ShowErrorMessage( theResult );
                IsBusy = false;
                return;
            }

            Process.Start( aFileName );
            App.Current.Shutdown();
        }

        public void ShowErrorMessage( string aMessage )
        {
            mDialogs.ShowMessageDialog( "Fehler", aMessage );
        }

        public bool IsBusy
        {
            get
            {
                return mIsBusy;
            }
            set
            {
                if( Set( nameof( IsBusy ), ref mIsBusy, value ) )
                {
                    OpenFileCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool mIsBusy = false;

        public string ProgressStatus
        {
            get
            {
                return mProgressStatus;
            }
            set
            {
                Set( nameof( ProgressStatus ), ref mProgressStatus, value );
            }
        }
        private string mProgressStatus = null;

        public RelayCommand OpenFileCommand { get; private set; }

        private readonly IDialogService mDialogs;
        private readonly IPdfService mPdf;
    }
}