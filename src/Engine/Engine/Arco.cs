using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlberoPkg
{
    public class Arco : Elemento
    {
        public int ID;
        public Dictionary<String, String[]> attributi;

        public Nodo nodoPadre;
        public Nodo nodoFiglio;

        // costruttore vuoto
        public Arco(Nodo p, Nodo f, int ID, Dictionary<String, String[]> attrDef)
        {
            this.nodoPadre = p;
            this.nodoFiglio = f;
            this.ID = ID;
            this.attributi = new Dictionary<String, String[]>();

            Random r = new Random(ID);

            foreach (KeyValuePair<String, String[]> attr in attrDef)
            {
                String[] value = new String[] { String.Copy(attr.Value[0]), String.Copy(attr.Value[1]) };
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
