using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu
{
    public interface IPlayer
    {
        void Play(string path, string subtitle);

        void Queue(string path, string subtitle);

        bool IsPlaying { get; }

        event PlayStateChangingHandler PlayStateChanging;
    }

    public delegate void PlayStateChangingHandler(IPlayer sender, bool oldState, bool newState);
}
