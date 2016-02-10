using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class InfoAlbero
    {
        public String nome;
        public String tipo;
        public int splitSize;
        public int depth;

        public Dictionary<String, String[]> edgeAttributeList;
        public Dictionary<String, String[]> vertexAttributeList;

        public InfoAlbero(String nome, String tipo, int splitSize, int depth, Dictionary<String, String[]> eal, Dictionary<String, String[]> val)
        {
            this.nome = nome;
            this.tipo = tipo;
            this.splitSize = splitSize;
            this.depth = depth;
            this.edgeAttributeList = new Dictionary<string, string[]>();
            this.vertexAttributeList = new Dictionary<string, string[]>();

            foreach (KeyValuePair<String, String[]> attr in val)
            {
                this.vertexAttributeList.Add(String.Copy(attr.Key), new String[] { String.Copy(attr.Value[0]), String.Copy(attr.Value[1]) });
            }
            foreach (KeyValuePair<String, String[]> attr in eal)
            {
                this.edgeAttributeList.Add(String.Copy(attr.Key), new String[] { String.Copy(attr.Value[0]), String.Copy(attr.Value[1]) });
            }
        }
    }
}
