using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AlberoPkg;
using System.Data.SqlClient;

namespace Engine
{
	public class DatabaseInterface
	{
		public string lastException;
		private SqlConnection connection;

		// il costruttore si occupa di stabilire la connessione al database server
		public DatabaseInterface(String address, String user, String pass)
		{
			if (address.Equals("local"))
			{
				this.connection = new SqlConnection("Persist Security Info=False;Integrated Security=true; Initial Catalog=metadata;server=(local)");
			}
			else
			{
				this.connection = new SqlConnection("Data Source="+ address +";" +
				                                    "Initial Catalog=MetadataSQL;" +
				                                    "User id=" + user + ";" +
				                                    "Password=" + pass + ";");
			}
		}

		/* controlla se è possibile connettersi al database con 
         * i dati forniti
         */
		public int check()
		{
			try
			{
				this.connection.Open();
			}
			catch (Exception e)
			{
				this.lastException = e.Message;
				return 1;
			}
			this.connection.Close();
			return 0;
		}

		/* modifica i valori di attributi dell'albero con nome albero */
		public int editValues(String nomeAlbero, int sVertex, int eVertex, Dictionary<String, String> vAttrList, Dictionary<String,String> eAttrList) {
			Albero a = this.getAlbero (nomeAlbero);
			if (a==null) {
				return 1;
			}
			return this.editValues (a, sVertex, eVertex, vAttrList, eAttrList);

		}

