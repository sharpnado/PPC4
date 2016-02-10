using System;
using System.Xml;
using System.IO;
using AlberoPkg;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Engine;

namespace Engine {
    public class FileEngine {


        public
        FileEngine(){
            /////////
            /* costruttore vuoto */
        }
        

        /*
         * @import: metodo per importare un albero nel database con cui
         * è stata effettuata la connessione. Viene preso in input uno
         * stream contenente codice XML altamente compresso, il quale
         * viene successivamente parsato per ricostruire l'albero origi-
         * nario. Se il processo di importazione avviene con successo
         * viene restituito 1, altrimenti 0.
         */

        public static int
        import(    MemoryStream        XmlCompressedStream, 
                   DatabaseInterface   dbConnection     ) {
            ///////////////////////////////////////////////

            int success = 0;
            int failure = 1;
            Albero treeToImport = FileEngine.parseXML(XmlCompressedStream);
           // Console.WriteLine(treeToImport.nome);

            if (!Convert.ToBoolean(dbConnection.storeAlbero(treeToImport))) {
                return success;
            } else {
                return failure;
            }
            /*DatabaseInterface db = new DatabaseInterface("local", null,null);
			Dictionary<String, String[]> vertexAttrib = new Dictionary<string, string[]>();
			vertexAttrib.Add("Altezza", new String[]{"string", "primoValore"});
			vertexAttrib.Add ("Peso", new String[]{"numeric", "1"});
			Albero a = new Albero("enzo4", "Triste", 3, 2, vertexAttrib, vertexAttrib);
            db.storeAlbero(a);*/
        } // End of method import()




        /* @export: metodo per esportare un albero dal database di
         * cui è indicata la connessione verso un client. Costituisce
         * l'esatto complemento al metodo import() sopra documentato.
         */

        public static MemoryStream
        export( String              treeName,
                DatabaseInterface   dbConnection) {
            ///////////////////////////////////////
            MemoryStream XmlCompressedStream = new MemoryStream();
            try {
                Albero treeToExport = dbConnection.getAlbero(treeName);
                XmlCompressedStream = FileEngine.assembleXML(treeToExport);
            } catch (Exception dbError) {
                Console.WriteLine(dbError.Message);
                return null;
            }
            
            return XmlCompressedStream;
        } // End of method export()




        /*
         * @parseXML: metodo che prende in input uno stream contenente un albero serializzato
         * in XML e lo ricostruisce in modalità two-pass. Se lo stream passato non contiene
         * XML relativo ad un albero viene restituito un oggetto nullo.
         */

        public static Albero 
        parseXML(     MemoryStream    XmlCompressedStream    ) {
            ////////////////////////////////////////////////////
            MemoryStream XmlDecompressedStream = new MemoryStream();
            XmlDecompressedStream = Exflator.decompress(XmlCompressedStream);

            /*** Rimozione dall'heap degli oggetti oramai non più utili ***/
            XmlCompressedStream.Close();
            XmlCompressedStream.Dispose();

            XmlDecompressedStream.Position = 0;
            XmlReader reader = XmlReader.Create(XmlDecompressedStream);
            /*
            /*** Controllo se il file XML contiene un albero ***
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element) {
                    switch(reader.Name) {

                        case "tree":
                            break;

                        case "header":
                            return null;
                    }
                }
            }
            */
                                      /*** Analisi del custom XML header ***/
            /*** Strutture ed oggetti che verranno riempiti per costruire l'albero ***/
            String treeName = "";
            String treeType = "";
            int treeSplit = 0;
            int treeDepth = 0;
            Dictionary<String, String[]> nodeAttributeList = new Dictionary<String, String[]>();
            Dictionary<String, String[]> edgeAttributeList = new Dictionary<String, String[]>();

            /*** Finchè non si raggiunge la treeDepth non si e' conclusa l'analisi dell'header ***/
            bool EndOfTreeInfo = false;

            while (reader.Read() && !EndOfTreeInfo) {
                if (reader.NodeType == XmlNodeType.Element) {
                    switch(reader.Name) {

                        case "treeName":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    treeName = reader.Value;
                                    break;
                                }
                            }
                            break;

                        case "treeType":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    treeType = reader.Value;
                                    break;
                                }
                            }
                            break;

