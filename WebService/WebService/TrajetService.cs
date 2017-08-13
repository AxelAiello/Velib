using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace WebService
{
    [ServiceContract]
    public interface TrajetService
    {
        [OperationContract]
        string submit(string d, string a);
    }
}
