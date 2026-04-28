using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class Health
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        public bool IsAlive { get; private set; }

        public void Initialize(float max)
        {
            Current = max;
            Max = max;
            IsAlive = true;
        }

        public void Damage(float amount)
        {
            if (!IsAlive) return;

            Current -= amount;
            IsAlive = Current > 0f;
        }
    }
}
