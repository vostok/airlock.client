using System;
using System.Text;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Binary
{
    internal interface IHerculesBinaryWriter : IBinaryWriter
    {
        bool IsOverflowed { get; set; }
    }
}