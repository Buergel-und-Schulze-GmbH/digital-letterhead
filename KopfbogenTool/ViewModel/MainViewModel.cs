using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KopfbogenTool.Basics;
using KopfbogenTool.Service;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
        public MainViewModel(IDialogService aDialogs, IPdfService aPdf)
        {
            mDialogs = aDialogs;
            mPdf = aPdf;

            OpenFileCommand = new RelayCommand(OpenFile, () => !IsBusy);

            CompanyOptions = new ObservableCollection<CompanyOption>
            {
                new CompanyOption { Name = "Bürgel & Schulze", LogoPath = "Resources/logo.gif", BackgroundFilePath = @"Resources\letterhead.pdf" },
                new CompanyOption { Name = "RN Nitsche GmbH", LogoPath = "Resources/logo-nitsche.png", BackgroundFilePath = @"Resources\letterhead-nitsche.pdf" }
            };

            SelectCompanyCommand = new RelayCommand<CompanyOption>(SelectCompany);
            SelectedCompany = CompanyOptions[0];
        }

        private void OpenFile()
        {
            string theFileName = mDialogs.ShowFileOpenDialog("PDF-Datei aussuchen, auf die der Kopfbogen angewendet werden soll", "*.pdf");

            if (!String.IsNullOrWhiteSpace(theFileName))
            {
                ProcessPdf(theFileName);
            }
        }

        public async void ProcessPdf(string aFileName)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            ProgressStatus = String.Format(@"Füge Kopfbogen hinzu für die Datei '{0}'...", Path.GetFileName(aFileName));

            await Task.Delay(250); // Just to allow user to see progress

            var theResult = await Task.Run(() => mPdf.SetBackgroundFirstPage(aFileName, SelectedCompany.BackgroundFilePath));
            if (theResult != null)
            {
                ShowErrorMessage(theResult);
                IsBusy = false;
                return;
            }

            Process.Start(aFileName);
            App.Current.Shutdown();
        }

        public void ShowErrorMessage(string aMessage)
        {
            mDialogs.ShowMessageDialog("Fehler", aMessage);
        }

        public bool IsBusy
        {
            get
            {
                return mIsBusy;
            }
            set
            {
                if (Set(nameof(IsBusy), ref mIsBusy, value))
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
                Set(nameof(ProgressStatus), ref mProgressStatus, value);
            }
        }
        private string mProgressStatus = null;

        public RelayCommand OpenFileCommand { get; private set; }

        public RelayCommand<CompanyOption> SelectCompanyCommand { get; private set; }

        public ObservableCollection<CompanyOption> CompanyOptions { get; private set; }

        public CompanyOption SelectedCompany
        {
            get
            {
                return mSelectedCompany;
            }
            set
            {
                if (Set(nameof(SelectedCompany), ref mSelectedCompany, value))
                {
                    RaisePropertyChanged(nameof(CurrentLogoPath));
                }
            }
        }
        private CompanyOption mSelectedCompany;

        public string CurrentLogoPath
        {
            get
            {
                return SelectedCompany?.LogoPath ?? "Resources/logo.gif";
            }
        }

        private void SelectCompany(CompanyOption aCompany)
        {
            SelectedCompany = aCompany;
        }

        private readonly IDialogService mDialogs;
        private readonly IPdfService mPdf;
    }

    public class CompanyOption
    {
        public string Name { get; set; }
        public string LogoPath { get; set; }

        public string BackgroundFilePath { get; set; }
    }
}