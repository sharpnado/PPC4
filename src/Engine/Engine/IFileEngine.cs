using System;
using System.Xml;
using System.IO;
using AlberoPkg;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Engine;

interface IFileEngine {
    
    public int
    import(    MemoryStream        XmlStream, 
               DatabaseInterface   DbConnection);
    
    public MemoryStream
    export(     String              treeName,
                DatabaseInterface   DbConnection);

} // End of interface IFileEngine