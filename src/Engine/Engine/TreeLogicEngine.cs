using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using AlberoPkg;
using Engine;

/*** manca il namespace di questa classe ***/
namespace Engine {
    public class TreeLogicEngine : ILogicEngine {
        

        public
        TreeLogicEngine(){
            /*** costruttore vuoto ***/
        } // End of TreeLogicEngine() constructor



        /*
         * @calculate: metodo che consente il calcolo (i.e. somma di valori) su albero.
         */

        public long
        calculate(  int                             startNode, 
                    int                             endNode,
                    String                          treeName,
                    Dictionary<String, String>      edgeAttrList,
                    Dictionary<String, String>      nodeAttrList,
                    DatabaseInterface               dbConnection) {
                    ///////////////////////////////////////////////

                    /* 
                     * DbConnection deve essere passato come argomento perche' e' costruito
                     * con i parametri di connessione; in questo modo si mantiene la stessa
                     * connessione per tutta la sessione di lavoro.
                     */

                       
                    List<String> nodeAttrToFwd = new List<String>();
                    List<String> edgeAttrToFwd = new List<String>();
                    int sumMode;

                    if (nodeAttrList.Count != 0)
                        sumMode = 2;
                    else
                        sumMode = 0;

                    if (edgeAttrList.Count != 0)
                        sumMode += 1;
                    else
                        sumMode += 0;

                    switch (sumMode) {
                        case 3: // considera sia archi che nodi per la somma
                            nodeAttrToFwd = TreeLogicEngine.handleAttrs(nodeAttrList);
                            edgeAttrToFwd = TreeLogicEngine.handleAttrs(edgeAttrList);
                            break;
                        case 2: // considera solo i nodi per la somma
                            nodeAttrToFwd = TreeLogicEngine.handleAttrs(nodeAttrList);
                            // edgeAttrToFwd = null;
                            break;
                        case 1: // considera solo gli archi per la somma
                            // nodeAttrToFwd = null;
                            edgeAttrToFwd = TreeLogicEngine.handleAttrs(edgeAttrList);
                            break;
                        case 0: // le liste per il calcolo sono vuote
                            return 0;
                    }


                    
                    List<String> valuesList = dbConnection.getValues(treeName, startNode, endNode, nodeAttrToFwd, edgeAttrToFwd);
                    //List <String> valuesList = LogicEngineTest.getValues(treeName, startNode, endNode, nodeAttrToFwd, edgeAttrToFwd);
                    /*** Check difensivo ***/
                    if (valuesList == null)
                        return 1;
   

                    long totalSum = 0;
                    int convertedToInt = 0;

                    /*** per ogni stringa nella lista... ***/
                    for (int i = 0; i < valuesList.Count; i++) {
                        convertedToInt = 0;
                        /*** per ogni carattere della stringa i-esima della lista ***/
                        for (int j = 0; j < valuesList[i].Length; j++) {
                            convertedToInt = convertedToInt * 10 + (valuesList[i][j] - '0');
                        }
                        totalSum += convertedToInt;
                        
                    }

                    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
                     *
                     *  Secondo benchmark testati dal nostro team, la conversione con metodo custom
                     *  è notevolmente piu' veloce del sistema fornito da .NET:
                     *  Referenza -> http://cc.davelozinski.com/c-sharp/fastest-way-to-convert-a-String-to-an-int
                     *
                     *  // alternativa al metodo custom
                     *  foreach (String value in valuesList) {
                     *        totalSum += int.Parse(value)
                     *  }
                     *
                     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

                    return totalSum;
        } // End of method calculate()




        /*
         * @handleAttrs: gestisce gli attributi selezionando soltanto
         * quelli numerici, ed ignorando gli altri. Questo avviene in
         * modo "silent" in quanto e' previsto dalla UI una notifica di
         * avvertimento pre-calcolo qualora si selezionino attributi
         * di tipo "string" e si decida di proseguire.
         */

        private static List<String>
        handleAttrs(    Dictionary<String, String>  attrList    ) {
            ///////////////////////////////////////////////////////
            List<String> attrListToFwd = new List<String>();
            foreach (KeyValuePair<String, String> attrToFwd in attrList){
                
                if (attrToFwd.Value.Equals("numeric")) {
                    attrListToFwd.Add(attrToFwd.Key);
                }
            }
            return attrListToFwd;
        } // End of method handleAttrs()




        /*
         * @create: metodo per creare un oggetto albero da cui ricarare del codice XML
         * da esportare sul client. E' utilizzato uno stream di memoria per migliorare
         * le performance e ridurre al minimo le operazioni di I/O su disco.
         */

        public MemoryStream
        create
            (   String                                                  treeName,
                String                                                  treeType,
                String[]                                                buildingParameters,
                Dictionary<String, String[]>                            nodeAttrList,
                Dictionary<String, String[]>                            edgeAttrList) {
                ///////////////////////////////////////////////////////////////////////
                    /*
                     * Il parametro buildingParameters e' un array di stringhe ciascuna
                     * contenente un'informazione utile alla costruzione del generico
                     * grafo, in questo caso "splitSize" e "depth" per determinare la
                     * struttura dell'albero. I dizionari devono essere cosi impiegati:
                     * Dictionary<String -> NomeAttributo, String[] -> ParametriAddizionali
                     * Nello specifico:
                     *      <String[0] -> Tipo, String[1] -> RegolaGenerazione
                     *      Tipo := "string", "numeric"
                     *      RegolaGenerazione := "string", "random"
                     * Nota: la regola di generazione random e' acquisita inserendo un dash
                     * per indicare l'intervallo K-N (e.g. "17-244" ).
                     */
                int splitSize = int.Parse(buildingParameters[0]);
                int depth = int.Parse(buildingParameters[1]);
                Albero tree = new Albero (treeName, treeType, splitSize, depth, nodeAttrList, edgeAttrList); 
                /*** buildingParameters[0] = splitSize; buildingParameters[1] = depth; ***/

                MemoryStream XmlCompressedStream = new MemoryStream();
                XmlCompressedStream = FileEngine.assembleXML(tree);
                
                /* * * * * * * * * * * * * * * * * * * * * * * * * * * 
                 * ATTENZIONE: Chiusura del writer, cleanup finale
                 * da effettuare nel Main(), dopo l'invio dello stream
                 * al client!
                 *
                 * XmlStream.Close();
                 * XmlStream.Dispose();
                 * * * * * * * * * * * * * * * * * * * * * * * * * * */

                 return XmlCompressedStream;
        } // End of method create()




        /* 
         * @edit: metodo per modificare i valori di un determinato percorso di un albero.
         */

        public int
        edit(   String                          treeName,
                int                             startNode,
                int                             endNode,
                Dictionary<String, String>      newNodeAttrList,
                Dictionary<String, String>      newEdgeAttrList,
                DatabaseInterface               dbConnection) {
                ///////////////////////////////////////////////
                if (!Convert.ToBoolean(dbConnection.editValues(treeName, startNode, endNode, newNodeAttrList, newEdgeAttrList))) {
                    return 0; 
                } 
                else { 
                    return 1; 
                }
        } // End of method edit()




        /*
         * @delete: metodo che consente di eliminare un albero presente in database,
         * avendo a disposizione una connessione verso il database di destinazione
         * valida (i.e. credenziali adeguate).
         */
/*
        public int
        delete( String                  treeName,
                DatabaseInterface       dbConnection ) {
                ////////////////////////////////////////
                if (Convert.ToBoolean(dbConnection.deleteTree(treeName))) {
                    return 0; 
                } 
                else { 
                    return 1; 
                }
        } // End of method delete()*/
    }
}


