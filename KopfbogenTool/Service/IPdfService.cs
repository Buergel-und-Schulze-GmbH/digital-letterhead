using System.Collections.Generic;

namespace KopfbogenTool.Service
{
    public interface IPdfService
    {
        IEnumerable<string> Split( string aFilename, string aDestinationFolder );
        bool SetBackground( string aInputFile, string aBackgroundFile, string aOutputFile );
        bool Splice( IEnumerable<string> aFiles, string aOutputFile );
        string SetBackgroundFirstPage( string aFilename );
    }
}
