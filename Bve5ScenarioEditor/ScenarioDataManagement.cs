using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bve5ScenarioEditor
{
    class ScenarioDataManagement
    {
        public Stack<List<Scenario>> SnapShot { get; private set; }

        public ScenarioDataManagement()
        {
            SnapShot = new Stack<List<Scenario>>();
        }

        public void SetNewMemento(List<Scenario> snap)
        {
            SnapShot.Push(snap);
        }
    }
}
