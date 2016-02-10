using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using AlberoPkg;
using Engine;



interface ILogicEngine {


    long 
    calculate(  int                             startNode, 
                int                             endNode,
                String                          graphName,
                Dictionary<String, String>      edgeAttrList,
                Dictionary<String, String>      nodeAttrList,
                DatabaseInterface               dbConnection);

    /* 
     * Per il momento si sta assumendo che l'unica operazione di
     * calcolo richiesta sia la somma di valori di attributi su 
     * nodi e archi.
     */



    int 
    edit(       String                          graphName,
                int                             startNode,
                int                             endNode,
                Dictionary<String, String>      newNodeAttrList,
                Dictionary<String, String>      newEdgeAttrList,
                DatabaseInterface               dbConnection);
    /* 
     * Dictionary<String, String> :
     * primo parametro String: Nome Attributo
     * secondo parametro String: Nuovo Valore da assegnare
     */


    /*********************************************************
    int 
    delete(     String                          graphName,
                DatabaseInterface               dbConnection);
    *********************************************************/


    MemoryStream  
    create
        (       String                          treeName,
                String                          treeType,
                String[]                        buildingParameters,
                Dictionary<String, String[]>    nodeAttrList,
                Dictionary<String, String[]>    edgeAttrList);

    /*
     *  E' stato usato un array buildingParameters per cercare di
     *  assumere in input molteplici parametri addizionali. Ad esempio,
     *  nel caso degli alberi questi saranno splitSize e depth, ed
     *  entrambi verranno incapsulati in due oggetti di tipo Int.
     *  buildingParameters[0] -> splitSize dell'albero
     *  buildingParameters[1] -> depth dell'albero
     */
    
}