		/* modifica i valori di attributi dell'albero con struttura dati albero */
		public int editValues(Albero a, int sVertex, int eVertex, Dictionary<String, String> vAttrList, Dictionary<String,String> eAttrList) {
            Elemento[] eList = this.getPath(a, sVertex, eVertex);

            SqlCommand cmd = new SqlCommand("", this.connection);
            this.connection.Open();
            this.connection.ChangeDatabase(a.nome);
            SqlDataReader res;

            Dictionary<String,Guid> vAttrUid = new Dictionary<String,Guid>();
            foreach (KeyValuePair<String, String> attr in vAttrList)
            {
                cmd.CommandText = "SELECT AttrDefUid FROM AttrDefVertex WHERE Name='"+ attr.Key +"'";
                res = cmd.ExecuteReader();
                res.Read();
                vAttrUid.Add(attr.Key, res.GetGuid(0));
                res.Close();
            }
            Dictionary<String,Guid> eAttrUid = new Dictionary<String,Guid>();
            foreach (KeyValuePair<String, String> attr in eAttrList)
            {
                cmd.CommandText = "SELECT AttrDefUid FROM AttrDefEdge WHERE Name='"+ attr.Key +"'";
                res = cmd.ExecuteReader();
                res.Read();
                eAttrUid.Add(attr.Key, res.GetGuid(0));
                res.Close();
            }

            Guid eUid;
            foreach(Elemento e in eList) {
                // caso di e nodo
                if (e.GetType().Name.Equals("Nodo"))
                {
                    // prendo uid dell'elemento corrente
                    cmd.CommandText = "SELECT VertexUid FROM Vertex WHERE VertexId=" + ((Nodo)e).ID;
                    
                    res = cmd.ExecuteReader();
                    res.Read();
                    eUid = res.GetGuid(0);
                    res.Close();

                    foreach (KeyValuePair<String, String> attr in vAttrList)
                    {
                        ((Nodo)e).attributi[attr.Key] = new String[] { ((Nodo)e).attributi[attr.Key][0], attr.Value };
                        cmd.CommandText = "UPDATE AttrUsageVertex SET Value='"+ attr.Value +"' WHERE AttrDefUid='"+ vAttrUid[attr.Key] + "' AND VertexUid='" + eUid + "'";
                        cmd.ExecuteNonQuery();
                    }
                } else
                {
                // caso di e arco
                    // prendo uid dell'elemento corrente
                    cmd.CommandText = "SELECT EdgeUid FROM Edge WHERE EdgeId=" + ((Arco)e).ID;
                    res = cmd.ExecuteReader();
                    res.Read();
                    eUid = res.GetGuid(0);
                    res.Close();

                    foreach (KeyValuePair<String, String> attr in eAttrList)
                    {
                        ((Arco)e).attributi[attr.Key] = new String[] { ((Arco)e).attributi[attr.Key][0], attr.Value };
                        cmd.CommandText = "UPDATE AttrUsageEdge SET Value='" + attr.Value + "' WHERE AttrDefUid='" + eAttrUid[attr.Key] + "' AND EdgeUid='" + eUid + "'";
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            this.connection.Close();

            return 0;
		}

		/* restituisce la attribute definition, ovvero la lista
         * di tutti gli attributi di tutti gli alberi presenti finora.
         * Viene utilizzata per fornire all'utente una scelta iniziale
         * su attributi che si possono usare in un albero.
         */
		public Dictionary<String, String> getAttributeDefinition()
		{
			Dictionary<String, String> ad = new Dictionary<string, string>();

			try
			{

				SqlCommand ADQuery = new SqlCommand("SELECT * FROM AttrDef", this.connection);
				this.connection.Open();
				this.connection.ChangeDatabase("metadata");
				SqlDataReader res = ADQuery.ExecuteReader();
				

				while (res.Read())
				{
					ad.Add(res.GetString(res.GetOrdinal("Name")), res.GetString(res.GetOrdinal("Type")));
				}

			}
			catch (Exception e)
			{
				this.lastException = e.Message;
                Console.WriteLine(e.Message);
				return null;
			}
            this.connection.Close();
			return ad;
		}

		/* restituisce la lista degli alberi presenti sul database server */
		public InfoAlbero[] getListaAlberi()
		{

			/*
             * Idea: creare un database di metadati su ogni albero
             * disponibile al quale connettersi per ottenere per ogni
             * albero:
             * - nome,
             * - tipo,
             * - splitSize,
             * - depth,
             * - edgeAttributeList,
             * - vertexAttributeList
             * 
             * edgeAttributeList e vertexAttributeList saranno salvati nella tabella
             * come stringhe nel formato: nome.tipo.regola,nome.tipo.regola,...
             */

			List<InfoAlbero> treeList = new List<InfoAlbero>();
			// connessione a db e query
			try
			{
				SqlCommand c = new SqlCommand("SELECT * FROM dbo.Tree", this.connection);
				this.connection.Open();
				this.connection.ChangeDatabase("metadata");
				SqlDataReader res = c.ExecuteReader();

				if (res.HasRows)
				{
					while (res.Read())
					{
						Dictionary<String, String[]> eal = new Dictionary<string, string[]>();
						Dictionary<String, String[]> val = new Dictionary<string, string[]>();

						String[] singleAttr = new String[3];

						// split e popolazione di attributi degli edge
						String[] splitOne = res.GetString(res.GetOrdinal("EdgeAttributeList")).Split(',');
						for (int j = 0; j < splitOne.Length; j++)
						{
							singleAttr = splitOne[j].Split('.');
							eal.Add(singleAttr[0], new String[] { new String(singleAttr[1].ToCharArray()), new String(singleAttr[2].ToCharArray()) });
						}

						// split e popolazione di attributi dei vertex
						splitOne = res.GetString(res.GetOrdinal("VertexAttributeList")).Split(',');
						for (int j = 0; j < splitOne.Length; j++)
						{
							singleAttr = splitOne[j].Split('.');
							val.Add(singleAttr[0], new String[] { new String(singleAttr[1].ToCharArray()), new String(singleAttr[2].ToCharArray()) });
						}

						treeList.Add(new InfoAlbero(res.GetString(res.GetOrdinal("Name")),
						                            res.GetString(res.GetOrdinal("Type")),
						                            res.GetInt32(res.GetOrdinal("SplitSize")),
						                            res.GetInt32(res.GetOrdinal("Depth")),
						                            eal,
						                            val));

					}
				}

			}
			catch (Exception e)
			{
				this.lastException = e.Message;
				return null;
			}


			this.connection.Close();
            
			// restituzione risultato come array semplice
			return treeList.ToArray();
		}

		/* restituisce valori di attributi di archi e nodi su un percorso tra sVertex e eVertex partendo dal nome di un albero */
		public List<String> getValues(String nomeAlbero, int sVertex, int eVertex, List<String> vAttr, List<String> eAttr)
		{
			Albero a = this.getAlbero (nomeAlbero);
			return getValues (a, sVertex, eVertex, vAttr, eAttr);

		}

		/* restituisce valori di attributi di archi e nodi su un percorso tra sVertex e eVertex partendo da un albero */
		public List<String> getValues(Albero a, int sVertex, int eVertex, List<String> vAttr, List<String> eAttr)
		{
			// prendo tutti gli elementi tra sVertex e eVertex usando il metodo getPath
			Elemento[] path = getPath(a, sVertex, eVertex);

   

			// prendo i valori degli attributi richiesti e li inserisco in una List<String>;
			List<String> result = new List<String>();
			for (int i = 0; i < path.Length; i++)
			{
               
				if (path[i].GetType().Name.Equals("Nodo"))
				{
           
					// cerca attributi da vAttr
					foreach (String att in vAttr)
					{
						result.Add(((Nodo)path[i]).attributi[att][1]);
					}
				}
				else
				{
					// cerca attributi da eAttr
					foreach (String att in eAttr)
					{
						result.Add(((Arco)path[i]).attributi[att][1]);
					}
				}
			}
        
			return result;

		}

		/* restituisce un array di elementi che formano un percorso tra i due vertici passati come parametri */
		public Elemento[] getPath(String albero, int sVertex, int eVertex)
		{

			Albero a = getAlbero (albero);
			if (a == null)
				return null;

			return getPath (a, sVertex, eVertex);

		}

		/* restituisce un array di elementi che formano un percorso tra i due vertici passati come parametri */
		public Elemento[] getPath(Albero a, int sVertex, int eVertex) {

			// controllo che esistano nodo di partenza e nodo di arrivo
			int nodiTotali = (int)(Math.Pow((Double)(a.splitSize), (Double)(a.depth + 1))) -1;
			
			if (a.splitSize > 2) {
				nodiTotali = nodiTotali / (a.splitSize - 1);
			}
			

			if (sVertex < 0
				|| sVertex >= nodiTotali
				|| eVertex < 0
				|| eVertex >= nodiTotali)
				return null;

			// non posso sapere qual è più grande, me li organizzo
			int start, end;
			if (sVertex <= eVertex) {
				start = sVertex;
				end = eVertex;
			} else {
				start = eVertex;
				end = sVertex;
			}

			// inizio la ricerca del path
			/* cerco di andare verso il nodo iniziale.
			 * Una volta trovato lo aggiungo al risultato
			 * ed aggiungo ogni nodo su cui mi muovo al risultato.
			 * Se arrivo ad un nodo senza figli prima di arrivare
			 * al nodo finale ho fallito e restituisco null.
			 * Per ogni nodo aggiunto (tranne quello iniziale)
			 * aggiungo alla lista anche l'arco entrante.
			*/
			List<Elemento> res = new List<Elemento> ();

			Nodo curr = a.radice;
			int obj = start;

			while (curr.ID != end) {
				
				// arrivo in fondo senza trovare il nodo che mi interessava
				if (curr.nodiFigli [0] == null)
					return null;

				// aggiunta elementi se necessario
				if (curr.ID == start) {
					res.Add (curr);
					
					obj = end;
				} else if (curr.ID > start) {
					
					res.Add (curr.arcoEntrante);
					res.Add (curr);
				}

				// mi dirigo verso l'obiettivo
				for (int i=a.splitSize-1; i>=0; i--) {
					if (obj >= curr.nodiFigli [i].ID) {
						curr = curr.nodiFigli [i];
						break;
					}
				}

			}

			if (start != end) {
				res.Add (curr.arcoEntrante);
			}
			res.Add (curr);
			
			Elemento[] arrRes = res.ToArray<Elemento>();
			return arrRes;

		}

		// Scomponi una struttura dati albero e salvala nel database
		public int storeAlbero(Albero a)
		{
			this.connection.Open();
			SqlCommand creaDb;

			// creazione database
			this.connection.ChangeDatabase("master");
    
			string[] queries = this.dbCreationQuery(a.nome).Split('$');

			try
			{
				/*
                 * CREAZIONE DATABASE 
                 */
				foreach (string query in queries)
				{
					creaDb = new SqlCommand(query, this.connection);
					creaDb.ExecuteNonQuery();
				}

				/*
                 *	INSERIMENTO METADATA ALBERO 
                 */
				// costruzione vertex ed edge attribute list
				// nel formato stringa da salvare in db
				string eal = "";

				this.connection.ChangeDatabase(a.nome); 
				// mentre creo le attribute list per metadata
				// approfitto per popolare le tabelle degli
				// attributi nel db dell'albero appena creato
				SqlCommand insAttr;
				Dictionary<string, Guid> edgeAttributeGuid = new Dictionary<string, Guid>();
				foreach (KeyValuePair<String, String[]> d in a.EdgeAttributeList)
				{
					eal += d.Key + "." + d.Value[0] + "." + d.Value[1] + ",";
					insAttr = new SqlCommand("INSERT INTO AttrDefEdge (Name, Type, GenerationRule) OUTPUT (inserted.AttrDefUid) VALUES ('"+ d.Key + "', '" + d.Value[0] + "', '" + d.Value[1] + "')", this.connection);
					SqlDataReader res = insAttr.ExecuteReader();
					res.Read();
					edgeAttributeGuid.Add(new String(d.Key.ToCharArray()), (Guid)res["AttrDefUid"]);
					res.Close();
				}
				if (eal.Length != 0)
				{
					eal = eal.Substring(0, eal.Length - 1);
				}

				string val = "";
				Dictionary<string, Guid> vertexAttributeGuid = new Dictionary<string, Guid>();
				foreach (KeyValuePair<String, String[]> d in a.VertexAttributeList)
				{
					val += d.Key + "." + d.Value[0] + "." + d.Value[1] + ",";
					insAttr = new SqlCommand("INSERT INTO AttrDefVertex (Name, Type, GenerationRule) OUTPUT (inserted.AttrDefUid) VALUES ('"+ d.Key + "', '" + d.Value[0] + "', '" + d.Value[1] + "')", this.connection);
					SqlDataReader res = insAttr.ExecuteReader();
					res.Read();
					vertexAttributeGuid.Add(new String(d.Key.ToCharArray()), (Guid)res["AttrDefUid"]);
					res.Close();
				}
				if (val.Length != 0)
				{
					val = val.Substring(0, val.Length - 1);
				}

				// query di inserimento
				this.connection.ChangeDatabase("metadata");
				creaDb = new SqlCommand("INSERT INTO dbo.Tree (Name,Type,SplitSize,Depth,EdgeAttributeList,VertexAttributeList) VALUES ('" + a.nome + "','" + a.tipo + "'," + a.splitSize + "," + a.depth + ",'" + eal + "','" + val + "')", this.connection);
				creaDb.ExecuteNonQuery();


				// visito l'albero a partire dalla radice.
				Nodo current = a.radice;
				this.connection.ChangeDatabase(a.nome);

				addNodoRecur(current, current.nodiFigli, vertexAttributeGuid, edgeAttributeGuid, Guid.Empty);

			}
			catch (Exception e)
			{
				this.lastException = e.Message;
				Console.WriteLine(this.lastException);
				this.connection.Close();
				return 1;
			}
            
			this.connection.Close();
			return 0;
		}

		private void addNodoRecur(Nodo n, Nodo[] figli, Dictionary<string, Guid> attrVertex, Dictionary<string, Guid> attrEdge, Guid guidPadre)
		{
			// inserimento nodo in Vertex
			SqlCommand insertNode = new SqlCommand("INSERT INTO Vertex (VertexId, Name, Type) OUTPUT (inserted.VertexUid) VALUES (" + n.ID + ", '" + n.nome + "', '" + n.tipo + "')", this.connection);
			SqlDataReader res = insertNode.ExecuteReader();

			// inserimento attributi del nodo in AttrUsageVertex
			Guid currVertexGuid;
			res.Read();
			currVertexGuid = (Guid)res["VertexUid"];
			res.Close();

			SqlCommand insAttrV;

			foreach (KeyValuePair<String, String[]> attr in n.attributi)
			{
				insAttrV = new SqlCommand("INSERT INTO AttrUsageVertex (VertexUid, AttrDefUid, Value) VALUES ('" + currVertexGuid.ToString() + "', '" + attrVertex[attr.Key].ToString() + "', '" + n.attributi[attr.Key][1] + "')", this.connection);
				insAttrV.ExecuteNonQuery();
			}

			// inserimento in db dell'arco che connette al nodo padre
			Guid currEdgeGuid = new Guid();
			if (guidPadre != Guid.Empty) {
				SqlCommand insEdge = new SqlCommand ("INSERT INTO Edge (EdgeId, StartVertexUid, EndVertexUid) OUTPUT (inserted.EdgeUid) VALUES ("+ n.arcoEntrante.ID +", '"+ guidPadre.ToString() +"', '" + currVertexGuid.ToString() + "')", this.connection);
				res = insEdge.ExecuteReader ();
				res.Read ();
				currEdgeGuid = (Guid)res ["EdgeUid"];
				res.Close ();
			}
			SqlCommand insAttrE;

			if (n.arcoEntrante != null) {
				foreach (KeyValuePair<String, String[]> attr in n.arcoEntrante.attributi) {
					insAttrE = new SqlCommand ("INSERT INTO AttrUsageEdge (EdgeUid, AttrDefUid, Value) VALUES ('" + currEdgeGuid.ToString () + "', '" + attrEdge [attr.Key].ToString () + "', '" + n.arcoEntrante.attributi [attr.Key] [1] + "')", this.connection);
					insAttrE.ExecuteNonQuery ();
				}
			}

			foreach (Nodo f in figli)
			{
				if (f != null)
					addNodoRecur(f, f.nodiFigli, attrVertex, attrEdge, currVertexGuid);
			}
		}

		public Albero getAlbero(String nomeAlbero)
		{
			// controlliamo se esiste l'albero nella lista dei nostri alberi
			InfoAlbero[] list = getListaAlberi();
			bool alberoEsiste = false;
			foreach (InfoAlbero a in list)
			{
				if (a.nome.Equals(nomeAlbero))
				{
					alberoEsiste = true;
					break;
				}
			}
			if (!alberoEsiste) return null;

			// Se arriviamo qui l'albero esiste, dobbiamo raccogliere i dati
			// dal database e costruire la struttura dati da restituire all'esterno

			// raccogliamo attributi per creazione dell'albero di base
			SqlCommand meta = new SqlCommand ("SELECT * FROM Tree WHERE Name='"+ nomeAlbero +"'", this.connection);
			this.connection.Open ();
			this.connection.ChangeDatabase ("metadata");

			SqlDataReader res = meta.ExecuteReader ();

			res.Read ();
			string type = res.GetString (1);
			int splitSize = res.GetInt32 (2);
			int depth = res.GetInt32 (3);
			string eal = res.GetString (4);
			string val = res.GetString (5);

			// conversione eal e val in dictionaries
			Dictionary<String, String[]> edgeAttributeList = new Dictionary<string, string[]>();
			Dictionary<String, String[]> vertexAttributeList = new Dictionary<string, string[]>();
			string[] splitLvlOne = eal.Split (',');
			string[] splitLvlTwo;
			foreach (string attr in splitLvlOne) {
				splitLvlTwo = attr.Split ('.');
				edgeAttributeList.Add (splitLvlTwo[0], new String[]{splitLvlTwo[1], splitLvlTwo[2]});
			}

			splitLvlOne = val.Split (',');
			foreach (string attr in splitLvlOne) {
				splitLvlTwo = attr.Split ('.');
				vertexAttributeList.Add (splitLvlTwo[0], new String[]{splitLvlTwo[1], splitLvlTwo[2]});
			}

			// istanziazione albero
			Albero alberoRisultante = new Albero(nomeAlbero, type, splitSize, depth, vertexAttributeList, edgeAttributeList);

			// visita ricorsiva su archi e nodi per assegnare i valori presenti nel database
			res.Close ();
			this.connection.ChangeDatabase (nomeAlbero);
			setValoriCorretti (alberoRisultante.radice);

			this.connection.Close ();

			return alberoRisultante;
		}

		private void setValoriCorretti(Nodo n) {

			try {

				// raccogli dati per nodo corrente
				SqlCommand cmd = new SqlCommand ("SELECT AttrDefVertex.Name, AttrDefVertex.Type, AttrUsageVertex.Value FROM Vertex, AttrDefVertex, AttrUsageVertex WHERE Vertex.VertexId="+n.ID+" AND Vertex.VertexUid=AttrUsageVertex.VertexUid AND AttrUsageVertex.AttrDefUid=AttrDefVertex.AttrDefUid", this.connection);
				
				SqlDataReader res = cmd.ExecuteReader ();
				
				// assegna dati a nodo corrente
				int h=0;
				while (res.Read()) {
					
					n.attributi [res.GetString(0)] = new String[] { res.GetString(1), res.GetString(2) };
				}
				res.Close ();
				
			}
			catch (Exception e) {
				this.lastException = e.Message;
				Console.WriteLine ("dati nodo");
				Console.WriteLine (e.Message);
			}
			try {
				// raccogli dati per arco entrante
				if (n.arcoEntrante != null) {
					SqlCommand cmd2 = new SqlCommand("SELECT AttrDefEdge.Name, AttrDefEdge.Type, AttrUsageEdge.Value FROM Edge, AttrDefEdge, AttrUsageEdge WHERE Edge.EdgeId="+n.arcoEntrante.ID+" AND Edge.EdgeUid=AttrUsageEdge.EdgeUid AND AttrUsageEdge.AttrDefUid=AttrDefEdge.AttrDefUid", this.connection);
					SqlDataReader res2 = cmd2.ExecuteReader ();

					// assegna dati ad arco entrante
					while (res2.Read()) {
						n.arcoEntrante.attributi [res2.GetString(0)] = new String[] { res2.GetString(1), res2.GetString(2) };
					}
					res2.Close ();
				}

			} catch (Exception e) {
				this.lastException = e.Message;
				Console.WriteLine ("dati arco");
				Console.WriteLine (e.Message);
			}

			// per ogni nodo figlio richiame questo metodo
			foreach (Nodo figlio in n.nodiFigli) {
				if (figlio != null)
					setValoriCorretti (figlio);
			}

		}

		private string dbCreationQuery(string nomeNuovoDatabase)
		{
			SqlCommand cmd = new SqlCommand ("SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID(N'metadata') AND type_desc = 'ROWS'", this.connection);
			SqlDataReader res = cmd.ExecuteReader ();
			res.Read ();
			String path = res.GetString (0);
			res.Close ();
			string suffix = "metadata.mdf";
			path = path.Substring(0, path.Length - suffix.Length);
			return "CREATE DATABASE [" + nomeNuovoDatabase + "] CONTAINMENT = NONE ON  PRIMARY ( NAME = N'" + nomeNuovoDatabase + "', FILENAME = N'" + path + nomeNuovoDatabase + ".mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ) LOG ON ( NAME = N'" + nomeNuovoDatabase + "_log', FILENAME = N'C:\\Program Files\\Microsoft SQL Server\\MSSQL12.MSSQLSERVER\\MSSQL\\DATA\\" + nomeNuovoDatabase + "_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)$ALTER DATABASE [" + nomeNuovoDatabase + "] SET COMPATIBILITY_LEVEL = 120$IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')) begin EXEC [" + nomeNuovoDatabase + "].[dbo].[sp_fulltext_database] @action = 'enable'end$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ANSI_NULL_DEFAULT OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ANSI_NULLS OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ANSI_PADDING OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ANSI_WARNINGS OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ARITHABORT OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET AUTO_CLOSE OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET AUTO_SHRINK OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET AUTO_UPDATE_STATISTICS ON$ALTER DATABASE [" + nomeNuovoDatabase + "] SET CURSOR_CLOSE_ON_COMMIT OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET CURSOR_DEFAULT  GLOBAL$ALTER DATABASE [" + nomeNuovoDatabase + "] SET CONCAT_NULL_YIELDS_NULL OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET NUMERIC_ROUNDABORT OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET QUOTED_IDENTIFIER OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET RECURSIVE_TRIGGERS OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET  DISABLE_BROKER$ALTER DATABASE [" + nomeNuovoDatabase + "] SET AUTO_UPDATE_STATISTICS_ASYNC OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET DATE_CORRELATION_OPTIMIZATION OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET TRUSTWORTHY OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET ALLOW_SNAPSHOT_ISOLATION OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET PARAMETERIZATION SIMPLE$ALTER DATABASE [" + nomeNuovoDatabase + "] SET READ_COMMITTED_SNAPSHOT OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET HONOR_BROKER_PRIORITY OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET RECOVERY FULL$ALTER DATABASE [" + nomeNuovoDatabase + "] SET  MULTI_USER$ALTER DATABASE [" + nomeNuovoDatabase + "] SET PAGE_VERIFY CHECKSUM$ALTER DATABASE [" + nomeNuovoDatabase + "] SET DB_CHAINING OFF$ALTER DATABASE [" + nomeNuovoDatabase + "] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )$ALTER DATABASE [" + nomeNuovoDatabase + "] SET TARGET_RECOVERY_TIME = 0 SECONDS$ALTER DATABASE [" + nomeNuovoDatabase + "] SET DELAYED_DURABILITY = DISABLED$ALTER DATABASE [" + nomeNuovoDatabase + "] SET  READ_WRITE$USE [" + nomeNuovoDatabase + "]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$SET ANSI_PADDING ON$CREATE TABLE [dbo].[Vertex]([VertexUid] [uniqueidentifier] NOT NULL, [VertexId] [int] NULL, [Name] [varchar](100) NULL, [Type] [varchar](100) NULL, CONSTRAINT [Vertex_pk] PRIMARY KEY CLUSTERED ([VertexUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$SET ANSI_PADDING OFF$ALTER TABLE [dbo].[Vertex] ADD  DEFAULT (newsequentialid()) FOR [VertexUid]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$CREATE TABLE [dbo].[Edge]([EdgeUid] [uniqueidentifier] NOT NULL, [EdgeId] [int] NULL, [StartVertexUid] [uniqueidentifier] NULL, [EndVertexUid] [uniqueidentifier] NULL, CONSTRAINT [Edge_pk] PRIMARY KEY CLUSTERED ([EdgeUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$ALTER TABLE [dbo].[Edge] ADD  DEFAULT (newsequentialid()) FOR [EdgeUid]$ALTER TABLE [dbo].[Edge]  WITH CHECK ADD  CONSTRAINT [EndVertex_to_Edge] FOREIGN KEY([EndVertexUid]) REFERENCES [dbo].[Vertex] ([VertexUid])$ALTER TABLE [dbo].[Edge] CHECK CONSTRAINT [EndVertex_to_Edge]$ALTER TABLE [dbo].[Edge]  WITH CHECK ADD  CONSTRAINT [StartVertex_to_Edge] FOREIGN KEY([StartVertexUid]) REFERENCES [dbo].[Vertex] ([VertexUid])$ALTER TABLE [dbo].[Edge] CHECK CONSTRAINT [StartVertex_to_Edge]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$SET ANSI_PADDING ON$CREATE TABLE [dbo].[AttrDefEdge]([AttrDefUid] [uniqueidentifier] NOT NULL, [Name] [varchar](50) NULL, [Type] [varchar](50) NULL, [GenerationRule] [varchar](50) NULL, CONSTRAINT [AttrDefEdge_pk] PRIMARY KEY CLUSTERED ([AttrDefUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$SET ANSI_PADDING OFF$ALTER TABLE [dbo].[AttrDefEdge] ADD  DEFAULT (newsequentialid()) FOR [AttrDefUid]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$SET ANSI_PADDING ON$CREATE TABLE [dbo].[AttrDefVertex]([AttrDefUid] [uniqueidentifier] NOT NULL, [Name] [varchar](50) NULL, [Type] [varchar](50) NULL, [GenerationRule] [varchar](50) NULL, CONSTRAINT [AttrDefVertex_pk] PRIMARY KEY CLUSTERED ([AttrDefUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$SET ANSI_PADDING OFF$ALTER TABLE [dbo].[AttrDefVertex] ADD  DEFAULT (newsequentialid()) FOR [AttrDefUid]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$SET ANSI_PADDING ON$CREATE TABLE [dbo].[AttrUsageEdge]([EdgeUid] [uniqueidentifier] NOT NULL, [AttrDefUid] [uniqueidentifier] NOT NULL, [Value] [varchar](1000) NULL, CONSTRAINT [AttrUsageEdge_pk] PRIMARY KEY CLUSTERED ([EdgeUid] ASC, [AttrDefUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$SET ANSI_PADDING OFF$ALTER TABLE [dbo].[AttrUsageEdge]  WITH CHECK ADD  CONSTRAINT [AttrDefUid_to_AttrUsageEdge] FOREIGN KEY([AttrDefUid]) REFERENCES [dbo].[AttrDefEdge] ([AttrDefUid])$ALTER TABLE [dbo].[AttrUsageEdge] CHECK CONSTRAINT [AttrDefUid_to_AttrUsageEdge]$ALTER TABLE [dbo].[AttrUsageEdge]  WITH CHECK ADD  CONSTRAINT [VertexUid_to_AttrUsageEdge] FOREIGN KEY([EdgeUid]) REFERENCES [dbo].[Edge] ([EdgeUid])$ALTER TABLE [dbo].[AttrUsageEdge] CHECK CONSTRAINT [VertexUid_to_AttrUsageEdge]$SET ANSI_NULLS ON$SET QUOTED_IDENTIFIER ON$SET ANSI_PADDING ON$CREATE TABLE [dbo].[AttrUsageVertex]([VertexUid] [uniqueidentifier] NOT NULL, [AttrDefUid] [uniqueidentifier] NOT NULL, [Value] [varchar](1000) NULL, CONSTRAINT [AttrUsageVertex_pk] PRIMARY KEY CLUSTERED ([VertexUid] ASC, [AttrDefUid] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]$SET ANSI_PADDING OFF$ALTER TABLE [dbo].[AttrUsageVertex]  WITH CHECK ADD  CONSTRAINT [AttrDefUid_to_AttrUsageVertex] FOREIGN KEY([AttrDefUid]) REFERENCES [dbo].[AttrDefVertex] ([AttrDefUid])$ALTER TABLE [dbo].[AttrUsageVertex] CHECK CONSTRAINT [AttrDefUid_to_AttrUsageVertex]$ALTER TABLE [dbo].[AttrUsageVertex]  WITH CHECK ADD  CONSTRAINT [VertexUid_to_AttrUsageVertex] FOREIGN KEY([VertexUid]) REFERENCES [dbo].[Vertex] ([VertexUid])$ALTER TABLE [dbo].[AttrUsageVertex] CHECK CONSTRAINT [VertexUid_to_AttrUsageVertex]";
		}

	}

}
