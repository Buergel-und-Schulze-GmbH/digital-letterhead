using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KopfbogenTool.Service
{
    public sealed class PdfService : IPdfService
    {
        public bool SetBackground( string aInputFile, string aBackgroundFile, string aOutputFile )
        {
            var theStartInfo = new ProcessStartInfo();
            theStartInfo.CreateNoWindow = false;
            theStartInfo.UseShellExecute = false;
            theStartInfo.FileName = @"Resources\pdftk.exe";
            theStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            theStartInfo.Arguments = String.Format( "\"{0}\" background \"{1}\" output \"{2}\"", aInputFile, aBackgroundFile, aOutputFile );

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using( var theProcess = Process.Start( theStartInfo ) )
                {
                    theProcess.WaitForExit();

                    if( theProcess.ExitCode != 0 )
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool Splice( IEnumerable<string> aFiles, string aOutputFile )
        {
            var theStartInfo = new ProcessStartInfo();
            theStartInfo.CreateNoWindow = false;
            theStartInfo.UseShellExecute = false;
            theStartInfo.FileName = @"Resources\pdftk.exe";
            theStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            string theFileList = "";
            foreach( var theFile in aFiles )
            {
                theFileList += theFile + " ";
            }

            theStartInfo.Arguments = String.Format( "{0}cat output \"{1}\"", theFileList, aOutputFile );

            var theReturnList = new List<string>();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using( var theProcess = Process.Start( theStartInfo ) )
                {
                    theProcess.WaitForExit();

                    if( theProcess.ExitCode != 0 )
                    {
                        return false;
                    }
                }
            }
            catch
            {
            }

            return true;
        }

        public IEnumerable<string> Split( string aFilename, string aDestinationFolder )
        {
            var theStartInfo = new ProcessStartInfo();
            theStartInfo.CreateNoWindow = false;
            theStartInfo.UseShellExecute = false;
            theStartInfo.FileName = @"Resources\pdftk.exe";
            theStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            theStartInfo.Arguments = String.Format( "\"{0}\" burst output \"{1}\\Seite%03d.pdf\"", aFilename, aDestinationFolder );

            var theReturnList = new List<string>();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using( var theProcess = Process.Start( theStartInfo ) )
                {
                    theProcess.WaitForExit();

                    if( theProcess.ExitCode == 0 )
                    {
                        var theContents = Directory.GetFiles( aDestinationFolder );
                        foreach( var theFile in theContents )
                        {
                            theReturnList.Add( theFile );
                        }
                    }
                }
            }
            catch
            {
            }

            return theReturnList;
        }

        public string SetBackgroundFirstPage( string aFilename )
        {
            // Create Temp path, make sure it's empty
            string theTempPath = Path.Combine( Path.GetTempPath(), "KopfbogenTool" );
            try
            {
                if( Directory.Exists( theTempPath ) )
                {
                    Directory.Delete( theTempPath, recursive: true );
                }
                Directory.CreateDirectory( theTempPath );
            }
            catch
            {
                return "Temporärer Pfad konnte nicht erstellt werden.";
            }

            // Sometimes splitting fails, probably because folder isn't ready
            Thread.Sleep( 500 );

            // Split Files
            var theFiles = Split( aFilename, theTempPath ).ToList();
            if( theFiles.Count() == 0 )
            {
                return "Konnte PDF nicht in verschiedene Seiten zerlegen.";
            }

            // Preserve original file, overwrite backup file if necessary
            string theBackupFile = String.Concat( aFilename, ".bak" );
            try
            {
                File.Copy( aFilename, theBackupFile, overwrite: true );
            }
            catch
            {
                return "Konnte Datei nicht kopieren.";
            }

            // Remove original file so we can re-write it
            try
            {
                File.Delete( aFilename );
            }
            catch
            {
                return "Konnte Datei nicht löschen.";
            }

            // Set background for first file, store of a temporary
            var theTempFile = Path.Combine( theTempPath, "temp.pdf" );
            if( !SetBackground( theFiles.First(), @"Resources\letterhead.pdf", theTempFile ) )
            {
                return "Konnte Kopfbogen nicht hinzufügen.";
            }

            // Just copy back if it's only one page
            if( theFiles.Count() == 1 )
            {
                try
                {
                    File.Copy( theTempFile, aFilename );
                }
                catch
                {
                    return "Konnte Datei nicht zurückkopieren.";
                }
            }
            else // Splice pages together again
            {
                theFiles[ 0 ] = theTempFile;
                if( !Splice( theFiles, aFilename ) )
                {
                    return "Konnte Seiten nicht wieder zu einem Dokument zusammenfügen.";
                }
            }

            return null;
        }
    }
}
