using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace EcsTweens
{
    public struct LinkToEntity : IComponentData
    {
        public Entity e;
    }
}
