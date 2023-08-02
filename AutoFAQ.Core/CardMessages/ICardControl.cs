using Kook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Core.CardMessages {
    public interface ICardControl {
        ICardBuilder AddControl(ICardBuilder cardBuilder);
    }
}
