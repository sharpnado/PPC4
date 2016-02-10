using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlberoPkg
{
    public class Albero
    {
        public String tipo;
        public String nome;

        public int splitSize;
        public int depth;
        public int currNodeID = 0;
        public int currEdgeID = -1;

        /*
         * le attribute list sono implementate come dizionari.
         * La chiave è il nome dell'attributo, il valore è un array
         * di due stringe contenente tipo e regola di generazione.
         * Regola di generazione dei valori iniziali:
         *  - random: valore casuale
         *  - default: la stringa conterrà il valore invece della parola default
         */
        public Dictionary<String, String[]> VertexAttributeList;
        public Dictionary<String, String[]> EdgeAttributeList;

        public Nodo radice;

        public Albero(String nome, String tipo, int split, int depth, Dictionary<String, String[]> VertexAttr, Dictionary<String, String[]> EdgeAttr)
        {
            this.nome = nome;
            this.tipo = tipo;
            this.splitSize = split;
            this.depth = depth;

            this.VertexAttributeList = new Dictionary<String, String[]>();
            this.EdgeAttributeList = new Dictionary<String, String[]>();
            /*
             * Ho bisogno di una deep copy per salvare le liste di attributi
             * passate come parametri nell'albero
             */
            foreach (KeyValuePair<String, String[]> attr in VertexAttr)
            {
                this.VertexAttributeList.Add(String.Copy(attr.Key), new String[]{String.Copy(attr.Value[0]), String.Copy(attr.Value[1])});
            }
            foreach (KeyValuePair<String, String[]> attr in EdgeAttr)
            {
                this.EdgeAttributeList.Add(String.Copy(attr.Key), new String[] { String.Copy(attr.Value[0]), String.Copy(attr.Value[1]) });
            }

            // creazione del nodo radice
            this.radice = new Nodo(this.nome, this.tipo, currNodeID, this.VertexAttributeList, this.splitSize, null);
         

            // creazione dell'albero
            buildNodes(this.depth, radice);

			
        }

        // procedura ricorsiva per la creazione dei nodi
        private void buildNodes(int currDepth, Nodo parentNode)
        {
            if (currDepth == 0) return;
            currDepth = currDepth - 1;
            for (int i = 0; i < parentNode.nodiFigli.Length; i++)
            {
                this.currNodeID++;
                // creazione di ognuno dei nodi figli
                parentNode.nodiFigli[i] = new Nodo(this.nome, this.tipo, currNodeID, this.VertexAttributeList, this.splitSize, null);
                // creazione del corretto arco uscente per collegare il nodo appena creato
                parentNode.archiUscenti[i] = new Arco(parentNode, parentNode.nodiFigli[i], ++this.currEdgeID, this.EdgeAttributeList);
                // collagamento del nuovo arco nel nodo appena creato
                parentNode.nodiFigli[i].arcoEntrante = parentNode.archiUscenti[i];

                buildNodes(currDepth, parentNode.nodiFigli[i]);
            }
        }
    }
}
