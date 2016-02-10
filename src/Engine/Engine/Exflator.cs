using System.IO;
using System.IO.Compression;

namespace Engine{
    public class Exflator {



        public
        Exflator() {
            ////////
            /*** costruttore vuoto ***/
        }



        /* @compress: metodo che permette la compressione di uno stream
         * tramite gli algoritmi di deflating di EXI.
         */

        public static MemoryStream
        compress(   MemoryStream    XmlStream   ) {
            ///////////////////////////////////////
            XmlStream.Position = 0;
            MemoryStream XmlCompressedStream = new MemoryStream();
            ExiStream ExiCompressor = new ExiStream(XmlCompressedStream, CompressionMode.Compress, true);
            XmlStream.CopyTo(ExiCompressor);

            /*** Chiusura del compressore (la chiusura avverrà solo a scrittura completata) ***/
            ExiCompressor.Close();
            /*** Rilascio delle risorse ***/
            ExiCompressor.Dispose();

            return XmlCompressedStream;

        } // End of method compress()




        /* @decompress: metodo che permette la decompressione di uno stream
         * tramite gli algoritmo di inflating EXI.
         */

        public static MemoryStream
        decompress(     MemoryStream    XmlCompressedStream     ) {
            ///////////////////////////////////////////////////////
            XmlCompressedStream.Position = 0;
            MemoryStream XmlDecompressedStream = new MemoryStream();
            ExiStream ExiDecompressor = new ExiStream(XmlCompressedStream, CompressionMode.Decompress, true);
            ExiDecompressor.CopyTo(XmlDecompressedStream);

            /*** Pulizia e rilascio delle risorse allocate dal decompressore ***/
            ExiDecompressor.Close();
            ExiDecompressor.Dispose();

            return XmlDecompressedStream;
            
        } // End of method decompress()

    } // End of class Exflator

} // End of namespace Engine