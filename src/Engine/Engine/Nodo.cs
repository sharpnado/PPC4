using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlberoPkg
{
    public class Nodo : Elemento
    {
        public String nome;
        public String tipo;
        public int ID;
        public Dictionary<String, String[]> attributi;

        // manteniamo sia collegamenti diretti ai nodi che collegamenti attraverso oggetti arco
        public Nodo[] nodiFigli;
        public Nodo nodoPadre;
        // la cardinalità di archiUscenti è determinata dalla splitSize dell'albero
        public Arco[] archiUscenti;
        public Arco arcoEntrante;

        // costruttore vuoto
        public Nodo()
        {
        }

        // costruttore
        public Nodo(String nome, String tipo, int ID, Dictionary<String,String[]> attrDescr, int split, Nodo padre)
        {
            this.nodoPadre = padre;
            this.nodiFigli = new Nodo[split];
            this.archiUscenti = new Arco[split];
            this.nome = nome;
            this.tipo = tipo;
            this.ID = ID;
            Random r = new Random(ID); // qui viene sempre fuori lo stesso valore. Non so perchè.
            /*
             * Inizializzazione degli attributi: se l'attributo ha "random" come
             * regola di inizializzazione gli assegnamo un valore casuale.
             * Altrimenti il valore prefissato che sarà passato al posto di random.
             */
            this.attributi = new Dictionary<String, String[]>();
            foreach (KeyValuePair<String,String[]> attr in attrDescr) {
                String[] value = new String[] { String.Copy(attr.Value[0]), String.Copy(attr.Value[1])};
				if (attr.Value[1].IndexOf('-') != -1)
				{
					/* se il valore contiene il trattino allora è nella forma
                     * n1-n2 e sta ad indicare un valore random compreso tra
                     * n1 ed n2
                     */
					String[] range = attr.Value [1].Split ('-');

					value[1] = r.Next(Int32.Parse(range[0]), Int32.Parse(range[1])).ToString();

				}
                this.attributi.Add(String.Copy(attr.Key), value);
            }

        }

        
    }
}
