using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InverseCurveSidebarBot
{
    [Function("get_dy_underlying", "uint256")]
    public class GetDyUnderlyingFunction : FunctionMessage
    {
        [Parameter("int128", "i")]
        public BigInteger i { get; set; }

        [Parameter("int128", "j")]
        public BigInteger j { get; set; }

        [Parameter("uint256", "dx")]
        public BigInteger dx { get; set; }
    }
}
