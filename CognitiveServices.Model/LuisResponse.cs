using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServices.Model {

    // Type created for JSON at <<root>>
    [System.Runtime.Serialization.DataContractAttribute()]
    public partial class LUISResponse {

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string query;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public Intents[] intents;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public Entities[] entities;
    }

    // Type created for JSON at <<root>> --> intents
    [System.Runtime.Serialization.DataContractAttribute(Name = "intents")]
    public partial class Intents {

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string intent;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public double score;
    }

    // Type created for JSON at <<root>> --> entities
    [System.Runtime.Serialization.DataContractAttribute(Name = "entities")]
    public partial class Entities {

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string entity;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string type;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int startIndex;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int endIndex;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public double score;
    }
}
