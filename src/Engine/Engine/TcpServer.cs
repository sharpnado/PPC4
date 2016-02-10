using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Engine;

namespace Engine
{
    class TcpServer
    {
        private TcpListener server;
        private Boolean isRunning;
       // private string key;
/*___________________________________________________________________________________________________*/

        public TcpServer(Int32 port)
        {
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            server.Start();

            isRunning = true;

            LoopClients();
        }
/*___________________________________________________________________________________________________*/

        public void LoopClients()
        {
            while (isRunning)
            {
                TcpClient newClient = server.AcceptTcpClient();

                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }
/*___________________________________________________________________________________________________*/

        public void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.UTF8);
            StreamReader sReader = new StreamReader(client.GetStream(), Encoding.UTF8);

            Boolean bClientConnected = true;
            String[] dbCred =sReader.ReadLine().Split('$');
            DatabaseInterface dbInter = new DatabaseInterface(dbCred[0], dbCred[1], dbCred[2]);
            String check_auth = dbInter.check().ToString();
            sWriter.WriteLine(check_auth);
            sWriter.Flush();
            /*Connessione al database fallita*/
            if (check_auth.Equals("1"))
            {
                client.Close();
                return;
            }
            /*Connessione al database andata a buon fine*/
            /*Prendiamo i dati dal database*/
            /*Dobbiamo separare i dati contenuti in infoAlbero[] in lista alberi con relativi attributi (Divisi tra 
             * vertex ed edge)*/
            /*NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/
            InfoAlbero[] infoAlbero= dbInter.getListaAlberi();
            if (infoAlbero.Length == 0)
            {
                sWriter.WriteLine("Vuoto");
                sWriter.Flush();
            }
            else
            {
                //Console.WriteLine(parametersParser(dbInter.getListaAlberi()));
                sWriter.WriteLine(parametersParser(infoAlbero));
                sWriter.Flush();
            }
            /*Dobbiamo spedire la attribute definitions*/
            Dictionary<String, String> attributeDefinition = dbInter.getAttributeDefinition();
            /*foreach (KeyValuePair<String, String> val in attributeDefinition)
            {
                Console.WriteLine(val);
            }*/
            //Console.WriteLine(parametersParser(attributeDefinition));
            sWriter.WriteLine(parametersParser(attributeDefinition));
            sWriter.Flush();
            String response = "";
            /*_____Inizio scelta funzioni in base all'operazione scelta__________________________________________________*/
            NetworkStream netStr = client.GetStream();
            TreeLogicEngine engine = new TreeLogicEngine();
            