                        case "treeSplit":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    treeSplit = Int32.Parse(reader.Value);
                                    break;
                                }
                            }
                            break;

                        case "treeDepth":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    treeDepth = Int32.Parse(reader.Value);
                                    EndOfTreeInfo = true;
                                    break;
                                }
                            }
                            break; 
                    }    
                } 
            }
            nodeAttributeList = FileEngine.nodeAttrParser(XmlDecompressedStream);
            edgeAttributeList = FileEngine.edgeAttrParser(XmlDecompressedStream);
            Albero Tree = new Albero(treeName, treeType, treeSplit, treeDepth, nodeAttributeList, edgeAttributeList);

                                        /*** Analisi dell'XML elementTable ***/
            restoreValues(Tree, XmlDecompressedStream);
            
            return Tree;
        } // End of method parseXML()




        /*
         * @nodeAttrParser: metodo ausiliario che costruisce la lista degli
         * attributi relativi ai nodi di un albero indicato.
         */

        private static Dictionary<String, String[]>
        nodeAttrParser(    MemoryStream     XmlStream   ) {
            ///////////////////////////////////////////////
            XmlStream.Position = 0;
            XmlReader reader = XmlReader.Create(XmlStream);

            String attrName = "";
            String[] attr = new String[2];
            Dictionary<String, String[]> nodeAttributeList = new Dictionary<String, String[]>();

            bool EndOfHeader = false;
            while (reader.Read() && !EndOfHeader) {
                if (reader.NodeType == XmlNodeType.Element) {
                    switch (reader.Name) {

                        case "treeNodeAttrName":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                        attrName = reader.Value;
                                        break;
                                    }
                                }
                            break;
                        
                        case "treeNodeAttrType":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attr[0] = reader.Value;
                                    break;
                                }
                            }
                            break;

                        case "treeNodeAttrGenRule":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attr[1] = reader.Value;
                                    nodeAttributeList.Add(attrName, attr);
                                    break;
                                    }
                                }
                            break;

                        case "elementTable":
                            EndOfHeader = true;
                            break;
                    }
                }
            }

            return nodeAttributeList;
        } // End of method nodeAttrParser()




        /*
         * @edgeAttrParser: metodo ausiliario che costruisce la lista degli
         * attributi relativi agli archi di un albero indicato.
         */

        private static Dictionary<String, String[]>
        edgeAttrParser(     MemoryStream    XmlStream   ) {
            ///////////////////////////////////////////////
            XmlStream.Position = 0;
            XmlReader reader = XmlReader.Create(XmlStream);

            String attrName = "";
            String[] attr = new String[2];
            Dictionary<String, String[]> edgeAttributeList = new Dictionary<String, String[]>();

            bool EndOfHeader = false;

            while (reader.Read() && !EndOfHeader) {
                if (reader.NodeType == XmlNodeType.Element) {
                    switch (reader.Name) {
                        
                        case "treeEdgeAttrName":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attrName = reader.Value;
                                    break;
                                }
                            }
                            break;

                        case "treeEdgeAttrType":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attr[0] = reader.Value;
                                    break;
                                }
                            }
                            break;

                        case "treeEdgeAttrGenRule":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attr[1] = reader.Value;
                                    edgeAttributeList.Add(attrName, attr);
                                    break;
                                }
                            }
                            break;

                        case "elementTable":
                            EndOfHeader = true;
                            break;
                    }
                }          
            }

            return edgeAttributeList;
        } // End of method edgeAttrParser()




       /*
         * @restoreValues: metodo che assume in input uno stream di
         * memoria contente il codice XML di un albero costruito con
         * il sistema PPC ed un albero sul quale ripristinare tutti
         * gli attributi dei suoi nodi e archi. Costituisce il mezzo
         * per finalizzare l'importazione di un oggetto albero che si
         * vuole importare in database.
         */

        private static void
        restoreValues(  Albero          tree, 
                        MemoryStream    XmlStream) {
            ////////////////////////////////////////
            XmlStream.Position = 0;
            XmlReader reader = XmlReader.Create(XmlStream);
            reader.MoveToContent();
            restoreValues(tree.radice, reader);
        } // End of method restoreValues()




        /*
         * @restoreValues: metodo overloaded che, prendendo in input
         * la radice di un albero ed un XmlReader dal quale leggere
         * codice XML, ripristina ricorsivamente in preorder gli
         * attributi originari di ogni suo arco o suo nodo.
         */

        private static void
        restoreValues(  Nodo        root,
                        XmlReader   reader) {
            /////////////////////////////////
            try {
                restoreElementValues(root, reader);
            } catch (NullReferenceException NoValidNode) {
                Console.WriteLine(NoValidNode.Message);
                return;
            }

            try {
                for (int i = 0; i < root.archiUscenti.Length; i++) {
                    restoreElementValues(root.archiUscenti[i], reader);
                    restoreValues(root.archiUscenti[i].nodoFiglio, reader);
                }
            } catch (NullReferenceException EndOfTree) {
                Console.WriteLine(EndOfTree.Message);
                return;
            }
        } // End of method restoreValues()




        /* 
         * @restoreElementValues: metodo che ripristina tutti gli attributi di
         * uno specifico nodo.
         */

        private static void
        restoreElementValues(   Nodo        node,
                                XmlReader   reader) {
            /////////////////////////////////////////
            /*** verifichiamo se il nodo è davvero esistente ***/
            if (node == null)
                return;

            String attrName = "";
            String attrValue = "";
            String attrType = "";

            bool exit = false;

            while (reader.Read() && !exit) {   
                if (reader.NodeType == XmlNodeType.Element) {
                    switch (reader.Name) {

                        case "nodeAttrName":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attrName = reader.Value;
                                    break;
                                }
                            }
                            while (reader.Read()) {
                                if (reader.Name == "nodeAttrType") {
                                    while (reader.Read()) {
                                        if (reader.NodeType == XmlNodeType.Text) {
                                            attrType = reader.Value;
                                            break;
                                        }   
                                    }
                                    break;
                                }
                            }
                            while (reader.Read()) {
                                if (reader.Name == "nodeAttrValue") {
                                    while (reader.Read()) {
                                        if (reader.NodeType == XmlNodeType.Text) {
                                            attrValue = reader.Value;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }

                            /*** viene ripristinato l'attributo i-esimo del nodo con nome, tipo e valore originari ***/
                            node.attributi[attrName][0] = attrType;
                            node.attributi[attrName][1] = attrValue;
                            break;

                        case "edge":
                            exit = true;
                            break; 
                    }
                }
            }
        }




        /* 
         * @restoreElementValues: metodo che ripristina tutti gli attributi di
         * uno specifico arco.
         */

        private static void
        restoreElementValues(   Arco        edge,
                                XmlReader   reader) {
            /////////////////////////////////////////
            /*** verifichiamo se l'arco è davvero esistente ***/
            if (edge == null)
                return;

            String attrName = "";
            String attrValue = "";
            String attrType = "";

            bool exit = false;

            while (reader.Read() && !exit){
                if (reader.NodeType == XmlNodeType.Element) {
                    switch (reader.Name) {

                        case "edgeAttrName":
                            while (reader.Read()) {
                                if (reader.NodeType == XmlNodeType.Text) {
                                    attrName = reader.Value;
                                    break;
                                }
                            }
                            while (reader.Read()) {
                                if (reader.Name == "edgeAttrType") {
                                    while (reader.Read()) {
                                        if (reader.NodeType == XmlNodeType.Text) {
                                            attrType = reader.Value;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            while (reader.Read()) {
                                if (reader.Name == "edgeAttrValue") {
                                    while (reader.Read()) {
                                        if (reader.NodeType == XmlNodeType.Text) {
                                            attrValue = reader.Value;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }

                            /*** viene ripristinato l'attributo i-esimo dell'arco con nome, tipo e valore originari ***/
                            edge.attributi[attrName][0] = attrType;
                            edge.attributi[attrName][1] = attrValue;
                            break;

                        case "node":
                            exit = true;
                            break; 
                    }
                }
            }
        }




        /*
         * @assembleXML: metodo che, dato un albero in input, costruire uno
         * stream in memoria contenente il codice XML relativo all'albero stesso.
         */

        public static MemoryStream
        assembleXML(    Albero      Tree    ) {
            ///////////////////////////////////

            /*** Settaggio impostazioni XmlWriter ***/
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.Encoding = System.Text.Encoding.UTF8;
            settings.NewLineChars = Environment.NewLine;
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.OmitXmlDeclaration = false;

            MemoryStream XmlStream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(XmlStream, settings);

            /*** Inizio scrittura del codice XML ***/
            String depth = Convert.ToString(Tree.depth);
            String splitSize = Convert.ToString(Tree.splitSize);

            writer.WriteStartDocument();
            writer.WriteStartElement("tree");
                writer.WriteStartElement("header");
                    writer.WriteElementString("treeName", Tree.nome);
                    writer.WriteElementString("treeType", Tree.tipo);
                    writer.WriteElementString("treeSplit", splitSize);
                    writer.WriteElementString("treeDepth", depth);

                    writer.WriteStartElement("treeNodeAttributeList");
                        foreach (KeyValuePair<String, String[]> pair in Tree.VertexAttributeList) {
                            writer.WriteStartElement("treeNodeAttribute");
                                writer.WriteElementString("treeNodeAttrName", pair.Key.ToString());
                                writer.WriteElementString("treeNodeAttrType", pair.Value[0].ToString());
                                writer.WriteElementString("treeNodeAttrGenRule", pair.Value[1].ToString());
                            writer.WriteEndElement();
                        }
                    writer.WriteEndElement();

                    writer.WriteStartElement("treeEdgeAttributeList");
                        foreach (KeyValuePair<String, String[]> pair in Tree.EdgeAttributeList) {
                            writer.WriteStartElement("treeEdgeAttribute");
                                writer.WriteElementString("treeEdgeAttrName", pair.Key.ToString());
                                writer.WriteElementString("treeEdgeAttrType", pair.Value[0].ToString());
                                writer.WriteElementString("treeEdgeAttrGenRule", pair.Value[1].ToString());
                            writer.WriteEndElement();
                        }
                    writer.WriteEndElement();

                /*** Chiusura di header ***/
                writer.WriteEndElement();
            writer.WriteStartElement("elementTable");
            
            FileEngine.XmlTraversal(writer, Tree);

            /*** Chiusura di elementTable ***/
            writer.WriteEndElement();

            /*** Chiusura di tree ***/
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();

            MemoryStream XmlCompressedStream = new MemoryStream();
            XmlCompressedStream = Exflator.compress(XmlStream);

            XmlStream.Close();
            XmlStream.Dispose();

            return XmlCompressedStream;
        } // End of method assembleXML()


        /*** Formato del documento XML generato
                     *
                     *  <tree>
                     *      <header>
                     *          <treeName>  ... <treeName/>
                     *          <treeType>  ... <treeType/>
                     *          <treeSplit> ... <treeSplit/>
                     *          <treeDepth> ... <treeDepth/>
                     *          <treeNodeAttributeList>
                     *                      <treeNodeAttribute>
                     *                          <treeNodeAttrName>     ...   <treeNodeAttrName/>
                     *                          <treeNodeAttrType>     ...   <treeNodeAttrType/>
                     *                          <treeNodeAttrGenRule>  ...   <treeNodeAttrGenRule/>
                     *                      <treeNodeAttribute/>
                     *                      ...
                     *                      <treeNodeAttribute>                 
                     *                          <treeNodeAttrName>     ...   <treeNodeAttrName/>
                     *                          <treeNodeAttrType>     ...   <treeNodeAttrType/>
                     *                          <treeNodeAttrGenRule>  ...   <treeNodeAttrGenRule/>
                     *                      <treeNodeAttribute/>
                     *          <treeNodeAttributeList/>
                     *          <treeEdgeAttributeList>
                     *                      <treeEdgeAttribute>
                     *                          <treeEdgeAttrName>     ...   <treeEdgeAttrName/>
                     *                          <treeEdgeAttrType>     ...   <treeEdgeAttrType/>
                     *                          <treeEdgeAttrGenRule>  ...   <treeEdgeAttrGenRule/>
                     *                      <treeEdgeAttribute/>
                     *                      ...
                     *                      <treeEdgeAttribute>
                     *                          <treeEdgeAttrName>     ...   <treeEdgeAttrName/>
                     *                          <treeEdgeAttrType>     ...   <treeEdgeAttrType/>
                     *                          <treeEdgeAttrGenRule>  ...   <treeEdgeAttrGenRule/>
                     *                      <treeEdgeAttribute/>
                     *          <treeEdgeAttributeList/>                 
                     *      <header/>
                     *      <elementTable>
                     *          <node>
                     *              <nodeID> ... <nodeID/>
                     *              <nodeAttributeList>
                     *                  <nodeAttribute>
                     *                      <nodeAttrName>  ... <nodeAttrName/>
                     *                      <nodeAttrType>  ... <nodeAttrType/>
                     *                      <nodeAttrValue> ... <nodeAttrValue/>
                     *                  <nodeAttribute/>
                     *                  ...
                     *                  <nodeAttribute>
                     *                      <nodeAttrName>  ... <nodeAttrName/>
                     *                      <nodeAttrType>  ... <nodeAttrType/>
                     *                      <nodeAttrValue> ... <nodeAttrValue/>
                     *                  <nodeAttribute/>
                     *              <nodeAttributeList/>
                     *          <node/>
                     *          ...
                     *          <edge>
                     *              <edgeID> ... <edgeID/>
                     *              <edgeAttributeList>
                     *                  <edgeAttribute>
                     *                      <edgeAttrName>  ... <edgeAttrName/>
                     *                      <edgeAttrType>  ... <edgeAttrType/>
                     *                      <edgeAttrValue> ... <edgeAttrValue/>
                     *                  <edgeAttribute/>
                     *                  ...
                     *                  <edgeAttribute>
                     *                      <edgeAttrName>  ... <edgeAttrName/>
                     *                      <edgeAttrType>  ... <edgeAttrType/>
                     *                      <edgeAttrValue> ... <edgeAttrValue/>
                     *                  <edgeAttribute/>
                     *              <edgeAttributeList/>
                     *          <edge/>                      
                     *          ...
                     *      <elementTable/>
                     *  <tree/>
                     *
                     ***/ 



        /* 
          * @XmlTraversal: metodo che si occupa di visitare l'albero elemento
          * dopo elemento (nodo -> arco -> nodo -> ...) in preorder, ovvero
          * nella stessa modalità in cui è stato creato l'oggetto albero stesso,
          * in modo da sfruttare il principio di località della memoria per
          * performance (teoricamente) migliori.
          */

        private static int
        XmlTraversal(   XmlWriter   writer,
                        Albero      tree ) {
            ////////////////////////////////
            return XmlTraversal(writer, tree.radice);
        }
        /*
         * Overloading del metodo sottostante, utile a consentire
         * il passaggio come parametro di un intero albero, piuttosto
         * che della sua radice. Non influisce sulle prestazioni.
         */



        private static int
        XmlTraversal(   XmlWriter   writer, 
                        Nodo        root) {
            ///////////////////////////////

            /* Verifica se il nodo e' esistente */
            try {
                buildElementXML(writer, root);
            } catch(NullReferenceException NoValidNode) {
                Console.WriteLine(NoValidNode.Message);
                return 0;
            }

            /* Verifica se esistono archi uscenti relativi ad un nodo */
            try {
                for (int i = 0; i < root.archiUscenti.Length; i++) {
                        buildElementXML(writer, root.archiUscenti[i]);
                        XmlTraversal(writer, root.archiUscenti[i].nodoFiglio);
                }
            } catch (NullReferenceException EndOfTree) {
                Console.WriteLine(EndOfTree.Message);
                return 0;
            }
            return 0;
        } // End of method XmlTraversal()



        /* 
         * @buildElementXML: un metodo polimorfo che consente di ottenere il codice
         * XML relativo a qualsiasi elemento dell'albero che gli venga dato in input,
         * sia esso un nodo oppure un arco.
         */

        private static void
        buildElementXML(    XmlWriter   writer,
                            Nodo        node) {
            ///////////////////////////////////
            /*
             * Si verifica se l'arco di cui costruire l'XML sia un riferimento
             * a null poiche' se esso non esiste viene stampato un elemento "<node />"
             * che deformatta l'intero documento e non rende possibile un parsing corretto.
             */
            if (node == null)
                return;
            writer.WriteStartElement("node");
            writer.WriteElementString("nodeID", Convert.ToString(node.ID));
                writer.WriteStartElement("nodeAttributeList");
                foreach (KeyValuePair<String, String[]> pair in node.attributi) {
                    writer.WriteStartElement("nodeAttribute");
                        writer.WriteElementString("nodeAttrName", pair.Key.ToString());
                        writer.WriteElementString("nodeAttrType", pair.Value[0].ToString());
                        writer.WriteElementString("nodeAttrValue", pair.Value[1].ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            writer.WriteEndElement();
        } // End of method buildElementXML[nodo]




        private static void
        buildElementXML(    XmlWriter   writer,
                            Arco        edge) {
            ///////////////////////////////////
            /*
             * Si verifica se l'arco di cui costruire l'XML sia un riferimento
             * a null poiche' se esso non esiste viene stampato un elemento "<edge />"
             * che deformatta l'intero documento e non rende possibile un parsing corretto.
             */
            if (edge == null)
                return;
            writer.WriteStartElement("edge");
            writer.WriteElementString("edgeID", Convert.ToString(edge.ID));
                writer.WriteStartElement("edgeAttributeList");
                foreach (KeyValuePair<String, String[]> pair in edge.attributi) {
                    writer.WriteStartElement("edgeAttribute");
                        writer.WriteElementString("edgeAttrName", pair.Key.ToString());
                        writer.WriteElementString("edgeAttrType", pair.Value[0].ToString());
                        writer.WriteElementString("edgeAttrValue", pair.Value[1].ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            writer.WriteEndElement();
        } // End of method buildElementXML[arco]
    
    } // End of class FileEngine

} // End of namespace Engine