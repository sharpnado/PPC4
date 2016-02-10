using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PPC
{
    public class TreeHandler
    {
        public String nome;
        public Dictionary<String, String> vertexAttrList;
        public Dictionary<String, String> edgeAttrList;

        public TreeHandler(String nome, Dictionary<String, String> vertexAttrList, Dictionary<String, String> edgeAttrList)
        {
            this.nome = nome;
            this.vertexAttrList = vertexAttrList;
            this.edgeAttrList = edgeAttrList;
        }



    }
}