            while (bClientConnected)
            {
                response = sReader.ReadLine();
                if(response.Equals("Return")){continue;}

              switch (response)
                    {
                        /******************IMPORTA FILE*****************************/
                        case "Importa":
                            sWriter.WriteLine("Importa");
                            sWriter.Flush();
                            /*Leggiamo la dimensione del file da leggere*/
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                            int length = int.Parse(response);
                            //Console.WriteLine(length);
                            byte[] file = new byte[length];
                            netStr.Read(file, 0, length);
                            //Console.WriteLine(file.Length);
                            MemoryStream stream = new MemoryStream(file);
                            
                           
                            /*  public static int import(MemoryStream XmlStream, DatabaseInterface   DbConnection);*/
                            
                            sWriter.WriteLine(FileEngine.import(stream, dbInter).ToString());
                            sWriter.Flush();
                            
                            break;

                        /******************ESPORTA FILE*****************************/
                        case "Esporta":
                            sWriter.WriteLine("Esporta");
                            sWriter.Flush();
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                            /*public static MemoryStream export(String treeName,  DatabaseInterface DbConnection);*/
                            //byte[] fileXml = File.ReadAllBytes("Tree.xml");
                            byte[] fileXml = FileEngine.export(response, dbInter).ToArray();
                            sWriter.WriteLine(fileXml.Length.ToString());
                            sWriter.Flush();
                            netStr.Write(fileXml, 0, fileXml.Length);
                            break;

                        /******************CREA ALBERO*****************************/
                        case "Crea":
                            sWriter.WriteLine("Crea");
                            sWriter.Flush();
                            String[] NomTipSplitDepth = new String[4];
                            String[] SplitDept = new String[2];
                            
                            //Nome$Tipo$Splitsize$Depth
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                            NomTipSplitDepth = response.Split('$');
                            SplitDept[0] = NomTipSplitDepth[2];
                            SplitDept[1] = NomTipSplitDepth[3];

                            //NomeAttrVertex1#tipoAttr:valore$NomeAttrVertex2#tipoAttr2:valore2$etc
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                            
                            Dictionary<String, String[]> vertexAttrList = parametersParser(response);
                            //NomeAttrEdge1#tipoAttr:valore$NomeAttrEdge2#tipoAttr2:valore2$etc
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                           
                            Dictionary<String, String[]> edgeAttrList = parametersParser(response);
                            

                            //byte[] fileX = File.ReadAllBytes("Tree.xml");
                           
                            byte[] fileX = engine.create(NomTipSplitDepth[0], NomTipSplitDepth[1], SplitDept, vertexAttrList, edgeAttrList).ToArray();
                            sWriter.WriteLine(fileX.Length.ToString());
                            sWriter.Flush();
                            netStr.Write(fileX, 0, fileX.Length);
                            break;
                        
                        /******************MODIFICA ALBERO*****************************/
                        case "Modifica":
                       /* int edit(String graphName,int startNode,int endNode, Dictionary<String, String> newNodeAttrList, Dictionary<String, String> newEdgeAttrList, DatabaseInterface dbConnection);*/
                            sWriter.WriteLine("Modifica"); 
                            sWriter.Flush();
                            infoAlbero= dbInter.getListaAlberi();
                            if (infoAlbero.Length == 0)
                            {
                            sWriter.WriteLine("Vuoto");
                            sWriter.Flush();
                            }
                            else
                            {
                          //Console.WriteLine(parametersParser(dbInter.getListaAlberi()));
                          /*NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/
                             sWriter.WriteLine(parametersParser(infoAlbero));
                             sWriter.Flush();
                             }
                            
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                           
                            String[] nomeInizFin = response.Split('$');
                            response = sReader.ReadLine();
                            String newAttrVertex = response;
                           
                            if (response.Equals("Return")) { break; }
                            
                            Dictionary<String, String> newNodeAttrList = paramParser(response);
                           
                            response = sReader.ReadLine();
                            String newAttrEdge = response;
                         
                            if (response.Equals("Return")) { break; }
                            
                            Dictionary<String, String> newEdgeAttrList = paramParser(response);
                           
                            sWriter.WriteLine(engine.edit(nomeInizFin[0], int.Parse(nomeInizFin[1]), int.Parse(nomeInizFin[2]), newNodeAttrList, newEdgeAttrList, dbInter).ToString());
                            sWriter.Flush();
                            break;

                        /******************CALCOLA ALBERO*****************************/
                        case "Calcola":
                            sWriter.WriteLine("Calcola");
                            sWriter.Flush();
                            attributeDefinition = dbInter.getAttributeDefinition();
                            infoAlbero= dbInter.getListaAlberi();
                            if (infoAlbero.Length == 0)
                            {
                            sWriter.WriteLine("Vuoto");
                            sWriter.Flush();
                            }
                            else
                            {
                          //Console.WriteLine(parametersParser(dbInter.getListaAlberi()));
                          /*NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/
                             sWriter.WriteLine(parametersParser(infoAlbero));
                             sWriter.Flush();
                             }

                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                           
                            String[] nif = response.Split('$');
                      
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                          
                            String[] vertAttr = response.Split('$');
                            response = sReader.ReadLine();
                            if (response.Equals("Return")) { break; }
                       
                            String[] edgAttr = response.Split('$');
                            
                            /*long calculate(int startNode, int endNode, String treeName, Dictionayr<String, String> edgeAttrList, Dictionary<String, String> vertexAttrList, DatabaseInterface dbInterface)*/
                            Dictionary<String, String> edgeAttr = getAttrType(edgAttr, attributeDefinition);
                            Dictionary<String, String> vertexAttr = getAttrType(vertAttr, attributeDefinition);

                            sWriter.WriteLine(engine.calculate(int.Parse(nif[1]), int.Parse(nif[2]), nif[0], edgeAttr, vertexAttr, dbInter).ToString());
                            sWriter.Flush();
                            break;
                        
                        case "Disconnect":
                            bClientConnected = false;
                            break;
                                
                        
                        default:
                            Console.WriteLine("Errore...");
                            bClientConnected = false;
                            break;
                           

                    }//Fine switch
                }//Fine while
            client.Close();
        }//Fine HandleClient
/*___________________________________________________________________________________________________*/
        /*NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/
        private String parametersParser(InfoAlbero[] ninfoAlbero)
        {
            String ninfo ="";
            int count = 0;
            foreach (InfoAlbero ninfus in ninfoAlbero)
            {

                ninfo += ninfus.nome + "#";
                ninfo += parametersParser(ninfus.vertexAttributeList);
                ninfo += "#";
                ninfo += parametersParser(ninfus.edgeAttributeList);
                if (count != ninfoAlbero.Length - 1)
                {
                  ninfo += "$";
                }
                count += 1;
            }
            return ninfo;
        }
/*___________________________________________________________________________________________________*/

        private String parametersParser(Dictionary<String, String[]> attributelist)
        {
            /*attr1Vertex?tipo:attr2Vertex?tipoetc*/
            String attrString = "";
            int count = 0;
            
            foreach(KeyValuePair<String, String[]> values in attributelist)
            {

                attrString += values.Key + "?" + values.Value[0];
                if (count != attributelist.Count - 1)
                {
                    attrString += ":";
                }
                count += 1;
            }
            return attrString;
        }
/*___________________________________________________________________________________________________*/
        /*Dato l'attribute def, ritorna una stringa formata così: NomeAttr1:tipo$NomeAttr2:tipo$etc*/
        private String parametersParser(Dictionary<String, String> dict)
        {
            String attrDef = "";
            
            String[] attrs = new String[dict.Count];
            int count = 0;
            foreach (KeyValuePair<String, String> attributes in dict)
            {
                attrDef += attributes.Key + ":" + attributes.Value;
                if (count != dict.Count - 1)
                {
                    attrDef += "$";
                }
                count += 1;
            }
            return attrDef;
        }
/*___________________________________________________________________________________________________*/

        private Dictionary<String, String[]> parametersParser(String attrList)
        {
            //NomeAttrVertex1#tipoAttr:valore$NomeAttrVertex2#tipoAttr2:valore2$etc
            Dictionary<String, String[]> dictList = new Dictionary<String, String[]>();
            String[] singleAttr = attrList.Split('$');
            foreach (String attribute in singleAttr)
            {
                String[] values = attribute.Split('#');
                String[] others = values[1].Split(':');
                dictList.Add(values[0], others);
            }
            return dictList;
        }
/*___________________________________________________________________________________________________*/

        private Dictionary<String, String> paramParser(String attrList)
        {
            /*nomeAttrVertex1:newVal1$nomeAttrVertex2:newVal2$etc*/
            Dictionary<String, String> dict = new Dictionary<String, String>();
            if (attrList.Equals(""))
            {
                return dict;
            }
            String[] attributes = attrList.Split('$');
            String[] nomeValore;
            foreach (String attr in attributes)
            {
                nomeValore = attr.Split(':');
               
                dict.Add(nomeValore[0], nomeValore[1]);
            }
            return dict;
        }

/*___________________________________________________________________________________________________*/

        private Dictionary<String, String> getAttrType(String[] attributes, Dictionary<String, String> attributeDefinition)
        {
            Dictionary<String, String> dict = new Dictionary<String, String>();
            

            foreach (KeyValuePair<String, String> pairs in attributeDefinition)
            {
                foreach (String name in attributes)
                {
                    if (pairs.Key.Equals(name))
                    {
                        
                        dict.Add(name, pairs.Value);
                    }
                }
            }
            return dict;
        }

   } //Fine classe
} //Fine namespace
