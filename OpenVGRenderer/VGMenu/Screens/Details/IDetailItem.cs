using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens.Details
{
    public interface IDetailItem
    {
        string BackdropPath { get;  }

        string FullCoverPath { get;  }

        string Title { get;  }

        string Description { get;  }

        IEnumerable<PlayableFile> PlayableFiles { get; }
    }
}
